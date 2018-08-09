using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Physics;

public class EntityAttack : MonoBehaviour
{
    public List<AABB> entityBoundingBoxes;
    private GameObject[] boxList;
    private AABB colliderTest1;
    private AABB colliderTest2;
    private AABB areaOfEffect;

    public bool isTargetInside = false;

    public void InitAttack()
    {
        boxList = GameObject.FindGameObjectsWithTag("Entity");
        for (int i = 0; i < boxList.Length; i++)
        {
            entityBoundingBoxes.Add(boxList[i].GetComponent<AABB>());
        }
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        entityBoundingBoxes.Add(go.GetComponent<AABB>());

        areaOfEffect = GetComponent<AABB>();
    }


    public Vector2 GetDirection(CustomRigidbody rb)
    {
        Vector2 t = rb.attackDirection;
        return t;
    }

    public bool IsColliding(CustomRigidbody rb)
    {
        // check that an entity is colliding with the attack area
        for (int j = 0; j < entityBoundingBoxes.Count; j++)
        {
            colliderTest1 = rb.GetComponent<AABB>();
            colliderTest2 = areaOfEffect;

            if (TestAABBOverlap(colliderTest1, colliderTest2))
            {
                //Debug.Log("collision");
                isTargetInside = true;
                return true;
            }

        }
        return false;
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
        if (objectB == null)
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
}
