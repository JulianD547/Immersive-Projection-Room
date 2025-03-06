using UnityEngine;

namespace ReformSim
{
    public class ImageShaderMusicInput : MonoBehaviour
    {
        public AudioSource m_audioSource;

        [Tooltip("Number of values (the length of the samples array provided) must be a power of 2. (ie 128/256/512 etc).")]
        [Range(512, 8192)]
        public int m_sampleNum = 512;

        [Tooltip("Use window to reduce leakage between frequency bins/bands. Note, the more complex window type, the better the quality, but reduced speed.")]
        public FFTWindow m_fftWindow = FFTWindow.BlackmanHarris;

        public enum TextureNameType
        {
            MainTex,
            SecondTex,
            ThirdTex,
            FourthTex,
        }

        public TextureNameType m_textureNameType = TextureNameType.MainTex;
        protected string m_textureName;

        public float m_iSampleRate = 44100;

        public float m_spectrumMultiplier = 1.5f;
        public float m_spectrumOffset = 1.0f;
        //public float m_spectrumMapLogBase = 10;

        [Range(1f, 100.0f)]
        public float m_spectrumSmooth = 20.0f;
        protected float[] m_prevSpectrumData;

        public float m_waveAmplitudeOffset = 1.0f;
        public float m_waveAmplitudeMultiplier = 0.5f;

        protected float[] m_spectrumData;
        protected Color[] m_spectrumColorArray;

        protected float[] m_waveAmplitudeData;
        protected Color[] m_waveAmplitudeColorArray;

        public Texture2D m_audioDataTex;
        public FilterMode m_texFilterMode = FilterMode.Point;
        public TextureWrapMode m_texWrapMode = TextureWrapMode.Clamp;
        protected const int m_audioDataTexWidth = 512;

        public Material m_material = null;

        protected void Start()
        {
            if (m_audioSource == null)
            {
                m_audioSource = GetComponent<AudioSource>();
            }

            if (m_audioSource == null)
            {
                Debug.LogError("Error: Please assign a AudioSource component first!", this);
                return;
            }

            if (m_audioSource.clip == null)
            {
                Debug.LogError("Error: Please assign a clip to the AudioSource component first!", this);
                return;
            }

            m_iSampleRate = m_audioSource.clip.frequency;

            m_spectrumData = new float[m_sampleNum];
            m_spectrumColorArray = new Color[m_sampleNum];

            m_prevSpectrumData = new float[m_sampleNum];

            m_waveAmplitudeData = new float[m_sampleNum];
            m_waveAmplitudeColorArray = new Color[m_sampleNum];

            m_audioDataTex = new Texture2D(m_audioDataTexWidth, 2, TextureFormat.R16, true, true);
            m_audioDataTex.filterMode = m_texFilterMode;
            m_audioDataTex.wrapMode = m_texWrapMode;

            if (m_material == null)
            {
                Renderer render = GetComponent<Renderer>();
                m_material = render.material;
            }

            m_textureName = "_" + m_textureNameType.ToString();
            m_material.SetTexture(m_textureName, m_audioDataTex);
        }

