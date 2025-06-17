using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NameInputManager : MonoBehaviour
{
	public InputField nameInputField; // Assign in Inspector
	public Button submitButton;       // Assign in Inspector

	void Start()
	{
		submitButton.onClick.AddListener(OnSubmit);
	}

	void OnSubmit()
	{
		string playerName = nameInputField.text.Trim();
		if (!string.IsNullOrEmpty(playerName))
		{
			// Store player name in PlayerPrefs
			PlayerPrefs.SetString("PlayerName", playerName);
			PlayerPrefs.Save();
			// Load the main game scene
			SceneManager.LoadScene("menu"); // Replace "Game" with your main game scene name
		}
		else
		{
			Debug.LogWarning("Please enter a valid name!");
			// Optionally add UI feedback (e.g., error text)
		}
	}
}