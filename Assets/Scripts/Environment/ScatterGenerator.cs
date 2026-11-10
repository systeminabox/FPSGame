using System.Collections.Generic;
using UnityEngine;

namespace Environment.Editor
{
    public class ScatterGenerator
    {
        private const int MaxCandidatesPerEntry = 200000; // safety cap against a typo'd density

        private readonly ResourceSpawner spawner;
        private readonly SurfaceSampler sampler;

        public ScatterGenerator(ResourceSpawner spawner)
        {
            this.spawner = spawner;
            sampler = new SurfaceSampler(spawner);
        }

        public List<ScatterPoint> Generate()
        {
            var results = new List<ScatterPoint>();
            ResourceEntry[] entries = spawner.resourceEntries;
            if (entries == null) return results;

            // One shared occupancy grid so later entries respect earlier ones.
            var occupied = new SpatialHash(GetLargestSpacing(entries));

            // List order is priority: earlier entries claim space first.
            for (int e = 0; e < entries.Length; e++)
            {
                ResourceEntry entry = entries[e];
                if (entry == null || !entry.enabled || entry.profile == null) continue;
                GenerateForEntry(entry, e, occupied, results);
            }

            return results;
        }

        private void GenerateForEntry(ResourceEntry entry, int entryIndex,
            SpatialHash occupied, List<ScatterPoint> results)
        {
            ResourceProfile profile = entry.profile;
            float radius = spawner.radius;
            if (radius <= 0f || profile.peakDensity <= 0f) return;

            Vector3 center = spawner.transform.position;
            var rng = new System.Random(MakeSeed(entry, entryIndex));

            // Expected count at PEAK density. Thinning below brings it down
            // to the real density field.
            float area = Mathf.PI * radius * radius;
            float expected = profile.peakDensity * area
                             * entry.densityMultiplier * spawner.globalRichness;
            int candidateCount = Mathf.Min(Mathf.RoundToInt(expected), MaxCandidatesPerEntry);

            float minDist = Mathf.Max(profile.minimumDistance, 0f);
            float footprint = Mathf.Max(minDist * 0.5f, 0.25f);
            int nodeIndex = 0;

            for (int i = 0; i < candidateCount; i++)
            {
                // Always draw the same four numbers per candidate, so tweaking a
                // rule later doesn't reshuffle every point after it.
                float angle = (float)(rng.NextDouble() * Mathf.PI * 2.0);
                float dist = radius * Mathf.Sqrt((float)rng.NextDouble()); // sqrt = uniform over the disc
                float acceptRoll = (float)rng.NextDouble();
                float yaw = (float)(rng.NextDouble() * 360.0);
                float scaleRoll = (float)rng.NextDouble();

                float x = center.x + Mathf.Cos(angle) * dist;
                float z = center.z + Mathf.Sin(angle) * dist;

                // 1. Density: resource curve x crater edge fade. Reject by probability.
                float curve = Mathf.Clamp01(profile.densityCurve.Evaluate(dist / radius));
                float density = curve * EdgeFade(dist);
                if (acceptRoll > density) continue;

                // 2. Find the ground.
                SurfaceSample surface = sampler.Sample(x, z);
                if (!surface.valid) continue;

                // 3. Per-resource surface rules.
                float slope = surface.SlopeAngle;
                if (slope < profile.minSlope || slope > profile.maxSlope) continue;
                if (surface.position.y < profile.minAltitude ||
                    surface.position.y > profile.maxAltitude) continue;

                // 4. Spacing (cheap) before exclusion (physics query).
                if (occupied.IsBlocked(surface.position, minDist)) continue;
                if (sampler.IsExcluded(surface.position, footprint)) continue;

                // 5. Accepted: decide orientation and placement.
                Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
                Quaternion rotation = profile.alignToSurface
                    ? Quaternion.FromToRotation(Vector3.up, surface.normal) * yawRotation
                    : yawRotation;

                float uniformScale = Mathf.Lerp(profile.minScale, profile.maxScale, scaleRoll);

// Sink along the object's own down axis, scaled, so a big rock on a slope
// sinks into the slope instead of straight down in world space.
                Vector3 sinkDirection = rotation * Vector3.down;
                Vector3 finalPosition = surface.position + sinkDirection * (profile.sinkOffset * uniformScale);

                occupied.Add(surface.position, minDist);

                results.Add(new ScatterPoint
                {
                    entry = entry,
                    entryIndex = entryIndex,
                    index = nodeIndex++,
                    position = finalPosition,
                    normal = surface.normal,
                    rotation = rotation,
                    scale = Vector3.one * uniformScale,
                    density = density
                });
            }
        }

        // Full strength inside the inner radius, smooth fade to zero at the outer.
        private float EdgeFade(float dist)
        {
            float inner = spawner.innerRadius;
            float outer = spawner.radius;
            if (dist <= inner) return 1f;
            if (dist >= outer) return 0f;
            return 1f - Mathf.SmoothStep(0f, 1f, (dist - inner) / (outer - inner));
        }

        // Plain arithmetic on purpose. HashCode.Combine is randomized per process,
        // which would break determinism between editor sessions.
        private int MakeSeed(ResourceEntry entry, int entryIndex)
        {
            unchecked
            {
                return (spawner.seed * 73856093)
                       ^ (entry.seedOffset * 19349663)
                       ^ (entryIndex * 83492791);
            }
        }

        private static float GetLargestSpacing(ResourceEntry[] entries)
        {
            float largest = 1f;
            foreach (ResourceEntry entry in entries)
            {
                if (entry == null || entry.profile == null) continue;
                largest = Mathf.Max(largest, entry.profile.minimumDistance);
            }
            return largest;
        }

        // Grid of placed points (XZ only) so spacing checks don't scan every point.
        private class SpatialHash
        {
            private struct Occupant
            {
                public Vector2 xz;
                public float minDist;
            }

            private readonly float cellSize;
            private readonly Dictionary<Vector2Int, List<Occupant>> cells = new();

            // cellSize >= the largest spacing, so checking the 3x3 neighbourhood is always enough.
            public SpatialHash(float cellSize)
            {
                this.cellSize = Mathf.Max(cellSize, 0.5f);
            }

            private Vector2Int CellOf(Vector2 xz) =>
                new(Mathf.FloorToInt(xz.x / cellSize), Mathf.FloorToInt(xz.y / cellSize));

            public void Add(Vector3 position, float minDist)
            {
                var xz = new Vector2(position.x, position.z);
                Vector2Int cell = CellOf(xz);
                if (!cells.TryGetValue(cell, out var list))
                {
                    list = new List<Occupant>();
                    cells[cell] = list;
                }
                list.Add(new Occupant { xz = xz, minDist = minDist });
            }

            public bool IsBlocked(Vector3 position, float minDist)
            {
                var xz = new Vector2(position.x, position.z);
                Vector2Int cell = CellOf(xz);

                for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (!cells.TryGetValue(new Vector2Int(cell.x + dx, cell.y + dy), out var list))
                        continue;

                    foreach (Occupant o in list)
                    {
                        // Use the larger of the two spacings, so a wide-spaced resource
                        // is respected by narrow-spaced ones too.
                        float required = Mathf.Max(minDist, o.minDist);
                        if ((o.xz - xz).sqrMagnitude < required * required) return true;
                    }
                }
                return false;
            }
        }
    }
}