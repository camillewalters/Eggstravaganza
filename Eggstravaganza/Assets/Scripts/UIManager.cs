using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    GameObject StartPanel;
    
    [SerializeField]
    GameObject GamePanel;
    
    [SerializeField]
    GameObject ResultsPanel;
    
    [SerializeField]
    GameDataScriptableObject GameData;
    
    [SerializeField]
    TextMeshProUGUI TimerText;

    [SerializeField]
    TextMeshProUGUI[] PlayerScores;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var player in GameData.Players)
        {
            PlayerScores[player.Key].SetText("0");
            GameData.Players[player.Key].OnScoreChange += (newVal) => { PlayerScores[player.Key].SetText(newVal.ToString()); }; 
        }

        for (int i = GameData.Players.Count; i < 4; i++)
        {
            PlayerScores[i].gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        TimerText.SetText(GameData.RuntimeTimer.ToString("F"));
    }

    public void EndGame()
    {
        // TODO: transitions
        GamePanel.SetActive(false);
        PopulateResults();
        ResultsPanel.SetActive(true);
    }

    void PopulateResults()
    {
        // TODO
    }
}
