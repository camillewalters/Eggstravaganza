using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager m_Instance;

    public static GameManager Instance => m_Instance;

    [SerializeField]
    GameDataScriptableObject GameData;
    
    [SerializeField] 
    UIManager UIManager;

    void Awake()
    {
        if (m_Instance != null && m_Instance != this)
        {
            Debug.LogError("Multiple GameManagers in scene!");
            Destroy(gameObject);
        }
        else
        {
            m_Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // GameData.Players.Add(0, new PlayerData(0, "Player 0"));
        // GameData.Players.Add(1, new PlayerData(1, "Player 1"));
        // GameData.Players.Add(2, new PlayerData(2, "Player 2"));
        // GameData.Players.Add(3, new PlayerData(3, "Player 3"));
    }

    // Update is called once per frame
    void Update()
    {
        DecrementGameTime();

        // if (Input.GetKeyDown(KeyCode.Alpha1))
        // {
        //     GameData.IncrementPlayerScore(0, 1);
        // }
        // if (Input.GetKeyDown(KeyCode.Alpha2))
        // {
        //     GameData.IncrementPlayerScore(1, 1);
        // }
        // if (Input.GetKeyDown(KeyCode.Alpha3))
        // {
        //     GameData.IncrementPlayerScore(2, 1);
        // }
        // if (Input.GetKeyDown(KeyCode.Alpha4))
        // {
        //     GameData.IncrementPlayerScore(3, 1);
        // }
        //
        // if (Input.GetKeyDown(KeyCode.Escape))
        // {
        //     EndRound();
        // }
    }

    void DecrementGameTime()
    {
        if (GameData.RuntimeTimer > 0)
        {
            GameData.RuntimeTimer -= Time.deltaTime;
        }
        else 
        {
            EndRound();    
        }
    }

    void EndRound()
    {
        UIManager.EndGame();
    }
}
