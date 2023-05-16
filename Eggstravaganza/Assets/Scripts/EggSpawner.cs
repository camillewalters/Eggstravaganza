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

    public void SpawnEgg(Vector3 spawnPosition, int index = 0)
    {
        var currentEggToSpawn = eggs[index];
        Instantiate(currentEggToSpawn.prefab, spawnPosition, Quaternion.identity);
        
        Debug.Log($"Spawned {currentEggToSpawn.prefab.name} now!");
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
}
