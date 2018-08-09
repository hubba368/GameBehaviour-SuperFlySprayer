using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class QuitApplication : MonoBehaviour
{
    private ShowPanels showPanels;
    private StartOptions ui;

    public void Quit()
	{
		//If we are running in a standalone build of the game
	#if UNITY_STANDALONE
		//Quit the application
		Application.Quit();
	#endif

		//If we are running in the editor
	#if UNITY_EDITOR
		//Stop playing the scene
		UnityEditor.EditorApplication.isPlaying = false;
	#endif
	}

    public void ReturnToMenu()
    {
        showPanels = GetComponent<ShowPanels>();
        ui = GetComponent<StartOptions>();
        showPanels.HidePausePanel();
        showPanels.ShowMenu();
        ui.changeScenes = false;
        ui.inMainMenu = true;

        SceneManager.LoadScene(0);
    }
}
