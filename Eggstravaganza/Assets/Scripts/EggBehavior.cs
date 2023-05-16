using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EggBehavior: MonoBehaviour
{
	public bool isBeingHeld = false;
	public bool isBeingThrown = false;

	private Rigidbody rb;
	private Rigidbody Rb => rb;

    private SphereCollider coll;
    public Transform player;

    public float pickUpRange;

    private void Awake()
    {
       rb = GetComponent<Rigidbody>(); 
    }

    //private void Update()
    //{
    //    Vector3 distanceToPlayer = player.position - transform.position;
    //    if (distanceToPlayer.magnitude <= pickUpRange)
    //}
}
