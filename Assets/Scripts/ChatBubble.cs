using System;
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

    [SerializeField]
    private float _cornerRadius = 0.1f;

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
        // The ChatBubbleManager now handles positioning and facing.
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
    /// Now we use a screen–aligned billboard approach so the bubble always faces the camera.
    /// </summary>
    public void FaceCamera()
    {
        Camera cam = Camera.main;
        // Use the camera's forward and up directions to create a billboard that is fully aligned with the screen.
        transform.LookAt(
            transform.position + cam.transform.rotation * Vector3.forward,
            cam.transform.rotation * Vector3.up
        );
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

        Camera cam = Camera.main;
        if (cam == null)
            return;

        // Get screen positions of the bubble and the source
        Vector3 bubbleScreenPos = cam.WorldToScreenPoint(transform.position);
        Vector3 sourceScreenPos = cam.WorldToScreenPoint(_source.position);

        // If source screen position is not visible, hide the bubble
        if (sourceScreenPos.z < 0)
        {
            _handle.gameObject.SetActive(false);
            return;
        }
        else
        {
            _handle.gameObject.SetActive(true);
        }

        // Compute direction in screen space
        Vector2 direction = (sourceScreenPos - bubbleScreenPos).normalized;

        if (direction == Vector2.zero)
            return;

        // Calculate the bubble’s half–dimensions in local space, adjusted for the handle's size
        float halfWidth = _bubbleRenderer.size.x / 2f + _handleRenderer.size.x / 2f;
        float halfHeight = _bubbleRenderer.size.y / 2f + _handleRenderer.size.y / 2f;

        // Calculate intersection with the bubble's edge along this direction
        float tx =
            direction.x != 0
                ? (direction.x > 0 ? halfWidth : -halfWidth) / direction.x
                : Mathf.Infinity;
        float ty =
            direction.y != 0
                ? (direction.y > 0 ? halfHeight : -halfHeight) / direction.y
                : Mathf.Infinity;
        float t = Mathf.Min(tx, ty);

        Vector2 edgePoint = direction * t;

        // If the intersection point is on the vertical edges (left or right)
        bool isVertical = tx < ty;

        float clampedAngle = 0f;
        if (isVertical)
        {
            clampedAngle = direction.x > 0 ? 90f : -90f;

            // Adjust the edge point to the edge of the bubble
            edgePoint.y = Mathf.Clamp(
                edgePoint.y,
                -halfHeight + _cornerRadius,
                halfHeight - _cornerRadius
            );
        }
        else
        {
            clampedAngle = direction.y > 0 ? 180f : 0f;
            edgePoint.x = Mathf.Clamp(
                edgePoint.x,
                -halfWidth + _cornerRadius,
                halfWidth - _cornerRadius
            );
        }

        _handle.localPosition = edgePoint;
        _handle.localRotation = Quaternion.Euler(0, 0, clampedAngle);
    }
}
