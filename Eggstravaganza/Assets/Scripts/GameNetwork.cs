using System;
using Unity.Netcode;
using UnityEngine;

public class GameNetwork : NetworkBehaviour
{
    [SerializeField]
    GameManager GameManager;
    
    [SerializeField]
    GameDataScriptableObject GameData;

    readonly NetworkVariable<float> m_GameTimer = new(writePerm: NetworkVariableWritePermission.Owner);

    void Awake()
    {
        m_GameTimer.Value = GameData.InitialTimer;
    }

    void Update()
    {
        DecrementGameTime();
    }
    
    void DecrementGameTime()
    {
        if (IsOwner)
        {
            if (m_GameTimer.Value > 0)
            {
                m_GameTimer.Value -= Time.deltaTime;
                GameData.RuntimeTimer = m_GameTimer.Value;
            }
            else 
            {
                GameManager.EndRound();    
            }
        }
        GameData.RuntimeTimer = m_GameTimer.Value;
    }
}