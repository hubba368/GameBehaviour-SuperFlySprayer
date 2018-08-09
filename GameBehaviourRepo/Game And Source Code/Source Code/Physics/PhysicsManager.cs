 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI;

namespace Physics
{
    public class PhysicsManager : MonoBehaviour
    {
        public bool isPlayerActive = true;
        public bool isEnemyActive = true;
        public bool disabled = false;                       // debug bool for drawing gizmos

        // PHYSICS VARIABLES
        private const float fps = 100;                      // Max frames per second
        private const float deltaTime = 1 / fps;            // Time in seconds per frame
        private float accum = 0;                            // Time allocated for calculating physics per frame

        //[SerializeField]
        public List<CustomRigidbody> rigidBodiesInScene;              // List of all rigidbodies in scene
        [SerializeField]
        private List<AABB> floorBoundingBoxes;              // List of all floor/platform colliders (AABB) in scene
        [SerializeField]
        private List<AABB> boundaryBoundingBoxes;           // List of all playing field boundaries in the scene
        //[SerializeField]
        public List<AABB> entityBoundingBoxes;             // List of all entity colliders (AABB) in scene
        [SerializeField]
        private List<CollisionPair> currentCollisionPair;   // 'List' of the current Collision pair being resolved

        private CollisionPair colPair;                      // collion pair object
        private GameObject[] boxList;                       // List used to initialise the other AABB lists
        private AABB colliderTest1;                         // Variables for collision pairs
        private AABB colliderTest2;

        private float depthX;                               // penetration depth x component
        private float depthY;                               // penetration depth y component
        private float yDistance;                            // distance between 2 colliding objects y component
        private float xDistance;                            // distance between 2 colliding objects x component
        private float xDirection;                           // impulse direction vector x component
        private float yDirection;                           // impulse direction vector y component

        // PATHFINDING VARIABLES
        private NavGraph grid;
        public List<Vector2> path;
        public List<Vector2> dPath;
        public Vector2 playerPos;
        public Vector2 enemyPos;

        NavNode playerNode;
        NavNode enemyNode;

        public float chargeDmg;
        public float chargeRate;
        bool recentHit = false;

