using System;
using Environment.Editor;
using UnityEngine;

namespace Environment
{
    [Serializable]
    public class ResourceEntry
    {
        public ResourceProfile profile;
        public bool enabled = true;
        public float densityMultiplier = 1;
        public int seedOffset;
    }
    
    
    public class ResourceSpawner : MonoBehaviour
    {
        public float radius = 100;
        public float innerRadius = 80;
        public int seed;
        public float globalRichness = 1;
        public ResourceEntry[] resourceEntries;

        public LayerMask groundLayer;
        public LayerMask excludedLayer;
        public Terrain terrainReference;
        public float maxRaycastHeight = 1000;

        private void OnValidate()
        {
            if(innerRadius > radius) innerRadius = radius;
            
        }

        private void OnDrawGizmosSelected()
        {
            // var sampler = new SurfaceSampler(this);
            // var generator = new ScatterGenerator(this);
            // var points = generator.Generate();
            // foreach (var point in points)
            // {
            //     Vector3 p = point.position;
            //     float len = point.scale.x * 10;
            //
            //     Gizmos.color = Color.green; // up: alignment + scale
            //     Gizmos.DrawLine(p, p + point.rotation * Vector3.up * len);
            //
            //     Gizmos.color = Color.blue;  // forward: yaw
            //     Gizmos.DrawLine(p, p + point.rotation * Vector3.forward * len);
            // }
            
        }
    }
}