using UnityEngine;

namespace Environment.Editor
{
    public enum SurfaceFailReason { None, NoGround, Excluded }

    public struct SurfaceSample
    {
        public bool valid;
        public SurfaceFailReason reason;
        public Vector3 position;
        public Vector3 normal;

        public float SlopeAngle => Vector3.Angle(normal, Vector3.up);
    }

    public class SurfaceSampler
    {
        private readonly Terrain terrain;
        private readonly LayerMask groundMask;
        private readonly LayerMask excludeMask;
        private readonly float castStartY;
        private readonly float castDistance;

        // Snapshot the spawner's settings once, so a preview or bake
        // always uses one consistent set of values.
        public SurfaceSampler(ResourceSpawner spawner)
        {
            terrain = spawner.terrainReference;
            groundMask = spawner.groundLayer;
            excludeMask = spawner.excludedLayer;
            castStartY = spawner.transform.position.y + spawner.maxRaycastHeight;
            castDistance = spawner.maxRaycastHeight * 2f;
        }

        public SurfaceSample Sample(float x, float z)
        {
            var origin = new Vector3(x, castStartY, z);

            // Raycast is the source of truth: it sees terrain AND meshes.
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit,
                    castDistance, groundMask, QueryTriggerInteraction.Ignore))
            {
                return new SurfaceSample
                {
                    valid = true,
                    reason = SurfaceFailReason.None,
                    position = hit.point,
                    normal = hit.normal
                };
            }

            // Fallback: sample the terrain heightmap directly.
            if (TrySampleTerrain(x, z, out Vector3 pos, out Vector3 normal))
            {
                return new SurfaceSample
                {
                    valid = true,
                    reason = SurfaceFailReason.None,
                    position = pos,
                    normal = normal
                };
            }

            return new SurfaceSample { valid = false, reason = SurfaceFailReason.NoGround };
        }

        // Separate check, run after you have a ground hit.
        public bool IsExcluded(Vector3 position, float footprintRadius)
        {
            return Physics.CheckSphere(position, footprintRadius, excludeMask,
                QueryTriggerInteraction.Ignore);
        }

        private bool TrySampleTerrain(float x, float z, out Vector3 position, out Vector3 normal)
        {
            position = default;
            normal = Vector3.up;

            if (terrain == null) return false;

            TerrainData data = terrain.terrainData;
            Vector3 terrainPos = terrain.transform.position;

            // Normalized 0-1 coordinates across the terrain.
            float u = (x - terrainPos.x) / data.size.x;
            float v = (z - terrainPos.z) / data.size.z;
            if (u < 0f || u > 1f || v < 0f || v > 1f) return false;

            float y = terrain.SampleHeight(new Vector3(x, 0f, z)) + terrainPos.y;
            position = new Vector3(x, y, z);
            normal = data.GetInterpolatedNormal(u, v);
            return true;
        }
    }
}