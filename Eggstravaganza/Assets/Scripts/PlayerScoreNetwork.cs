using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerScoreNetwork : NetworkBehaviour
{
    [SerializeField]
    GameDataScriptableObject GameData;

    readonly NetworkVariable<PlayerScoreData> m_Score = new(writePerm: NetworkVariableWritePermission.Owner);

    // TODO: serialize an array???
    readonly NetworkVariable<PlayerData.PlayerRegisterData> m_Player0Registered =
        new(writePerm: NetworkVariableWritePermission.Owner);
    readonly NetworkVariable<PlayerData.PlayerRegisterData> m_Player1Registered =
        new(writePerm: NetworkVariableWritePermission.Owner);
    readonly NetworkVariable<PlayerData.PlayerRegisterData> m_Player2Registered =
        new(writePerm: NetworkVariableWritePermission.Owner);
    readonly NetworkVariable<PlayerData.PlayerRegisterData> m_Player3Registered =
        new(writePerm: NetworkVariableWritePermission.Owner);

    readonly Dictionary<int, NetworkVariable<PlayerData.PlayerRegisterData>> m_PlayerRegisteredMap = new();

    void Awake()
    {
        m_PlayerRegisteredMap.Add(0, m_Player0Registered);
        m_PlayerRegisteredMap.Add(1, m_Player1Registered);
        m_PlayerRegisteredMap.Add(2, m_Player2Registered);
        m_PlayerRegisteredMap.Add(3, m_Player3Registered);
        
        m_Player0Registered.OnValueChanged += OnPlayerRegistered;
        m_Player1Registered.OnValueChanged += OnPlayerRegistered;
        m_Player2Registered.OnValueChanged += OnPlayerRegistered;
        m_Player3Registered.OnValueChanged += OnPlayerRegistered;
    }

    void OnPlayerRegistered(PlayerData.PlayerRegisterData previousvalue, PlayerData.PlayerRegisterData newvalue)
    {
        Debug.Log($"OnPlayerRegistered, previousvalue: {previousvalue}, newvalue: {newvalue.ID}");
        RegisterNewPlayer((int)newvalue.ID);
    }

    public override void OnNetworkSpawn()
    {
        var localID = (int)NetworkManager.LocalClientId;
        Debug.Log($"My client ID is {localID}");
        RegisterNewPlayer(localID);
    }

    void RegisterNewPlayer(int id)
    {
        // TODO: register later joiners if joined earlier
        if (IsOwner)
        {
            var reg = new PlayerData.PlayerRegisterData()
            {
                ID = NetworkManager.LocalClientId,
                // Name = $"Player {localID}" // TODO: fix this
            };
            switch (id)
            {
                case 0:
                    m_Player0Registered.Value = reg;
                    break;
                case 1:
                    m_Player1Registered.Value = reg;
                    break;
                case 2:
                    m_Player2Registered.Value = reg;
                    break;
                case 3:
                    m_Player3Registered.Value = reg;
                    break;
            }
            GameManager.Instance.RegisterNewPlayer(id);
        }
        else
        {
            // TODO: move this out
            for (int i = 0; i < Utils.k_MaxPlayers; i++)
            {
                if (m_PlayerRegisteredMap[i] != null)
                {
                    GameManager.Instance.RegisterNewPlayer((int)m_PlayerRegisteredMap[i].Value.ID);    
                }
            }
        }
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
        }
        GameData.UpdatePlayerScore(m_Score.Value);
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