        public void InitPhysicsManager()
        {
            //find all colliders in the scene A$AP
            boxList = GameObject.FindGameObjectsWithTag("Ground");
            for (int i = 0; i < boxList.Length; i++)
            {
                floorBoundingBoxes.Add(boxList[i].GetComponent<AABB>());
            }
            boxList = null;
            boxList = GameObject.FindGameObjectsWithTag("Entity");
            for (int i = 0; i < boxList.Length; i++)
            {
                entityBoundingBoxes.Add(boxList[i].GetComponent<AABB>());
            }
            // add player collider
            entityBoundingBoxes.Add(GameObject.FindGameObjectWithTag("Player").GetComponent<AABB>());

            boxList = null;
            boxList = GameObject.FindGameObjectsWithTag("Boundary");
            for(int i = 0; i < boxList.Length; i++)
            {
                boundaryBoundingBoxes.Add(boxList[i].GetComponent<AABB>());
            }
            // initialise collision data
            colPair = new CollisionPair();

            // initialise all rigidbodies in scene
            InitRigidbodiesInScene();

            // initialise nav graph
            grid = GetComponent<NavGraph>();
            grid.InitGraph();

            chargeRate = 0;
            chargeDmg = 0;

            dPath = new List<Vector2>();
        }

       
        // all physics calculations are done within here
        // this is called from within MainManager
        public void PhysicsUpdate()
        {
    
            accum += deltaTime;

            //Calculate physics on the current frame
            while (accum > deltaTime)
            {
                
                colliderTest1 = null;
                colliderTest2 = null;
                currentCollisionPair = new List<CollisionPair>();
                depthX = 0f;
                depthY = 0f;
                xDistance = 0f;
                yDistance = 0f;
                xDirection = 0f;
                yDirection = 0f;

                // if no path simulate normally
                //calculate rigidbodies
                for (int i = 0; i < rigidBodiesInScene.Count; i++)
                {
                    // entity calc
                    if (rigidBodiesInScene[i].tag == "Entity")
                    {

                        bool charging = false;
                        recentHit = false;

                        // check if player attacked
                        // player charge attack
                        if (Input.GetMouseButtonDown(0))
                        {
                            chargeDmg = 0;
                            chargeRate = 1000f;
                            charging = true;

                            if (charging == true)
                            {
                                chargeDmg += chargeRate * Time.deltaTime;
                            }

                        }
                        if (Input.GetMouseButtonUp(0))
                        {
                            charging = false;
                            chargeDmg = Mathf.Clamp(chargeDmg, 0f, 100f);
                            //create a player attack area and check if the enemy is inside it
                            GameObject t = Instantiate(Resources.Load("EntityAttack")) as GameObject;
                            GameObject graphic = Instantiate(Resources.Load("AttackGraphic")) as GameObject;
                            EntityAttack et = t.GetComponent<EntityAttack>();
                            et.InitAttack();
                            CustomRigidbody player = GameObject.FindGameObjectWithTag("Player").GetComponent<CustomRigidbody>();

                            Vector2 direction = et.GetDirection(player).normalized;
                            t.transform.position = (Vector2)player.transform.position + direction;

                            graphic.transform.position = (Vector2)player.transform.position + direction;
                            graphic.transform.SetParent(player.transform);

                            Transform[] q = graphic.GetComponentsInChildren<Transform>();

                            foreach (Transform tr in q)
                            {
                                if (tr.tag == "pointer")
                                {
                                    graphic.transform.rotation = tr.rotation;
                                }
                            }


                            if (et.IsColliding(rigidBodiesInScene[i]))
                            {
                                // if hit, set enemy velocity to the direction of where the player pointed
                                rigidBodiesInScene[i].m_IsIgnoringGravity = false;
                                rigidBodiesInScene[i].m_Velocity = direction * chargeDmg;
                                rigidBodiesInScene[i].m_IsEnemyDead = true;
                                Destroy(et.gameObject);
                                DestroyObject(graphic.gameObject, 0.5f);
                            }
                            else
                            {
                                DestroyObject(graphic.gameObject);
                                Destroy(et.gameObject, 0.5f);
                            }
                        }

                        // if entity has been attacked, dont path find
                        AStarPathfind a = new AStarPathfind();
                        if (rigidBodiesInScene[i].m_IsEnemyDead != true)
                        {
                            // calculate entity positions for pathfinding
                            playerPos = new Vector2(0, 0);
                            enemyPos = new Vector2(0, 0);

                            for (int l = 0; l < entityBoundingBoxes.Count; l++)
                            {
                                if (entityBoundingBoxes[l].tag == "Player")
                                {
                                    playerPos = entityBoundingBoxes[l].transform.position;
                                }

                            }
                            // generate path to player
                            playerNode = grid.NodeFromWorldPoint(playerPos);
                            enemyNode = grid.NodeFromWorldPoint(rigidBodiesInScene[i].transform.position);

                            path = a.AStarSearch(grid, enemyNode, playerNode);
                            dPath = path;
                            //path = null;
                        }
                        else
                        {
                            Destroy(a);
                            path = null;
                        }

                        // if path exists move through it
                        if (path != null)
                        {
                            for (int j = 0; j < path.Count; j++)
                            {
                                rigidBodiesInScene[i].m_EnemyHasPath = false;
                                Vector2 direction = (path[j] - (Vector2)rigidBodiesInScene[i].transform.position);
                                direction.Normalize();
                                rigidBodiesInScene[i].m_EnemyMoveDirection = direction;
                                rigidBodiesInScene[i].m_EnemyHasPath = true;
                                rigidBodiesInScene[i].EnemyPhysicsLoop();

                            }

                        }
                        // if entity has been attacked, re enable gravity
                        else if (path == null || rigidBodiesInScene[i].m_IsEnemyDead == true)
                        {
                            rigidBodiesInScene[i].m_EnemyHasPath = false;
                            rigidBodiesInScene[i].m_IsIgnoringGravity = false;
                            rigidBodiesInScene[i].EnemyPhysicsLoop();
                        }
                    }
                    // calc player and other rigidbodies
                    if (rigidBodiesInScene[i].tag == "Player")
                    {
                        rigidBodiesInScene[i].PlayerPhysicsLoop();
                    }
                    else if (rigidBodiesInScene[i].tag != "Player" || rigidBodiesInScene[i].tag != "Entity")
                    {
                        rigidBodiesInScene[i].PlayerPhysicsLoop();
                    }
                }

                //calculate collisions
                for (int j = 0; j < entityBoundingBoxes.Count; j++)
                {
                    // this is the current entity we are checking colliders against
                    // we dont need to check collisions between platforms.
                    colliderTest1 = entityBoundingBoxes[j];

                    //collision between entity and ground
                    for (int k = 0; k < floorBoundingBoxes.Count; k++)
                    {
                        colliderTest2 = floorBoundingBoxes[k];

                        if (TestAABBOverlap(colliderTest1, colliderTest2))
                        {
                            if (colliderTest1.GetComponent<CustomRigidbody>().m_IsEnemyDead == true)
                            {
                                entityBoundingBoxes.Remove(colliderTest1);
                                rigidBodiesInScene.Remove(colliderTest1.GetComponent<CustomRigidbody>());
                                Destroy(colliderTest1.gameObject);
                                isEnemyActive = false;
                            }
                            colliderTest1.GetComponent<CustomRigidbody>().m_IsGrounded = true;
                            //Debug.Log(string.Format("Ground Collision between: {0} and {1}", colliderTest1, colliderTest2));
                            PopulateCollisionData(colliderTest1, colliderTest2);

                        }
                    }

                    //collision between entity and entity
                    for (int l = 0; l < entityBoundingBoxes.Count; l++)
                    {
                        colliderTest2 = entityBoundingBoxes[l];

                        //Don't collide with itself
                        if (colliderTest1 == colliderTest2)
                        {
                            continue;
                        }

                        if (TestAABBOverlap(colliderTest1, colliderTest2))
                        {
                            //Debug.Log(string.Format("Entity Collision between: {0} and {1}", colliderTest1, colliderTest2));
                            PopulateCollisionData(colliderTest1, colliderTest2);

                            float[] moveVals = { 12, -12 };

                            int chooseAxis = 0;
                            // enemy 'attacks'
                            // choose a random direction to set the players velocity in
                            if (colliderTest1.tag == "Player")
                            {
                                CustomRigidbody rb = colliderTest1.GetComponent<CustomRigidbody>();
                                rb.m_PlayerHasBeenHit = true;
                                float v = Random.Range(0, moveVals.Length);
                                chooseAxis = Random.Range(0, 1);
                                if(chooseAxis == 0)
                                {
                                    rb.m_Velocity = new Vector2(moveVals[Random.Range(0, moveVals.Length)], 0);
                                }
                                if (chooseAxis == 1)
                                {
                                    rb.m_Velocity = new Vector2(0, moveVals[Random.Range(0, moveVals.Length)]);
                                }

                            }
                            if (colliderTest2.tag == "Player")
                            {
                                CustomRigidbody rb = colliderTest2.GetComponent<CustomRigidbody>();
                                rb.m_PlayerHasBeenHit = true;
                                int v = Random.Range(0, moveVals.Length);
                                chooseAxis = Random.Range(0, 1);
                                if (chooseAxis == 0)
                                {
                                    rb.m_Velocity = new Vector2(moveVals[Random.Range(0, moveVals.Length)], 0);
                                }
                                if (chooseAxis == 1)
                                {
                                    rb.m_Velocity = new Vector2(0, moveVals[Random.Range(0, moveVals.Length)]);
                                }
                            }
                        }
                    }

                    // Finally check whether an entity has hit the boundaries of the screen
                    // and needs to be removed from scene
                    for (int i = 0; i < boundaryBoundingBoxes.Count; i++)
                    {
                        colliderTest2 = boundaryBoundingBoxes[i];

                        if (TestAABBOverlap(colliderTest1, colliderTest2))
                        {
                            if (colliderTest1.tag == "Player")
                            {
                                entityBoundingBoxes.Remove(colliderTest1);
                                rigidBodiesInScene.Remove(colliderTest1.GetComponent<CustomRigidbody>());
                                Destroy(colliderTest1.gameObject);
                                isPlayerActive = false;
                            }
                            if (colliderTest1.tag == "Entity")
                            {
                                entityBoundingBoxes.Remove(colliderTest1);
                                rigidBodiesInScene.Remove(colliderTest1.GetComponent<CustomRigidbody>());
                                Destroy(colliderTest1.gameObject);
                                isEnemyActive = false;
                            }

                        }
                    }
                }


                accum -= deltaTime;
            }
        }


