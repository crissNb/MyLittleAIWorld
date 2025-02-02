using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LLMAPIHandler : MonoBehaviour
{
    public static LLMAPIHandler Instance { get; private set; }

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

    [System.Serializable]
    public enum ChatModel
    {
        GPT4o,
        Claude3_5Sonnet
    }

    [System.Serializable]
    private class Message
    {
        public string role;
        public string content;

        public Message(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }

    [System.Serializable]
    private class ChatRequest
    {
        public List<Message> messages;
        public string model;
        public string provider;
    }

    [System.Serializable]
    private class ChatResponse
    {
        public List<Choice> choices;

        [System.Serializable]
        public class Choice
        {
            public Message message;
        }
    }

    [Header("API Configuration")]
    [SerializeField]
    private string apiUrl = "http://localhost:1337/v1/chat/completions";

    [SerializeField]
    private ChatModel defaultModel = ChatModel.GPT4o;

    [SerializeField]
    private string[] providers = { "AIChatFree", "AutonomousAI" };

    public void SendChatRequest(
        string userMessage,
        System.Action<string> callback,
        System.Action<string> errorCallback = null
    )
    {
        StartCoroutine(SendRequestCoroutine(userMessage, defaultModel, callback, errorCallback));
    }

    private IEnumerator SendRequestCoroutine(
        string userMessage,
        ChatModel model,
        System.Action<string> callback,
        System.Action<string> errorCallback = null
    )
    {
        string providers = string.Join(" ", this.providers);

        // Create request data
        var requestData = new ChatRequest
        {
            messages = new List<Message> { new Message("user", userMessage) },
            model = ModelToString(model),
            provider = providers
        };

        // Convert to JSON
        string jsonData = JsonUtility.ToJson(requestData);

        // Create web request
        using UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Send request
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            errorCallback?.Invoke(request.error);
            yield break;
        }

        // Parse response
        var response = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
        if (response != null && response.choices != null && response.choices.Count > 0)
        {
            callback?.Invoke(response.choices[0].message.content);
        }
        else
        {
            errorCallback?.Invoke("Failed to parse response");
        }
    }

    private string ModelToString(ChatModel model)
    {
        return model switch
        {
            ChatModel.GPT4o => "gpt-4o",
            ChatModel.Claude3_5Sonnet => "claude-3.5-sonnet",
            _ => "gpt-4o" // default
        };
    }
}
