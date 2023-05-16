using System;
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

    readonly NetworkVariable<float> m_GameTimer = new(writePerm: NetworkVariableWritePermission.Server);
    readonly NetworkVariable<GameState> m_GameState = new(writePerm: NetworkVariableWritePermission.Server);

    EggSpawner m_EggSpawner;
    NetworkVariable<int> m_EggToSpawnIndex = new(writePerm: NetworkVariableWritePermission.Server);
    NetworkVariable<Vector3> m_EggSpawnPosition = new(writePerm: NetworkVariableWritePermission.Server);

    [SerializeField]
    float maxEggSpawnTime = 8;
    float m_TimeRemaining;
    bool m_TimerIsRunning;

    void Awake()
    {
        m_GameTimer.Value = GameData.InitialTimer;
        
        m_EggSpawner = this.gameObject.GetComponent<EggSpawner>();
        m_TimeRemaining = Random.Range(0, maxEggSpawnTime);
        m_TimerIsRunning = true;
        
        // Choose first egg to spawn and its position
        m_EggToSpawnIndex.Value = m_EggSpawner.ChooseEggToSpawn();
        m_EggSpawnPosition.Value = m_EggSpawner.ChooseSpawnPosition();
    }

    public override void OnNetworkSpawn()
    {
        // TODO: fix initial state
        m_GameState.Value = GameState.Playing;
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
        DecrementGameTime();
        
        // DEBUG
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndRound();
        }
        SpawnCountdown();
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

    void SpawnCountdown()
    {
        if (!IsOwner)
            return;
        
        // Spawns an egg at a random time from 1 to maxEggSpawnTime seconds
        if (m_TimerIsRunning)
        {
            if (m_TimeRemaining > 0)
            {
                m_TimeRemaining -= Time.deltaTime;
            }
            else
            {
                // Update timer
                m_TimeRemaining = 0;
                m_TimerIsRunning = false;

                // Spawn chosen egg
                RequestSpawnEggServerRpc();
                m_EggSpawner.SpawnEgg(m_EggSpawnPosition.Value, m_EggToSpawnIndex.Value);
        
                // Choose next egg to spawn and its position
                m_EggToSpawnIndex.Value = m_EggSpawner.ChooseEggToSpawn();
                m_EggSpawnPosition.Value = m_EggSpawner.ChooseSpawnPosition();
            }
        }
        else
        {
            m_TimeRemaining = Random.Range(0, maxEggSpawnTime);
            m_TimerIsRunning = true;
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
        if (!IsOwner)
        {
            m_EggSpawner.SpawnEgg(m_EggSpawnPosition.Value, m_EggToSpawnIndex.Value);
        }
    }
}