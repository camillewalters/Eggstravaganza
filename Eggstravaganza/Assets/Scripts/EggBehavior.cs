using UnityEngine;

public class EggBehavior: MonoBehaviour
{
	public bool isBeingHeld = false;
	public bool isBeingThrown = false;

    Rigidbody rb;

    public PlayerController thrownBy;
    public PlayerController droppedBy;

    [SerializeField]
    public int value;

    private void Awake()
    {
         rb = GetComponent<Rigidbody>();
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
