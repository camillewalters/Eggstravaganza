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
        Debug.Log($"My client ID is {NetworkManager.LocalClientId}");
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // DEBUG
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            IncrementPlayerScore(1);
        }
    }
    
    void IncrementPlayerScore(int amt)
    {
        var localID = (int)NetworkManager.LocalClientId;
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
            for (int i = 0; i < GameData.NumPlayers; i++)
            {
                if (GameData.Players[i].ID != localID)
                {
                    GameData.UpdatePlayerScore(i, GameData.GetPlayerScore(localID) + amt);
                }
            }
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
