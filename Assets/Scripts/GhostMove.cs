using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GhostMove : MonoBehaviour
{
	// Navigation variables
	private Vector3 waypoint;
	private Queue<Vector3> waypoints;
	public Vector3 _direction;
	public Vector3 direction
	{
		get { return _direction; }
		set
		{
			_direction = value;
			Vector3 pos = new Vector3((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
			waypoint = pos + _direction;
			Debug.Log(gameObject.name + ": Waypoint set to (" + waypoint.x + ", " + waypoint.y + "), direction: " + _direction);
		}
	}

	public float speed = 0.3f;

	// Ghost mode variables
	public float scatterLength = 5f;
	public float waitLength = 0.0f;
	private float timeToEndScatter;
	private float timeToEndWait;
	enum State { Wait, Init, Scatter, Chase, Run }
	private State state;
	private Vector3 _startPos;
	private float _timeToWhite;
	private float _timeToToggleWhite;
	private float _toggleInterval;
	private bool isWhite = false;

	// Handles
	public GameGUINavigation GUINav;
	public PlayerController pacman;
	private GameManager _gm;

	public float DISTANCE;

	void Start()
	{
		_gm = GameObject.Find("Game Manager").GetComponent<GameManager>();
		if (_gm == null) Debug.LogError(gameObject.name + ": GameManager not found");
		_toggleInterval = _gm.scareLength * 0.33f * 0.20f;
		InitializeGhost();
	}

	void FixedUpdate()
	{
		if (GameManager.gameState != GameManager.GameState.Game)
		{
			GetComponent<Rigidbody2D>().velocity = Vector2.zero;
			Debug.Log(gameObject.name + ": Movement stopped, gameState: " + GameManager.gameState);
			return;
		}

		DISTANCE = Vector3.Distance(transform.position, waypoint);

		try
		{
			animate();

			switch (state)
			{
			case State.Wait:
				Wait();
				break;
			case State.Init:
				Init();
				break;
			case State.Scatter:
				Scatter();
				break;
			case State.Chase:
				ChaseAI();
				break;
			case State.Run:
				RunAway();
				break;
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError(gameObject.name + ": FixedUpdate error: " + e.Message);
			_direction = GetFallbackDirection();
			UpdateWaypoint();
		}
	}

	public void InitializeGhost()
	{
		_startPos = getStartPosAccordingToName();
		waypoint = transform.position;
		state = State.Wait;
		timeToEndWait = Time.time + waitLength + (GUINav != null ? GUINav.initialDelay : 0f);
		InitializeWaypoints(state);
		Debug.Log(gameObject.name + ": Initialized at (" + _startPos.x + ", " + _startPos.y + ")");
	}

	public void InitializeGhost(Vector3 pos)
	{
		transform.position = pos;
		waypoint = transform.position;
		state = State.Wait;
		timeToEndWait = Time.time + waitLength + (GUINav != null ? GUINav.initialDelay : 0f);
		InitializeWaypoints(state);
		Debug.Log(gameObject.name + ": Initialized at (" + pos.x + ", " + pos.y + ")");
	}

	private void InitializeWaypoints(State st)
	{
		string data = "";
		switch (gameObject.name.ToLower())
		{
		case "blinky":
			data = @"22 20
22 26

27 26
27 30
22 30
22 26";
			break;
		case "pinky":
			data = @"14.5 17
14 17
14 20
7 20

7 26
7 30
2 30
2 26";
			break;
		case "inky":
			data = @"16.5 17
15 17
15 20
22 20

22 8
19 8
19 5
16 5
16 2
27 2
27 5
22 5";
			break;
		case "clyde":
			data = @"12.5 17
14 17
14 20
7 20

7 8
7 5
2 5
2 2
13 2
13 5
10 5
10 8";
			break;
		}

		waypoints = new Queue<Vector3>();
		Vector3 wp;

		if (st == State.Init)
		{
			using (StringReader reader = new StringReader(data))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (line.Length == 0) break;
					string[] values = line.Split(' ');
					float x = float.Parse(values[0]);
					float y = float.Parse(values[1]);
					wp = new Vector3(x, y, 0);
					waypoints.Enqueue(wp);
				}
			}
		}

		if (st == State.Scatter)
		{
			bool scatterWps = false;
			using (StringReader reader = new StringReader(data))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (line.Length == 0)
					{
						scatterWps = true;
						continue;
					}
					if (scatterWps)
					{
						string[] values = line.Split(' ');
						int x = Int32.Parse(values[0]);
						int y = Int32.Parse(values[1]);
						wp = new Vector3(x, y, 0);
						waypoints.Enqueue(wp);
					}
				}
			}
		}

		if (st == State.Wait)
		{
			Vector3 pos = transform.position;
			if (gameObject.name.ToLower() == "inky" || gameObject.name.ToLower() == "clyde")
			{
				waypoints.Enqueue(new Vector3(pos.x, pos.y - 0.5f, 0f));
				waypoints.Enqueue(new Vector3(pos.x, pos.y + 0.5f, 0f));
			}
			else
			{
				waypoints.Enqueue(new Vector3(pos.x, pos.y + 0.5f, 0f));
				waypoints.Enqueue(new Vector3(pos.x, pos.y - 0.5f, 0f));
			}
		}
	}

	private Vector3 getStartPosAccordingToName()
	{
		switch (gameObject.name.ToLower())
		{
		case "blinky": return new Vector3(15f, 20f, 0f);
		case "pinky": return new Vector3(14.5f, 17f, 0f);
		case "inky": return new Vector3(16.5f, 17f, 0f);
		case "clyde": return new Vector3(12.5f, 17f, 0f);
		default: return new Vector3();
		}
	}

	void animate()
	{
		Vector3 dir = waypoint - transform.position;
		GetComponent<Animator>().SetFloat("DirX", dir.x);
		GetComponent<Animator>().SetFloat("DirY", dir.y);
		GetComponent<Animator>().SetBool("Run", false);
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.name == "pacman")
		{
			if (state == State.Run)
			{
				Calm();
				InitializeGhost(_startPos);
				pacman.UpdateScore();
			}
			else
			{
				_gm.LoseLife();
			}
		}
	}

	void Wait()
	{
		if (Time.time >= timeToEndWait)
		{
			state = State.Init;
			waypoints.Clear();
			InitializeWaypoints(state);
		}
		MoveToWaypoint(true);
	}

	void Init()
	{
		_timeToWhite = 0;
		if (waypoints.Count == 0)
		{
			state = State.Scatter;
			string name = GetComponent<SpriteRenderer>().sprite.name;
			if (name[name.Length - 1] == '0' || name[name.Length - 1] == '1') direction = Vector3.right;
			if (name[name.Length - 1] == '2' || name[name.Length - 1] == '3') direction = Vector3.left;
			if (name[name.Length - 1] == '4' || name[name.Length - 1] == '5') direction = Vector3.up;
			if (name[name.Length - 1] == '6' || name[name.Length - 1] == '7') direction = Vector3.down;
			InitializeWaypoints(state);
			timeToEndScatter = Time.time + scatterLength;
			return;
		}
		MoveToWaypoint();
	}

	void Scatter()
	{
		if (Time.time >= timeToEndScatter)
		{
			waypoints.Clear();
			state = State.Chase;
			return;
		}
		MoveToWaypoint(true);
	}

	void ChaseAI()
	{
		try
		{
			if (Vector3.Distance(transform.position, waypoint) > 0.0001f)
			{
				Vector2 p = Vector2.MoveTowards(transform.position, waypoint, speed);
				GetComponent<Rigidbody2D>().MovePosition(p);
			}
			else
			{
				AI ai = GetComponent<AI>();
				if (ai == null)
				{
					Debug.LogError(gameObject.name + ": AI component missing in ChaseAI");
					_direction = GetFallbackDirection();
					UpdateWaypoint();
					return;
				}

				Debug.Log(gameObject.name + ": Calling AILogic, current position: (" + transform.position.x + ", " + transform.position.y + ")");
				ai.AILogic();
				if (_direction == Vector3.zero)
				{
					Debug.LogWarning(gameObject.name + ": AILogic returned zero direction, using fallback");
					_direction = GetFallbackDirection();
				}
				UpdateWaypoint();
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError(gameObject.name + ": ChaseAI error: " + e.Message);
			_direction = GetFallbackDirection();
			UpdateWaypoint();
		}
	}

	void RunAway()
	{
		try
		{
			GetComponent<Animator>().SetBool("Run", true);
			if (Time.time >= _timeToWhite && Time.time >= _timeToToggleWhite) ToggleBlueWhite();

			if (Vector3.Distance(transform.position, waypoint) > 0.0001f)
			{
				Vector2 p = Vector2.MoveTowards(transform.position, waypoint, speed);
				GetComponent<Rigidbody2D>().MovePosition(p);
			}
			else
			{
				AI ai = GetComponent<AI>();
				if (ai == null)
				{
					Debug.LogError(gameObject.name + ": AI component missing in RunAway");
					_direction = GetFallbackDirection();
					UpdateWaypoint();
					return;
				}

				Debug.Log(gameObject.name + ": Calling RunLogic, current position: (" + transform.position.x + ", " + transform.position.y + ")");
				ai.RunLogic();
				if (_direction == Vector3.zero)
				{
					Debug.LogWarning(gameObject.name + ": RunLogic returned zero direction, using fallback");
					_direction = GetFallbackDirection();
				}
				UpdateWaypoint();
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError(gameObject.name + ": RunAway error: " + e.Message);
			_direction = GetFallbackDirection();
			UpdateWaypoint();
		}
	}

	void MoveToWaypoint(bool loop = false)
	{
		try
		{
			if (waypoints.Count == 0)
			{
				Debug.LogWarning(gameObject.name + ": Waypoints queue empty in MoveToWaypoint");
				return;
			}

			waypoint = waypoints.Peek();
			if (Vector3.Distance(transform.position, waypoint) > 0.0001f)
			{
				_direction = Vector3.Normalize(waypoint - transform.position);
				Vector2 p = Vector2.MoveTowards(transform.position, waypoint, speed);
				GetComponent<Rigidbody2D>().MovePosition(p);
			}
			else
			{
				if (loop) waypoints.Enqueue(waypoints.Dequeue());
				else waypoints.Dequeue();
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError(gameObject.name + ": MoveToWaypoint error: " + e.Message);
			_direction = GetFallbackDirection();
			UpdateWaypoint();
		}
	}

	public void Frighten()
	{
		state = State.Run;
		_direction *= -1;
		_timeToWhite = Time.time + _gm.scareLength * 0.66f;
		_timeToToggleWhite = _timeToWhite;
		GetComponent<Animator>().SetBool("Run_White", false);
	}

	public void Calm()
	{
		if (state != State.Run) return;
		waypoints.Clear();
		state = State.Chase;
		_timeToToggleWhite = 0;
		_timeToWhite = 0;
		GetComponent<Animator>().SetBool("Run_White", false);
		GetComponent<Animator>().SetBool("Run", false);
	}

	void ToggleBlueWhite()
	{
		isWhite = !isWhite;
		GetComponent<Animator>().SetBool("Run_White", isWhite);
		_timeToToggleWhite = Time.time + _toggleInterval;
	}

	private void UpdateWaypoint()
	{
		waypoint = (Vector2)transform.position + (Vector2)_direction;
		Debug.Log(gameObject.name + ": Updated waypoint to (" + waypoint.x + ", " + waypoint.y + ")");
	}

	private Vector3 GetFallbackDirection()
	{
		Vector3[] directions = { Vector3.up, Vector3.right, Vector3.down, Vector3.left };
		foreach (Vector3 dir in directions)
		{
			Vector2 pos = transform.position;
			Vector2 target = pos + (Vector2)dir;
			RaycastHit2D hit = Physics2D.Linecast(pos, target);
			if (hit.collider == null || hit.collider.tag != "Wall")
			{
				Debug.Log(gameObject.name + ": Fallback direction: " + dir);
				return dir;
			}
		}
		Debug.LogWarning(gameObject.name + ": No fallback direction available");
		return Vector3.zero;
	}
}