        bool TestAABBOverlap(AABB objectA, AABB objectB)
        {
            // Seperating Axis Theorem

            //make sure the objects exist in scene
            if (objectA == null)  
            {
                Debug.Log(objectA);
                return false;
            }
            if(objectB == null)
            {
                Debug.Log(objectB);
                return false;
            }

            //update positions of min and max vectors to where the object is located in scene
            Vector2 aMin = objectA.m_Min + (Vector2)objectA.transform.position;
            Vector2 aMax = objectA.m_Max + (Vector2)objectA.transform.position;
            Vector2 bMin = objectB.m_Min + (Vector2)objectB.transform.position;
            Vector2 bMax = objectB.m_Max + (Vector2)objectB.transform.position;

            // if distances between each min and max are above 0 they aren't colliding
            float d1x = bMin.x - aMax.x;
            float d1y = bMin.y - aMax.y;
            float d2x = aMin.x - bMax.x;
            float d2y = aMin.y - bMax.y;

            if (d1x > 0.0f || d1y > 0.0f)
            {
                return false;
            }
            if (d2x > 0.0f || d2y > 0.0f)
            {
                return false;
            }

            return true;
        }


        void PopulateCollisionData(AABB colliderA, AABB colliderB)
        {
            // initialise collision data variables
            currentCollisionPair = new List<CollisionPair>();
            colPair = new CollisionPair();
            colPair.m_ObjectA = colliderA;
            colPair.m_ObjectB = colliderB;

            // calculate the penetration depth between the 2 colliders
            CalcPenetration(colliderA, colliderB);

            // check which axis has the least amount of penetration
            if (Mathf.Abs(xDirection) < Mathf.Abs(yDirection))
            {
                //point the collision normal either left or right depending on the sign
                colPair.m_CollisionNormal = new Vector2(Mathf.Sign(xDistance), 0);
                colPair.m_CollisionPoint = new Vector2(colliderA.transform.position.x
                                            + (colliderA.m_Width * Mathf.Sign(xDistance)),
                                            colliderB.transform.position.y);
            }
            else
            {
                // point the collision normal up or down depending on the sign
                colPair.m_CollisionNormal = new Vector2(0, Mathf.Sign(yDistance));
                colPair.m_CollisionPoint = new Vector2(colliderA.transform.position.x
                                            + (colliderA.m_Width * Mathf.Sign(xDistance)),
                                            colliderB.transform.position.y);
            }

            // add the final depth to collision data
            colPair.m_PenetrationDepth = new Vector2(xDirection, yDirection);
            
            currentCollisionPair.Add(colPair);

            // resolve the current collision based on the data we found
            ResolveCollisionPair(currentCollisionPair);

            //TODO: change to private
            depthX = 0f;
            depthY = 0f;
            xDistance = 0f;
            yDistance = 0f;
            xDirection = 0f;
            yDirection = 0f;

        }


