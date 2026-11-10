using Inventory;
using UnityEngine;

namespace Environment
{
    [CreateAssetMenu(fileName = "New Resource Profile", menuName = "Environment/Profile", order = 0)]
    public class ResourceProfile : ScriptableObject
    {
        public ItemDefinition resource;
        public ResourceBody resourcePrefab;
        public AnimationCurve densityCurve;
        [Tooltip("Nodes per square unit")]
        public float peakDensity;
        public float minimumDistance;
        public Color gizmoColor;
        
        [Header("Layer 2")]
        public float minSlope;
        public float maxSlope = 90;
        public float minAltitude;
        public float maxAltitude = 100000;
        public bool alignToSurface;
        public float sinkOffset;
        
        [Header("Layer 3")]
        public float minScale = 1f;
        public float maxScale = 1f;
    }
}