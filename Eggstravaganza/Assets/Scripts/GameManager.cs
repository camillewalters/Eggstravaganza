using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
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
            enabled = false; // Destroy() causes NetworkObject issues
        }
        else
        {
            m_Instance = this;
        }
        
        // DEBUG
        GameData.Players.Add(0, new PlayerData(0, "Player 0"));
        GameData.Players.Add(1, new PlayerData(1, "Player 1"));
    }

    public void RegisterNewPlayer(int id)
    {
        GameData.Players.TryAdd(id, new PlayerData(id, $"Player {id}"));
        UIManager.RegisterNewPlayer(id);
    }

    public void EndRound()
    {
        UIManager.EndGame();
    }
}
