using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Physics
{

    // Class that all rigidbodies have access to.
    // This class defines the friction values of each rigidbody
    // TODO add restitution
    public class PhysicsMaterial
    {
        public float m_StaticFriction { get; set; }
        public float m_DynamicFriction { get; set; }
    }
}