        void CalcPenetration(AABB objectA, AABB objectB)
        {
            // get the distance vector from object a to object b
            xDistance = objectB.m_CentrePoint.x - objectA.m_CentrePoint.x;
            yDistance = objectB.m_CentrePoint.y - objectA.m_CentrePoint.y;

            // get the depth of the intersection on each axis
            // add the half extents of x and y then subtract by the distance
            depthX = (objectB.m_Width ) + (objectA.m_Width ) - Mathf.Abs(xDistance);
            depthY = (objectB.m_Height ) + (objectA.m_Height) - Mathf.Abs(yDistance);

            // get the direction vector by checking whether the distance is negative or positive
            // i.e. up would be positive and down would be negative
            xDirection = depthX * (Mathf.Sign(xDistance));
            yDirection = depthY * (Mathf.Sign(yDistance));
        }


        void ResolveCollisionPair(List<CollisionPair> pairs)
        {
            CustomRigidbody objectA = pairs[0].m_ObjectA.GetComponent<CustomRigidbody>();
            CustomRigidbody objectB = pairs[0].m_ObjectB.GetComponent<CustomRigidbody>();

            // get relative velocity of each object this is used to find out how far to move each object
            Vector2 objAVelocity = objectA.m_Velocity;
            Vector2 objBVelocity = objectB.m_Velocity;

            Vector2 relativeVelocity = objBVelocity - objAVelocity;

            // normalise the contact normal to make sure it only points in the right direction
            Vector2 normalMagnitude = pairs[0].m_CollisionNormal;
            normalMagnitude.Normalize();

            // get the velocity along the normal
            float velocityAlongNormal = Vector2.Dot(relativeVelocity, normalMagnitude);

            // calculate the restitution (elasticity) which is between 0 and 1
            // should change to a property of each rigidbody
            float coefficientOfRestitution = Mathf.Min(0.01f, 0.02f);

            // calculate the inverse mass to impulse (force) the objects away
            // do this to save from having to calculate it like 5 more times
            float aInverseMass;
            float bInverseMass;

            if (objectA.m_Mass == 0)
            {
                aInverseMass = 0;
            }
            else
            {
                aInverseMass = 1 / objectA.m_Mass;
            }
            if (objectB.m_Mass == 0)
            {
                bInverseMass = 0;
            }
            else
            {
                bInverseMass = 1 / objectB.m_Mass;
            }

            // calculate the impulse
            float impulse = -(1 + coefficientOfRestitution) * velocityAlongNormal / (aInverseMass + bInverseMass);

            // create and assign the new velocities
            Vector2 imp = impulse * normalMagnitude;

            objectA.m_Velocity -= aInverseMass * imp;
            objectB.m_Velocity += bInverseMass * imp;


            // stop sinking objects by offsetting the positions to above a certain threshold
            // depending on the penetration depth

            float percent = 0.2f;           // the amount we want to move each colliding object
            float slop = 0.01f;             // the amount we let the 2 objects sink into eachother to stop jittering
            float xAmount = 0f;
            float yAmount = 0f;

            // get the side that is being most penetrated
            xAmount = pairs[0].m_PenetrationDepth.x;
            yAmount = pairs[0].m_PenetrationDepth.y;

            Vector2 correction = Mathf.Max(-yAmount - slop, 0.0f) / (aInverseMass + bInverseMass) * percent * pairs[0].m_CollisionNormal;


            objectA.m_Velocity -= aInverseMass * correction;
            objectB.m_Velocity += bInverseMass * correction;

        }


