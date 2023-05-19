using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public Button startGameButton;
    
    // Start is called before the first frame update
    void Start()
    {
        startGameButton.onClick.AddListener(StartNewGame);
        startGameButton.interactable = false;
    }

    void Update()
    {
        startGameButton.interactable = nameInputField.text != "";
    }

    static void StartNewGame()
    {
        Debug.Log("Starting Game");
        // TODO: Change this 
        SceneManager.LoadScene("MainScene"); 
    }
}
