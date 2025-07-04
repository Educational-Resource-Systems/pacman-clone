using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HighscoreDisplay : MonoBehaviour
{
	public Text highscoreText; // Assign in Inspector

	void Start()
	{
		ScoreManager.instance.GetHighscores(DisplayScores);
	}

	void DisplayScores(List<ScoreEntry> scores)
	{
		if (scores == null || scores.Count == 0)
		{
			highscoreText.text = "No scores available";
			return;
		}

		string displayText = "Highscores:\n";
		for (int i = 0; i < scores.Count && i < 20; i++) // Ensure no more than 20 entries
		{
			displayText = displayText + (i + 1) + ". " + scores[i].name + ": " + scores[i].score + "\n";
		}
		highscoreText.text = displayText;
	}
}