        void ApplyFrictionForce(CustomRigidbody objectA, CustomRigidbody objectB, Vector2 normal, float impValue, Vector2 impulse)
        {
            //assign new friction values
            float aInverseMass = 1 / objectA.m_Mass;
            float bInverseMass = 1 / objectB.m_Mass;

            // Get the recalculated relative velocity after the previous impulse has been applied
            Vector2 rv = objectB.m_Velocity - objectA.m_Velocity;

            // Retrieve the tangent vector, the vector that determines the direction of friction force to apply
            Vector2 tangent = rv - Vector2.Dot(rv, normal) * normal;
            tangent.Normalize();

            // calculate the friction impulse value
            float jT = -Vector2.Dot(rv, tangent) / (aInverseMass + bInverseMass);

            // Coulombs law: force of friction <= force along the normal * mu
            float mu = (objectA.m_MaterialType.m_StaticFriction + objectB.m_MaterialType.m_StaticFriction) / 2;

            Vector2 frictionImpulse;

            // clamp the magnitude of the friction and create the friction impulse vector
            if (Mathf.Abs(jT) < impValue * mu)
            {
                frictionImpulse = jT * tangent;
            }
            else
            {
                float dynamicFriction = (objectA.m_MaterialType.m_DynamicFriction + objectB.m_MaterialType.m_DynamicFriction) / 2;
                impulse.Scale(tangent);
                Vector2 j = new Vector2(-impulse.x, -impulse.y);
                frictionImpulse = j * dynamicFriction;
            }

            objectA.m_Velocity -= (aInverseMass) * frictionImpulse;
            objectB.m_Velocity += (bInverseMass) * frictionImpulse;

        }


