using UnityEngine;
using System.Collections;

/// <summary>
/// Whichever object this script is placed on will float up and down at specified amplitude and frequency
/// </summary>
public class Float : MonoBehaviour
{
    public float amplitude;
    public float frequency;

    Vector3 tempPos = new Vector3();

    void Start()
    {
        amplitude = Random.Range(0.0005f, 0.002f);
        frequency = Random.Range(0.5f, 1.2f);
    }

    void Update()
    {
        tempPos = transform.position;
        tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;

        transform.position = tempPos;
    }
}
