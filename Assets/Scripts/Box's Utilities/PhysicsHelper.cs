
using UnityEngine;

public static class PhysicsHelper
{
    public static bool Raycast(Transform start, Vector3 direction, float interactRange, out RaycastHit hit, LayerMask layerMask = default)
    {
        var color = Color.red;
        var output = Physics.Raycast(start.transform.position, direction, out hit, interactRange, layerMask);
        if(output) color = Color.green;
        Debug.DrawRay(start.position, direction * interactRange, color, 5f);
        return output;
    }
}
