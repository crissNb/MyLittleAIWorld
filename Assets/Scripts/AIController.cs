using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent _navMeshAgent;

    [SerializeField]
    private AIInterpreter _aiInterpreter;

    [SerializeField]
    private Transform _residence;

    private float _actionTimer;
    private float _actionInterval;

    private AIState _aiState;

    private enum AIState
    {
        Idle,
        Busy
    }

    public void StartAI(float actionOffset, float actionInterval)
    {
        _actionTimer = actionOffset;
        _actionInterval = actionInterval;
        _aiState = AIState.Idle;
    }

    private void Update()
    {
        if (_aiState == AIState.Busy)
        {
            return;
        }

        _actionTimer -= Time.deltaTime;

        if (_actionTimer <= 0)
        {
            _aiInterpreter.SendRequest();
            _aiState = AIState.Busy;
            _actionTimer = _actionInterval;
        }
    }

    public void GoTo(string place)
    {
        // Look for residence
        Transform residence = AIRepository.Instance.FindResidence(place);

        if (residence != null)
        {
            _navMeshAgent.SetDestination(residence.position);
            return;
        }

        // Look for other locations
    }

    public void Talk(string targetPerson, string message)
    {

    }

    public Personality GetPersonality()
    {
        return _aiInterpreter.Personality;
    }

    public Transform GetResidence()
    {
        return _residence;
    }
}
