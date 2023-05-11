using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
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
    
    double m_AccumulatedWeights;
    System.Random m_Rand = new();

    void Awake()
    {
        CalculateWeights();
    }

    public void SpawnEgg()
    {
        var currentEggToSpawn = eggs[ChooseEggToSpawn()];
        
        // TODO: Maybe modify spawn height? Not sure what value we'd want yet
        var randomPosition = Random.insideUnitCircle;
        var spawnPosition = new Vector3(randomPosition.x, 2, randomPosition.y);
        Instantiate(currentEggToSpawn.prefab, spawnPosition, Quaternion.identity);
        
        Debug.Log($"Spawned {currentEggToSpawn.prefab.name} now!");
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
        }
    }
    
    // TODO: Uncomment this only for testing purposes, delete and call SpawnEgg() in GameManager instead after it is created 
    /*
    public float timeRemaining;
    public bool timerIsRunning;
    const float k_SpawnInterval = 20;

    void Start()
    {
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
                SpawnEgg();
                timeRemaining = 0;
                timerIsRunning = false;
            }
        }
        else
        {
            timeRemaining = k_SpawnInterval;
            timerIsRunning = true;
        }
    }
    */
}
