using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameNetwork : NetworkBehaviour
{
    enum GameState { Start, Lobby, Playing, Pause, EndRound }
    [SerializeField]
    GameManager GameManager;
    
    [SerializeField]
    GameDataScriptableObject GameData;

    readonly NetworkVariable<float> m_GameTimer = new(writePerm: NetworkVariableWritePermission.Server);

    void Awake()
    {
        m_GameTimer.Value = GameData.InitialTimer;
    }

    

    void Update()
    {
        DecrementGameTime();
        
        // DEBUG
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
}