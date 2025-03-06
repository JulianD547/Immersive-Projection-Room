using UnityEngine;

public class ArchString : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int numberOfPoints = 10;  // Number of points in the curve
    public float archHeight = 5f;    // Height of the arch

    void Start()
    {
        // Initialize the LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        
        Vector3[] points = new Vector3[numberOfPoints];
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i / (float)(numberOfPoints - 1);
            float x = Mathf.Lerp(-5f, 5f, t);
            float y = Mathf.Sin(t * Mathf.PI) * archHeight;  // Create an arch with sine wave
            points[i] = new Vector3(x, y, 0);
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }
}
