using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public TMP_InputField ipInputField;


    public void OnHostClicked()
    {
        NetworkManager.Instance.StartHost();

        SceneManager.LoadScene("GameScene");
    }

    public void OnJoinClicked()
    {
        string ip = ipInputField.text.Trim();
        if (string.IsNullOrEmpty(ip)) ip = "127.0.0.1";
        NetworkManager.Instance.StartClient(ip);

        SceneManager.LoadScene("GameScene");
    }

    //// Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{
        
    //}

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
