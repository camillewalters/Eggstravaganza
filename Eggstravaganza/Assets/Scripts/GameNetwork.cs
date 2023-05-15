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
    readonly NetworkVariable<GameState> m_GameState = new(writePerm: NetworkVariableWritePermission.Server);

    void Awake()
    {
        m_GameTimer.Value = GameData.InitialTimer;
    }

    public override void OnNetworkSpawn()
    {
        // TODO: fix initial state
        m_GameState.Value = GameState.Lobby;
        m_GameState.OnValueChanged += OnStateChange;
    }
    
    void OnStateChange(GameState prev, GameState next)
    {
        Debug.Log($"Game state changed from {prev} to {next}");
        switch (next)
        {
            case GameState.Start:
                break;
            case GameState.Lobby:
                break;
            case GameState.Playing:
                GameManager.StartGame();
                break;
            case GameState.Pause:
                break;
            case GameState.EndRound:
                GameManager.EndRound();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(next), next, null);
        }
    }

    void Update()
    {
        // TODO: move this out to classes and not enum check...
        switch (m_GameState.Value)
        {
            case GameState.Start:
                break;
            case GameState.Lobby:
                // TODO: check for new registered players..?
                
                break;
            case GameState.Playing:
                DecrementGameTime();
        
                // DEBUG
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    EndRound();
                }
                break;
            case GameState.Pause:
                break;
            case GameState.EndRound:
                GameManager.EndRound();
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
                EndRound();
            }
        }
        GameData.RuntimeTimer = m_GameTimer.Value;
    }

    void EndRound()
    {
        if (IsOwner)
        {
            m_GameState.Value = GameState.EndRound;
        }
    }
    
    public void StartGame()
    {
        if (IsOwner)
        {
            m_GameState.Value = GameState.Playing;
        }
    }
}