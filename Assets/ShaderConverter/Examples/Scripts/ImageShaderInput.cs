using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
//using UnityEngine.XR;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ReformSim
{
    /// <summary>
    /// Shadertoy Inputs:
    /// vec3  iResolution   image/buffer        The viewport resolution(z is pixel aspect ratio, usually 1.0)
    /// float iTime         image/sound/buffer  Current time in seconds
    /// float iTimeDelta    image/buffer        Time it takes to render a frame, in seconds
    /// int   iFrame        image/buffer        Current frame
    /// float iFrameRate    image/buffer        Number of frames rendered per second
    /// float iChannelTime[4] image/buffer      Time for channel(if video or sound), in seconds
    /// vec3  iChannelResolution[4] image/buffer/sound Input texture resolution for each channel
    /// vec4  iMouse        image/buffer        xy = current pixel coords(if LMB is down). zw = click pixel
    /// sampler2D iChannel{i} image/buffer/sound Sampler for input textures i
    /// vec4  iDate         image/buffer/sound  Year, month, day, time in seconds in .xyzw
    /// float iSampleRate   image/buffer/sound  The sound sample rate(typically 44100)
    /// </summary>
    public class ImageShaderInput : MonoBehaviour
    {
        public int m_iFrame = 0;
        public float m_iFrameRate = 0;
        protected const float m_updateInterval = 0.5f;
        protected int m_framesCount;
        protected float m_framesTime;

        public Vector4 m_iDate;

        public float m_iSampleRate = 44100;

        public Material m_material = null;

        public bool m_isRenderToTexture = false;
        public RenderTexture m_renderTexture = null;
        
        protected Vector2 m_mouseLastClickPos;

        protected virtual void Start()
        {
            if (!m_isRenderToTexture && m_material == null)
            {
                Renderer render = GetComponent<Renderer>();
                m_material = render.material;
            }
            
            if (m_material.HasProperty("_iSampleRate"))
            {
                m_material.SetFloat("_iSampleRate", m_iSampleRate);
            }

            if (m_isRenderToTexture)
            {
                if (m_renderTexture == null)
                {
                    m_renderTexture = new RenderTexture(Screen.width, Screen.height, 0, GraphicsFormat.R32G32B32A32_SFloat);
                    m_renderTexture.useMipMap = true;
                    m_renderTexture.Create();
                }
                m_renderTexture.DiscardContents();
                Graphics.Blit(Texture2D.blackTexture, m_renderTexture);
            }
        }

        protected virtual void Update()
        {
            m_framesCount++;
            m_framesTime += Time.unscaledDeltaTime;
            if (m_framesTime > m_updateInterval)
            {
                m_iFrameRate = m_framesCount / m_framesTime;

                m_framesCount = 0;
                m_framesTime = 0;
            }

            m_iDate = new Vector4(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, (float)DateTime.Now.TimeOfDay.TotalSeconds);

            UpdateMateialProperties(m_material);

            if (m_isRenderToTexture)
            {
                Graphics.Blit(null, m_renderTexture, m_material);
            }

            m_iFrame++;
        }

        protected virtual void UpdateMateialProperties(Material mat)
        {
            if (mat == null)
            {
                return;
            }

            if (mat.HasProperty("_iFrame"))
            {
                mat.SetInt("_iFrame", m_iFrame);
            }

            if (mat.HasProperty("_iFrameRate"))
            {
                mat.SetFloat("_iFrameRate", m_iFrameRate);
            }

            if (mat.HasProperty("_iResolution"))
            {
                Vector4 iResolution = new Vector4(Screen.width, Screen.height, 1, 0);
                //if (XRSettings.enabled)
                //{
                //    iResolution = new Vector4(XRSettings.eyeTextureWidth, XRSettings.eyeTextureHeight, 1, 0);
                //}
                mat.SetVector("_iResolution", iResolution);
            }

            // mouse input:
            //      mouse.xy  = mouse position during last button down
            //  abs(mouse.zw) = mouse position during last button click
            // sign(mouze.z)  = button is down
            // sign(mouze.w)  = button is clicked
            if (mat.HasProperty("_iMouse"))
            {
#if ENABLE_INPUT_SYSTEM
                Vector2 mouseCurrentPos = Mouse.current.position.ReadValue();
                if (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.middleButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
                {
                    Vector4 mousePosition = new Vector4(mouseCurrentPos.x, mouseCurrentPos.y, mouseCurrentPos.x, mouseCurrentPos.y);
                    mat.SetVector("_iMouse", mousePosition);
                    m_mouseLastClickPos = mouseCurrentPos;
                }
                else if (Mouse.current.leftButton.isPressed || Mouse.current.middleButton.isPressed || Mouse.current.rightButton.isPressed)
                {
                    Vector4 mousePosition = new Vector4(mouseCurrentPos.x, mouseCurrentPos.y, m_mouseLastClickPos.x, -m_mouseLastClickPos.y);
                    mat.SetVector("_iMouse", mousePosition);
                }
                else if (Mouse.current.leftButton.wasReleasedThisFrame || Mouse.current.middleButton.wasReleasedThisFrame || Mouse.current.rightButton.wasReleasedThisFrame)
                {
                    Vector4 mousePosition = new Vector4(mouseCurrentPos.x, mouseCurrentPos.y, -m_mouseLastClickPos.x, -m_mouseLastClickPos.y);
                    mat.SetVector("_iMouse", mousePosition);
                }
#else
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                {
                    Vector4 mousePosition = new Vector4(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.x, Input.mousePosition.y);                
                    mat.SetVector("_iMouse", mousePosition);
                    m_mouseLastClickPos = Input.mousePosition;
                }
                else if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
                {
                    Vector4 mousePosition = new Vector4(Input.mousePosition.x, Input.mousePosition.y, m_mouseLastClickPos.x, -m_mouseLastClickPos.y);
                    mat.SetVector("_iMouse", mousePosition);
                }
                else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
                {
                    Vector4 mousePosition = new Vector4(Input.mousePosition.x, Input.mousePosition.y, -m_mouseLastClickPos.x, -m_mouseLastClickPos.y);
                    mat.SetVector("_iMouse", mousePosition);
                }
#endif
            }

            if (mat.HasProperty("_iDate"))
            {
                mat.SetVector("_iDate", m_iDate);
            }
        }
    }
}