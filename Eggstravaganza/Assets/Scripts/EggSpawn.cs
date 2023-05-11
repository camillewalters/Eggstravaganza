using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class Egg
{
    public GameObject prefab;
    [Range(0f, 100)]
    public float chance = 100;
    [HideInInspector]
    public double weight;
}

public class EggSpawn : MonoBehaviour
{
    public List<Egg> eggs;
    
    // Timer to track time between spawns
    public float timeRemaining;
    public bool timerIsRunning;
    
    Egg m_CurrentSpawnChoice;
    double m_AccumulatedWeights;
    System.Random m_Rand = new();
    const float k_SpawnInterval = 20;

    void Awake()
    {
        CalculateWeights();
    }

    void Start()
    {
        m_CurrentSpawnChoice = eggs[ChooseEggToSpawn()];
        
        timeRemaining = k_SpawnInterval;
        timerIsRunning = true;
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.fixedDeltaTime;
            }
            else
            {
                Debug.Log($"Spawning {m_CurrentSpawnChoice.prefab.name} now!");
                SpawnEgg();
                
                m_CurrentSpawnChoice = null;
                timeRemaining = 0;
                timerIsRunning = false;
            }
        }
        else
        {
            m_CurrentSpawnChoice = eggs[ChooseEggToSpawn()];
            timeRemaining = k_SpawnInterval;
            timerIsRunning = true;
        }
    }

    void SpawnEgg()
    {
        var randomPosition = Random.insideUnitCircle;
        var spawnPosition = new Vector3(randomPosition.x, 2, randomPosition.y);
        Instantiate(m_CurrentSpawnChoice.prefab, spawnPosition, Quaternion.identity);
    }

    int ChooseEggToSpawn()
    {
        var randomWeight = m_Rand.NextDouble() * m_AccumulatedWeights;
        Debug.Log(randomWeight);
        for (var i = 0; i < eggs.Count; ++i)
        {
            if (eggs[i].weight >= randomWeight)
                return i;
        }

        return 0;
    }

    void CalculateWeights()
    {
        m_AccumulatedWeights = 0;
        foreach (var egg in eggs)
        {
            m_AccumulatedWeights += egg.chance;
            egg.weight = m_AccumulatedWeights;
            Debug.Log($"{egg.prefab.name} and {egg.weight}");
        }
    }
}