        protected void Update()
        {
            if (m_audioSource == null || m_audioSource.clip == null)
            {
                return;
            }

            if (m_material.HasProperty("_iSampleRate"))
            {
                m_material.SetFloat("_iSampleRate", m_iSampleRate);
            }

            if (m_material.HasProperty("_iChannelTime"))
            {
                float[] channelTimeArray = new float[] { m_audioSource.time, m_audioSource.time, m_audioSource.time, m_audioSource.time };
                m_material.SetFloatArray("_iChannelTime", channelTimeArray);
            }

            //AudioSettings.outputSampleRate = 12000;
            m_audioSource.GetSpectrumData(m_spectrumData, 0, m_fftWindow);
            m_audioSource.GetOutputData(m_waveAmplitudeData, 0);

            float minSpectrum = float.MaxValue;
            float maxSpectrum = float.MinValue;
            for (int i = 0; i < m_audioDataTex.width; i++)
            {
                m_spectrumData[i] = m_spectrumData[i] * (m_spectrumMultiplier * i + m_spectrumOffset);
                //if (!Mathf.Approximately(m_spectrumData[i], 0))
                //{
                //    m_spectrumData[i] = Mathf.Log(m_spectrumData[i], m_spectrumMapLogBase) /** m_spectrumMultiplier*/;
                //    m_spectrumData[i] = Mathf.Log((m_spectrumData[i]/((i+1)*(i+1))), m_spectrumMapLogBase);
                //}

                minSpectrum = Mathf.Min(minSpectrum, m_spectrumData[i]);
                maxSpectrum = Mathf.Max(maxSpectrum, m_spectrumData[i]);
            }
            float spectrumRange = maxSpectrum - minSpectrum;
            if (Mathf.Approximately(spectrumRange, 0))
            {
                return;
            }

            //Debug.Log(string.Format("Range: {0}; min: {1}; max: {2}", spectrumRange, minSpectrum, maxSpectrum));
            //Debug.Log(string.Format("m_spectrumData[0]: {0}", m_spectrumData[0]));

            //float rawDeltaFrequency = AudioSettings.outputSampleRate / (float)m_sampleNum;
            //float deltaFrequency = AudioSettings.outputSampleRate / (4.0f * m_audioDataTexWidth);
            //for (int i = 0; i < m_audioDataTex.width; i++)
            //{
            //    float currFrequency = i * deltaFrequency;
            //    int index = (int)(currFrequency / rawDeltaFrequency);
            //    float t = (currFrequency % rawDeltaFrequency) / rawDeltaFrequency;
            //    //float spectrumData = m_spectrumData[index];
            //    float spectrumData = Mathf.Lerp(m_spectrumData[index], m_spectrumData[index + 1], t);
            //    float normalizedSpectrum = (spectrumData - minSpectrum) / spectrumRange;
            //    Color c = new Color(normalizedSpectrum, 0, 0);
            //    //Color c = new Color(m_spectrumData[i] * m_frequencyMultiplier, 0, 0);
            //    //m_audioDataTex.SetPixel(i, 0, c);
            //    m_spectrumColorArray[i] = c;
            //}
            //m_audioDataTex.SetPixels(0, 0, m_audioDataTex.width, 1, m_spectrumColorArray, 0);

            for (int i = 0; i < m_audioDataTex.width; i++)
            {
                float spectrumData = m_spectrumData[i];
                float normalizedSpectrum = (spectrumData - minSpectrum) / spectrumRange * m_spectrumMultiplier;

                normalizedSpectrum = Mathf.Lerp(m_prevSpectrumData[i], normalizedSpectrum, m_spectrumSmooth * Time.deltaTime);

                Color c = new Color(normalizedSpectrum, 0, 0);
                //m_audioDataTex.SetPixel(i, 0, c);
                m_spectrumColorArray[i] = c;

                m_prevSpectrumData[i] = normalizedSpectrum;
            }
            m_audioDataTex.SetPixels(0, 0, m_audioDataTex.width, 1, m_spectrumColorArray, 0);

            for (int i = 0; i < m_audioDataTex.width; i++)
            {
                Color c = new Color((m_waveAmplitudeData[i] + m_waveAmplitudeOffset) * m_waveAmplitudeMultiplier, 0, 0);
                //m_audioDataTex.SetPixel(i, 1, c);
                m_waveAmplitudeColorArray[i] = c;
            }
            m_audioDataTex.SetPixels(0, 1, m_audioDataTex.width, 1, m_waveAmplitudeColorArray, 0);

            m_audioDataTex.Apply();
        }

//#if UNITY_EDITOR
//        public bool m_debugShowRenderTextures = false;

//        protected void OnGUI()
//        {
//            ShowRenderTextures(m_debugShowRenderTextures);
//        }
//#endif

        public void ShowRenderTextures(bool showRenderTexture)
        {
            if (showRenderTexture)
            {
                if (m_audioDataTex != null)
                {
                    GUI.DrawTexture(new Rect(0, 100, m_audioDataTex.width*2, m_audioDataTex.height*100), m_audioDataTex, ScaleMode.StretchToFill, false);
                }
            }
        }

        protected void OnDestroy()
        {
            Destroy(m_audioDataTex);
        }
    }
}