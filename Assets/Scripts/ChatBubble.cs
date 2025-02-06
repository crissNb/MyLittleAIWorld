using TMPro;
using UnityEngine;

public class ChatBubble : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform _handle;

    [SerializeField]
    private TMP_Text text;

    [SerializeField]
    private Vector2 padding = new Vector2(0.5f, 0.3f);

    [SerializeField]
    private SpriteRenderer _bubbleRenderer;

    // If you wish, assign _handleRenderer in the inspector; otherwise, we find it in Start.
    [SerializeField]
    private SpriteRenderer _handleRenderer;

    private Transform _source;

    /// <summary>
    /// The target transform this bubble is attached to.
    /// </summary>
    public Transform Source => _source;

    /// <summary>
    /// How far above the target (in world units) the bubble should be anchored.
    /// Adjust as needed.
    /// </summary>
    public float VerticalOffset => 1.0f;

    private void Start()
    {
        // If you did not assign these in the inspector, try to find them.
        if (_bubbleRenderer == null)
            _bubbleRenderer = GetComponent<SpriteRenderer>();
        if (_handleRenderer == null && _handle != null)
            _handleRenderer = _handle.GetComponent<SpriteRenderer>();

        // (If your bubble prefab already has proper pivot settings, you might not need additional adjustments.)
    }

    /// <summary>
    /// Displays the bubble with the given message and attaches it to the target.
    /// </summary>
    public void Display(Transform source, string message)
    {
        _source = source;
        text.text = message;
        UpdateBubbleSize();
        UpdateHandlePosition();
        FaceCamera();
    }

    private void Update()
    {
        if (_source == null)
            return;

        UpdateHandlePosition();
        // Note: The ChatBubbleManager now handles positioning and facing.
    }

    /// <summary>
    /// Calculates the size of the bubble (in screen pixels) based on the SpriteRenderer bounds.
    /// </summary>
    public Vector2 GetScreenSize()
    {
        Camera cam = Camera.main;
        // Get the world–space size from the bubble renderer.
        Vector3 worldSize = _bubbleRenderer.bounds.size;
        // Assume the bubble is centered at transform.position.
        Vector3 screenCorner1 = cam.WorldToScreenPoint(transform.position - worldSize * 0.5f);
        Vector3 screenCorner2 = cam.WorldToScreenPoint(transform.position + worldSize * 0.5f);
        return new Vector2(
            Mathf.Abs(screenCorner2.x - screenCorner1.x),
            Mathf.Abs(screenCorner2.y - screenCorner1.y)
        );
    }

    /// <summary>
    /// Sets the bubble’s world position.
    /// </summary>
    public void SetWorldPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    /// <summary>
    /// Makes the bubble face the camera (billboarding).
    /// </summary>
    public void FaceCamera()
    {
        Camera cam = Camera.main;
        // Rotate so that the forward vector points from the camera toward the bubble.
        // (Alternatively, you can use a LookAt if you wish to preserve an upright orientation.)
        Vector3 direction = transform.position - cam.transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            // Keep the bubble upright by zeroing out any rotation around the z–axis.
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
        }
    }

    /// <summary>
    /// Updates the size of the bubble background based on the text’s mesh bounds.
    /// </summary>
    private void UpdateBubbleSize()
    {
        text.ForceMeshUpdate();
        Bounds textBounds = text.textBounds;
        Vector2 requiredSize = new Vector2(
            textBounds.size.x + padding.x,
            textBounds.size.y + padding.y
        );
        _bubbleRenderer.size = requiredSize;
    }

    /// <summary>
    /// Updates the handle’s (pointer’s) position along the edge of the bubble.
    /// This calculation uses the bubble’s local space.
    /// </summary>
    private void UpdateHandlePosition()
    {
        if (_source == null)
            return;

        // Compute direction from the bubble (in world space) to the target.
        Vector3 toSourceWorld = _source.position - transform.position;
        // Convert that direction into the bubble’s local space.
        Vector3 toSourceLocal = transform.InverseTransformDirection(toSourceWorld.normalized);
        Vector2 direction = new Vector2(toSourceLocal.x, toSourceLocal.y).normalized;
        if (direction == Vector2.zero)
            return;

        // Calculate the bubble’s half–dimensions in local space.
        float halfWidth = _bubbleRenderer.size.x / 2f + _handleRenderer.size.x / 2f;
        float halfHeight = _bubbleRenderer.size.y / 2f + _handleRenderer.size.y / 2f;

        // Calculate the scale factor for intersection with the bubble’s bounds.
        float tx =
            direction.x != 0
                ? (direction.x > 0 ? halfWidth : -halfWidth) / direction.x
                : Mathf.Infinity;
        float ty =
            direction.y != 0
                ? (direction.y > 0 ? halfHeight : -halfHeight) / direction.y
                : Mathf.Infinity;
        float t = Mathf.Min(tx, ty);

        // Position the handle at the intersection point.
        Vector2 edgePoint = direction * t;
        _handle.localPosition = edgePoint;

        // Set the handle’s rotation so that it “points” back to the target.
        // (Here we pick one of two fixed angles based on whether the pointer is more horizontal or vertical.)
        bool isHorizontal = Mathf.Abs(direction.x) > Mathf.Abs(direction.y);
        float clampedAngle = 0f;
        if (isHorizontal)
            clampedAngle = direction.x > 0 ? 90f : -90f;
        else
            clampedAngle = direction.y > 0 ? 180f : 0f;
        _handle.localRotation = Quaternion.Euler(0, 0, clampedAngle);
    }
}
