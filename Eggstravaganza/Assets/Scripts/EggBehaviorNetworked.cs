using UnityEngine;
using Unity.Netcode;
using Object = UnityEngine.Object;

public class EggBehaviorNetworked: NetworkBehaviour
{
    public bool isBeingHeld = false;
    public bool isBeingThrown = false;

    Rigidbody rb;

    public Object thrownBy;
    public Object droppedBy;

    [SerializeField, Tooltip("The egg model that will be displayed in the player's hand")]
    private GameObject m_DisplayEgg;
    public GameObject DisplayEgg => m_DisplayEgg;

    private void Awake()
    {
         rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        rb.detectCollisions = !isBeingHeld;

        // if (isBeingHeld)
        // {
            // Debug.Log("following goph");
        // }
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
