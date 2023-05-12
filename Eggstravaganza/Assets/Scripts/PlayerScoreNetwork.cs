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

    public override void OnNetworkSpawn()
    {
        var localID = (int)NetworkManager.LocalClientId;
        Debug.Log($"My client ID is {localID}");
        // TODO: move this elsewhere
        // GameManager.Instance.RegisterNewPlayer(localID);
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
