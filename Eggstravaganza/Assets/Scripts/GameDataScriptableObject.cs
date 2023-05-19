using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Chicken Game/Game Data")]
public class GameDataScriptableObject : ScriptableObject, ISerializationCallbackReceiver
{
    [FormerlySerializedAs("InitialTimer")]
    public int InitialGameTimer = 120;
    public int InitialLobbyTimer = 30;
    public readonly Dictionary<int, PlayerData> Players = new Dictionary<int, PlayerData>();
    public int NumPlayers => Players.Count;

    [NonSerialized]
    public float RuntimeTimer;
    
    [NonSerialized]
    public float LobbyTimer;

    public void OnAfterDeserialize()
    {
        LobbyTimer = InitialLobbyTimer;
        RuntimeTimer = InitialGameTimer;
    }

    public void OnBeforeSerialize() { }

    public void UpdatePlayerScore(PlayerScoreNetwork.PlayerScoreData scoreData)
    // public void UpdatePlayerScore(Goal.PlayerScoreData scoreData)
    {
        Debug.Log($"Update player {scoreData.ID} score to {scoreData.Score}");
        var id = (int)scoreData.ID;
        Players[id].Score = (int)scoreData.Score;
    }
}
