using UnityEngine;
using System.Collections;

public class ModelRotator : MonoBehaviour
{
    public float rotationSpeed = 100f; // Speed of rotation
    private Quaternion initialRotation; // Store the original rotation

    void Start()
    {
        // Save the initial rotation
        initialRotation = transform.rotation;
        StartCoroutine(RotateRoutine());
    }

    IEnumerator RotateRoutine()
    {
        while (true)
        {
            // Rotate 360 degrees around the forward axis
            float angleRotated = 0f;
            while (angleRotated < 360f)
            {
                float rotationStep = rotationSpeed * Time.deltaTime;
                transform.Rotate(Vector3.forward, rotationStep);
                angleRotated += rotationStep;
                yield return null;
            }

            // Ensure final rotation is exactly back to 360 degrees forward
            transform.rotation = initialRotation;

            // Wait for 4 seconds before rotating again
            yield return new WaitForSeconds(4f);
        }
    }
}
