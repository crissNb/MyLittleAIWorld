using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent _agent;

    [SerializeField]
    private Animator _animator;

    [Header("Animation Settings")]
    [SerializeField]
    private string _velocityParameter = "Speed";

    [Header("Movement Settings")]
    [SerializeField]
    private float _moveSpeed = 1.5f;

    [SerializeField]
    private float _acceleration = 15f;

    [SerializeField]
    private float _rotationSmoothTime = 0.1f;

    private Quaternion _targetRotation;

    private void Start()
    {
        _agent.speed = _moveSpeed;
        _agent.acceleration = _acceleration;
        _agent.angularSpeed = 0;
        _agent.updateRotation = false;
        _targetRotation = _agent.transform.rotation;
    }

    private void Update()
    {
        UpdateAnimator();
        UpdateRotationDirection();
        HandleRotation();
    }

    private void UpdateAnimator()
    {
        float velocity = _agent.velocity.magnitude / _agent.speed;
        _animator.SetFloat(_velocityParameter, velocity);
    }

    private Vector3 GetNextPathCorner()
    {
        // Use the 2nd corner in the path (1st is current position)
        if (_agent.path != null && _agent.path.corners.Length > 1)
        {
            return _agent.path.corners[1];
        }
        return _agent.destination; // Fallback to final destination
    }

    private void HandleRotation()
    {
        _agent.transform.rotation = Quaternion.Slerp(
            _agent.transform.rotation,
            _targetRotation,
            Time.deltaTime / _rotationSmoothTime
        );
    }

    private void UpdateRotationDirection()
    {
        // Only update rotation when moving
        if (_agent.pathPending || _agent.remainingDistance <= _agent.stoppingDistance)
            return;

        // Get the next path corner the agent is moving toward
        Vector3 nextPathCorner = GetNextPathCorner();
        Vector3 direction = (nextPathCorner - _agent.transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            _targetRotation = Quaternion.LookRotation(direction);
        }
    }

    public void Move(Vector3 destination)
    {
        _agent.SetDestination(destination);
    }

    public bool IsMoving()
    {
        return _agent.remainingDistance > _agent.stoppingDistance;
    }
}
