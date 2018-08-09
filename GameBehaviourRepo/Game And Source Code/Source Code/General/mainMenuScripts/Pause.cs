using UnityEngine;
using System.Collections;

public class Pause : MonoBehaviour
{


	private ShowPanels showPanels;						//Reference to the ShowPanels script used to hide and show UI panels
	private bool isPaused;								//Bool to check if the game is paused or not
	private StartOptions startScript;					
	

	void Awake()
	{
		showPanels = GetComponent<ShowPanels> ();
		startScript = GetComponent<StartOptions> ();
	}


	void Update ()
    {

		if (Input.GetButtonDown ("Cancel") && !isPaused && !startScript.inMainMenu) 
		{
			DoPause();
		} 
		//If the button is pressed and the game is paused and not in main menu
		else if (Input.GetButtonDown ("Cancel") && isPaused && !startScript.inMainMenu) 
		{
			UnPause ();
		}
	
	}


	public void DoPause()
	{
		isPaused = true;
		Time.timeScale = 0;
		showPanels.ShowPausePanel ();
	}


	public void UnPause()
	{
		isPaused = false;
		Time.timeScale = 1;
		showPanels.HidePausePanel ();
	}


}
