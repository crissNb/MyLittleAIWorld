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
        AwaitResponse,
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
        if (GetPersonality().Name == "Sebastian" || GetPersonality().Name == "Johny")
        {
            return;
        }
        if (_aiState == AIState.AwaitResponse)
        {
            return;
        }

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
            _aiState = AIState.AwaitResponse;
            _actionTimer = _actionInterval;
        }
    }

    public string GoTo(string place)
    {
        // Look for residence
        Transform residence = AIRepository.Instance.FindResidence(place);

        if (residence != null)
        {
            _npcController.Move(residence.position);
            _aiState = AIState.Walking;
            return "";
        }

        // Look for other locations

        return place + " not found";
    }

    public string Talk(LLMAction llmAction)
    {
        string targetPerson = llmAction.target;
        string message = llmAction.content;
        AIController targetAI = AIRepository.Instance.GetAIController(targetPerson);

        if (targetAI == null)
        {
            return targetPerson + " not found";
        }

        // Check if target is within talking distance
        if (Vector3.Distance(transform.position, targetAI.transform.position) > _talkDistance)
        {
            return targetPerson + " is too far away";
        }

        // Talk to target
        targetAI.ReceiveTalk(llmAction);

        _aiState = AIState.Talking;

        return "";
    }

    public void ReceiveTalk(LLMAction llmAction)
    {
        // Handle incoming message
        _aiState = AIState.Talking;
        _aiInterpreter.SetOpponentAction(new OpponentAction(llmAction.target, llmAction));
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

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(GetPersonality().Name + " is entering: " + other.name);

        _aiInterpreter.SetLocation(other.name);
    }
}
