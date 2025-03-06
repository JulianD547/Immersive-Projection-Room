using UnityEngine;

public class MaterialColorChanger : MonoBehaviour
{
    public Material material; // The material to change color
    public float colorChangeSpeed = 1f; // Speed of color change
    private Color emissionColor; // Store the emission color

    void Start()
    {
        // Enable emission on the material
        material.EnableKeyword("_EMISSION");
        // Set a fixed emission color
        emissionColor = Color.white * 4f; // Change 4f to your desired intensity
        material.SetColor("_EmissionColor", emissionColor);
    }

    void Update()
    {
        // Generate a new color over time using a sine function for smooth transitions
        float r = Mathf.Sin(Time.time * colorChangeSpeed) * 0.5f + 0.5f;
        float g = Mathf.Sin((Time.time + 2f) * colorChangeSpeed) * 0.5f + 0.5f;
        float b = Mathf.Sin((Time.time + 4f) * colorChangeSpeed) * 0.5f + 0.5f;

        // Apply the new color to the material
        Color newColor = new Color(r, g, b);
        material.color = newColor;

        // Keep the emission color constant
        material.SetColor("_EmissionColor", emissionColor);
    }
}
