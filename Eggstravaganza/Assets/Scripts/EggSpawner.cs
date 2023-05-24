using System;
using System.Collections.Generic;
using Unity.Netcode;
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

public class EggSpawner : MonoBehaviour
{
    [SerializeField]
    List<Egg> eggs;

    [SerializeField]
    float spawnAreaScale = 1;

    double m_AccumulatedWeights;
    System.Random m_Rand = new();

    void Awake()
    {
        CalculateWeights();
    }

    public EggBehaviorNetworked SpawnEgg(Vector3 spawnPosition, int index = 0)
    {
        var currentEggToSpawn = eggs[index];
        var egg = Instantiate(currentEggToSpawn.prefab, spawnPosition, Quaternion.identity);

        Debug.Log($"Spawned {currentEggToSpawn.prefab.name}!");
        return egg.GetComponent<EggBehaviorNetworked>();
    }

    public Vector3 ChooseSpawnPosition()
    {
        // TODO: Maybe modify spawn height? Not sure what value we'd want yet
        var randomPosition = Random.insideUnitCircle * spawnAreaScale;
        var spawnPosition = new Vector3(randomPosition.x, 2, randomPosition.y);

        return spawnPosition;
    }

    public int ChooseEggToSpawn()
    {
        var randomWeight = m_Rand.NextDouble() * m_AccumulatedWeights;
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
    const float k_SpawnInterval = 10;

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
                SpawnEgg(ChooseSpawnPosition(), ChooseEggToSpawn());
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
