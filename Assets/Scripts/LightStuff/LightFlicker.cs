using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class LightFlicker : MonoBehaviour
{
    private Light2D fireLight;

    [Header("Flicker Settings")]
    public float baseIntensity = 1f; 
    public float flickerRange = 0.2f;
    public float flickerSpeed = 3f;

    private float randomOffset;

    void Start()
    {
        fireLight = GetComponent<Light2D>();

        randomOffset = Random.Range(0f, 10000f);
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed + randomOffset, 0f);

        float intensityAdjustment = (noise * 2f - 1f) * flickerRange;

        fireLight.intensity = baseIntensity + intensityAdjustment;
    }
}