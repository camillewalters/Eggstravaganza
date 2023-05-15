using System;
using Unity.Netcode;
using UnityEngine.Serialization;

[Serializable]
public class PlayerData
{
    int m_Score;

    public int Score
    {
        get => m_Score;
        set
        {
            if (m_Score == value)
            {
                return;
            }
            m_Score = value;
            OnScoreChange?.Invoke(m_Score);
        }
    }
    
    public int ID;
    public string Name;
    
    public PlayerData(int id, string name)
    {
        ID = id;
        Name = name;
    }
    
    public delegate void OnVariableChangeDelegate(int newVal);
    public event OnVariableChangeDelegate OnScoreChange;
    
    public struct PlayerRegisterData : INetworkSerializable
    {
        internal ulong ID;
        // internal string Name;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ID);
            // serializer.SerializeValue(ref Name);
        }
    }
}