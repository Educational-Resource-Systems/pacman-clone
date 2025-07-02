using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
	public int high, score;
	public List<Image> lives = new List<Image>(3); // Three Pacman sprites in Inspector
	private Text txt_score, txt_high, txt_level, txt_ready; // Added txt_ready
	private ScoreManager scoreManager; // Cache ScoreManager reference

	void Start()
	{
		Text[] texts = GetComponentsInChildren<Text>();
		if (texts.Length >= 4) // Adjusted for txt_ready
		{
			txt_high = texts[0];
			txt_score = texts[1];
			txt_level = texts[2];
			txt_ready = texts[3]; // Assuming fourth Text is for "Ready"
		}
		else
		{
			Debug.LogError("Not enough Text components found! Expected at least 4, found " + texts.Length);
		}

		scoreManager = GameObject.Find("Game Manager").GetComponent<ScoreManager>();
		if (scoreManager == null)
		{
			Debug.LogError("ScoreManager not found on Game Manager!");
		}

		// Initialize score, high score, level, and lives
		if (scoreManager != null)
		{
			high = scoreManager.High();
		}
		else
		{
			high = 0;
			Debug.LogWarning("ScoreManager is null, using high score 0");
		}
		score = GameManager.score;
		if (txt_score != null) txt_score.text = "Score\n" + score;
		if (txt_high != null) txt_high.text = "High Score\n" + high;
		if (txt_level != null) txt_level.text = "Level\n" + (GameManager.Level + 1);
		if (txt_ready != null && GameManager.gameState != GameManager.GameState.Init)
		{
			txt_ready.text = "";
		}

		UpdateLivesDisplay();
	}

	void Update()
	{
		try
		{
			if (scoreManager != null)
			{
				high = scoreManager.High();
			}
			else
			{
				high = 0;
				Debug.LogWarning("ScoreManager is null, using high score 0");
			}
			score = GameManager.score;
			if (txt_score != null) txt_score.text = "Score\n" + score;
			if (txt_high != null) txt_high.text = "High Score\n" + high;
			if (txt_level != null) txt_level.text = "Level\n" + (GameManager.Level + 1);
			if (txt_ready != null && GameManager.gameState != GameManager.GameState.Init)
			{
				txt_ready.text = "";
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError("Exception in UIScript Update: " + e.Message);
		}
	}

	public void UpdateLivesDisplay()
	{
		if (lives == null || lives.Count < GameManager.lives)
		{
			Debug.LogWarning("UIScript: Lives list not properly initialized, expected at least " + GameManager.lives + " lives, found " + (lives != null ? lives.Count : 0));
			return;
		}

		Debug.Log("UIScript: Updating lives display, GameManager.lives: " + GameManager.lives);
		for (int i = 0; i < lives.Count; i++)
		{
			if (lives[i] != null)
			{
				lives[i].gameObject.SetActive(i < GameManager.lives);
			}
		}
	}
}