using System;
using Unity.Netcode;
using UnityEngine;
public class Goal : MonoBehaviour
{
    // TODO: Ensure the player matches their goals, hardcoded currently
    [SerializeField]
    int goalId;

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"OnCollisionEnter {collision.gameObject.name}");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter");
        // If not a player triggers, ignore
        var playerController = other.gameObject.GetComponent<PlayerController>();
        if (!playerController) return;
        // If not correct player at the goal, ignore
        var playerScoreNetwork = other.gameObject.GetComponent<PlayerScoreNetwork>();
        Debug.Log($"Player ID = {playerScoreNetwork.LocalClientID}");
        if (playerScoreNetwork.LocalClientID != goalId) return;
        
        Debug.Log($"OnTriggerEnter {other.name}");
        var eggs = playerController.eggInventory;
        Debug.Log($"Player has {eggs.Count} eggs");
        var score = 0;
        foreach (var egg in eggs)
        {
            // TODO: check the egg value
            // score += egg<Egg>.value;
            score += 1;
            Destroy(egg);
        }
        // TODO: update score after calculating it
        playerScoreNetwork.IncrementPlayerScore(score);
    }
}
