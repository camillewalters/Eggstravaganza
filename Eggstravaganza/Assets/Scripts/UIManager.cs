using System.Collections.Generic;
using System.Linq;
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
    
    static readonly Dictionary<int, string> PlaceToPlacement = new Dictionary<int, string>()
    {
        { 0, "1st" }, { 1, "2nd" }, { 2, "3rd" }, { 3, "4th" }
    };

    // Start is called before the first frame update
    void Start()
    {
        foreach (var player in GameData.Players)
        {
            PlayerScores[player.Key].SetText("0");
            GameData.Players[player.Key].OnScoreChange += (newVal) => { PlayerScores[player.Key].SetText(newVal.ToString()); }; 
        }

        for (int i = GameData.Players.Count; i < Utils.k_MaxPlayers; i++)
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
        var parent = ResultsPanel.transform.Find("Results").transform;
        var sorted = GameData.Players.OrderByDescending(i => i.Value.Score);
        var count = 0;
        var prevScore = -1;
        var prevPlacement = "";
        
        // Populate text results
        foreach (var entry in sorted)
        {
            var place = parent.GetChild(count);
            var placementText = place.Find("Placement").GetComponent<TextMeshProUGUI>();
            if (count > 0)
            {
                if (entry.Value.Score == prevScore)
                {
                    placementText.SetText(prevPlacement);
                }
                else
                {
                    placementText.SetText(PlaceToPlacement[count]);
                }
            }
            else
            {
                placementText.SetText("1st");
            }
            place.Find("Name").GetComponent<TextMeshProUGUI>().SetText(entry.Value.Name);
            place.Find("Score").GetComponent<TextMeshProUGUI>().SetText(entry.Value.Score.ToString());
            count++;
            prevScore = entry.Value.Score;
            prevPlacement = placementText.text;
        }

        // Deactivate inactive players
        for (int i = count; i < Utils.k_MaxPlayers; i++)
        {
            var place = parent.GetChild(i);
            place.gameObject.SetActive(false);
        }
    }
}
