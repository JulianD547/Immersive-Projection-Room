using System.Collections.Generic;
using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    public GameObject qua; // Assign the cube or any GameObject in the Unity Inspector
    public List<Material> materials; // Drag and drop materials in the Unity Inspector
    private int currentMaterialIndex = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ChangeMaterial();
        }
    }

    void ChangeMaterial()
    {
        if (materials.Count == 0)
        {
            Debug.LogWarning("No materials assigned.");
            return;
        }

        // Cycle through the materials
        currentMaterialIndex = (currentMaterialIndex + 1) % materials.Count;
        
        // Apply the next material to the GameObject
        qua.GetComponent<Renderer>().material = materials[currentMaterialIndex];
    }
}
