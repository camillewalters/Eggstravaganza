using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerScoreNetwork : NetworkBehaviour
{
    [SerializeField]
    GameDataScriptableObject GameData;

    readonly NetworkVariable<PlayerScoreData> m_Score = new(writePerm: NetworkVariableWritePermission.Owner);

    // TODO: why can't I serialize an array easily omg sorry
    readonly NetworkVariable<PlayerRegisterData> m_Player0Registered =
        new(writePerm: NetworkVariableWritePermission.Owner);
    readonly NetworkVariable<PlayerRegisterData> m_Player1Registered =
        new(writePerm: NetworkVariableWritePermission.Owner);
    readonly NetworkVariable<PlayerRegisterData> m_Player2Registered =
        new(writePerm: NetworkVariableWritePermission.Owner);
    readonly NetworkVariable<PlayerRegisterData> m_Player3Registered =
        new(writePerm: NetworkVariableWritePermission.Owner);

    readonly Dictionary<int, NetworkVariable<PlayerRegisterData>> m_PlayerRegisteredMap = new();
    
    int m_LocalClientID = -1;
    GameObject[] m_Prefabs;
    public readonly NetworkVariable<int> m_Id = new();
    
    void Awake()
    {
        m_PlayerRegisteredMap.Add(0, m_Player0Registered);
        m_PlayerRegisteredMap.Add(1, m_Player1Registered);
        m_PlayerRegisteredMap.Add(2, m_Player2Registered);
        m_PlayerRegisteredMap.Add(3, m_Player3Registered);

        foreach (var player in m_PlayerRegisteredMap)
        {
            player.Value.OnValueChanged += OnPlayerRegistered;
        }
        
        m_Prefabs = Resources.LoadAll<GameObject>("Hats");
        m_Id.Value = -1;
        m_Score.OnValueChanged += OnScoreUpdate;
        m_Id.OnValueChanged += OnIdUpdate;
    }

    public override void OnDestroy() {
        m_Id.OnValueChanged -= OnIdUpdate;
    }

    private void OnIdUpdate(int prev, int next) {
        AssignHat(next);
    }

    void OnScoreUpdate(PlayerScoreData _, PlayerScoreData next)
    {
        if (!IsOwner)
        {
            GameData.UpdatePlayerScore(next);
        }
    }

    void OnPlayerRegistered(PlayerRegisterData _, PlayerRegisterData next)
    {
        if (!IsOwner && IsClient || IsServer && (int)next.ID != -1)
        {
            RegisterNewPlayer((int)next.ID);
        }
    }

    public override void OnNetworkSpawn()
    {
        // Manually decrement by 1 assuming the server is 0
        m_LocalClientID = (int)NetworkManager.LocalClientId - 1;
        Debug.Log($"My client ID is {m_LocalClientID}");
        if (IsClient)
        {
            RegisterNewPlayer(m_LocalClientID);
        }
        
        if (IsOwner)
        {
            Debug.Log("i am the owner!!!");
            // PlayersAndIds[m_LocalClientID] = this.transform.gameObject;
            
            CommitNetworkIdServerRpc(m_LocalClientID);
        }
        else
        {
            AssignHat(m_Id.Value);
        }
    }

    [ServerRpc]
    private void CommitNetworkIdServerRpc(int id) {
        m_Id.Value = id;
    }
    
    /// <summary>
    /// Assign NetworkVariable to register joining player, update in local GameManager
    /// </summary>
    /// <param name="id"></param>
    void RegisterNewPlayer(int id)
    {
        Debug.Log($"Registering player {id}");
        if (IsOwner)
        {
            var reg = new PlayerRegisterData()
            {
                ID = (ulong)m_LocalClientID,
                // Name = $"Player {localID}" // TODO: add this
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
            // Locally register all other already-registered players
            for (int i = 0; i < Utils.k_MaxPlayers; i++)
            {
                if (m_PlayerRegisteredMap[i] != null && (int)m_PlayerRegisteredMap[i].Value.ID == i)
                {
                    GameManager.Instance.RegisterNewPlayer(i);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // DEBUG add points
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            IncrementPlayerScore(1);
        }
    }
    
    void IncrementPlayerScore(int amt)
    {
        if (IsOwner)
        {
            m_Score.Value = new PlayerScoreData()
            {
                ID = (ulong)m_LocalClientID,
                Score = m_Score.Value.Score + amt
            };
        }
        GameData.UpdatePlayerScore(m_Score.Value);
    }

    void AssignHat(int index)
    {
        Debug.Log($"My client ID from assign hat is {index}");
        var hat = Instantiate(m_Prefabs[index]);
        hat.transform.parent = this.transform.gameObject.transform.GetChild(3);
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

    struct PlayerRegisterData : INetworkSerializable
    {
        internal ulong ID;
        // internal string Name;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ID);
            // serializer.SerializeValue(ref Name);
        }
    }
}
