using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Physics
{
    public class CustomRigidbody : MonoBehaviour
    {

        public Vector2 m_Force;                         // The force currently acting on the rigidbody
        public Vector2 m_Velocity;                      // Linear velocity of the rigidbody
        [SerializeField]
        private Vector2 m_Position;                     // Vector position of the rigidbody
        [SerializeField]
        private float m_GravityScale = -9.81f;          // gravity force applied to a rigidbody
        [SerializeField]
        private float m_AirResistance = 0.02f;          // amount of air resistance acting upon a rigidbody

        public PhysicsMaterial m_MaterialType;          // material that defines the friction values of the rigidbody
        public float m_Mass;                            // mass of the rigidbody

        public bool m_IsIgnoringGravity = false;
        public bool m_IsGround = false;
        public bool m_IsPlayer = false;                 // bool to check if player should be controlling the rigidbody
        public bool m_IsEnemy = true;
        public bool m_IsGrounded = true;
        [HideInInspector]
        public Vector2 m_EnemyMoveDirection;
        [HideInInspector]
        public bool m_EnemyHasPath;
        public bool m_IsEnemyDead;
        public bool m_PlayerHasBeenHit;

        private Vector2 m_EnemyMoveForce;
        private Vector2 m_MovementForce;                // direction force for entity movement

        public Vector2 attackDirection;
        public float jumpTimer = 4f;                   // amount of time a player is allowed to jump
        public float currentTimer;




        public void PlayerPhysicsLoop()
        {
            // not sure if right, but ground rigidbodies should have no forces acting on them
            if(gameObject.tag == "Ground" || m_Mass == 0)
            {
                return;
            }

            // calc all external forces (gravity, etc) first
            m_Force = ComputeGravity();

            // calc constraint forces
            m_MovementForce = m_Force;

            if (m_IsPlayer)
            {
                MovePlayer();

                // calc final force for this physics update
                ComputeForce(m_MovementForce);

                RotatePlayerPointer();

                // calc linear acceleration by doing force / mass
                Vector2 linearAccell = new Vector2(m_Force.x / m_Mass, m_Force.y / m_Mass);

                // calc velocity by doing acceleration * delta time
                m_Velocity.x += linearAccell.x * Time.deltaTime;
                m_Velocity.y += linearAccell.y * Time.deltaTime;

                // update actual position in scene over time
                m_Position.x += m_Velocity.x * Time.deltaTime;
                m_Position.y += m_Velocity.y * Time.deltaTime;

                // calc air resistance to make sure velocity is not constant
                m_Velocity.x *= (1 - m_AirResistance);
                m_Velocity.y *= (1 - m_AirResistance);

                //update position in the scene
                transform.position = m_Position;

                //DebugLog();
            }
                
        }


        public void EnemyPhysicsLoop()
        {
            if (m_IsEnemy)
            {
                // not sure if right, but ground rigidbodies should have no forces acting on them
                if (gameObject.tag == "Ground" || m_Mass == 0)
                {
                    return;
                }

                // calc all external forces (gravity, etc) first
                m_Force = ComputeGravity();

                m_EnemyMoveForce = m_Force;

                MoveEntity(m_EnemyMoveDirection, m_EnemyHasPath);
                ComputeForce(m_EnemyMoveForce);

                // calc linear acceleration by doing force / mass
                Vector2 linearAccel = new Vector2(m_Force.x / m_Mass, m_Force.y / m_Mass);

                // calc velocity by doing acceleration * delta time
                m_Velocity.x += linearAccel.x * Time.deltaTime;
                m_Velocity.y += linearAccel.y * Time.deltaTime;

                // update actual position in scene over time
                m_Position.x += m_Velocity.x * Time.deltaTime;
                m_Position.y += m_Velocity.y * Time.deltaTime;

                // calc air resistance to make sure velocity is not constant
                m_Velocity.x *= (1 - m_AirResistance);
                m_Velocity.y *= (1 - m_AirResistance);

                //update position in the scene
                transform.position = m_Position;
            }
        }

        
        //Default 
        public void InitBody()
        {
            m_Position = transform.position;
            m_Velocity = new Vector2(0, 0);
            m_Force = new Vector2(0, 0);
            m_Mass = 1f;
            m_MaterialType = new PhysicsMaterial();
            m_MaterialType.m_DynamicFriction = 0f;
            m_MaterialType.m_StaticFriction = 0f;
            m_EnemyHasPath = false;
            m_IsEnemyDead = false;
            m_PlayerHasBeenHit = false;
        }

        public Vector2 ComputeForce(Vector2 f)
        {
            m_Force = f;
            return m_Force; 
        }

        void MovePlayer()
        {
            // player movement
            if (m_IsPlayer)
            {
                // left and right
                if (Input.GetKey("a"))
                {
                    m_MovementForce = (new Vector2(-1, 0) * 10f);
                }

                if (Input.GetKey("d"))
                {
                    m_MovementForce = (new Vector2(1, 0) * 10f);
                }

                // player jumping
                bool jumpActive = Input.GetKeyDown("space");

                if (jumpActive && m_IsGrounded)
                {
                    m_MovementForce = (new Vector2(0, 1) * 500f);
                    jumpActive = false;
                    m_IsGrounded = false;
                }

            }
        }

        public void MoveEntity(Vector2 direction, bool hasPath)
        {
            if (hasPath == true)
            {
                bool jump = false;
                m_EnemyMoveForce = direction * 0.5f;
                /*if(direction.y > 0)
                {
                    jump = true;
                }

                if (direction.x < 0)
                {
                    m_EnemyMoveForce = new Vector2(-1, 0) * 0.5f;
                }

                /*if (jump)
                {
                    return m_EnemyMoveForce = direction * 0.5f;
                }
                else
                {
                    return m_EnemyMoveForce = direction * 0.5f;
                }*/
            }



        }
        

        Vector2 ComputeGravity()
        {
            Vector2 f = new Vector2(0, 0);
            if (m_IsIgnoringGravity)
            {
                f = new Vector2(0,0);
            }
            else
            {
                f = new Vector2(0, m_Mass * m_GravityScale);
            }

            return f;
        }


        void RotatePlayerPointer()
        {
            // control player mouse pointer
            if (m_IsPlayer)
            {
                Transform pointer = transform;
                Transform[] t = GetComponentsInChildren<Transform>();

                foreach (Transform tr in t)
                {
                    if(tr.tag == "pointer")
                    {
                        pointer = tr;
                    }
                }
                // rotate pointer sprite around player character position
                // and make it face the mouse position
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                Vector3 pointToMouseDir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                pointToMouseDir.z = 0;
                pointer.position = transform.position + (1.0f * pointToMouseDir.normalized);
                pointer.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(pos.y - transform.position.y, pos.x - transform.position.x) * Mathf.Rad2Deg + 90);
                attackDirection = pointer.localPosition.normalized;
                //Debug.Log(attackDirection);
            }
        }


        void DebugLog()
        {
            Debug.Log(string.Format("rigidbody pos = ({0}.2f, {1}.2f)", m_Position.x, m_Position.y));
        }
    }
}


