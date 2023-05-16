using UnityEngine;

public class EggBehavior: MonoBehaviour
{
	public bool isBeingHeld = false;
	public bool isBeingThrown = false;

    private void Update()
    {
        if (transform.position.y == 0) //whenever it hits the ground its fair game to be picked up and won't damage anyone
        {
            isBeingThrown = false;
            isBeingHeld = false;
        }
    }
}
