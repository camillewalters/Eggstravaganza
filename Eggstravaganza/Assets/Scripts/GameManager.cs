using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    static GameManager m_Instance;
    public static GameManager Instance => m_Instance;

    public GameNetwork GameNetwork => GetComponent<GameNetwork>();

    [SerializeField]
    GameDataScriptableObject GameData;
    
    [SerializeField] 
    UIManager UIManager;

    public Goal[] Goals;

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
    }

    /// <summary>
    /// Add new player to local GameData, enable UI
    /// </summary>
    /// <param name="id">Local client ID of joining player</param>
    public void RegisterNewPlayer(int id)
    {
        if (GameData.Players.TryAdd(id, new PlayerData(id, $"Player {id}")))
        {
            UIManager.RegisterNewPlayer(id);
            if (GameData.Players.Count >= 2)
            {
                GameNetwork.HaveEnoughPlayers();   
                UIManager.StartLobbyCountdown();
            }
        }
    }
    
    public void StartGame()
    {
        UIManager.StartGame();
    }

    public void EndRound()
    {
        UIManager.EndGame();
    }
}
