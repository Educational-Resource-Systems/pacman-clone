using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Scores : MonoBehaviour
{
	public Text highscoreText; // Assign in Inspector

	void Start()
	{
		if (ScoreManager.instance != null)
		{
			ScoreManager.instance.GetHighscores(DisplayScores);
		}
		else
		{
			Debug.LogError("ScoreManager instance is null!");
			if (highscoreText != null)
			{
				highscoreText.text = "Error: ScoreManager not found";
			}
		}
	}

	void DisplayScores(List<ScoreEntry> scores)
	{
		try
		{
			if (highscoreText == null)
			{
				Debug.LogError("highscoreText is not assigned!");
				return;
			}

			if (scores == null || scores.Count == 0)
			{
				highscoreText.text = "No scores available";
				return;
			}

			string displayText = "Highscores:\n";
			for (int i = 0; i < scores.Count && i < 20; i++)
			{
				displayText = displayText + (i + 1) + ". " + scores[i].name + ": " + scores[i].score + "\n";
			}
			highscoreText.text = displayText;
		}
		catch (System.Exception e)
		{
			Debug.LogError("Exception in DisplayScores: " + e.Message);
			if (highscoreText != null)
			{
				highscoreText.text = "Error displaying scores";
			}
		}
	}
}