        void InitRigidbodiesInScene()
        {
            CustomRigidbody[] rigidBodies;
            // initialise all rigidbodies
            rigidBodies = FindObjectsOfType<CustomRigidbody>();

            for (int i = 0; i < rigidBodies.Length; i++)
            {
                rigidBodies[i].InitBody();

                // Initialise the properties of each floor/entity in scene
                if (rigidBodies[i].tag == "Player")
                {
                    rigidBodies[i].m_MaterialType.m_StaticFriction = 0f;
                    rigidBodies[i].m_MaterialType.m_DynamicFriction = 0f;
                    rigidBodies[i].m_Mass = 1f;
                }
                if (rigidBodies[i].tag == "Entity")
                {
                    rigidBodies[i].m_MaterialType.m_StaticFriction = 0f;
                    rigidBodies[i].m_MaterialType.m_DynamicFriction = 0f;
                    rigidBodies[i].m_Mass = 1f;
                }
                if (rigidBodies[i].tag == "Ground")
                {
                    rigidBodies[i].m_MaterialType.m_StaticFriction = 0.7f;
                    rigidBodies[i].m_MaterialType.m_DynamicFriction = 0.7f;
                    rigidBodies[i].m_Mass = 0f;
                }

                rigidBodiesInScene.Add(rigidBodies[i]);
            }
        }


        public Vector3 UpdateCameraPos(Vector3 pos, Vector3 desiredPos)
        {
            Vector3 moveVelocity = new Vector3(0,0,0);
            pos = Vector3.SmoothDamp(pos, desiredPos, ref moveVelocity, 0.2f);
            return pos;
        }



        //Debug functions
        
        private void OnDrawGizmos()
        {

            if (!disabled)
            {
                Gizmos.DrawWireCube(transform.position, new Vector3(grid.gridSize.x, grid.gridSize.y, 1));

                if (grid != null)
                {

                    foreach (NavNode n in grid.navGrid)
                    {
                        Gizmos.color = Color.green;
                        if (path != null)
                        {
                            if (path.Contains(n.worldPosition))
                            {
                                Gizmos.color = Color.blue;
                            }
                        }
                        if (n == playerNode || n == enemyNode)
                        {
                            Gizmos.color = Color.yellow;
                        }

                        if (n.walkable == false)
                        {
                            Gizmos.color = Color.red;
                        }

                        Gizmos.DrawCube(n.worldPosition, Vector2.one * (grid.nodeDiameter - 0.1f));


                    }
                }
            }
            
        }
        

    }
}


