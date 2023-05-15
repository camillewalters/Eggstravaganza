using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
public class ItemsSpawn: MonoBehaviour
{
    GameObject[] m_Prefabs;
    [SerializeField]
    int numItemsToSpawn = 5;
    [SerializeField]
    float spawnAreaScale = 1;
    
    void Awake()
    {
        m_Prefabs = Resources.LoadAll<GameObject>("SpawnItems");
    }
    // Function to be called by Game Manager
    public void SpawnItem()
    {
        for (var i = 0; i < numItemsToSpawn; i++)
        {
            var pickRand = Random.Range(0, m_Prefabs.Length);
            var spawnPos = Random.insideUnitSphere * spawnAreaScale;
            Instantiate(m_Prefabs[pickRand],  new Vector3(spawnPos.x, Random.Range(2, 6), spawnPos.y), Quaternion.identity);
        }
    }
}

[CustomEditor(typeof(ItemsSpawn))]
public class SpawnEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var myScript = (ItemsSpawn)target;
        if(GUILayout.Button("Spawn Items"))
        {
            myScript.SpawnItem();
        }
    }
}
