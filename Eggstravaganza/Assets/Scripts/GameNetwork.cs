using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameNetwork : NetworkBehaviour
{
    [SerializeField]
    GameManager GameManager;
    
    [SerializeField]
    GameDataScriptableObject GameData;

    readonly NetworkVariable<float> m_GameTimer = new(writePerm: NetworkVariableWritePermission.Owner);
    // TODO: turn into INetworkVariable....
    readonly NetworkVariable<Dictionary<int, PlayerData>> m_Players = new();

    void Awake()
    {
        m_GameTimer.Value = GameData.InitialTimer;
    }

    void Update()
    {
        DecrementGameTime();
        // DEBUG
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            IncrementPlayerScore(0, 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            IncrementPlayerScore(1, 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            IncrementPlayerScore(2, 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            IncrementPlayerScore(3, 1);
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.EndRound();
        }
    }
    
    void DecrementGameTime()
    {
        if (IsOwner)
        {
            if (m_GameTimer.Value > 0)
            {
                m_GameTimer.Value -= Time.deltaTime;
                GameData.RuntimeTimer = m_GameTimer.Value;
            }
            else 
            {
                GameManager.EndRound();    
            }
        }
        GameData.RuntimeTimer = m_GameTimer.Value;
    }

    void IncrementPlayerScore(int id, int amt)
    {
        if (IsOwner)
        {
            m_Players.Value[id].Score += amt;
        }
        GameData.UpdatePlayerScores(id, m_Players.Value[id].Score);
    }
}