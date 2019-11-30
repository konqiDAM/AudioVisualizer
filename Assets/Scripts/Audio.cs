using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Audio : MonoBehaviour
{
    [SerializeField] int amountOfBands = 8;
    [SerializeField] GameObject myPrefab;
    [SerializeField] Transform img;
    GameObject[] prefabs;
    AudioSource source;
    float[] samples;
    float[] freqBand;
    float[] audioBandHighest;
    float[] audioBand;
    float[] audioBandBuffer;
    float[] bufferDecrease;
    float bufferedAmplitude;

    void Start()
    {
        source = GetComponent<AudioSource>();
        prefabs = new GameObject[amountOfBands];
        freqBand = new float[amountOfBands];
        audioBandHighest = new float[amountOfBands];
        audioBand = new float[amountOfBands];
        audioBandBuffer = new float[amountOfBands];
        bufferDecrease = new float[amountOfBands];
        samples = new float[512];
        for (int i = 0; i < prefabs.Length; i++)
        {
            audioBandHighest[i] = 0f;
            prefabs[i] = Instantiate(myPrefab, new Vector2(-8 + i*2, -3), Quaternion.identity);
        }
    }

    private void SmothTrackBand()
    {
        for (int i = 0; i < amountOfBands; i++)
        {
            if (audioBand[i] > audioBandBuffer[i])
            {
                audioBandBuffer[i] = audioBand[i];
                bufferDecrease[i] = 0.01f;
            }
            if(audioBand[i] < audioBandBuffer[i])
            {
                audioBandBuffer[i] -= bufferDecrease[i];
                bufferDecrease[i] *= 1.2f;
            }
        }
    }

    private void getSepctrumData()
    {
        source.GetSpectrumData(samples, 0, FFTWindow.Hamming);
    }

    private void getAudioBand()
    {
        for (int i = 0; i < amountOfBands; i++)
        {
            if (freqBand[i] > audioBandHighest[i])
                audioBandHighest[i] = freqBand[i];

            audioBand[i] = freqBand[i] / audioBandHighest[i];
            if (float.IsNaN(audioBand[i]))
                audioBand[i] = 0;
        }
    }

    private void getSamples()
    {
        int count = 0;
        for (int i = 0; i < amountOfBands; i++)
        {
            float avg = 0;
            int sampleCount = (int)Math.Pow(2, i + 1);
            if (i == 7)
            {
                sampleCount += 2;
            }

            for (int j = 0; j < sampleCount; j++)
            {
                avg += samples[count];
                count++;
            }

            avg /= sampleCount;
            
            freqBand[i] = avg;
            
        }
    }

    private void moveImages()
    {
        for (int i = 0; i < amountOfBands; i++)
        {
            prefabs[i].transform.localScale = new Vector3(1, (float)audioBandBuffer[i] * 4 + 1,1 );
        }
        img.transform.localScale = new Vector3(0.9f, bufferedAmplitude*0.7f, 1);
    }

    private void getAmplitude()
    {
        for (int i = 0; i < amountOfBands; i++)
        {
            bufferedAmplitude += audioBandBuffer[i];
        }

        bufferedAmplitude = bufferedAmplitude / amountOfBands;
    }

    void Update()
    {
        getSepctrumData();
        getSamples();
        getAudioBand();
        SmothTrackBand();
        getAmplitude();
        moveImages();
    }
}
