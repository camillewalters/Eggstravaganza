using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float lerpParam = 10f;
    public Transform body;

    public PlayerInputActions playerControls;

    private InputAction move;
    private InputAction interact;
    private Vector2 lookValue;

    public List<GameObject> eggInventory;
    private float stunTime = 1.5f;
    bool isStunned = false;

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
        //look = playerControls.Player.Look;
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

    private void OnTriggerEnter(Collider other)
    {
        //Egg egg = other.GetComponent<Egg>();

        Debug.Log(other.gameObject);
        //other.gameObject.transform.SetParent(body.transform, false);
        EggBehavior eggBehavior = other.gameObject.GetComponent<EggBehavior>();

        if (!eggBehavior.isBeingHeld)
        {
            PickUpEgg(eggBehavior);
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
        movement.Normalize();

        //Move the player
        transform.position = transform.position + movement * speed * Time.deltaTime;

        Vector3 rotation = new Vector3(lookValue.x, 0f, lookValue.y);
        rotation.Normalize();

        //Rotate the player
        if (rotation != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(rotation, Vector3.up);
            body.rotation = Quaternion.Lerp(body.rotation, toRotation, Time.deltaTime * lerpParam);
        }
    }

    private void Interact (InputAction.CallbackContext context)
    {
        ThrowEgg();
        Debug.Log("we interacted");
    }

    private void ThrowEgg()
    {
        //choose egg to throw
        GameObject eggToThrow = eggInventory[eggInventory.Count - 1];
        eggToThrow.transform.parent = null;//unparent

        //throw
        eggToThrow.GetComponent<Rigidbody>().velocity = body.transform.forward * 20;
        
    }

    private void PickUpEgg(EggBehavior eggBehavior)
    {
        GameObject eggToBePickedUp = eggBehavior.gameObject;
        //add egg to inventory
        eggInventory.Add(eggToBePickedUp);
        //eggToBePickedUp.transform.position = body.transform.position;
        eggToBePickedUp.transform.parent = body;
    }

    private IEnumerator LoseEgg()
    {
        //remove egg from inventory using LIFO
        GameObject eggToRemove = eggInventory[eggInventory.Count - 1];
        eggToRemove.transform.parent = null;//unparent
        eggInventory.Remove(eggToRemove);

        //gets stunned
        isStunned = true;
        yield return new WaitForSeconds(stunTime);
        isStunned = false;
    }
}
