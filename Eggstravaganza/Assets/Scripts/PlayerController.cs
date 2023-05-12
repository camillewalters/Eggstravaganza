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
    private InputAction look;

    public int[] eggInventory; //temporarily using an int, use Egg class later

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

        look = playerControls.Player.Look;
    }
    private void OnDisable()
    {
        playerControls?.Player.Disable();
        interact.performed -= Interact;
    }

    private void Update()
    {
        if (!isStunned)
        {
            Move();
        }
    }

    private void Move()
    {
        Vector2 movementDirection = move.ReadValue<Vector2>();
        Vector3 movement = new Vector3(movementDirection.x, 0f, movementDirection.y);
        movement.Normalize();

        //Move the player
        transform.position = transform.position + movement * speed * Time.deltaTime;

        Vector2 lookDirection = look.ReadValue<Vector2>();
        Vector3 rotation = new Vector3(lookDirection.x, 0f, lookDirection.y);
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
        Debug.Log("we interacted");
    }

    private void ThrowEgg()
    {
        //implement later
    }

    private void PickUpEgg()
    {
        //add egg to inventory
    }

    private IEnumerator LoseEgg()
    {
        //remove egg from inventory

        //gets stunned
        isStunned = true;
        yield return new WaitForSeconds(stunTime);
        isStunned = false;
    }
}
