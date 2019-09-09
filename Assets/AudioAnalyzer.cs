using NAudio.Dsp;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioAnalyzer : MonoBehaviour
{

    public float[] FFT
    {
        get { return m_fftOutput; }
    }

    public float Attack
    {
        get { return m_attack; }
        set { m_attack = value; }
    }

    public float Decay
    {
        get { return m_decay; }
        set { m_decay = value; }
    }

    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float m_attack = 1.0f;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float m_decay = 1.0f;

    [SerializeField]
    private LineRenderer m_wavLine;

    [SerializeField]
    private LineRenderer m_fftLine;

    private float[] m_rawBuffer;
    private float[] m_fftOutput;
    private float[] m_rawBufferComplex;

    const int CONST_M = 10;

    private void SetLine(LineRenderer line, float[] data)
    {
        if (line.positionCount != data.Length)
        {
            line.positionCount = data.Length;
        }

        for (int i = 0; i < data.Length; i++)
        {
            line.SetPosition(i,
                new Vector3((float)i / data.Length, data[i], 0f));
        }
    }

    void Start()
    {
        m_rawBuffer = new float[1024];
        m_rawBufferComplex = new float[1024];
        m_fftOutput = new float[1024/2];
    }

    void Update()
    {
        float attack = Mathf.Pow(m_attack, 2.3f) * -128;
        float decaySpeed = m_decay < 1.0f ? m_decay * 10 + 0.5f : 100.0f;

        Lasp.AudioInput.RetrieveWaveform(Lasp.FilterType.Bypass, m_rawBuffer);
        SetLine(m_wavLine, m_rawBuffer);

        FastFourierTransform.FFT(true, CONST_M, m_rawBuffer, m_rawBufferComplex);

        for( int i = 0; i < m_rawBuffer.Length/2; i ++)
        {
            var newVal = Mathf.Sqrt(m_rawBuffer[i] * m_rawBuffer[i] + m_rawBufferComplex[i] * m_rawBufferComplex[i]);

            // attack
            if (m_attack < 1.0f)
            {
                newVal -= (newVal - m_fftOutput[i]) * Mathf.Exp(attack * Time.deltaTime);
            }

            // decay
            m_fftOutput[i] = Mathf.Max(newVal, m_fftOutput[i] - Time.deltaTime * decaySpeed);


            m_rawBufferComplex[i] = 0;
        }

        SetLine(m_fftLine, m_fftOutput);
    }

}
