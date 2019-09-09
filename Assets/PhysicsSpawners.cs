using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsSpawners : MonoBehaviour
{
    [SerializeField]
    private AudioAnalyzer m_audioAnalyzer;

    [SerializeField]
    private GameObject m_physicsPrefab;

    [SerializeField]
    private int m_numParticles;

    [SerializeField]
    private Rigidbody m_center;

    [SerializeField]
    private AnimationCurve m_scaleCurve;

    [SerializeField]
    private AnimationCurve m_frequencyCurve;

    [SerializeField]
    private AnimationCurve m_frequencyAmpCurve;

    [SerializeField]
    private Gradient m_colorGrad;

    [SerializeField]
    private Mesh m_mesh;

    [SerializeField]
    private Material m_material;

    [SerializeField]
    private bool m_doMaterialProperties;

    [SerializeField]
    private int m_plexusAmount;

    [SerializeField]
    private float m_plexusThreshold;
    
    [SerializeField]
    private GameObject m_plexusPrefab;

    [SerializeField]
    private float m_spread = 100;

    private Matrix4x4[] m_matrices;
    private MaterialPropertyBlock m_materialPropertyBlock;

    private float m_mass = 0.2f;
    private float m_drag = 1;
    private float m_spring = 1;

    public float Mass
    {
        get { return m_mass; }
        set { m_mass = value; }
    }

    public float Drag
    {
        get { return m_drag; }
        set { m_drag = value; }
    }

    public float Spring
    {
        get { return m_spring; }
        set { m_spring = value; }
    }

    private List<LineRenderer> m_plexusPool = new List<LineRenderer>();

    private class Particle
    {
        public Rigidbody Rigidbody;
        public SpringJoint SpringJoint;
        public Transform Transform;
        public Renderer Renderer;
    }

    private List<Particle> m_allParticles = new List<Particle>();

    void Start()
    {
        for(int i = 0; i < m_numParticles; i++)
        {
            var particle = GameObject.Instantiate(m_physicsPrefab, transform);

            Particle particleObject = new Particle();
            particleObject.Rigidbody = particle.GetComponent<Rigidbody>();
            particleObject.SpringJoint = particle.GetComponent<SpringJoint>();
            particleObject.Transform = particle.transform;
            particleObject.Renderer = particle.GetComponent<Renderer>();


            particleObject.SpringJoint.connectedBody = m_center;
            particleObject.SpringJoint.spring = 1f;
            if  (m_spread > 0)
            {
                particleObject.SpringJoint.connectedAnchor = Vector3.right * (-m_spread / 2f + m_spread * (float)i / (float)m_numParticles);
            }

            m_allParticles.Add(particleObject);
        }

        m_matrices = new Matrix4x4[m_numParticles];
        m_materialPropertyBlock = new MaterialPropertyBlock();
    }

    private void ResizePool()
    {
        if (m_plexusPrefab == null)
        {
            return;
        }

        while (m_plexusAmount > m_plexusPool.Count)
        {
            var plexus = GameObject.Instantiate(m_plexusPrefab, transform);
            m_plexusPool.Add(plexus.GetComponent<LineRenderer>());
        }
        while (m_plexusAmount < m_plexusPool.Count)
        {
            GameObject.Destroy(m_plexusPool[m_plexusPool.Count - 1].gameObject);
            m_plexusPool.RemoveAt(m_plexusPool.Count - 1);
        }
    }

    void Update()
    {
        if (m_plexusAmount > 0 )
        {
            if (m_plexusAmount != m_plexusPool.Count)
            {
                ResizePool();
            }
        }

        int plexusAvailabe = m_plexusAmount;

        for (int i = 0; i < m_allParticles.Count; i++)
        {
            float percent = ((float)i) / (float)(m_allParticles.Count);
            float binNum = m_frequencyCurve.Evaluate(percent) * m_audioAnalyzer.FFT.Length;

            float rawValue = m_audioAnalyzer.FFT[(int)binNum % m_audioAnalyzer.FFT.Length];
            float mappedValue = rawValue * m_frequencyAmpCurve.Evaluate(percent);

            float remap = m_scaleCurve.Evaluate(mappedValue);
            m_allParticles[i].Transform.localScale = Vector3.one * remap;

            m_allParticles[i].SpringJoint.spring = m_spring;
            m_allParticles[i].Rigidbody.mass = m_mass;
            m_allParticles[i].Rigidbody.drag = m_drag;

            m_matrices[i] = m_allParticles[i].Transform.localToWorldMatrix;

            if (m_doMaterialProperties)
            {
                m_materialPropertyBlock.SetColor("_BaseColor", m_colorGrad.Evaluate(mappedValue * 10f));
                m_allParticles[i].Renderer.SetPropertyBlock(m_materialPropertyBlock);
            }

            if (plexusAvailabe > 0 && mappedValue > m_plexusThreshold )
            {
                float closestDistance = 99999;
                int closestIndex = 0;

                for (int j = Mathf.Max(0, i-10); j < Mathf.Min(i+10, m_allParticles.Count); j++)
                {
                    if ( i != j ) //&& m_allParticles[j].Transform.localScale.x > 0.5f * m_allParticles[i].Transform.localScale.x)
                    {
                        var distance = (m_allParticles[i].Transform.position - m_allParticles[j].Transform.position).sqrMagnitude;
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestIndex = j;
                        }
                    }
                }
                m_plexusPool[plexusAvailabe-1].SetPosition(0, m_allParticles[i].Transform.position);
                m_plexusPool[plexusAvailabe-1].SetPosition(1, m_allParticles[closestIndex].Transform.position);
                plexusAvailabe--;
            }
        }

        for ( int i = plexusAvailabe-1; i > 0; i--)
        {
            m_plexusPool[i].SetPosition(0, 1e10f * Vector3.one);
            m_plexusPool[i].SetPosition(1, 1e10f * Vector3.one);
        }

        //DrawInstanced(m_matrices);
    }

    private void DrawInstanced(Matrix4x4[]  matrices)
    {
        Graphics.DrawMeshInstanced(m_mesh, 0, m_material, matrices, matrices.Length,
                           m_materialPropertyBlock, UnityEngine.Rendering.ShadowCastingMode.On, true); 
    }

}
