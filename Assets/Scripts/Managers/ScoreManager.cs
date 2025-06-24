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
	private string topscoresURL = "https://ers-dev.com/ERS/_pacman/build5/topscores.php";
	private int currentScore = 0;
	private string playerName;
	private string playerEmail;
	private int lowestHighScore = 0;
	private int highestHighScore = 0;
	private List<ScoreEntry> cachedScores = new List<ScoreEntry>(); // Cache full list
	private bool isSavingScore = false;
	private bool needsPlayerPrefsSave = false;

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
		// Load cached scores
		string cachedCsv = PlayerPrefs.GetString("CachedScores", "");
		if (!string.IsNullOrEmpty(cachedCsv))
		{
			Debug.Log("Loading cached scores");
			cachedScores = ParseCSV(cachedCsv);
			UpdateHighLowScores();
		}
		StartCoroutine(FetchScores()); // Single fetch
	}

	void Update()
	{
		if (needsPlayerPrefsSave)
		{
			Debug.Log("Saving PlayerPrefs");
			PlayerPrefs.Save();
			needsPlayerPrefsSave = false;
		}
	}

	public void SaveScore(int score)
	{
		if (isSavingScore)
		{
			Debug.LogWarning("Score save in progress, skipping");
			return;
		}
		currentScore = score;
		StartCoroutine(SubmitScore());
	}

	IEnumerator SubmitScore()
	{
		isSavingScore = true;
		WWWForm form = new WWWForm();
		form.AddField("player_name", playerName);
		form.AddField("email", playerEmail);
		form.AddField("score", currentScore);

		Debug.Log("Submitting score: " + currentScore);
		UnityWebRequest www = UnityWebRequest.Post(topscoresURL, form);
		yield return www.Send();

		if (www.isError)
		{
			Debug.LogError("Score submission failed: " + www.error);
		}
		else
		{
			try
			{
				string response = www.downloadHandler.text;
				Debug.Log("POST response: " + response);
				if (response.Contains("successfully"))
				{
					Debug.Log("Score submitted successfully");
					PlayerPrefs.SetInt("LastScore", currentScore);
					needsPlayerPrefsSave = true;
					StartCoroutine(FetchScores()); // Refresh scores
				}
				else
				{
					Debug.LogError("Score submission error: " + response);
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError("Exception in SubmitScore: " + e.Message);
			}
		}
		isSavingScore = false;
	}

	public void GetHighscores(System.Action<List<ScoreEntry>> callback)
	{
		if (cachedScores.Count > 0)
		{
			Debug.Log("Using cached highscores");
			callback(cachedScores);
		}
		else
		{
			StartCoroutine(FetchHighscores(callback));
		}
	}

	IEnumerator FetchHighscores(System.Action<List<ScoreEntry>> callback)
	{
		UnityWebRequest www = UnityWebRequest.Get(topscoresURL);
		yield return www.Send();

		if (www.isError)
		{
			Debug.LogError("Failed to fetch highscores: " + www.error);
			callback(cachedScores.Count > 0 ? cachedScores : null);
		}
		else
		{
			try
			{
				string response = www.downloadHandler.text;
				Debug.Log("Raw response: " + response);
				cachedScores = ParseCSV(response);
				if (cachedScores != null)
				{
					PlayerPrefs.SetString("CachedScores", response);
					needsPlayerPrefsSave = true;
					UpdateHighLowScores();
				}
				callback(cachedScores);
			}
			catch (System.Exception e)
			{
				Debug.LogError("Exception parsing highscores: " + e.Message);
				callback(cachedScores.Count > 0 ? cachedScores : null);
			}
		}
	}

	public int LowestHigh()
	{
		return lowestHighScore;
	}

	public int High()
	{
		return highestHighScore;
	}

	IEnumerator FetchScores()
	{
		UnityWebRequest www = UnityWebRequest.Get(topscoresURL);
		yield return www.Send();

		if (www.isError)
		{
			Debug.LogError("Failed to fetch scores: " + www.error);
		}
		else
		{
			try
			{
				string response = www.downloadHandler.text;
				Debug.Log("Raw response (FetchScores): " + response);
				cachedScores = ParseCSV(response);
				if (cachedScores != null)
				{
					PlayerPrefs.SetString("CachedScores", response);
					needsPlayerPrefsSave = true;
					UpdateHighLowScores();
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError("Exception in FetchScores: " + e.Message);
			}
		}
	}

	private void UpdateHighLowScores()
	{
		if (cachedScores == null || cachedScores.Count == 0)
		{
			lowestHighScore = 0;
			highestHighScore = 0;
			return;
		}
		highestHighScore = cachedScores[0].score; // First is highest (sorted DESC)
		lowestHighScore = cachedScores[0].score;
		for (int i = 1; i < cachedScores.Count && i < 20; i++)
		{
			if (cachedScores[i].score < lowestHighScore)
			{
				lowestHighScore = cachedScores[i].score;
			}
		}
		Debug.Log("Updated high/low scores: High=" + highestHighScore + ", Low=" + lowestHighScore);
	}

	private List<ScoreEntry> ParseCSV(string csv)
	{
		if (string.IsNullOrEmpty(csv))
		{
			Debug.LogWarning("Empty CSV response");
			return null;
		}

		List<ScoreEntry> scores = new List<ScoreEntry>();
		string[] lines = csv.Split('\n');
		if (lines.Length < 2)
		{
			Debug.LogWarning("Invalid CSV: Too few lines");
			return null;
		}

		for (int i = 1; i < lines.Length; i++)
		{
			string line = lines[i].Trim();
			if (string.IsNullOrEmpty(line)) continue;

			string[] parts = SplitCSVLine(line);
			if (parts.Length != 2)
			{
				Debug.LogWarning("Invalid CSV line: " + line);
				continue;
			}

			try
			{
				ScoreEntry entry = new ScoreEntry();
				entry.name = parts[0].Replace("\\,", ",");
				entry.score = int.Parse(parts[1]);
				entry.email = "";
				scores.Add(entry);
			}
			catch (System.Exception e)
			{
				Debug.LogWarning("Error parsing CSV line " + line + ": " + e.Message);
			}
		}

		return scores.Count > 0 ? scores : null;
	}

	private string[] SplitCSVLine(string line)
	{
		List<string> parts = new List<string>();
		string current = "";
		bool escaping = false;

		for (int i = 0; i < line.Length; i++)
		{
			char c = line[i];
			if (c == '\\' && i + 1 < line.Length && line[i + 1] == ',')
			{
				current += ',';
				i++;
			}
			else if (c == ',')
			{
				parts.Add(current);
				current = "";
			}
			else
			{
				current += c;
			}
		}

		if (current.Length > 0 || line.EndsWith(","))
		{
			parts.Add(current);
		}

		return parts.ToArray();
	}
}