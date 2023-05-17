using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    Rigidbody m_Rigidbody;
    Animator m_Animator;
    static readonly int IsWalking = Animator.StringToHash("isWalking");

    // Start is called before the first frame update
    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        var isMoving = m_Rigidbody.velocity != Vector3.zero;
        m_Animator.SetBool(IsWalking, isMoving);
        m_Animator.speed = isMoving ? m_Rigidbody.velocity.magnitude / 2 : 0.2f;
    }
}
