using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public Transform body;

    public PlayerInputActions playerControls;

    private InputAction move;
    private InputAction interact;
    private InputAction look;

    private void Awake()
    {
        playerControls = new PlayerInputActions();
    }
    private void OnEnable()
    {
        move = playerControls.Player.Move;
        move.Enable();

        interact = playerControls.Player.Fire;
        interact.Enable();
        interact.performed += Interact;

        look = playerControls.Player.Look;
        look.Enable();

    }
    private void OnDisable()
    {
        move.Disable();
        interact.Disable();
        look.Disable();
    }

    private void Update()
    {
        Vector2 movementDirection = move.ReadValue<Vector2>();
        Vector3 movement = new Vector3(movementDirection.x, 0f, movementDirection.y);
        movement.Normalize();

        // Move the player
        transform.Translate(movement * speed * Time.deltaTime);

        Vector2 lookDirection = look.ReadValue<Vector2>();
        Vector3 rotation = new Vector3(lookDirection.x, 0f, lookDirection.y);
        rotation.Normalize();

        //Rotate the player
        if (rotation != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(rotation, Vector3.up);
            body.rotation = Quaternion.Lerp(body.rotation, toRotation, Time.deltaTime * 10f);
        }

    }

    private void Interact (InputAction.CallbackContext context)
    {
        Debug.Log("we interacted");//whatever picking up/throwing egg logic we want here
    }

}
