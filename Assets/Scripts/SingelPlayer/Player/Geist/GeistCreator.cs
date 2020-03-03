using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a randomized mask color and particle effect for a Gheist
/// </summary>
public class GeistCreator : MonoBehaviour
{
    private ParticleSystem particleSystem;

    private float startLifetime;
    private float startSpeed;
    private float startSize;
    private int emissionRate;
    private int maxParticles;
    private float gravity;

    private Color particleColor;

    public GameObject mask;
    public Sprite[] masks;

    public List<Material> particleMats = new List<Material>();

    private void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();

        int maskInt = Random.Range(0, masks.Length);

        mask.GetComponent<SpriteRenderer>().sprite = masks[maskInt];
        mask.GetComponent<SpriteRenderer>().color = RandomizeColor(0.8f);

        particleColor = RandomizeColor(0.3f);

        startLifetime = Random.Range(100, 115);
        startSpeed = startLifetime * Random.Range(0.8f, 1.10f);
        startSize = Random.Range(50, 130);
        emissionRate = Random.Range(7, 99);
        maxParticles = Random.Range(5, 1000);
        gravity = Random.Range(0, -0.1f);

        particleSystem.startLifetime = startLifetime / 100;
        particleSystem.startSpeed = startSpeed / 100;
        particleSystem.startSize = startSize / 100;
        particleSystem.emissionRate = emissionRate;
        particleSystem.maxParticles = maxParticles;
        particleSystem.GetComponent<Renderer>().material = particleMats[Random.Range(0, particleMats.Count)];
        particleSystem.startColor = particleColor;
        particleSystem.gravityModifier = gravity;

    }

    public Color RandomizeColor(float alphaMin)
    {
        float red = Random.Range(0.1f, 1);
        float green = Random.Range(0.1f, 1);
        float blue = Random.Range(0.1f, 1);
        float alpha = Random.Range(alphaMin, 0.95f);

        return new Color(red, green, blue, alpha);
    }
}
