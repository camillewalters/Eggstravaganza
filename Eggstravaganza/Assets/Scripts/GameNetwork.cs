using System;
using Unity.Netcode;
using UnityEngine;

public class GameNetwork : NetworkBehaviour
{
    enum GameState { Start, Lobby, Playing, Pause, EndRound }
    
    [SerializeField]
    GameManager GameManager;
    
    [SerializeField]
    GameDataScriptableObject GameData;

    readonly NetworkVariable<bool> m_EnoughPlayers = new(writePerm: NetworkVariableWritePermission.Server);
    readonly NetworkVariable<float> m_LobbyTimer = new(writePerm: NetworkVariableWritePermission.Server);
    readonly NetworkVariable<float> m_GameTimer = new(writePerm: NetworkVariableWritePermission.Server);
    readonly NetworkVariable<GameState> m_GameState = new(writePerm: NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        m_GameTimer.Value = GameData.InitialGameTimer;
        m_LobbyTimer.Value = GameData.InitialLobbyTimer;
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
        // TODO: move this out to classes and not enum check
        switch (m_GameState.Value)
        {
            case GameState.Start:
            case GameState.Lobby:
                if (m_EnoughPlayers.Value)
                {
                    DecrementLobbyTime();
                }
                break;
            case GameState.Playing:
                DecrementGameTime();
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

    void DecrementLobbyTime()
    {
        if (IsOwner)
        {
            if (m_LobbyTimer.Value > 0)
            {
                m_LobbyTimer.Value -= Time.deltaTime;
                GameData.LobbyTimer = m_LobbyTimer.Value;
            }
            else 
            {
                StartGame();
            }
        }
        GameData.LobbyTimer = m_LobbyTimer.Value;
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
    
    /// <summary>
    /// Base call to start the game!
    /// </summary>
    public void StartGame()
    {
        if (IsOwner)
        {
            m_GameState.Value = GameState.Playing;
        }
    }

    public void HaveEnoughPlayers()
    {
        if (IsOwner)
        {
            m_EnoughPlayers.Value = true;
        }
    }
}