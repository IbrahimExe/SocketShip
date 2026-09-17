using UnityEngine;

public class NetworkTestUI : MonoBehaviour
{
    void OnEnable()
    {
        NetworkManager.Instance.OnMessageReceived += HandleMessage;
        NetworkManager.Instance.OnClientConnected += () => Debug.Log("A client connected!");
        NetworkManager.Instance.OnConnectedToHost += () => Debug.Log("Connected to host!");
    }

    void HandleMessage(string msg)
    {
        Debug.Log("Received: " + msg);
    }

    public void SendTestMessage()
    {
        NetworkManager.Instance.SendMessageToPeer("hello from " + (NetworkManager.Instance.IsHost ? "host" : "client"));
    }
}
