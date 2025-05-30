using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking; // For UnityWebRequest

public class ScoreManager : MonoBehaviour
{
    private string TopScoresURL = "https://ers-dev.com/ERS/_pacman/topscores.php";
    private string username;
    private int _highscore;
    private int _lowestHigh;
    private bool _scoresRead;
    private bool _isTableFound;

    public class Score
    {
        public string name { get; set; }
        public int score { get; set; }

        public Score(string n, int s)
        {
            name = n;
            score = s;
        }

        public Score(string n, string s)
        {
            name = n;
            score = Int32.Parse(s);
        }
    }

    List<Score> scoreList = new List<Score>(10);

    void OnLevelWasLoaded(int level)
    {
        if (level == 2) StartCoroutine(ReadScoresFromDB()); // Start reading scores when scores scene is loaded
        if (level == 1) _lowestHigh = _highscore = 99999;
    }

    IEnumerator GetHighestScore()
    {
        Debug.Log("GETTING HIGHEST SCORE");
        float timeOut = Time.time + 4;
        while (!_scoresRead)
        {
            yield return new WaitForSeconds(0.01f);
            if (Time.time > timeOut)
            {
                Debug.Log("Timed out fetching highest score");
                break;
            }
        }

        if (scoreList.Count > 0)
        {
            _highscore = scoreList[0].score;
            _lowestHigh = scoreList[scoreList.Count - 1].score;
        }
    }

    IEnumerator UpdateGUIText()
    {
        float timeOut = Time.time + 4;
        while (!_scoresRead)
        {
            yield return new WaitForSeconds(0.01f);
            if (Time.time > timeOut)
            {
                Debug.Log("Timeout fetching scores");
                scoreList.Clear();
                scoreList.Add(new Score("DATABASE TEMPORARILY UNAVAILABLE", 999999));
                break;
            }
        }

        GameObject scoresText = GameObject.FindGameObjectWithTag("ScoresText");
        if (scoresText != null)
        {
            scoresText.GetComponent<Scores>().UpdateGUIText(scoreList);
        }
        else
        {
            Debug.LogError("ScoresText GameObject not found!");
        }
    }

   IEnumerator ReadScoresFromDB()
{
    _scoresRead = false;
    using (UnityWebRequest www = UnityWebRequest.Get(TopScoresURL))
    {
        yield return www.Send();

        if (www.isError)
        {
            scoreList.Add(new Score(www.error, 1234));
            StartCoroutine(UpdateGUIText());
        }
        else
        {
            string responseText = www.downloadHandler.text;

            string[] textlist = responseText.Split(new string[] { "\n", "\t" }, StringSplitOptions.RemoveEmptyEntries);

            if (textlist.Length == 1)
            {
                scoreList.Clear();
                scoreList.Add(new Score(textlist[0], -123));
            }
            else
            {
                string[] Names = new string[Mathf.FloorToInt(textlist.Length / 2)];
                string[] Scores = new string[Names.Length];

                for (int i = 0; i < textlist.Length; i++)
                {
                    if (i % 2 == 0)
                        Names[Mathf.FloorToInt(i / 2)] = textlist[i];
                    else
                        Scores[Mathf.FloorToInt(i / 2)] = textlist[i];
                }

                scoreList.Clear();
                for (int i = 0; i < Names.Length; i++)
                {
                    try
                    {
                        scoreList.Add(new Score(Names[i], Scores[i]));
                    }
                    catch (Exception e)
                    {
                        // Handle error
                    }
                }

                _scoresRead = true;
            }
            StartCoroutine(UpdateGUIText());
        }
    }
}

    public int High()
    {
        return _highscore;
    }

    public int LowestHigh()
    {
        return _lowestHigh;
    }
}