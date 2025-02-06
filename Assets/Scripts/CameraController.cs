using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField]
    private float _panSpeed = 30f;

    [SerializeField]
    private float _zoomSpeed = 20f;

    [SerializeField]
    private float _smoothSpeed = 10f;

    [SerializeField]
    private float _edgeScrollThreshold = 20f;

    [Header("Boundaries")]
    [SerializeField]
    private float _minX = -50f;

    [SerializeField]
    private float _maxX = 50f;

    [SerializeField]
    private float _minZ = -50f;

    [SerializeField]
    private float _maxZ = 50f;

    [Header("Distance Settings")]
    [SerializeField]
    private float _minDistance = 1f;

    [SerializeField]
    private float _maxDistance = 6f;

    [SerializeField]
    private float _defaultDistance = 4f;

    [Header("Behind Mode Rotation Settings")]
    // When at the default offset distance, the camera pitch is 68°.
    // When zoomed all the way in (_minDistance), the camera pitch should be 20°.
    [SerializeField]
    private float _behindPitch = 68f; // Pitch at default offset.

    [SerializeField]
    private float _minBehindPitch = 20f; // Pitch at minimum distance.

    // Use a fixed yaw so that the camera does not snap to the player's rotation.
    [SerializeField]
    private float _behindYaw = 0f;

    [Header("Top-Down Mode Rotation Settings")]
    [SerializeField]
    private float _topDownPitch = 90f;

    [SerializeField]
    private float _topDownYaw = 0f;

    [Header("Target Settings")]
    [SerializeField]
    private Transform _playerBody;

    // The pivot around which the camera orbits.
    // It starts centered on the player but can be panned, and reset with Space.
    private Vector3 _targetPosition;

    // The current camera distance (modified via mouse scroll).
    private float _currentDistance;

    private void Start()
    {
        _currentDistance = _defaultDistance;
        _targetPosition = _playerBody.position;
        UpdateCameraTransform(true);
    }

    private void Update()
    {
        HandleInput();
        UpdateCameraTransform();
    }

    private void HandleInput()
    {
        // Allow free panning regardless of zoom level.
        HandlePanning();
        HandleZooming();

        // Recenter the free–pan pivot on the player when pressing Space.
        if (Input.GetKey(KeyCode.Space))
        {
            _targetPosition = _playerBody.position;
            ClampTargetToBoundaries();
        }
    }

    private void HandlePanning()
    {
        Vector3 moveDirection = Vector3.zero;
        Vector2 mousePosition = Input.mousePosition;
        if (mousePosition.x < _edgeScrollThreshold)
            moveDirection.x = -1;
        if (mousePosition.x > Screen.width - _edgeScrollThreshold)
            moveDirection.x = 1;
        if (mousePosition.y < _edgeScrollThreshold)
            moveDirection.z = -1;
        if (mousePosition.y > Screen.height - _edgeScrollThreshold)
            moveDirection.z = 1;

        // Also support WASD (or arrow keys via Horizontal/Vertical axes).
        moveDirection.x += Input.GetAxis("Horizontal");
        moveDirection.z += Input.GetAxis("Vertical");
        moveDirection = moveDirection.normalized;

        _targetPosition += _panSpeed * Time.deltaTime * moveDirection;
        ClampTargetToBoundaries();
    }

    private void HandleZooming()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // Update the current distance and clamp it.
            _currentDistance = Mathf.Clamp(
                _currentDistance - scroll * _zoomSpeed,
                _minDistance,
                _maxDistance
            );
            // Note: We no longer auto–recenter when zooming in.
        }
    }

    private void ClampTargetToBoundaries()
    {
        _targetPosition.x = Mathf.Clamp(_targetPosition.x, _minX, _maxX);
        _targetPosition.z = Mathf.Clamp(_targetPosition.z, _minZ, _maxZ);
    }

    /// <summary>
    /// Updates the camera’s transform by blending between behind mode and top–down mode.
    /// In behind mode the camera orbits around the free–pan pivot using an interpolated pitch.
    /// When at the default distance the pitch is 68°, and when zoomed in all the way (min distance)
    /// the pitch is 20°.
    /// </summary>
    /// <param name="instant">If true, the camera jumps immediately to the new transform.</param>
    private void UpdateCameraTransform(bool instant = false)
    {
        // Use the free–pan pivot for both modes.
        Vector3 pivot = _targetPosition;

        // --- Behind Mode Calculation (for zoomed–in view) ---
        // Calculate t so that when _currentDistance equals _defaultDistance, t is 0,
        // and when _currentDistance equals _minDistance, t is 1.
        float tBehind = (_defaultDistance - _currentDistance) / (_defaultDistance - _minDistance);
        tBehind = Mathf.Clamp01(tBehind);
        // Interpolate pitch from _behindPitch (68° at default) to _minBehindPitch (20° at min distance).
        float currentBehindPitch = Mathf.Lerp(_behindPitch, _minBehindPitch, tBehind);
        Quaternion behindRotation = Quaternion.Euler(currentBehindPitch, _behindYaw, 0f);
        // Compute the behind position by moving backwards from the pivot along the rotated forward vector.
        Vector3 behindPosition = pivot - (behindRotation * Vector3.forward) * _currentDistance;

        // --- Top–Down Mode Calculation ---
        Quaternion topRotation = Quaternion.Euler(_topDownPitch, _topDownYaw, 0f);
        Vector3 topPosition = pivot + Vector3.up * _currentDistance;

        // --- Blend Between Modes ---
        // When at _defaultDistance, we're fully in behind mode.
        // As we zoom out past _defaultDistance, blend toward the top–down view.
        float blendFactor =
            (_currentDistance - _defaultDistance) / (_maxDistance - _defaultDistance);
        blendFactor = Mathf.Clamp01(blendFactor);

        Quaternion finalRotation = Quaternion.Slerp(behindRotation, topRotation, blendFactor);
        Vector3 finalPosition = Vector3.Lerp(behindPosition, topPosition, blendFactor);

        // Smoothly update the camera’s transform.
        if (instant)
        {
            transform.position = finalPosition;
            transform.rotation = finalRotation;
        }
        else
        {
            transform.position = Vector3.Lerp(
                transform.position,
                finalPosition,
                Time.deltaTime * _smoothSpeed
            );
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                finalRotation,
                Time.deltaTime * _smoothSpeed
            );
        }
    }
}
