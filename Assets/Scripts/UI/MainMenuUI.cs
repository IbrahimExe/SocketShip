using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public TMP_InputField ipInputField;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkManager.Instance.OnConnectedToHost += OnJoinSuccess;

        NetworkManager.Instance.OnConnectionFailed += OnJoinFailed;
    }

    public void OnHostClicked()
    {
        NetworkManager.Instance.StartHost();

        SceneManager.LoadScene("GameScene");
    }

    public void OnJoinClicked()
    {
        string ip = ipInputField.text.Trim();

        if (string.IsNullOrEmpty(ip))
        {
            ip = "127.0.0.1"; // local mp so i can test
        }

        NetworkManager.Instance.StartClient(ip);
    }

    void OnJoinSuccess()
    {
        SceneManager.LoadScene("GameScene");
    }

    void OnJoinFailed(string error)
    {
        Debug.LogWarning("Join Failed: " + error);
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
