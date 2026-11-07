using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private int id;          // 0 = unassigned
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;

        public int ID => id;
        public string DisplayName => displayName;
        public Sprite Icon => icon;

#if UNITY_EDITOR
        // Only the database's editor-time validation should call this.
        public void EditorSetID(int newID) => id = newID;
#endif
    }
}