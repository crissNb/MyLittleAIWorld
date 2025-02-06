using System.Collections.Generic;
using UnityEngine;

public class ChatBubbleManager : MonoBehaviour
{
    public static ChatBubbleManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField]
    private GameObject _chatBubblePrefab;

    [SerializeField]
    private Transform testTransform;

    [SerializeField]
    private Transform testTransform2;

    private readonly Dictionary<Transform, ChatBubble> _chatBubbles =
        new Dictionary<Transform, ChatBubble>();

    /// <summary>
    /// Creates (or reuses) a chat bubble for the given target.
    /// </summary>
    public void CreateChatBubble(string message, Transform target)
    {
        ChatBubble chatBubble;
        if (_chatBubbles.ContainsKey(target))
        {
            chatBubble = _chatBubbles[target];
        }
        else
        {
            chatBubble = Instantiate(_chatBubblePrefab, transform).GetComponent<ChatBubble>();
            _chatBubbles.Add(target, chatBubble);
        }

        chatBubble.Display(target, message);
    }

    /// <summary>
    /// Helper class used to do screen–space bubble collision resolution.
    /// </summary>
    private class ChatBubbleInfo
    {
        public ChatBubble Bubble;
        public Vector3 ScreenPos;
        public Vector2 Size;

        /// <summary>
        /// The z–value (depth) of the bubble in screen–space (so we can convert back to world).
        /// </summary>
        public float Depth;

        public ChatBubbleInfo(ChatBubble bubble, Vector3 screenPos, Vector2 size, float depth)
        {
            Bubble = bubble;
            ScreenPos = screenPos;
            Size = size;
            Depth = depth;
        }

        /// <summary>
        /// Returns the screen–space rectangle for this bubble (centered at ScreenPos).
        /// </summary>
        public Rect GetScreenRect()
        {
            return new Rect(
                ScreenPos.x - Size.x * 0.5f,
                ScreenPos.y - Size.y * 0.5f,
                Size.x,
                Size.y
            );
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            CreateChatBubble(
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                testTransform
            );
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            CreateChatBubble(
                "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                testTransform2
            );
        }

        // Early out if there are no chat bubbles.
        if (_chatBubbles.Count == 0)
            return;

        Camera cam = Camera.main;
        List<ChatBubbleInfo> bubbleInfos = new List<ChatBubbleInfo>();

        // First, build a list of bubbles with their “desired” screen positions.
        foreach (KeyValuePair<Transform, ChatBubble> kvp in _chatBubbles)
        {
            ChatBubble bubble = kvp.Value;
            if (bubble.Source == null)
                continue;

            // Each bubble “wants” to appear at its target position plus a vertical offset.
            Vector3 desiredWorldPos = bubble.Source.position + Vector3.up * bubble.VerticalOffset;
            Vector3 screenPos = cam.WorldToScreenPoint(desiredWorldPos);
            // Get the size of the bubble in screen space.
            Vector2 screenSize = bubble.GetScreenSize();
            float depth = screenPos.z;
            bubbleInfos.Add(new ChatBubbleInfo(bubble, screenPos, screenSize, depth));
        }

        // Simple collision resolution:
        // For each pair, if their rectangles overlap, shift the lower one upward.
        for (int i = 0; i < bubbleInfos.Count; i++)
        {
            for (int j = i + 1; j < bubbleInfos.Count; j++)
            {
                Rect rectA = bubbleInfos[i].GetScreenRect();
                Rect rectB = bubbleInfos[j].GetScreenRect();
                if (rectA.Overlaps(rectB))
                {
                    // Calculate how much rectB must be moved upward so that its bottom
                    // is just above rectA.top.
                    float pushAmount = (rectA.yMax - rectB.yMin) + 5f; // add a 5px margin
                    bubbleInfos[j].ScreenPos = new Vector3(
                        bubbleInfos[j].ScreenPos.x,
                        bubbleInfos[j].ScreenPos.y + pushAmount,
                        bubbleInfos[j].ScreenPos.z
                    );
                }
            }
        }

        // Finally, apply the (possibly adjusted) positions and ensure bubbles face the camera.
        foreach (ChatBubbleInfo info in bubbleInfos)
        {
            Vector3 adjustedWorldPos = cam.ScreenToWorldPoint(
                new Vector3(info.ScreenPos.x, info.ScreenPos.y, info.Depth)
            );
            info.Bubble.SetWorldPosition(adjustedWorldPos);
            info.Bubble.FaceCamera();
        }
    }
}
