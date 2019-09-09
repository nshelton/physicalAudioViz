using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PhysicsControls : MonoBehaviour
{

    public float Mass
    {
        get { return m_physicsSpawners[0].Mass; }
        set { foreach(var p in m_physicsSpawners) { p.Mass = value; } }
    }

    public float Drag
    {
        get { return m_physicsSpawners[0].Drag; }
        set { foreach (var p in m_physicsSpawners) { p.Drag = value; } }

    }

    public float Spring
    {
        get { return m_physicsSpawners[0].Spring; }
        set { foreach (var p in m_physicsSpawners) { p.Spring = value; } }
    }

    private PhysicsSpawners[] m_physicsSpawners;

    void Start()
    {
        m_physicsSpawners = gameObject.GetComponentsInChildren<PhysicsSpawners>();
    }
}
