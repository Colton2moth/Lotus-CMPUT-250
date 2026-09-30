using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CrackBlockJiggler : MonoBehaviour
{
    [Header("Per piece")]
    [SerializeField] BrokenPlatformVisualFragment[] crackPieces;

    [SerializeField] Vector3 pieceDirectionalAmplitudes = new Vector3(0.05f, 0.05f, 0.05f);
    [SerializeField] float pieceNoiseFrequency = 10f;
    //set on start
    float _jigglePercentInfluence = 0.0f; 
    //Clamps from 0-100% range (0.0 to 1.0f)
    public float JigglePercentInfluence {get{return _jigglePercentInfluence;} set{_jigglePercentInfluence = Mathf.Clamp(value, 0.0f, 1.0f);}}


    [Header("Per entire block")]
    Vector3 blockRestPoint;
    [SerializeField] float blockNoiseFrequency = 5f;
    [SerializeField] Vector3 blockNoiseAmplitudes = new Vector3(0.1f, 0.05f, 0.1f);

    bool broken = false;
    [SerializeField] float splitForce = 2.0f;
    [SerializeField] float maxSplitTorque = 50f;

    [SerializeField] ParticleSystem particles;



    // Start is called before the first frame update
    void Start()
    {
        Vector3[] restPoints = new Vector3[crackPieces.Length]; 
        for(int i = 0; i < crackPieces.Length; i++)
        {
            crackPieces[i].restPosition = crackPieces[i].transform.position;
            crackPieces[i].GetComponent<Collider>().enabled = false;
            crackPieces[i].Rigidbody.isKinematic = true;
        }



        blockRestPoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
        if(JigglePercentInfluence == 0.0 || broken) return;
        if(!particles.isPlaying) particles.Play();
        for(int i = 0; i < crackPieces.Length; i++)
        {
            float pieceNoiseValue = Mathf.PerlinNoise(
                crackPieces[i].transform.position.x + Time.time*pieceNoiseFrequency, 
                crackPieces[i].transform.position.z +Time.time*pieceNoiseFrequency);
            crackPieces[i].transform.position = crackPieces[i].restPosition + pieceDirectionalAmplitudes * pieceNoiseValue * JigglePercentInfluence;
        }


        float noiseValue = Mathf.PerlinNoise(
                transform.position.x + Time.time*blockNoiseFrequency, 
                transform.position.z +Time.time*blockNoiseFrequency);
        transform.position = blockRestPoint + blockNoiseAmplitudes * noiseValue * JigglePercentInfluence;

    }

    public void Break()
    {
        particles.Stop();

        broken = true;
        for(int i = 0; i < crackPieces.Length; i++)
        {
            crackPieces[i].Rigidbody.isKinematic = false;
            crackPieces[i].GetComponent<Collider>().enabled = true;
            Vector3 forceDir = -(transform.position - crackPieces[i].transform.position).normalized;
            crackPieces[i].Rigidbody.AddForce(forceDir*splitForce);


            crackPieces[i].Rigidbody.AddTorque(new Vector3(
                UnityEngine.Random.Range(-maxSplitTorque, maxSplitTorque), 
            UnityEngine.Random.Range(-maxSplitTorque, maxSplitTorque), 
            UnityEngine.Random.Range(-maxSplitTorque, maxSplitTorque))
            );

        }
    }

    private IEnumerator fadeBlocks()
    {
        yield return new WaitForEndOfFrame();
    }

    public void Reform()
    {
        broken = false;
        for(int i = 0; i < crackPieces.Length; i++)
        {
            crackPieces[i].Rigidbody.isKinematic = true;
            crackPieces[i].GetComponent<Collider>().enabled = false;
            crackPieces[i].Rigidbody.position = crackPieces[i].restPosition;
            crackPieces[i].Rigidbody.rotation = Quaternion.Euler(Vector3.zero);
        }
    }
}
