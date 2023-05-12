using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    // TODO: add game states
    static GameManager m_Instance;
    public static GameManager Instance => m_Instance;

    [SerializeField]
    GameDataScriptableObject GameData;
    
    [SerializeField] 
    UIManager UIManager;

    void Awake()
    {
        if (m_Instance != null && m_Instance != this)
        {
            Debug.LogError("Multiple GameManagers in scene!");
            enabled = false;
        }
        else
        {
            m_Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // DEBUG
        GameData.Players.Add(0, new PlayerData(0, "Player 0"));
        GameData.Players.Add(1, new PlayerData(1, "Player 1"));
        // GameData.Players.Add(2, new PlayerData(2, "Player 2"));
        // GameData.Players.Add(3, new PlayerData(3, "Player 3"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EndRound()
    {
        UIManager.EndGame();
    }
}
