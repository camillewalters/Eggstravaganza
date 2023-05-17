using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerAnimatorController : MonoBehaviour
{
    [SerializeField]
    ParticleSystem ParticleSystem;
    
    Rigidbody m_Rigidbody;
    Animator m_Animator;
    static readonly int IsWalking = Animator.StringToHash("isWalking");
    static readonly int Throw = Animator.StringToHash("throw");

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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_Animator.SetTrigger(Throw);
        }
        
        // DEBUG
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // StartCoroutine(ConfettiParticles());
            ParticleSystem.Stop();
            ParticleSystem.Play();
        }
    }

    IEnumerator ConfettiParticles()
    {
        ParticleSystem.Stop();
        ParticleSystem.Play();
        yield return new WaitForSeconds(5f);
    }
}
