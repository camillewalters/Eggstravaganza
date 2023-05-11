using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public Transform body;

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical);//this might be off
        movement.Normalize();

        transform.Translate(movement * speed * Time.deltaTime);

        ////turn towards direction of movement, might not want this? idk really how it works?
        //if (movement != Vector3.zero)
        //{
        //    Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
        //    body.rotation = Quaternion.Lerp(body.rotation, toRotation, Time.deltaTime * 10f);
        //    //Debug.Log("rotate: " + body.rotation);
        //}

        float shootHorizontal = Input.GetAxis("Mouse X");
        float shootVertical = Input.GetAxis("Mouse Y");

        Vector3 shootingDirection = new Vector3(shootHorizontal, 0f, shootVertical);
        if (shootingDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(shootingDirection, Vector3.up);
            body.rotation = Quaternion.Lerp(body.rotation, toRotation, Time.deltaTime * 10f);
        }

        if (Input.GetButton("Fire1"))//this is click on the mouse. could change later too
        {
            Debug.Log("picked up egg");
        }
    }
}
