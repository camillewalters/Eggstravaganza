using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    Button startGameButton;
    
    // Start is called before the first frame update
    void Start()
    {
        startGameButton = this.gameObject.GetComponent<Button>();
        startGameButton.onClick.AddListener(StartNewGame);
    }

    void StartNewGame()
    {
        Debug.Log("Starting Game");
        SceneManager.LoadScene("PriyankaSceneMerged"); // TODO: Change this
    }
}
