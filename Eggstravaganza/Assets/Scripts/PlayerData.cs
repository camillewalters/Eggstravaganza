using System;

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
    public int PlayerID;
    public string Name;
    
    public PlayerData(int playerID, string name)
    {
        PlayerID = playerID;
        Name = name;
    }
    
    public delegate void OnVariableChangeDelegate(int newVal);
    public event OnVariableChangeDelegate OnScoreChange;
}