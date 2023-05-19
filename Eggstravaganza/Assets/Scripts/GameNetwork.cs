using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

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

    EggSpawner m_EggSpawner;
    NetworkVariable<int> m_EggToSpawnIndex = new(writePerm: NetworkVariableWritePermission.Server);
    NetworkVariable<Vector3> m_EggSpawnPosition = new(writePerm: NetworkVariableWritePermission.Server);

    [SerializeField]
    float maxEggSpawnTime = 8;
    float m_TimeRemaining;

    int m_EggCounter = 0;
    List<EggBehaviorNetworked> m_Eggs = new List<EggBehaviorNetworked>();

    void Start()
    {
        m_EggSpawner = this.gameObject.GetComponent<EggSpawner>();
        m_TimeRemaining = Random.Range(0, maxEggSpawnTime);
    }

    public override void OnNetworkSpawn()
    {
        // Choose first egg to spawn and its position
        // Seemingly broken right now?
        // m_EggToSpawnIndex.Value = m_EggSpawner.ChooseEggToSpawn();
        // m_EggSpawnPosition.Value = m_EggSpawner.ChooseSpawnPosition();

        // Need to manage game state on each client
        m_GameState.OnValueChanged += OnStateChange;

        // However, network variables should only be changed on server
        if (!IsServer) return;

        m_GameTimer.Value = GameData.InitialGameTimer;
        m_LobbyTimer.Value = GameData.InitialLobbyTimer;
        // TODO: fix initial state
        m_GameState.Value = GameState.Lobby;
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
                SpawnCountdown();
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
        if (IsServer)
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
        if (IsServer)
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
        if (IsServer)
        {
            m_GameState.Value = GameState.EndRound;
        }
    }
    
    /// <summary>
    /// Base call to start the game!
    /// </summary>
    public void StartGame()
    {
        if (IsServer)
        {
            m_GameState.Value = GameState.Playing;
        }
    }

    public void HaveEnoughPlayers()
    {
        if (IsServer)
        {
            m_EnoughPlayers.Value = true;
        }
    }

    void SpawnCountdown()
    {
        if (!IsServer)
            return;

        // Spawns an egg at a random time from 1 to maxEggSpawnTime seconds
        if (m_TimeRemaining > 0)
        {
            m_TimeRemaining -= Time.deltaTime;
        }
        else
        {
            // Update timer
            m_TimeRemaining = Random.Range(0, maxEggSpawnTime);

            // Spawn chosen egg
            RequestSpawnEggServerRpc();
            SpawnEggClientRpc();
            var spawnedEggBehaviour = m_EggSpawner.SpawnEgg(m_EggSpawnPosition.Value, m_EggToSpawnIndex.Value, m_EggCounter);
            m_Eggs.Add(spawnedEggBehaviour);
    
            // Choose next egg to spawn and its position
            m_EggToSpawnIndex.Value = m_EggSpawner.ChooseEggToSpawn();
            m_EggSpawnPosition.Value = m_EggSpawner.ChooseSpawnPosition();
            m_EggCounter += 1;
        }
    }

    [ServerRpc]
    void RequestSpawnEggServerRpc()
    {
        SpawnEggClientRpc();
    }

    [ClientRpc]
    void SpawnEggClientRpc()
    {
        if (!IsServer)
        {
            var spawnedEggBehaviour = m_EggSpawner.SpawnEgg(m_EggSpawnPosition.Value, m_EggToSpawnIndex.Value, m_EggCounter);
            m_Eggs.Add(spawnedEggBehaviour);
            m_EggCounter += 1;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PickUpEggOnPlayerServerRpc(int playerId, int eggId)
    {
        Debug.Log($"Called by {playerId} for {eggId} which is {m_Eggs[eggId]}");
        var player = NetworkManager.Singleton.ConnectedClients[(ulong) playerId].PlayerObject;
        Debug.Log($"helo maybe we found the player {player.name}");
        m_Eggs[eggId].transform.parent = player.transform;
    }
}