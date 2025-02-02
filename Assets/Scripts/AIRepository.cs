using System.Collections.Generic;
using UnityEngine;

public class AIRepository : MonoBehaviour
{
    public static AIRepository Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField]
    private AIController[] _aiControllers;

    [SerializeField]
    private float _actionInterval;

    [SerializeField]
    private float _talkDistance;

    private readonly List<Personality> _personalities = new();
    private readonly List<Transform> _residences = new();

    private void Start()
    {
        foreach (AIController aiController in _aiControllers)
        {
            _personalities.Add(aiController.GetPersonality());
            _residences.Add(aiController.GetResidence());
            aiController.StartAI(Random.Range(0, _actionInterval), _actionInterval, _talkDistance);
        }
    }

    public AIController GetAIController(string name)
    {
        foreach (AIController aiController in _aiControllers)
        {
            if (aiController.GetPersonality().Name == name)
            {
                return aiController;
            }
        }

        return null;
    }

    public Personality GetPersonality(string name)
    {
        foreach (Personality personality in _personalities)
        {
            if (personality.Name == name)
            {
                return personality;
            }
        }

        return null;
    }

    public string[] GetAllPersonalities()
    {
        string[] names = new string[_personalities.Count];

        for (int i = 0; i < _personalities.Count; i++)
        {
            names[i] = _personalities[i].Name;
        }

        return names;
    }

    public Transform FindResidence(string name)
    {
        foreach (Transform residence in _residences)
        {
            if (residence.name.Contains(name))
            {
                return residence;
            }
        }

        return null;
    }
}
