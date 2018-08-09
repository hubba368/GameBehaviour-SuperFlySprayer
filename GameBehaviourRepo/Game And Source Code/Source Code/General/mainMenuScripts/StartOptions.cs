using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;


public class StartOptions : MonoBehaviour
{

    public static StartOptions ui;

	public int sceneToStart = 1;
	public bool changeScenes;			


	[HideInInspector] public bool inMainMenu = true;	
	[HideInInspector] public Animator animColorFade; 					
	[HideInInspector] public Animator animMenuAlpha;					
	 public AnimationClip fadeColorAnimationClip;		
	[HideInInspector] public AnimationClip fadeAlphaAnimationClip;	

								
	private ShowPanels showPanels;							

	
	void Awake()
	{
        if (ui)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            ui = this;
            showPanels = GetComponent<ShowPanels>();
            changeScenes = false;
            inMainMenu = true;
        }

	}


	public void StartButtonClicked()
	{
        changeScenes = true;
		if (changeScenes) 
		{
            //animColorFade.SetTrigger("fade");
            LoadDelayed();           
		} 
		else 
		{
			StartGameInScene();
		}

	}
    /*
    void OnEnable()
    {
        SceneManager.sceneLoaded += SceneWasLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneWasLoaded;
    }*/


	public void LoadDelayed()
	{
		inMainMenu = false;

		showPanels.HideMenu ();

		SceneManager.LoadScene (sceneToStart);
	}

	public void HideDelayed()
	{
		showPanels.HideMenu();
	}

	public void StartGameInScene()
	{
		inMainMenu = false;

		animMenuAlpha.SetTrigger ("fade");
		Invoke("HideDelayed", fadeAlphaAnimationClip.length);

	}
}
