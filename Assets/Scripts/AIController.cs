using UnityEngine;

public class AIController : MonoBehaviour
{
    [SerializeField]
    private NPCController _npcController;

    [SerializeField]
    private AIInterpreter _aiInterpreter;

    [SerializeField]
    private Transform _residence;

    private float _actionTimer;
    private float _actionInterval;
    private float _talkDistance;

    private AIState _aiState;

    private enum AIState
    {
        Idle,
        Walking,
        Talking
    }

    public void StartAI(float actionOffset, float actionInterval, float talkDistance)
    {
        _actionTimer = actionOffset;
        _actionInterval = actionInterval;
        _aiState = AIState.Idle;
        _talkDistance = talkDistance;
        _aiInterpreter.SetLocation(_residence.name);
    }

    private void Update()
    {
        if (_aiState == AIState.Walking)
        {
            if (!_npcController.IsMoving())
            {
                _aiState = AIState.Idle;
            }
            return;
        }

        _actionTimer -= Time.deltaTime;

        if (_actionTimer <= 0)
        {
            Debug.Log("AI action timer expired");
            _aiInterpreter.SendRequest();
            _aiState = AIState.Walking;
            _actionTimer = _actionInterval;
        }
    }

    public bool GoTo(string place)
    {
        // Look for residence
        Transform residence = AIRepository.Instance.FindResidence(place);

        if (residence != null)
        {
            _npcController.Move(residence.position);
            _aiState = AIState.Walking;
            return true;
        }

        // Look for other locations

        return false;
    }

    public bool Talk(string targetPerson, string message)
    {
        AIController targetAI = AIRepository.Instance.GetAIController(targetPerson);

        if (targetAI == null)
        {
            Debug.LogError("AIController not found for " + targetPerson);
            return false;
        }

        // Check if target is within talking distance
        if (Vector3.Distance(transform.position, targetAI.transform.position) > _talkDistance)
        {
            Debug.LogError("Target " + targetPerson + " is too far away");
            return false;
        }

        // Talk to target
        targetAI.ReceiveTalk(GetPersonality().Name, message);

        _aiState = AIState.Talking;

        return true;
    }

    public void ReceiveTalk(string from, string message)
    {
        // Handle incoming message
        _aiState = AIState.Talking;
        _aiInterpreter.SetOpponentAction(new OpponentAction(from, message));
        _aiInterpreter.SendRequest();
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
