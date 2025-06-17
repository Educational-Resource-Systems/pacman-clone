using UnityEngine;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
	public Text playerNameText; // Assign in Inspector

	void Start()
	{
		// Retrieve the player name from PlayerPrefs
		string playerName = PlayerPrefs.GetString("PlayerName", "Anonymous");
		Debug.Log("Player Name: " + playerName);
		playerNameText.text = "Player: " + playerName; // Use string concatenation
	}
}