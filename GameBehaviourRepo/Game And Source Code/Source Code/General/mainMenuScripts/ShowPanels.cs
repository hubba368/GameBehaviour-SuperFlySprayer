using UnityEngine;
using System.Collections;

public class ShowPanels : MonoBehaviour
{

	public GameObject optionsPanel;					
	public GameObject optionsTint;					
	public GameObject menuPanel;
	public GameObject pausePanel;

	public void ShowMenu()
	{
		menuPanel.SetActive (true);
	}


	public void HideMenu()
	{
		menuPanel.SetActive (false);
	}
	

	public void ShowPausePanel()
	{
		pausePanel.SetActive (true);
		//optionsTint.SetActive(true);
	}


	public void HidePausePanel()
	{
		pausePanel.SetActive (false);
		//optionsTint.SetActive(false);

	}
}
