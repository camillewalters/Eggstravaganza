using System;
using UnityEngine;
public class Goal : MonoBehaviour
{
    // TODO: Ensure the player matches their goals, hardcoded for goals currently
    [SerializeField]
    int goalId;

    PlayerScoreNetwork m_Scorer;

    public void ActivateGoal(PlayerScoreNetwork player)
    {
        gameObject.SetActive(true);
        m_Scorer = player;
        
        // TODO: take goal away completely if no player
    }

    void OnTriggerEnter(Collider other)
    {
        var egg = other.GetComponent<EggBehavior>();
        if (egg != null)
        {
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
            
            m_Scorer.IncrementPlayerScore(egg.value);
            
            Destroy(egg);
        }
    }

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
}
