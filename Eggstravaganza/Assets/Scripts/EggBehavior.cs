using UnityEngine;

public class EggBehavior: MonoBehaviour
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
