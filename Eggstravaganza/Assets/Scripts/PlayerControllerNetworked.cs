using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerControllerNetworked : NetworkBehaviour
{
    public float speed = 80f;
    public float lerpParam = 10f;
    public Transform body;
    Rigidbody rb;
    Transform eggLocation;
    public BoxCollider eggHitbox;

    public PlayerInputActions playerControls;

    private InputAction move;
    private InputAction interact;
    private Vector2 lookValue;

    public List<GameObject> eggInventory;
    private float stunTime = 2f;
    bool isStunned = false;

    public int throwForwardFactor = 20;
    public int throwUpwardFactor = 5;
    public int dropForwardFactor = -3; //negative so it goes backwards

    private void Awake()
    {
        playerControls = new PlayerInputActions();
    }
    private void OnEnable()
    {
        playerControls.Player.Enable();

        move = playerControls.Player.Move;

        interact = playerControls.Player.Fire;
        interact.performed += Interact;

        playerControls.Player.Look.performed += LookPerformed;

        rb = GetComponent<Rigidbody>();
        eggLocation = body.Find("EggLocation");
    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            enabled = false;
    }
    private void OnDisable()
    {
        playerControls?.Player.Disable();
        interact.performed -= Interact;
        playerControls.Player.Look.performed -= LookPerformed;

    }

    private void Update()
    {
        if (!isStunned)
        {
            Move();
        }
    }

    public void HandleEggHitboxCollision(Collider other)
    {
        var eggBehavior = other.gameObject.GetComponent<EggBehaviorNetworked>();
        if (eggBehavior != null)
        {
            if (!eggBehavior.isBeingHeld && !eggBehavior.isBeingThrown && eggBehavior.droppedBy != this)
            {
                Debug.Log("pick up");
                PickUpEgg(other.gameObject);
                eggBehavior.isBeingHeld = true;
            }
            if (eggBehavior.isBeingThrown && eggBehavior.thrownBy != this)
            {
                Debug.Log("lose");
                StartCoroutine(LoseEgg());
            }
        }
    }

    private void LookPerformed(InputAction.CallbackContext context)
    {
        lookValue = context.ReadValue<Vector2>();

        if (context.control.device.ToString() == "Mouse:/Mouse")
        {
            Ray ray = Camera.main.ScreenPointToRay(lookValue);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayDistance;

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 point = ray.GetPoint(rayDistance);
                Vector3 lookDirection = (point - transform.position).normalized;
                lookValue = new Vector2(lookDirection.x, lookDirection.z);
            }

        }
    }

    private void Move()
    {
        Vector2 movementDirection = move.ReadValue<Vector2>();
        Vector3 movement = new Vector3(movementDirection.x, 0f, movementDirection.y);
        rb.AddForce(movement * speed);

        Vector3 rotation = new Vector3(lookValue.x, 0f, lookValue.y);
        rotation.Normalize();

        //Rotate the player
        if (rotation != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(rotation, Vector3.up);
            body.rotation = Quaternion.Lerp(body.rotation, toRotation, Time.deltaTime * lerpParam);
        }
    }

    private void Interact(InputAction.CallbackContext context)
    {
        //to *very roughly* test LoseEgg locally (just one player), comment line 122 and uncomment line 123
        ThrowEgg();
        //StartCoroutine(LoseEgg());
    }

    private void ThrowEgg()
    {
        if (eggInventory.Count > 0)
        {
            //choose egg to throw
            GameObject eggToThrow = eggInventory[eggInventory.Count - 1];
            var eggRb = eggToThrow.GetComponent<Rigidbody>();

            eggRb.isKinematic = false;
            eggToThrow.transform.parent = null;//unparent

            //throw from lowest point
            eggToThrow.transform.position = eggLocation.position;
            eggRb.velocity = rb.transform.forward * throwForwardFactor + rb.transform.up * throwUpwardFactor;
            eggInventory.Remove(eggToThrow);

            EggBehavior eggBehavior = eggToThrow.GetComponent<EggBehavior>();
            eggBehavior.isBeingThrown = true;
            eggBehavior.isBeingHeld = false;
            eggBehavior.thrownBy = this;

        }

    }
    
    GameObject eggToBePickedUpGlobal;
    
    private void PickUpEgg(GameObject eggToBePickedUp)
    {
        var eggRb = eggToBePickedUp.GetComponent<Rigidbody>();

        eggRb.isKinematic = true;
        eggInventory.Add(eggToBePickedUp);

        eggToBePickedUp.transform.position = NextEggHoldLocation();
        eggToBePickedUpGlobal = eggToBePickedUp;

        // eggToBePickedUp.transform.parent = gameObject.transform;
        if (IsOwner)
        {
            Debug.Log("entering server rpc section");
            // PickUpEggServerRpc();
            // GameNetwork.PickUpEggOnPlayer();
        }
    }

    [ServerRpc]
    void PickUpEggServerRpc()
    {
        Debug.Log($"picking up from server {eggToBePickedUpGlobal} {eggToBePickedUpGlobal.GetInstanceID()}");
        // eggToBePickedUpGlobal.transform.parent = gameObject.transform;
    }

    private Vector3 NextEggHoldLocation()
    {
        var basePosition = eggLocation.transform.position;
        var nextLocation = basePosition + new Vector3(0, 0.4f * (eggInventory.Count - 1), 0);//approx height of egg
        return nextLocation;
    }

    private IEnumerator LoseEgg()
    {
        //remove egg from inventory using LIFO
        if (eggInventory.Count > 0)
        {
            GameObject eggToRemove = eggInventory[eggInventory.Count - 1];
            eggToRemove.transform.parent = null;//unparent
            eggInventory.Remove(eggToRemove);

            var eggRb = eggToRemove.GetComponent<Rigidbody>();
            eggRb.velocity = body.transform.forward * dropForwardFactor;//falls backwards with a bit of velocity
            eggRb.isKinematic = false;

            EggBehavior eggBehavior = eggToRemove.GetComponent<EggBehavior>();
            eggBehavior.isBeingHeld = false;
            eggBehavior.droppedBy = this;
        }

        //gets stunned
        isStunned = true;
        yield return new WaitForSeconds(stunTime);
        isStunned = false;
    }
}
