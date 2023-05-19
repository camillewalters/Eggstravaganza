using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerScoreNetwork : NetworkBehaviour
{
    [SerializeField]
    GameObject[] GoalPrefabs;
    
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
    
    public int LocalClientID = -1;
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
        m_Id.OnValueChanged += OnIdUpdate;
    }

    public override void OnDestroy() {
        m_Id.OnValueChanged -= OnIdUpdate;
    }

    private void OnIdUpdate(int prev, int next)
    {
        AssignLocalPlayer(next);
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
        LocalClientID = (int)NetworkManager.LocalClientId - 1;
        Debug.Log($"My client ID is {LocalClientID}");
        if (IsClient)
        {
            RegisterNewPlayer(LocalClientID);
        }
        
        if (IsOwner)
        {
            CommitNetworkIdServerRpc(LocalClientID);
        }
        else
        {
            AssignLocalPlayer(m_Id.Value);
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
                ID = (ulong)LocalClientID,
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

    public void IncrementPlayerScore(int amt)
    {
        if (IsOwner)
        {
            m_Score.Value = new PlayerScoreData()
            {
                ID = (ulong)LocalClientID,
                Score = m_Score.Value.Score + amt
            };
        }
        GameData.UpdatePlayerScore(m_Score.Value);
    }
    
    void AssignLocalPlayer(int index)
    {
        if (index < 0)
        {
            return;
        }

        Debug.Log($"Calling AssignLocalPlayer with index {index}");
        Instantiate(m_Prefabs[index], transform.GetChild(3), true);
        if (IsOwner)
        {
            SpawnGoalServerRpc(LocalClientID);
        }
        else
        {
            var obj = NetworkManager.ConnectedClients[(ulong)index].PlayerObject;
            Debug.Log($"??? was not owner in assign local player. need to spawn (or assign) goal? for {obj}");
        //     var goal = GameObject.Find($"Goal {LocalClientID}");
        //     goal.GetComponent<Goal>().ActivateGoal(this);
        }
    }
    
    [ServerRpc]
    private void SpawnGoalServerRpc(int index)
    {
        Debug.Log($"calling spawn goal server rpc with index {index}");
        var goal = Instantiate(GoalPrefabs[index]);
        goal.name = $"Goal {index}";
        goal.GetComponent<NetworkObject>().Spawn();
        goal.GetComponent<Goal>().ActivateGoal(this);
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
