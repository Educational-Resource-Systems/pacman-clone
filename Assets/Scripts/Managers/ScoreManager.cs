using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ScoreEntry
{
	public string name;
	public string email; // Included for compatibility, not used in display
	public int score;
}

public class ScoreManager : MonoBehaviour
{
	public static ScoreManager instance;
	private string topscoresURL = "https://ers-dev.com//ERS/_pacman/build5/topscores.php"; // Update with HTTPS URL
	private int currentScore = 0;
	private string playerName;
	private string playerEmail;
	private int lowestHighScore = 0;
	private int highestHighScore = 0;

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
			try
			{
				string response = www.downloadHandler.text;
				Debug.Log("POST response: " + response);
				if (response.Contains("successfully"))
				{
					Debug.Log("Score submitted successfully");
					StartCoroutine(FetchLowestHigh(delegate(int score) { lowestHighScore = score; }));
					StartCoroutine(FetchHighestHigh(delegate(int score) { highestHighScore = score; }));
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
			try
			{
				string response = www.downloadHandler.text;
				Debug.Log("Raw response: " + response);
				List<ScoreEntry> scores = ParseCSV(response);
				callback(scores);
			}
			catch (System.Exception e)
			{
				Debug.LogError("Exception parsing highscores: " + e.Message);
				callback(null);
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
			try
			{
				string response = www.downloadHandler.text;
				Debug.Log("Raw response (LowestHigh): " + response);
				List<ScoreEntry> scores = ParseCSV(response);
				if (scores == null || scores.Count == 0)
				{
					callback(0);
				}
				else
				{
					int lowestScore = scores[0].score;
					for (int i = 1; i < scores.Count && i < 20; i++)
					{
						if (scores[i].score < lowestScore)
						{
							lowestScore = scores[i].score;
						}
					}
					callback(lowestScore);
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError("Exception in FetchLowestHigh: " + e.Message);
				callback(0);
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
			try
			{
				string response = www.downloadHandler.text;
				Debug.Log("Raw response (HighestHigh): " + response);
				List<ScoreEntry> scores = ParseCSV(response);
				if (scores == null || scores.Count == 0)
				{
					callback(0);
				}
				else
				{
					callback(scores[0].score);
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError("Exception in FetchHighestHigh: " + e.Message);
				callback(0);
			}
		}
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
		if (lines.Length < 2) // Expect at least header + 1 data row
		{
			Debug.LogWarning("Invalid CSV: Too few lines");
			return null;
		}

		// Skip header (name,score)
		for (int i = 1; i < lines.Length; i++)
		{
			string line = lines[i].Trim();
			if (string.IsNullOrEmpty(line))
				continue;

			// Handle escaped commas in names
			string[] parts = SplitCSVLine(line);
			if (parts.Length != 2)
			{
				Debug.LogWarning("Invalid CSV line: " + line);
				continue;
			}

			try
			{
				ScoreEntry entry = new ScoreEntry();
				entry.name = parts[0].Replace("\\,", ","); // Unescape commas
				entry.score = int.Parse(parts[1]);
				entry.email = ""; // Not used in GET response
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