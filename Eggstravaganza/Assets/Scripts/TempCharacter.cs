using Unity.Netcode;
using UnityEngine;

public class TempCharacter : NetworkBehaviour
{
    [SerializeField]
    float Speed = 1;
    
    CharacterController m_Controller;
    
    // Start is called before the first frame update
    void Awake()
    {
        m_Controller = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        var input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        m_Controller.Move(input * (Time.deltaTime * Speed));
    }
}
