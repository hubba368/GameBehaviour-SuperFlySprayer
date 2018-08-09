using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AI;
using Physics;


public class DebugInfo : MonoBehaviour
{
    private PhysicsManager DebugTestManager;
    public Text physicsText;
    public Text AIText;

    public CustomRigidbody player;
    public List<Vector2> debugPath;

	// Use this for initialization
	void Awake ()
    {
        DebugTestManager = GetComponent<PhysicsManager>();
        physicsText = GameObject.Find("DebugPhysicsText").GetComponent<Text>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<CustomRigidbody>();
        debugPath = DebugTestManager.dPath;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown("t"))
        {
            string playerVel = player.m_Velocity.ToString();
            string playerFor = player.m_Force.ToString();
            string playerMas = player.m_Mass.ToString();
            string playerGrav = "-9.81";

            physicsText.text = "Player Info: \n" + "Velocity: " + playerVel + "\n" + "Force: " + playerFor + "\n" + "Mass: " + playerMas + "\n" + "Gravity: " + playerGrav;
        }
        if (Input.GetKeyDown("y"))
        {
            if(DebugTestManager.dPath == null)
            {
                Debug.Log("no path");
            }
            for(int i = 0; i < DebugTestManager.dPath.Count; i++)
            {
                GameObject newNode = Instantiate(Resources.Load("DebugPathNode")) as GameObject;
                newNode.transform.position = DebugTestManager.dPath[i];
            }
        }
        if (Input.GetKeyDown("u"))
        {
            GameObject[] nodes = GameObject.FindGameObjectsWithTag("debug");
            foreach(GameObject g in nodes)
            {
                Destroy(g);
            }
        }

        if (Input.GetKeyDown("p"))
        {
            Time.timeScale = 0f;
        }
        if (Input.GetKeyDown("o"))
        {
            Time.timeScale = 1f;
        }

	}
}
