using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Chicken Game/Game Data")]
public class GameDataScriptableObject : ScriptableObject, ISerializationCallbackReceiver
{
    public int InitialTimer = 120;
    public Dictionary<int, PlayerData> Players = new Dictionary<int, PlayerData>();

    [NonSerialized]
    public float RuntimeTimer;

    public void OnAfterDeserialize()
    {
        RuntimeTimer = InitialTimer;
    }

    public void OnBeforeSerialize() { }

    public void IncrementPlayerScore(int id, int amt)
    {
        Players[id].Score += amt;
    }
}
