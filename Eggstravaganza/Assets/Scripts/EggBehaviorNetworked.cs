using System;
using UnityEngine;
using Unity.Netcode;
using UnityEditor.Rendering;
using Object = UnityEngine.Object;

public class EggBehaviorNetworked: NetworkBehaviour
{
	public bool isBeingHeld = false;
	public bool isBeingThrown = false;

    Rigidbody rb;

    public Object thrownBy;
    //public PlayerControllerNetworked thrownByNet;
    public Object droppedBy;

    private void Awake()
    {
         rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        // if (!IsOwner) Destroy(this);
    }

    private void Update()
    {
        rb.detectCollisions = !isBeingHeld;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.GetComponent<PlayerController>() == null)
        {
            isBeingThrown = false;
            isBeingHeld = false;
            droppedBy = null;
        }
    }
}
