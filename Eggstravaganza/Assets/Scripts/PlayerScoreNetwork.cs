using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerScoreNetwork : NetworkBehaviour
{
    [SerializeField]
    GameDataScriptableObject GameData;

    readonly NetworkVariable<PlayerScoreData> m_Score = new(writePerm: NetworkVariableWritePermission.Owner);
    readonly NetworkVariable<PlayerData.PlayerRegisterData[]> m_RegisteredPlayers =
        new(writePerm: NetworkVariableWritePermission.Owner) { Value = new PlayerData.PlayerRegisterData[4] };

    public override void OnNetworkSpawn()
    {
        var localID = (int)NetworkManager.LocalClientId;
        Debug.Log($"My client ID is {localID}");
        m_RegisteredPlayers.Value[localID] = new PlayerData.PlayerRegisterData()
        {
            ID = NetworkManager.LocalClientId,
            Name = $"Player {localID}" // TODO: fix this
        };
        GameManager.Instance.RegisterNewPlayer(localID);
    }

    // Update is called once per frame
    void Update()
    {
        // DEBUG
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            IncrementPlayerScore(1);
        }

        // TODO: take this out of update loop
        if (!IsOwner)
        {
            GameData.UpdatePlayerScore(m_Score.Value);
        }
    }
    
    void IncrementPlayerScore(int amt)
    {
        if (IsOwner)
        {
            m_Score.Value = new PlayerScoreData()
            {
                ID = NetworkManager.LocalClientId,
                Score = m_Score.Value.Score + amt
            };
            GameData.UpdatePlayerScore(m_Score.Value);
        }
        else
        {
            GameData.UpdatePlayerScore(m_Score.Value);
        }
    }

    public struct PlayerScoreData : INetworkSerializable
    {
        internal ulong ID;
        internal float Score;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ID);
            serializer.SerializeValue(ref Score);
        }
    }
}
