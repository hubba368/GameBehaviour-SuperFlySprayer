using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Physics
{
    // This is essentially the generated Manifold for each collision pair.
    public class CollisionPair
    {
        public AABB m_ObjectA { get; set; }
        public AABB m_ObjectB { get; set; }
        public Vector2 m_CollisionPoint { get; set; }
        public Vector2 m_CollisionNormal { get; set; }
        public Vector2 m_PenetrationDepth { get; set; }
    }
}
