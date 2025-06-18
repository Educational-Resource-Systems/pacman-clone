using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ScoreEntry
{
	public string name;
	public string email;
	public int score;
}

public class ScoreManager : MonoBehaviour
{
	public static ScoreManager instance;
	private string topscoresURL = "https://ers-dev.com//ERS/_pacman/build5/topscores.php"; // Update with your server URL
	private int currentScore = 0;
	private string playerName;
	private string playerEmail;
	private int lowestHighScore = 0; // Cache for lowest highscore
	private int highestHighScore = 0; // Cache for highest highscore

	void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
		playerName = PlayerPrefs.GetString("PlayerName", "Anonymous");
		playerEmail = PlayerPrefs.GetString("PlayerEmail", "none");
		// Initialize caches
		StartCoroutine(FetchLowestHigh(delegate(int score) { lowestHighScore = score; }));
		StartCoroutine(FetchHighestHigh(delegate(int score) { highestHighScore = score; }));
	}

	public void SaveScore(int score)
	{
		currentScore = score;
		StartCoroutine(SubmitScore());
	}

	IEnumerator SubmitScore()
	{
		WWWForm form = new WWWForm();
		form.AddField("player_name", playerName);
		form.AddField("email", playerEmail);
		form.AddField("score", currentScore);

		UnityWebRequest www = UnityWebRequest.Post(topscoresURL, form);
		yield return www.Send();

		if (www.isError)
		{
			Debug.LogError("Score submission failed: " + www.error);
		}
		else
		{
			Debug.Log("Score submitted successfully");
			// Update caches
			StartCoroutine(FetchLowestHigh(delegate(int score) { lowestHighScore = score; }));
			StartCoroutine(FetchHighestHigh(delegate(int score) { highestHighScore = score; }));
		}
	}

	public void GetHighscores(System.Action<List<ScoreEntry>> callback)
	{
		StartCoroutine(FetchHighscores(callback));
	}

	IEnumerator FetchHighscores(System.Action<List<ScoreEntry>> callback)
	{
		UnityWebRequest www = UnityWebRequest.Get(topscoresURL);
		yield return www.Send();

		if (www.isError)
		{
			Debug.LogError("Failed to fetch highscores: " + www.error);
			callback(null);
		}
		else
		{
			string json = www.downloadHandler.text;
			ScoreEntry[] scores = JsonUtility.FromJson<ScoreEntry[]>(json);
			callback(new List<ScoreEntry>(scores));
		}
	}

	public int LowestHigh()
	{
		return lowestHighScore;
	}

	public int High() // New method for highest score
	{
		return highestHighScore;
	}

	IEnumerator FetchLowestHigh(System.Action<int> callback)
	{
		UnityWebRequest www = UnityWebRequest.Get(topscoresURL);
		yield return www.Send();

		if (www.isError)
		{
			Debug.LogError("Failed to fetch highscores: " + www.error);
			callback(0);
		}
		else
		{
			string json = www.downloadHandler.text;
			ScoreEntry[] scores = JsonUtility.FromJson<ScoreEntry[]>(json);
			if (scores == null || scores.Length == 0)
			{
				callback(0);
			}
			else
			{
				int lowestScore = scores[0].score;
				for (int i = 1; i < scores.Length && i < 20; i++)
				{
					if (scores[i].score < lowestScore)
					{
						lowestScore = scores[i].score;
					}
				}
				callback(lowestScore);
			}
		}
	}

	IEnumerator FetchHighestHigh(System.Action<int> callback)
	{
		UnityWebRequest www = UnityWebRequest.Get(topscoresURL);
		yield return www.Send();

		if (www.isError)
		{
			Debug.LogError("Failed to fetch highscores: " + www.error);
			callback(0);
		}
		else
		{
			string json = www.downloadHandler.text;
			ScoreEntry[] scores = JsonUtility.FromJson<ScoreEntry[]>(json);
			if (scores == null || scores.Length == 0)
			{
				callback(0);
			}
			else
			{
				int highestScore = scores[0].score; // Top score is first (sorted DESC)
				callback(highestScore);
			}
		}
	}
}