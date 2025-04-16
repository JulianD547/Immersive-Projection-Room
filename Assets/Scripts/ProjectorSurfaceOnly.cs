using UnityEngine;

[ExecuteAlways]
public class ProjectorSurfaceOnly : MonoBehaviour
{
    public Camera projectorCamera;
    public Material projectorMaterial;

    void Start()
    {
        if (projectorCamera != null)
            projectorCamera.depthTextureMode = DepthTextureMode.Depth;
    }

    void LateUpdate()
    {
        if (projectorCamera && projectorMaterial)
        {
            Matrix4x4 viewProj = GL.GetGPUProjectionMatrix(projectorCamera.projectionMatrix, false) *
                                 projectorCamera.worldToCameraMatrix;

            projectorMaterial.SetMatrix("_ProjectorMatrix", viewProj);
            projectorMaterial.SetVector("_ProjectorCamPos", projectorCamera.transform.position);
        }
    }
}
