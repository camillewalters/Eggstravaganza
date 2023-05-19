using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxController : MonoBehaviour
{
    PlayerControllerNetworked playerController;
    // Start is called before the first frame update
    void Start()
    {
        playerController  = GetComponentInParent<PlayerControllerNetworked>();
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("in trigger enter of hit box ");
        playerController.HandleEggHitboxCollision(other);
    }
}
