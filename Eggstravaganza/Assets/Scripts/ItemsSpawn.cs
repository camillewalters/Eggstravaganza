using System;
using UnityEditor;
#if UNITY_EDITOR
using UnityEngine;
#endif
using Random = UnityEngine.Random;
public class ItemsSpawn: MonoBehaviour
{
    GameObject[] m_Prefabs;
    [SerializeField]
    int maxNumItemsToSpawn = 5;
    [SerializeField]
    float spawnAreaScale = 1;
    [SerializeField]
    int minHeightSpawn = 2;
    [SerializeField]
    int maxHeightSpawn = 6;
    
    void Awake()
    {
        m_Prefabs = Resources.LoadAll<GameObject>("SpawnItems");
    }
    // Function to be called by Game Manager
    public void SpawnItem()
    {
        for (var i = 0; i < Random.Range(1, maxNumItemsToSpawn + 1); i++)
        {
            var pickRand = Random.Range(0, m_Prefabs.Length);
            var spawnPos = Random.insideUnitCircle * spawnAreaScale;
            Instantiate(m_Prefabs[pickRand],  new Vector3(spawnPos.x, Random.Range(minHeightSpawn, maxHeightSpawn), spawnPos.y), Quaternion.identity);
        }
    }
}

#if UNITY_EDITOR
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
#endif
