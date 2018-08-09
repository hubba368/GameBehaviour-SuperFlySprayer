using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Physics
{
    public class AABB : MonoBehaviour
    {
        public Vector2 m_Min;           //lower bounds of the box
        public Vector2 m_Max;           //upper bounds of the box
        public Vector2 cPoint;
        public float w;                 // full width of object
        public float h;                 // full height of object

        public Vector2 m_CentrePoint { get; set; }  //center point/center of the object
        public float m_Width { get; set; }          // x extents of the object
        public float m_Height { get; set; }         // y extents of the object



        void Awake()
        {
            //get the extents and center point of the object 
            m_CentrePoint = gameObject.GetComponent<SpriteRenderer>().bounds.center;
            m_Width = gameObject.GetComponent<SpriteRenderer>().bounds.size.x ;
            m_Height = gameObject.GetComponent<SpriteRenderer>().bounds.size.y ;
            cPoint = m_CentrePoint;
            w = m_Width;
            h = m_Height;
            UpdateBoundingBox();
        }


        void Update()
        {
            //re update min and max vectors, re updating center point for debug purposes
            m_CentrePoint = gameObject.GetComponent<SpriteRenderer>().bounds.center;
            cPoint = m_CentrePoint;
            UpdateBoundingBox();
            //DebugLog();
        }

        void UpdateBoundingBox()
        {
            // m_Min = negative width / 2, negative height / 2
            // m_Max = width / 2, height / 2
            m_Min = new Vector2(-m_Width / 2, -m_Height / 2);
            m_Max = new Vector2(m_Width / 2, m_Height / 2);

        }


        //Debug functions//
        private void OnDrawGizmos()
        {
            // Draw green rectangle around all objects with AABB components
            Gizmos.color = Color.green;
            Vector2 bottomLeft = new Vector2(-m_Width / 2, -m_Height / 2);
            Vector2 topLeft = new Vector2(-m_Width / 2, m_Height / 2);
            Vector2 topRight = new Vector2(m_Width / 2, m_Height / 2);
            Vector2 bottomRight = new Vector2(m_Width /2 , -m_Height / 2);

            bottomLeft = bottomLeft + (Vector2)gameObject.transform.position;
            topLeft = topLeft + (Vector2)gameObject.transform.position;
            topRight = topRight + (Vector2)gameObject.transform.position;
            bottomRight = bottomRight + (Vector2)gameObject.transform.position;

            Gizmos.DrawLine(bottomLeft, topLeft);
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
        }

        void DebugLog()
        {
            Debug.Log(string.Format("aabb pos = ({0}.2f, {1}.2f)", m_CentrePoint.x, m_CentrePoint.y));
        }
    }
}