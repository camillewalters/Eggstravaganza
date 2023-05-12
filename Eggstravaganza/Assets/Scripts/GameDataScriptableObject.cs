using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Chicken Game/Game Data")]
public class GameDataScriptableObject : ScriptableObject, ISerializationCallbackReceiver
{
    public int InitialTimer = 120;
    public readonly Dictionary<int, PlayerData> Players = new Dictionary<int, PlayerData>();
    public int NumPlayers => Players.Count;

    [NonSerialized]
    public float RuntimeTimer;

    public void OnAfterDeserialize()
    {
        
    }

    public void OnBeforeSerialize() { }

    public void UpdatePlayerScore(PlayerScoreNetwork.PlayerScoreData scoreData)
    {
        var id = (int)scoreData.ID;
        Players[id].Score = (int)scoreData.Score;
        Debug.Log($"updating player score, id is {id}, score is now {Players[id].Score}");
    }
    
    public void UpdatePlayerScore(int id, int amt)
    {
        Players[id].Score = amt;
    }

    public int GetPlayerScore(int id) => Players[id].Score;
}
