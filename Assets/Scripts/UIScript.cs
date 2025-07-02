using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
	public int high, score;

	public List<Image> lives = new List<Image>(3);

	Text txt_score, txt_high, txt_level;
	private ScoreManager scoreManager; // Cache ScoreManager reference

	// Use this for initialization
	void Start()
	{
		Text[] texts = GetComponentsInChildren<Text>();
		if (texts.Length >= 3)
		{
			txt_score = texts[1];
			txt_high = texts[0];
			txt_level = texts[2];
		}
		else
		{
			Debug.LogError("Not enough Text components found!");
		}

		scoreManager = GameObject.Find("Game Manager").GetComponent<ScoreManager>();
		if (scoreManager == null)
		{
			Debug.LogError("ScoreManager not found on Game Manager!");
		}

		for (int i = 0; i < 3 - GameManager.lives; i++)
		{
			if (lives.Count > 0)
			{
				Destroy(lives[lives.Count - 1].gameObject); // Destroy GameObject
				lives.RemoveAt(lives.Count - 1);
			}
		}
	}

	// Update is called once per frame
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
		}
		catch (System.Exception e)
		{
			Debug.LogError("Exception in UIScript Update: " + e.Message);
		}
	}
}