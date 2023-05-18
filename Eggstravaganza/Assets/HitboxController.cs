using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxController : MonoBehaviour
{
    PlayerController playerController;
    // Start is called before the first frame update
    void Start()
    {
        playerController  = transform.parent.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        playerController.HandleEggHitboxCollision(other);
    }
}
