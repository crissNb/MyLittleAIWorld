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

    // The extra vertical offset (in pixels) for chat bubbles in top–down view
    [SerializeField]
    private float headScreenOffset = 50f;

    [SerializeField]
    private float _positionSmoothSpeed = 10f;

    [SerializeField]
    private float _topDownThreshold = 0.5f; // When to start transitioning to top-down offset

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

        // Build a list of bubbles with their “desired” screen positions.
        foreach (KeyValuePair<Transform, ChatBubble> kvp in _chatBubbles)
        {
            ChatBubble bubble = kvp.Value;
            if (bubble.Source == null)
                continue;

            // Calculate camera angle factor (0 = side view, 1 = top-down)
            float topDownFactor = Mathf.Clamp01(
                (Vector3.Dot(cam.transform.forward, Vector3.down) - _topDownThreshold)
                    / (1f - _topDownThreshold)
            );

            // Base world position above the source
            Vector3 baseWorldPos = bubble.Source.position + Vector3.up * bubble.VerticalOffset;
            Vector3 screenPos = cam.WorldToScreenPoint(baseWorldPos);

            // Calculate screen-space offset based on camera angle
            float currentHeadOffset = headScreenOffset * topDownFactor;
            screenPos.y += currentHeadOffset;

            // Get the size of the bubble in screen space
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
                    // is just above rectA.top, plus a small margin.
                    float pushAmount = (rectA.yMax - rectB.yMin) + 5f;
                    bubbleInfos[j].ScreenPos = new Vector3(
                        bubbleInfos[j].ScreenPos.x,
                        bubbleInfos[j].ScreenPos.y + pushAmount,
                        bubbleInfos[j].ScreenPos.z
                    );
                }
            }
        }

        // Finally, convert the (possibly adjusted) screen positions back to world positions,
        // and update the bubbles.
        foreach (ChatBubbleInfo info in bubbleInfos)
        {
            Vector3 targetWorldPos = cam.ScreenToWorldPoint(
                new Vector3(info.ScreenPos.x, info.ScreenPos.y, info.Depth)
            );

            // Get current position or use target position if bubble was just created
            Vector3 currentPos = info.Bubble.transform.position;

            // Smoothly interpolate to the target position
            Vector3 newPos = Vector3.Lerp(
                currentPos,
                targetWorldPos,
                Time.deltaTime * _positionSmoothSpeed
            );

            info.Bubble.SetWorldPosition(newPos);
            info.Bubble.FaceCamera();
        }
    }
}
