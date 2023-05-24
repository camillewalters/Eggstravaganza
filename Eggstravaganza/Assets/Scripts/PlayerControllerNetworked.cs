using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerControllerNetworked : NetworkBehaviour
{
    [SerializeField] private float m_Speed = 80f;
    [SerializeField] private float m_LerpParam = 10f;
    [SerializeField] private Transform body;

    private Rigidbody m_Rigidbody;
    private Transform m_EggLocation;

    private PlayerInputActions m_PlayerControls;

    private InputAction m_Move;
    private InputAction m_Interact;
    private Vector2 m_LookValue;

    private readonly List<GameObject> m_EggInventory = new();
    private float m_StunTime = 2f;
    private bool m_IsStunned;

    [SerializeField] private int m_ThrowForwardFactor = 20;
    [SerializeField] private int m_ThrowUpwardFactor = 5;
    [SerializeField] private int m_DropForwardFactor = -3; //negative so it goes backwards

    private readonly List<GameObject> m_DisplayEggInventory = new();

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_PlayerControls = new PlayerInputActions();
        m_EggLocation = body.Find("EggLocation");
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        m_PlayerControls.Player.Enable();

        m_Move = m_PlayerControls.Player.Move;

        m_Interact = m_PlayerControls.Player.Fire;
        m_Interact.performed += Interact;

        m_PlayerControls.Player.Look.performed += LookPerformed;
    }

    private void OnDisable()
    {
        m_PlayerControls?.Player.Disable();
        m_Interact.performed -= Interact;
        m_PlayerControls.Player.Look.performed -= LookPerformed;
    }

    private void Update()
    {
        if (!m_IsStunned && IsOwner)
        {
            Move();
        }
    }

    public void HandleEggHitboxCollision(Collider other)
    {
        var eggBehavior = other.gameObject.GetComponent<EggBehaviorNetworked>();
        if (eggBehavior != null)
        {
            Debug.Log(
                $"Collided with egg, being thrown? {eggBehavior.isBeingThrown}, dropped by {eggBehavior.droppedBy}");
            if (!eggBehavior.isBeingHeld && !eggBehavior.isBeingThrown && eggBehavior.droppedBy != this)
            {
                Debug.Log("pick up");
                PickUpEgg(other.gameObject);
                eggBehavior.isBeingHeld = true;
            }
            else if (eggBehavior.isBeingThrown && eggBehavior.thrownBy != this)
            {
                Debug.Log("lose");
                StartCoroutine(LoseEgg());
            }
        }
    }

    private void LookPerformed(InputAction.CallbackContext context)
    {
        m_LookValue = context.ReadValue<Vector2>();

        if (context.control.device.ToString() == "Mouse:/Mouse")
        {
            Ray ray = Camera.main.ScreenPointToRay(m_LookValue);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayDistance;

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 point = ray.GetPoint(rayDistance);
                Vector3 lookDirection = (point - transform.position).normalized;
                m_LookValue = new Vector2(lookDirection.x, lookDirection.z);
            }
        }
    }

    private void Move()
    {
        Vector2 movementDirection = m_Move.ReadValue<Vector2>();
        Vector3 movement = new Vector3(movementDirection.x, 0f, movementDirection.y);
        m_Rigidbody.AddForce(movement * m_Speed);

        Vector3 rotation = new Vector3(m_LookValue.x, 0f, m_LookValue.y);
        rotation.Normalize();

        //Rotate the player
        if (rotation != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(rotation, Vector3.up);
            body.rotation = Quaternion.Lerp(body.rotation, toRotation, Time.deltaTime * m_LerpParam);
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
        if (m_EggInventory.Count > 0 && IsOwner)
        {
            RequestThrowServerRpc();

            // Throw locally immediately
            ExecuteThrow();
        }
    }

    [ServerRpc]
    private void RequestThrowServerRpc() {
        ThrowClientRpc();
    }

    [ClientRpc]
    private void ThrowClientRpc() {
        if (!IsOwner) ExecuteThrow();
    }

    private void ExecuteThrow()
    {
        //choose egg to throw
        GameObject eggToThrow = m_EggInventory[^1];
        eggToThrow.SetActive(true);
        var eggRb = eggToThrow.GetComponent<Rigidbody>();

        eggRb.isKinematic = false;
        Destroy(m_DisplayEggInventory[^1]);
        m_DisplayEggInventory.RemoveAt(m_DisplayEggInventory.Count - 1);

        //throw from lowest point
        eggToThrow.transform.position = m_EggLocation.position;
        eggRb.velocity = m_Rigidbody.transform.forward * m_ThrowForwardFactor + m_Rigidbody.transform.up * m_ThrowUpwardFactor;
        m_EggInventory.Remove(eggToThrow);

        EggBehaviorNetworked eggBehavior = eggToThrow.GetComponent<EggBehaviorNetworked>();
        eggBehavior.isBeingThrown = true;
        eggBehavior.isBeingHeld = false;
        eggBehavior.thrownBy = this;
    }

    private void PickUpEgg(GameObject eggToBePickedUp)
    {
        var eggRb = eggToBePickedUp.GetComponent<Rigidbody>();

        eggRb.isKinematic = true;
        m_EggInventory.Add(eggToBePickedUp);

        Debug.Log($"We pickin up an egg rn at {eggRb.transform.position}");

        GameObject displayEggToSpawn = eggToBePickedUp.GetComponent<EggBehaviorNetworked>().DisplayEgg;
        GameObject displayEggInstance = Instantiate(displayEggToSpawn, NextEggHoldLocation(), Quaternion.identity, m_EggLocation);
        m_DisplayEggInventory.Add(displayEggInstance);
        eggToBePickedUp.SetActive(false);
    }

    private Vector3 NextEggHoldLocation()
    {
        var basePosition = m_EggLocation.transform.position;
        var nextLocation = basePosition + new Vector3(0, 0.4f * (m_EggInventory.Count - 1), 0);//approx height of egg
        return nextLocation;
    }

    private IEnumerator LoseEgg()
    {
        //remove egg from inventory using LIFO
        if (m_EggInventory.Count > 0)
        {
            GameObject eggToRemove = m_EggInventory[^1];
            eggToRemove.SetActive(true);
            m_EggInventory.Remove(eggToRemove);
            
            Destroy(m_DisplayEggInventory[^1]);
            m_DisplayEggInventory.RemoveAt(m_DisplayEggInventory.Count - 1);

            var eggRb = eggToRemove.GetComponent<Rigidbody>();
            eggRb.velocity = body.transform.forward * m_DropForwardFactor;//falls backwards with a bit of velocity
            eggRb.isKinematic = false;

            EggBehaviorNetworked eggBehavior = eggToRemove.GetComponent<EggBehaviorNetworked>();
            eggBehavior.isBeingHeld = false;
            eggBehavior.droppedBy = this;
        }

        //gets stunned
        m_IsStunned = true;
        yield return new WaitForSeconds(m_StunTime);
        m_IsStunned = false;
    }
}
