using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI;
using Physics;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{

    private PhysicsManager physicsManager;
    [SerializeField]
    private GameObject[] spawners;

    public GameObject playerSpawn;
    public List<GameObject> enemySpawn;

    private CustomRigidbody rb;

    [HideInInspector]
    public List<Transform> m_Targets;

    private Camera m_Camera;
    private Vector3 m_DesiredPosition;

    private GameObject player;
    private GameObject enemy;

    public int playerHealth = 3;
    private int playerScore;

    private Image heart1;
    private Image heart2;
    private Image heart3;

    private Text beginText;
    private Text playerScoreText;
    private Text debugText;

    void Awake ()
    {
        // init physics manager components
        physicsManager = GetComponent<PhysicsManager>();
        physicsManager.InitPhysicsManager();

        spawners = GameObject.FindGameObjectsWithTag("Respawn");
        enemySpawn = new List<GameObject>();
        for(int i = 0; i < spawners.Length; i++)
        {
            if(spawners[i].name == "Spawner1")
            {
                playerSpawn = spawners[i];
            }
            if(spawners[i].name == "Spawner2" || spawners[i].name == "Spawner3" || spawners[i].name == "Spawner4")
            {
                enemySpawn.Add(spawners[i]);
            }
        }

        // init camera components
        m_Camera = GameObject.Find("Main Camera").GetComponent<Camera>();

        m_Targets = new List<Transform>();

        enemy = GameObject.Find("enemy");
        player = GameObject.Find("player");

        m_Targets.Add(transform);
        m_Targets.Add(player.transform);
        m_Targets.Add(enemy.transform);

        heart1 = GameObject.Find("health1").GetComponent<Image>();
        heart2 = GameObject.Find("health2").GetComponent<Image>();
        heart3 = GameObject.Find("health3").GetComponent<Image>();

        beginText = GameObject.Find("BeginText").GetComponent<Text>();
        playerScoreText = GameObject.Find("PlayerScore").GetComponent<Text>();
        debugText = GameObject.Find("debugText").GetComponent<Text>();

        Time.timeScale = 0f;
    }
	

	void Update ()
    {
        // start game
        if (Input.GetMouseButtonDown(1))
        {
            Color c = beginText.color;
            c.a = 0f;
            beginText.color = c;
            debugText.color = c;
            Time.timeScale = 1f;

        }
        if(Input.GetKeyDown("left alt"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(2);
        }

        int score = 0;
        // calc all physics for this frame first
        physicsManager.PhysicsUpdate();

        /*if (playerHealth <= 0)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }*/

        //check if player and/or enemy has hit a boundary and been removed
        // reset rigidbodies, camera targets
        if (!physicsManager.isPlayerActive)
        {
            m_Targets.Remove(player.transform);

            GameObject newPlayer = Instantiate(Resources.Load("player")) as GameObject;
            player.transform.position = playerSpawn.transform.position;
            physicsManager.entityBoundingBoxes.Add(newPlayer.GetComponent<AABB>());
            physicsManager.rigidBodiesInScene.Add(newPlayer.GetComponent<CustomRigidbody>());
            rb = newPlayer.GetComponent<CustomRigidbody>();
            rb.InitBody();
            rb.m_MaterialType.m_StaticFriction = 0f;
            rb.m_MaterialType.m_DynamicFriction = 0f;
            rb.m_Mass = 1f;

            // take away one life from player
            // and check player current health for game state
            playerHealth--;
            if(playerHealth == 2 || rb.m_PlayerHasBeenHit)
            {
                Color temp = heart1.color;
                temp.a = 0f;
                heart1.color = temp;
            }
            if(playerHealth == 1)
            {
                Color temp = heart2.color;
                temp.a = 0f;
                heart2.color = temp;
            }
            if(playerHealth <= 0)
            {
                Color temp = heart3.color;
                temp.a = 0f;
                heart3.color = temp;
                Time.timeScale = 0f;
                temp.a = 255f;
                beginText.color = temp;
                beginText.text = "Your score was: " + playerScore;
                debugText.color = temp;
                debugText.text = "Press Escape To Quit.";
            }

            m_Targets.Add(newPlayer.transform);
            player = newPlayer;

            physicsManager.isPlayerActive = true;
        }
        // respawn enemies that have been removed
        if (!physicsManager.isEnemyActive)
        {
            m_Targets.Remove(enemy.transform);
            int rand = Random.Range(0, enemySpawn.Count);
            GameObject newEnemy = Instantiate(Resources.Load("enemy")) as GameObject;
            newEnemy.transform.position = enemySpawn[rand].transform.position;
            physicsManager.entityBoundingBoxes.Add(newEnemy.GetComponent<AABB>());
            physicsManager.rigidBodiesInScene.Add(newEnemy.GetComponent<CustomRigidbody>());
            rb = newEnemy.GetComponent<CustomRigidbody>();
            rb.InitBody();
            rb.m_MaterialType.m_StaticFriction = 0f;
            rb.m_MaterialType.m_DynamicFriction = 0f;
            rb.m_Mass = 1f;

            m_Targets.Add(newEnemy.transform);
            enemy = newEnemy;

            physicsManager.isEnemyActive = true;
        }

        //update UI
        score = (int)Time.timeSinceLevelLoad;
        playerScoreText.text = "Score: " + score;
        playerScore = score;

        FindAveragePosition();
        // Update camera position based on positions of entities
        m_Camera.transform.position = physicsManager.UpdateCameraPos(m_Camera.transform.position, m_DesiredPosition);
    }

    private void FindAveragePosition()
    {
        Vector3 averagePos = new Vector3();
        int numTargets = 0;

        // Go through all the targets and add their positions together
        for (int i = 0; i < m_Targets.Count; i++)
        {
            // If the target isn't active, go on to the next one.
            if (!m_Targets[i].gameObject.activeSelf)
                continue;

            // Add to the average and increment the number of targets in the average.
            averagePos += m_Targets[i].position;
            numTargets++;
        }

        // If there are targets divide the sum of the positions by the number of them to find the average.
        if (numTargets > 0)
            averagePos /= numTargets;

        // Keep the same z value.
        averagePos.z = m_Camera.transform.position.z;

        // The desired position is the average position;
        m_DesiredPosition = averagePos;

        //return m_DesiredPosition;
    }



}
