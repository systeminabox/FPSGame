using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class KeepAspectRatio : MonoBehaviour
{
    public enum DrivenBy { TargetWidth, TargetHeight }

    [SerializeField] private float widthRatio = 16f;
    [SerializeField] private float heightRatio = 9f;
    [SerializeField] private RectTransform targetToScaleTo;
    [SerializeField] private DrivenBy drivenBy = DrivenBy.TargetWidth;

    private RectTransform rect;
    private Vector2 lastTargetSize;

    private void OnEnable()  => Apply(true);
    private void OnValidate() => Apply(true);

    private void LateUpdate() => Apply(false);

    private void Apply(bool force)
    {
        if (targetToScaleTo == null || widthRatio <= 0f || heightRatio <= 0f)
            return;

        if (rect == null)
            rect = GetComponent<RectTransform>();

        Vector2 targetSize = targetToScaleTo.rect.size;

        // Skip work if nothing changed, and skip a not-yet-laid-out target.
        if (!force && targetSize == lastTargetSize)
            return;
        if (targetSize.x <= 0f || targetSize.y <= 0f)
            return;

        lastTargetSize = targetSize;

        if (drivenBy == DrivenBy.TargetWidth)
        {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSize.x);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSize.x * heightRatio / widthRatio);
        }
        else
        {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSize.y);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSize.y * widthRatio / heightRatio);
        }
    }
}