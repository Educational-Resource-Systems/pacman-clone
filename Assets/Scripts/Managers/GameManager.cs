using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	// Game variables
	public static int lives = 3;
	public static int score = 0;
	public static int Level = 0;
	public static GameState gameState;
	public float scareLength = 7f;
	public float SpeedPerLevel = 0.1f; // Example value, adjust as needed

	public enum GameState { Init, Game, Dead, Scores }

	private GameObject pacman;
	private GameObject blinky;
	private GameObject pinky;
	private GameObject inky;
	private GameObject clyde;
	private GameGUINavigation gui;
	public static bool scared;
	private float _timeToCalm;

	// Singleton implementation
	private static GameManager _instance;

	public static GameManager instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = GameObject.FindObjectOfType<GameManager>();
				if (_instance != null)
				{
					DontDestroyOnLoad(_instance.gameObject);
				}
			}
			return _instance;
		}
	}

	void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else if (this != _instance)
		{
			Debug.Log("Destroying duplicate GameManager");
			Destroy(this.gameObject);
			return;
		}

		AssignGhosts();
	}

	void Start()
	{
		Debug.Log("GameManager Start, initial score: " + score + ", gameState: " + gameState);
		if (gameState != GameState.Dead && gameState != GameState.Scores)
		{
			gameState = GameState.Init;
		}
	}

	void OnLevelWasLoaded()
	{
		Debug.Log("Level " + Level + " Loaded!, gameState: " + gameState);
		AssignGhosts();
		ResetVariables();

		// Adjust speeds
		if (blinky != null) blinky.GetComponent<GhostMove>().speed += Level * SpeedPerLevel;
		if (pinky != null) pinky.GetComponent<GhostMove>().speed += Level * SpeedPerLevel;
		if (inky != null) inky.GetComponent<GhostMove>().speed += Level * SpeedPerLevel;
		if (clyde != null) clyde.GetComponent<GhostMove>().speed += Level * SpeedPerLevel;
		if (pacman != null) pacman.GetComponent<PlayerController>().speed += Level * SpeedPerLevel / 2;
	}

	private void ResetVariables()
	{
		_timeToCalm = 0.0f;
		scared = false;
		PlayerController.killstreak = 0;
	}

	void Update()
	{
		if (scared && _timeToCalm <= Time.time)
		{
			CalmGhosts();
		}
	}

	public void ResetScene()
	{
		Debug.Log("Resetting scene: " + SceneManager.GetActiveScene().name + ", lives: " + lives + ", gameState: " + gameState);
		CalmGhosts();
		gameState = GameState.Dead; // Prevent gameplay during reset
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void ToggleScare()
	{
		if (!scared) ScareGhosts();
		else CalmGhosts();
	}

	public void ScareGhosts()
	{
		if (scared) return;
		scared = true;
		if (blinky != null) blinky.GetComponent<GhostMove>().Frighten();
		if (pinky != null) pinky.GetComponent<GhostMove>().Frighten();
		if (inky != null) inky.GetComponent<GhostMove>().Frighten();
		if (clyde != null) clyde.GetComponent<GhostMove>().Frighten();
		_timeToCalm = Time.time + scareLength;
		Debug.Log("Ghosts Scared");
	}

	public void CalmGhosts()
	{
		if (!scared) return;
		scared = false;
		if (blinky != null) blinky.GetComponent<GhostMove>().Calm();
		if (pinky != null) pinky.GetComponent<GhostMove>().Calm();
		if (inky != null) inky.GetComponent<GhostMove>().Calm();
		if (clyde != null) clyde.GetComponent<GhostMove>().Calm();
		PlayerController.killstreak = 0;
		Debug.Log("Ghosts Calmed");
	}

	private void AssignGhosts()
	{
		clyde = GameObject.Find("clyde");
		pinky = GameObject.Find("pinky");
		inky = GameObject.Find("inky");
		blinky = GameObject.Find("blinky");
		pacman = GameObject.Find("pacman");
		gui = GameObject.FindObjectOfType<GameGUINavigation>();

		if (clyde == null) Debug.LogWarning("Clyde is NULL");
		if (pinky == null) Debug.LogWarning("Pinky is NULL");
		if (inky == null) Debug.LogWarning("Inky is NULL");
		if (blinky == null) Debug.LogWarning("Blinky is NULL");
		if (pacman == null) Debug.LogWarning("Pacman is NULL");
		if (gui == null) Debug.LogWarning("GUI handle is NULL");
	}

	public void LoseLife()
	{
		Debug.Log("Losing life, lives: " + lives + ", score: " + score + ", gameState: " + gameState);
		lives--;
		gameState = GameState.Dead;
	}

	public static void DestroySelf()
	{
		Debug.Log("Destroying GameManager, resetting score and lives");
		score = 0;
		Level = 0;
		lives = 3;
		gameState = GameState.Init;
		if (_instance != null)
		{
			Destroy(_instance.gameObject);
			_instance = null;
		}
	}

	public void ResetGame()
	{
		Debug.Log("Resetting game state, score: " + score + " -> 0, lives: " + lives + " -> 3, gameState: " + gameState);
		score = 0;
		lives = 3;
		Level = 0;
		gameState = GameState.Init;
		CalmGhosts();
		if (gui != null) gui.H_ShowReadyScreen();
	}
}