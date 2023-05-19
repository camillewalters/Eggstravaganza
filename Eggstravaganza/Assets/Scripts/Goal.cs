using System;
using Unity.Netcode;
using UnityEngine;
public class Goal : NetworkBehaviour
{
    ParticleSystem m_ParticleSystem;
    
    // [SerializeField]
    // GameDataScriptableObject GameData;
    
    // TODO: Ensure the player matches their goals, hardcoded for goals currently
    [SerializeField]
    int goalId;

    PlayerScoreNetwork m_Scorer;

    // readonly NetworkVariable<PlayerScoreData> m_Score = new(writePerm: NetworkVariableWritePermission.Owner);

    void Awake()
    {
        m_ParticleSystem = GetComponentInChildren<ParticleSystem>();
        // m_Score.OnValueChanged += OnScoreUpdate;
    }
    
    // void OnScoreUpdate(PlayerScoreData _, PlayerScoreData next)
    // {
    //     if (!IsOwner)
    //     {
    //         GameData.UpdatePlayerScore(next);
    //     }
    // }

    public void ActivateGoal(PlayerScoreNetwork player)
    {
        Debug.Log($"Owner {OwnerClientId} or is owned by server {IsOwnedByServer} or is server {IsServer} or is client {IsClient}");
        
        // gameObject.SetActive(true);
        m_Scorer = player;
        
        // SetGoalServerRpc((ulong)player.LocalClientID);
    }
    
    // [ServerRpc]
    // private void SetGoalServerRpc(ulong id) {
    //     GetComponent<NetworkObject>().ChangeOwnership(id);
    // }

    void OnTriggerEnter(Collider other)
    {
        var egg = other.GetComponent<EggBehavior>();
        if (egg != null)
        {
            Debug.Log(egg);
            PlayerController whoThrew = null;
            if (egg.thrownBy != null)
            {
                whoThrew = egg.thrownBy;
            }
            else if (egg.droppedBy != null)
            {
                whoThrew = egg.droppedBy;
            }

            if (whoThrew != null)
            {
                whoThrew.eggInventory.Remove(egg.gameObject);
            }

            if (egg.value > 0)
            {
                Confetti();
            }
            
            // IncrementPlayerScore(egg.value);
            m_Scorer.IncrementPlayerScore(egg.value);
            
            Destroy(egg);
        }
        else
        {
            Debug.Log("no egg");
        }
    }

    // void IncrementPlayerScore(int amt)
    // {
    //     Debug.Log($"Calling IncrementPlayerScore with {amt} points, IsOwner = {IsOwner}, IsServer = {IsServer}, IsClient = {IsClient}");
    //     if (IsOwner) // Does this need to explicitly be client?
    //     {
    //         m_Score.Value = new PlayerScoreData()
    //         {
    //             ID = goalId,
    //             Score = m_Score.Value.Score + amt
    //         };
    //     }
    //     GameData.UpdatePlayerScore(m_Score.Value);
    // }

    // void OnTriggerEnter(Collider other)
    // {
    //     // If not a player triggers, ignore
    //     var playerController = other.gameObject.GetComponent<PlayerController>();
    //     if (!playerController) return;
    //     // If not correct player at the goal, ignore
    //     var playerScoreNetwork = other.gameObject.GetComponent<PlayerScoreNetwork>();
    //     Debug.Log($"Player ID = {playerScoreNetwork.LocalClientID}");
    //     if (playerScoreNetwork.LocalClientID != goalId) return;
    //     
    //     var eggs = playerController.eggInventory;
    //     Debug.Log($"Player has {eggs.Count} eggs");
    //     var score = 0;
    //     // Loop through every items player is holding
    //     foreach (var egg in eggs)
    //     {
    //         if (egg.TryGetComponent<EggBehavior>(out var comp))
    //         {
    //             // TODO: Store egg value somewhere proper in GO
    //             score += comp.value;
    //             Debug.Log($"Adding {comp.value} points");
    //         }
    //         // Destroy them at the goal
    //         Destroy(egg);
    //     }
    //     // Clear out inventory
    //     playerController.eggInventory.Clear();
    //     // Add score
    //     playerScoreNetwork.IncrementPlayerScore(score);
    // }
    
    // public struct PlayerScoreData : INetworkSerializable
    // {
    //     internal ulong ID;
    //     internal float Score;
    //     
    //     public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    //     {
    //         serializer.SerializeValue(ref ID);
    //         serializer.SerializeValue(ref Score);
    //     }
    // }
    
    public void Confetti()
    {
        m_ParticleSystem.Stop();
        m_ParticleSystem.Play();
    }
}
