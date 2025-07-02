using UnityEngine;
using System.Collections.Generic;

public class AI : MonoBehaviour
{
	public Transform target;
	private List<TileManager.Tile> tiles = new List<TileManager.Tile>();
	private TileManager manager;
	public GhostMove ghost;
	public TileManager.Tile nextTile = null;
	public TileManager.Tile targetTile;
	private TileManager.Tile currentTile;

	void Awake()
	{
		manager = GameObject.Find("Game Manager").GetComponent<TileManager>();
		tiles = manager.tiles;

		if (ghost == null) Debug.LogError(gameObject.name + ": Ghost component not found");
		if (manager == null) Debug.LogError(gameObject.name + ": Game Manager (TileManager) not found");
		if (target == null) Debug.LogError(gameObject.name + ": Target (Pacman) not found");
	}

	public void AILogic()
	{
		try
		{
			if (manager == null || tiles == null || tiles.Count == 0 || ghost == null || target == null)
			{
				Debug.LogError(gameObject.name + ": Invalid setup (manager, tiles, ghost, or target null)");
				ghost.direction = GetFallbackDirection();
				return;
			}

			// Get current tile
			Vector3 currentPos = new Vector3(transform.position.x + 0.499f, transform.position.y + 0.499f);
			int index = manager.Index((int)currentPos.x, (int)currentPos.y);
			if (index < 0 || index >= tiles.Count)
			{
				Debug.LogWarning(gameObject.name + ": Invalid tile index at position (" + currentPos.x + ", " + currentPos.y + ")");
				ghost.direction = GetFallbackDirection();
				return;
			}
			currentTile = tiles[index];
			Debug.Log(gameObject.name + ": Current tile (" + currentTile.x + ", " + currentTile.y + "), isIntersection: " + currentTile.isIntersection);

			targetTile = GetTargetTilePerGhost();
			if (targetTile == null)
			{
				Debug.LogWarning(gameObject.name + ": Target tile is null");
				ghost.direction = GetFallbackDirection();
				return;
			}

			// Get next tile according to direction
			nextTile = GetNextTile(currentPos);
			if (nextTile == null)
			{
				Debug.LogWarning(gameObject.name + ": Next tile is null at position (" + currentPos.x + ", " + currentPos.y + ")");
				ghost.direction = GetFallbackDirection();
				return;
			}

			if (nextTile.occupied || currentTile.isIntersection)
			{
				// If we bump into a wall
				if (nextTile.occupied && !currentTile.isIntersection)
				{
					if (ghost.direction.x != 0)
					{
						ghost.direction = currentTile.down == null ? Vector3.up : Vector3.down;
					}
					else if (ghost.direction.y != 0)
					{
						ghost.direction = currentTile.left == null ? Vector3.right : Vector3.left;
					}
					Debug.Log(gameObject.name + ": Wall hit, new direction: " + ghost.direction);
				}

				// If at intersection
				if (currentTile.isIntersection)
				{
					List<TileManager.Tile> availableTiles = GetAvailableTiles();
					if (availableTiles.Count == 0)
					{
						Debug.LogWarning(gameObject.name + ": No available tiles at intersection (" + currentTile.x + ", " + currentTile.y + ")");
						ghost.direction = GetFallbackDirection();
						return;
					}

					float minDist = float.MaxValue;
					TileManager.Tile chosenTile = null;
					foreach (TileManager.Tile tile in availableTiles)
					{
						float dist = manager.distance(tile, targetTile);
						if (dist < minDist)
						{
							minDist = dist;
							chosenTile = tile;
						}
					}

					if (chosenTile != null)
					{
						ghost.direction = Vector3.Normalize(new Vector3(chosenTile.x - currentTile.x, chosenTile.y - currentTile.y, 0));
						Debug.Log(gameObject.name + ": Intersection, chose tile (" + chosenTile.x + ", " + chosenTile.y + "), direction: " + ghost.direction);
					}
					else
					{
						ghost.direction = GetFallbackDirection();
					}
				}
			}
			else
			{
				ghost.direction = ghost.direction; // Maintain direction
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError(gameObject.name + ": AILogic error: " + e.Message);
			ghost.direction = GetFallbackDirection();
		}
	}

	public void RunLogic()
	{
		try
		{
			if (manager == null || tiles == null || tiles.Count == 0 || ghost == null || target == null)
			{
				Debug.LogError(gameObject.name + ": Invalid setup in RunLogic");
				ghost.direction = GetFallbackDirection();
				return;
			}

			// Get current tile
			Vector3 currentPos = new Vector3(transform.position.x + 0.499f, transform.position.y + 0.499f);
			int index = manager.Index((int)currentPos.x, (int)currentPos.y);
			if (index < 0 || index >= tiles.Count)
			{
				Debug.LogWarning(gameObject.name + ": Invalid tile index in RunLogic at (" + currentPos.x + ", " + currentPos.y + ")");
				ghost.direction = GetFallbackDirection();
				return;
			}
			currentTile = tiles[index];

			// Get next tile
			nextTile = GetNextTile(currentPos);
			if (nextTile == null)
			{
				Debug.LogWarning(gameObject.name + ": Next tile is null in RunLogic at (" + currentPos.x + ", " + currentPos.y + ")");
				ghost.direction = GetFallbackDirection();
				return;
			}

			if (nextTile.occupied || currentTile.isIntersection)
			{
				// If we bump into a wall
				if (nextTile.occupied && !currentTile.isIntersection)
				{
					if (ghost.direction.x != 0)
					{
						ghost.direction = currentTile.down == null ? Vector3.up : Vector3.down;
					}
					else if (ghost.direction.y != 0)
					{
						ghost.direction = currentTile.left == null ? Vector3.right : Vector3.left;
					}
					Debug.Log(gameObject.name + ": Wall hit in RunLogic, new direction: " + ghost.direction);
				}

				// If at intersection
				if (currentTile.isIntersection)
				{
					List<TileManager.Tile> availableTiles = GetAvailableTiles();
					if (availableTiles.Count == 0)
					{
						Debug.LogWarning(gameObject.name + ": No available tiles in RunLogic at intersection (" + currentTile.x + ", " + currentTile.y + ")");
						ghost.direction = GetFallbackDirection();
						return;
					}

					int rand = Random.Range(0, availableTiles.Count);
					TileManager.Tile chosenTile = availableTiles[rand];
					ghost.direction = Vector3.Normalize(new Vector3(chosenTile.x - currentTile.x, chosenTile.y - currentTile.y, 0));
					Debug.Log(gameObject.name + ": RunLogic chose tile (" + chosenTile.x + ", " + chosenTile.y + "), direction: " + ghost.direction);
				}
			}
			else
			{
				ghost.direction = ghost.direction; // Maintain direction
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError(gameObject.name + ": RunLogic error: " + e.Message);
			ghost.direction = GetFallbackDirection();
		}
	}

	private TileManager.Tile GetNextTile(Vector3 currentPos)
	{
		try
		{
			int index = -1;
			if (ghost.direction.x > 0) index = manager.Index((int)(currentPos.x + 1), (int)currentPos.y);
			else if (ghost.direction.x < 0) index = manager.Index((int)(currentPos.x - 1), (int)currentPos.y);
			else if (ghost.direction.y > 0) index = manager.Index((int)currentPos.x, (int)(currentPos.y + 1));
			else if (ghost.direction.y < 0) index = manager.Index((int)currentPos.x, (int)(currentPos.y - 1));

			if (index >= 0 && index < tiles.Count)
			{
				return tiles[index];
			}
			Debug.LogWarning(gameObject.name + ": Invalid next tile index: " + index);
			return null;
		}
		catch (System.Exception e)
		{
			Debug.LogError(gameObject.name + ": GetNextTile error: " + e.Message);
			return null;
		}
	}

	private List<TileManager.Tile> GetAvailableTiles()
	{
		List<TileManager.Tile> availableTiles = new List<TileManager.Tile>();
		if (currentTile.up != null && !currentTile.up.occupied && !(ghost.direction.y < 0)) availableTiles.Add(currentTile.up);
		if (currentTile.down != null && !currentTile.down.occupied && !(ghost.direction.y > 0)) availableTiles.Add(currentTile.down);
		if (currentTile.left != null && !currentTile.left.occupied && !(ghost.direction.x > 0)) availableTiles.Add(currentTile.left);
		if (currentTile.right != null && !currentTile.right.occupied && !(ghost.direction.x < 0)) availableTiles.Add(currentTile.right);
		return availableTiles;
	}

	private Vector3 GetFallbackDirection()
	{
		Vector3[] directions = { Vector3.up, Vector3.right, Vector3.down, Vector3.left };
		foreach (Vector3 dir in directions)
		{
			Vector2 pos = transform.position;
			int index = -1;
			if (dir == Vector3.up) index = manager.Index((int)(pos.x + 0.499f), (int)(pos.y + 1.499f));
			else if (dir == Vector3.right) index = manager.Index((int)(pos.x + 1.499f), (int)(pos.y + 0.499f));
			else if (dir == Vector3.down) index = manager.Index((int)(pos.x + 0.499f), (int)(pos.y - 0.499f));
			else if (dir == Vector3.left) index = manager.Index((int)(pos.x - 0.499f), (int)(pos.y + 0.499f));

			if (index >= 0 && index < tiles.Count && !tiles[index].occupied)
			{
				Debug.Log(gameObject.name + ": Fallback direction: " + dir);
				return dir;
			}
		}
		Debug.LogWarning(gameObject.name + ": No fallback direction available");
		return Vector3.zero;
	}

	private TileManager.Tile GetTargetTilePerGhost()
	{
		try
		{
			if (target == null || manager == null || tiles == null || tiles.Count == 0)
			{
				Debug.LogError(gameObject.name + ": Invalid setup in GetTargetTilePerGhost");
				return null;
			}

			Vector3 targetPos;
			TileManager.Tile targetTile;
			Vector3 dir;

			switch (gameObject.name.ToLower())
			{
			case "blinky":
				targetPos = new Vector3(target.position.x + 0.499f, target.position.y + 0.499f);
				int indexBlinky = manager.Index((int)targetPos.x, (int)targetPos.y);
				if (indexBlinky < 0 || indexBlinky >= tiles.Count) return null;
				targetTile = tiles[indexBlinky];
				break;
			case "pinky":
				dir = target.GetComponent<PlayerController>().getDir();
				targetPos = new Vector3(target.position.x + 0.499f, target.position.y + 0.499f) + 4 * dir;
				if (dir == Vector3.up) targetPos -= new Vector3(4, 0, 0);
				int indexPinky = manager.Index((int)targetPos.x, (int)targetPos.y);
				if (indexPinky < 0 || indexPinky >= tiles.Count) return null;
				targetTile = tiles[indexPinky];
				break;
			case "inky":
				dir = target.GetComponent<PlayerController>().getDir();
				GameObject blinky = GameObject.Find("blinky");
				if (blinky == null) return null;
				Vector3 blinkyPos = blinky.transform.position;
				Vector3 ambushVector = target.position + 2 * dir - blinkyPos;
				targetPos = new Vector3(target.position.x + 0.499f, target.position.y + 0.499f) + 2 * dir + ambushVector;
				int indexInky = manager.Index((int)targetPos.x, (int)targetPos.y);
				if (indexInky < 0 || indexInky >= tiles.Count) return null;
				targetTile = tiles[indexInky];
				break;
			case "clyde":
				targetPos = new Vector3(target.position.x + 0.499f, target.position.y + 0.499f);
				int indexClyde = manager.Index((int)targetPos.x, (int)targetPos.y);
				if (indexClyde < 0 || indexClyde >= tiles.Count) return null;
				targetTile = tiles[indexClyde];
				if (manager.distance(targetTile, currentTile) < 9)
				{
					indexClyde = manager.Index(0, 2);
					if (indexClyde < 0 || indexClyde >= tiles.Count) return null;
					targetTile = tiles[indexClyde];
				}
				break;
			default:
				Debug.LogWarning(gameObject.name + ": Unknown ghost name");
				return null;
			}
			return targetTile;
		}
		catch (System.Exception e)
		{
			Debug.LogError(gameObject.name + ": GetTargetTilePerGhost error: " + e.Message);
			return null;
		}
	}
}