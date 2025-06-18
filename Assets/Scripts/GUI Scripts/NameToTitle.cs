using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NameToTitle : MonoBehaviour {

	public Text title;


	void OnMouseEnter()
	{
		switch(name)
		{
		case "Pac-Man":
			title.color = Color.yellow;
			break;

		case "Blanko":
			title.color = Color.red;
			break;

		case "Fizzle":
			title.color = new Color(200f/255f, 214f/255f, 235f/255f);
			break;

		case "Misalingo":
			title.color = new Color(153f/255f, 186f/255f, 169f/255f);
			break;

		case "Clutterbug":
			title.color = new Color(221f/255f, 182f/255f, 139f/255f);
			break;
		}
		
		title.text = name;
	}

	void OnMouseExit()
	{
		title.text = "Dose Runner";
		title.color = Color.white;
	}
}
