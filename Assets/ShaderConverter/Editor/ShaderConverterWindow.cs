﻿using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ReformSim
{
    public enum ShaderLangType
    {
        ShaderToy,
        //OpenGLShader,
        //VulkanShader,
        //MetalShader,
    }

    public enum ShaderStage
    {
        Vertex = 0,

        Fragment = 4,
    }

    public class ShaderConverterWindow : EditorWindow
    {
        public string m_shaderName = "Custom/CustomShader";
        protected string m_shaderNameTemplate = "Custom/CustomShader";
        protected int m_nameIndex = 1;

        protected int m_selectedTabIndex = 0;
        protected int m_lastSelectedTabIndex = 0;

        public string m_glslFilePathName = "";
        public string m_glslFileDefaultPathName = "Assets/ShaderConverter/Examples/GLSL";

        public ShaderLangType m_inputShaderType;

        public ShaderStage m_inputShaderStage = ShaderStage.Fragment;

        public bool m_saveDebugFile;
        public string m_tempGLSLFileName = "TempShader.frag";
        public string m_tempHLSLFileName = "TempShader.hlsl";

        protected string[] m_shaderContentTypeArray = { "Image (Main)", "Common", "Buffer A", "Buffer B", "Buffer C", "Buffer D" };

        public string m_inputMainShaderContent;
        public string m_outputMainShaderContent;

        public string m_inputCommonShaderContent;
        public string m_outputCommonShaderContent;

        public string m_inputBufferAContent;
        public string m_outputBufferAContent;

        public string m_inputBufferBContent;
        public string m_outputBufferBContent;

        public string m_inputBufferCContent;
        public string m_outputBufferCContent;

        public string m_inputBufferDContent;
        public string m_outputBufferDContent;

        public Vector2 m_windowScrollPos;
        public Vector2 m_inputTextAreaScrollPos;
        public Vector2 m_outputTextAreaScrollPos;

        protected string[] m_mainShaderTypeArray = { "Object", "Full Screen" };
        protected int m_mainShaderType;
        protected bool m_extractProperties;
        protected string[] m_renderTypeArray = { "Opaque", "Transparent", "TransparentCutout", "Background", "Overlay", "TreeOpaque", "TreeTransparentCutout", "TreeBillboard", "Grass", "GrassBillboard" };
        protected int m_renderType;
        protected string[] m_renderQueueArray = { "Background", "Geometry", "AlphaTest", "GeometryLast", "Transparent", "Overlay" };
        protected int m_renderQueue = 1;
        protected string[] m_cullTypeArray = { "Back", "Off", "Front" };
        protected int m_cullType;
        protected string[] m_zWriteTypeArray = { "On", "Off" };
        protected int m_zWriteType;
        protected string[] m_zTestTypeArray = { "Less", "LEqual", "Equal", "GEqual", "Greater", "NotEqual", "Always" };
        protected int m_zTestType = 1;
        protected string[] m_blendTypeArray = { "Off",
            "SrcAlpha OneMinusSrcAlpha",  // Traditional transparency
            "One OneMinusSrcAlpha",   // Premultiplied transparency
            "One One",     // Additive
            "OneMinusDstColor One", // Soft additive
            "DstColor Zero",  // Multiplicative
            "DstColor SrcColor" };    // 2x multiplicative
        protected int m_blendType;

        protected int m_pragmaTarget = 1;
        protected string[] m_pragmaTargetArray = { "3.5", "4.0", "4.5", "4.6", "5.0" };

        protected int[] m_iChannel0Type = { 0, 0, 0, 0, 0, 0 };
        protected int[] m_iChannel1Type = { 0, 0, 0, 0, 0, 0 };
        protected int[] m_iChannel2Type = { 0, 0, 0, 0, 0, 0 };
        protected int[] m_iChannel3Type = { 0, 0, 0, 0, 0, 0 };
        protected string[] m_textureTypeArray = { "2D", "Cube" };

        public string m_shaderFilePathName = "Assets/ShaderConverter/Examples/Shaders/CustomShader.shader";
        public string m_materialFilePathName = "Assets/ShaderConverter/Examples/Materials/CustomMaterial.mat";

        [MenuItem("Tools/Shader Converter...")]
        public static void OpenWindow()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(ShaderConverterWindow));
            window.minSize = new Vector2(800, 450);
            window.titleContent = new GUIContent("Shader Converter");
            window.position = new Rect(320, 180, 1500, 800);
        }

        void OnGUI()
        {
            m_windowScrollPos = EditorGUILayout.BeginScrollView(m_windowScrollPos);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            m_shaderName = EditorGUILayout.TextField("Shader Name: ", m_shaderName);
            if (GUILayout.Button("New", GUILayout.MaxWidth(80)))
            {
                CreateNewShader();
            }
            // Check if name of shader has changed
            if (EditorGUI.EndChangeCheck())
            {
                OnShaderNameChanged(m_shaderName);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_glslFilePathName = EditorGUILayout.TextField("Input shader file: ", m_glslFilePathName);
            if (GUILayout.Button("Open...", GUILayout.MaxWidth(80)))
            {
                string glslFilePathName = EditorUtility.OpenFilePanel("Open GLSL Shader File Dialog", m_glslFileDefaultPathName, "");
                if (!string.IsNullOrEmpty(glslFilePathName))
                {
                    m_glslFilePathName = glslFilePathName;
                    string inputMainShaderContent = File.ReadAllText(m_glslFilePathName);
                    SetInputShaderContent(m_selectedTabIndex, inputMainShaderContent);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Input Shader Language: ", GUILayout.Width(150));
            m_inputShaderType = (ShaderLangType)EditorGUILayout.EnumPopup(m_inputShaderType, GUILayout.MaxWidth(130));

            if (m_inputShaderType != ShaderLangType.ShaderToy)
            {
                EditorGUILayout.LabelField("Stage:", GUILayout.Width(40));
                m_inputShaderStage = (ShaderStage)EditorGUILayout.EnumPopup(m_inputShaderStage, GUILayout.MaxWidth(100));
            }
            EditorGUILayout.Space();

            m_saveDebugFile = EditorGUILayout.ToggleLeft("Save Debug File", m_saveDebugFile, GUILayout.Width(120));
            if (GUILayout.Button("Clear All", GUILayout.Width(80)))
            {
                ClearAllContents();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            m_selectedTabIndex = GUILayout.Toolbar(m_selectedTabIndex, m_shaderContentTypeArray);

            if (m_selectedTabIndex != m_lastSelectedTabIndex)
            {
                GUIUtility.keyboardControl = 0;
                m_lastSelectedTabIndex = m_selectedTabIndex;
            }

            m_inputTextAreaScrollPos = EditorGUILayout.BeginScrollView(m_inputTextAreaScrollPos);
            switch (m_selectedTabIndex)
            {
                case 0:
                    m_inputMainShaderContent = EditorGUILayout.TextArea(m_inputMainShaderContent, GUILayout.ExpandHeight(true), GUILayout.MinWidth(400));
                    break;
                case 1:
                    m_inputCommonShaderContent = EditorGUILayout.TextArea(m_inputCommonShaderContent, GUILayout.ExpandHeight(true), GUILayout.MinWidth(400));
                    break;
                case 2:
                    m_inputBufferAContent = EditorGUILayout.TextArea(m_inputBufferAContent, GUILayout.ExpandHeight(true), GUILayout.MinWidth(400));
                    break;
                case 3:
                    m_inputBufferBContent = EditorGUILayout.TextArea(m_inputBufferBContent, GUILayout.ExpandHeight(true), GUILayout.MinWidth(400));
                    break;
                case 4:
                    m_inputBufferCContent = EditorGUILayout.TextArea(m_inputBufferCContent, GUILayout.ExpandHeight(true), GUILayout.MinWidth(400));
                    break;
                case 5:
                    m_inputBufferDContent = EditorGUILayout.TextArea(m_inputBufferDContent, GUILayout.ExpandHeight(true), GUILayout.MinWidth(400));
                    break;
            }
            EditorGUILayout.EndScrollView();

            if (m_inputShaderType == ShaderLangType.ShaderToy && m_selectedTabIndex != 1)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("iChannel0:", GUILayout.Width(65));
                m_iChannel0Type[m_selectedTabIndex] = EditorGUILayout.Popup(m_iChannel0Type[m_selectedTabIndex], m_textureTypeArray, GUILayout.MaxWidth(60));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("iChannel1:", GUILayout.Width(65));
                m_iChannel1Type[m_selectedTabIndex] = EditorGUILayout.Popup(m_iChannel1Type[m_selectedTabIndex], m_textureTypeArray, GUILayout.MaxWidth(60));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("iChannel2:", GUILayout.Width(65));
                m_iChannel2Type[m_selectedTabIndex] = EditorGUILayout.Popup(m_iChannel2Type[m_selectedTabIndex], m_textureTypeArray, GUILayout.MaxWidth(60));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("iChannel3:", GUILayout.Width(65));
                m_iChannel3Type[m_selectedTabIndex] = EditorGUILayout.Popup(m_iChannel3Type[m_selectedTabIndex], m_textureTypeArray, GUILayout.MaxWidth(60));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
            }

            EditorGUILayout.EndVertical();

            if (!string.IsNullOrEmpty(m_outputMainShaderContent))
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Output Shader: ");
                if (m_saveDebugFile)
                {
                    if (m_inputShaderType == ShaderLangType.ShaderToy)
                    {
                        if (GUILayout.Button("Show GLSL"))
                        {
                            if (!File.Exists(m_tempGLSLFileName))
                            {
                                Debug.LogError("Error: Please convert code first! Temp GLSL file doesn't exists: " + m_tempGLSLFileName);
                            }
                            else
                            {
                                m_outputMainShaderContent = File.ReadAllText(m_tempGLSLFileName);
                            }
                        }
                    }
                    if (GUILayout.Button("Show HLSL"))
                    {
                        if (!File.Exists(m_tempHLSLFileName))
                        {
                            Debug.LogError("Error: Please convert code first! Temp HLSL file doesn't exists: " + m_tempHLSLFileName);
                        }
                        else
                        {
                            m_outputMainShaderContent = File.ReadAllText(m_tempHLSLFileName);
                        }
                    }
                    if (GUILayout.Button("Show Unity Shader"))
                    {
                        Convert(m_selectedTabIndex);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Output Shader Content: " + m_shaderContentTypeArray[m_selectedTabIndex]);

                m_outputTextAreaScrollPos = EditorGUILayout.BeginScrollView(m_outputTextAreaScrollPos);
                switch (m_selectedTabIndex)
                {
                    case 0:
                        m_outputMainShaderContent = EditorGUILayout.TextArea(m_outputMainShaderContent, GUILayout.ExpandHeight(true), GUILayout.MinWidth(640));
                        break;
                    case 1:
                        m_outputCommonShaderContent = EditorGUILayout.TextArea(m_outputCommonShaderContent, GUILayout.ExpandHeight(true), GUILayout.MinWidth(640));
                        break;
                    case 2:
                        m_outputBufferAContent = EditorGUILayout.TextArea(m_outputBufferAContent, GUILayout.ExpandHeight(true), GUILayout.MinWidth(640));
                        break;
                    case 3:
                        m_outputBufferBContent = EditorGUILayout.TextArea(m_outputBufferBContent, GUILayout.ExpandHeight(true), GUILayout.MinWidth(640));
                        break;
                    case 4:
                        m_outputBufferCContent = EditorGUILayout.TextArea(m_outputBufferCContent, GUILayout.ExpandHeight(true), GUILayout.MinWidth(640));
                        break;
                    case 5:
                        m_outputBufferDContent = EditorGUILayout.TextArea(m_outputBufferDContent, GUILayout.ExpandHeight(true), GUILayout.MinWidth(640));
                        break;
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            //GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("label"));
            //labelStyle.fixedWidth = 80;
            //labelStyle.stretchWidth = false;
            //GUIStyle popupStyle = new GUIStyle(GUI.skin.GetStyle("popup"));
            //popupStyle.fontSize = 12;
            //popupStyle.fixedHeight = 18;
            //popupStyle.fixedWidth = 100;

            if (m_selectedTabIndex == 0)
            {
                EditorGUILayout.LabelField("Shader Type:", GUILayout.Width(80));
                m_mainShaderType = EditorGUILayout.Popup(m_mainShaderType, m_mainShaderTypeArray, GUILayout.Width(100));
            }
            EditorGUILayout.Space();
            m_extractProperties = EditorGUILayout.ToggleLeft("Extract Properties", m_extractProperties, GUILayout.Width(120));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Render Type:", GUILayout.Width(80));
            m_renderType = EditorGUILayout.Popup(m_renderType, m_renderTypeArray, GUILayout.MinWidth(130));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Render Queue:", GUILayout.Width(90));
            m_renderQueue = EditorGUILayout.Popup(m_renderQueue, m_renderQueueArray, GUILayout.MinWidth(100));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Cull:", GUILayout.Width(30));
            m_cullType = EditorGUILayout.Popup(m_cullType, m_cullTypeArray, GUILayout.MinWidth(50));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Z Write:", GUILayout.Width(50));
            m_zWriteType = EditorGUILayout.Popup(m_zWriteType, m_zWriteTypeArray, GUILayout.MinWidth(40));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Z Test:", GUILayout.Width(40));
            m_zTestType = EditorGUILayout.Popup(m_zTestType, m_zTestTypeArray, GUILayout.MinWidth(80));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Blend:", GUILayout.Width(40));
            m_blendType = EditorGUILayout.Popup(m_blendType, m_blendTypeArray, GUILayout.MinWidth(150));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Pragma Target:", GUILayout.Width(90));
            m_pragmaTarget = EditorGUILayout.Popup(m_pragmaTarget, m_pragmaTargetArray, GUILayout.MinWidth(50));
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Convert All Shaders"))
            {
                ConvertAll();
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            m_shaderFilePathName = EditorGUILayout.TextField("Shader File Path Name: ", m_shaderFilePathName);
            if (GUILayout.Button("Browse...", GUILayout.MaxWidth(80)))
            {
                string shaderFilePathName = EditorUtility.SaveFilePanel("Save Shader File Dialog", "", "CustomShader", "shader");
                if (!string.IsNullOrEmpty(shaderFilePathName))
                {
                    m_shaderFilePathName = shaderFilePathName;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Save Shader(s) To File(s)"))
            {
                SaveAllShaders(m_shaderFilePathName);
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            m_materialFilePathName = EditorGUILayout.TextField("Material File Path Name: ", m_materialFilePathName);
            if (GUILayout.Button("Browse...", GUILayout.MaxWidth(80)))
            {
                string materialFilePathName = EditorUtility.SaveFilePanelInProject("Save Material File Dialog", "CustomMaterial", "mat", "Save Material File");
                if (!string.IsNullOrEmpty(materialFilePathName))
                {
                    m_materialFilePathName = materialFilePathName;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Create Material(s) From Shader(s)"))
            {
                CreateAllMaterials(m_shaderName, m_materialFilePathName);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("About Shader Converter: Version: 1.7.1");
            EditorGUILayout.LabelField("(c) 2025 Reform Sim. All Rights Reserved.");

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        protected void OnShaderNameChanged(string shaderName)
        {
            string shaderFileName = shaderName;
            int tmpIndex = shaderFileName.LastIndexOf("/");
            if (tmpIndex >= 0)
            {
                shaderFileName = shaderFileName.Substring(tmpIndex + 1);
            }

            tmpIndex = m_shaderFilePathName.LastIndexOf("/");
            if (tmpIndex >= 0)
            {
                m_shaderFilePathName = m_shaderFilePathName.Substring(0, tmpIndex+1) + shaderFileName + ".shader";
            }

            //tmpIndex = m_materialFilePathName.LastIndexOf("/");
            //if (tmpIndex >= 0)
            //{
            //    m_materialFilePathName = m_materialFilePathName.Substring(0, tmpIndex+1) + shaderFileName + ".mat";
            //}
        }

        protected string GetInputShaderContent(int index)
        {
            switch (index)
            {
                case 0:
                    return m_inputMainShaderContent;
                case 1:
                    return m_inputCommonShaderContent;
                case 2:
                    return m_inputBufferAContent;
                case 3:
                    return m_inputBufferBContent;
                case 4:
                    return m_inputBufferCContent;
                case 5:
                    return m_inputBufferDContent;
                default:
                    break;
            }

            return string.Empty;
        }

        protected void SetInputShaderContent(int index, string inputShaderContent)
        {
            switch (index)
            {
                case 0:
                    m_inputMainShaderContent = inputShaderContent;
                    break;
                case 1:
                    m_inputCommonShaderContent = inputShaderContent;
                    break;
                case 2:
                    m_inputBufferAContent = inputShaderContent;
                    break;
                case 3:
                    m_inputBufferBContent = inputShaderContent;
                    break;
                case 4:
                    m_inputBufferCContent = inputShaderContent;
                    break;
                case 5:
                    m_inputBufferDContent = inputShaderContent;
                    break;
                default:
                    break;
            }
        }

        protected void SetOutputShaderContent(int index, string outputShaderContent)
        {
            switch (index)
            {
                case 0:
                    m_outputMainShaderContent = outputShaderContent;
                    break;
                case 1:
                    m_outputCommonShaderContent = outputShaderContent;
                    break;
                case 2:
                    m_outputBufferAContent = outputShaderContent;
                    break;
                case 3:
                    m_outputBufferBContent = outputShaderContent;
                    break;
                case 4:
                    m_outputBufferCContent = outputShaderContent;
                    break;
                case 5:
                    m_outputBufferDContent = outputShaderContent;
                    break;
                default:
                    break;
            }
        }

        protected void CreateNewShader()
        {
            m_nameIndex++;
            m_shaderName = m_shaderNameTemplate + m_nameIndex;
            m_inputShaderType = ShaderLangType.ShaderToy;
            m_inputShaderStage = ShaderStage.Fragment;
            m_saveDebugFile = false;

            ClearAllContents();

            m_renderType = 0;
            m_renderQueue = 1;
            m_cullType = 0;
            m_zWriteType = 0;
            m_zTestType = 1;
            m_blendType = 0;
            m_pragmaTarget = 1;

            for (int i=0; i<6; ++i)
            {
                m_iChannel0Type[i] =0;
                m_iChannel1Type[i] = 0;
                m_iChannel2Type[i] = 0;
                m_iChannel3Type[i] = 0;
            }

            m_shaderFilePathName = Regex.Replace(m_shaderFilePathName, @"CustomShader(\d*).shader", string.Format("CustomShader{0}.shader", m_nameIndex));
            m_materialFilePathName = Regex.Replace(m_materialFilePathName, @"CustomMaterial(\d*).mat", string.Format("CustomMaterial{0}.mat", m_nameIndex));
        }

        protected void ConvertAll()
        {
            for (int i = 0; i < 6; ++i)
            {
                if (i == 1)
                {
                    continue;
                }

                string inputShaderContent = GetInputShaderContent(i);
                if (string.IsNullOrEmpty(inputShaderContent) || string.IsNullOrWhiteSpace(inputShaderContent))
                {
                    continue;
                }

                Convert(i);
            }

            GUIUtility.keyboardControl = 0;
        }

        protected void Convert(int selectedTabIndex)
        {
            if (selectedTabIndex == 1)
            {
                Debug.LogWarning("Warning: Can't convert common shader separately.");
                return;
            }

            string inputShaderContent = GetInputShaderContent(selectedTabIndex);
            if (string.IsNullOrEmpty(inputShaderContent) || string.IsNullOrWhiteSpace(inputShaderContent))
            {
                Debug.LogWarning("Warning: Please input or paste working shader source code first.");
                return;
            }

            string commonShaderContent = GetInputShaderContent(1);
            if (!string.IsNullOrEmpty(commonShaderContent))
            {
                inputShaderContent = commonShaderContent + "\n" + inputShaderContent;
            }

            string shaderName = m_shaderName;
            int mainTexType = m_iChannel0Type[selectedTabIndex];
            int secondTexType = m_iChannel1Type[selectedTabIndex];
            int thirdTexType = m_iChannel2Type[selectedTabIndex];
            int fourthTexType = m_iChannel3Type[selectedTabIndex];
            bool isFullScreenShader = (m_mainShaderType == 1) ? true : false;
            bool isBufferShader = false;
            if (selectedTabIndex == 2)
            {
                shaderName = m_shaderName + "-BufferA";
                isFullScreenShader = false;
                isBufferShader = true;
            }
            else if (selectedTabIndex == 3)
            {
                shaderName = m_shaderName + "-BufferB";
                isFullScreenShader = false;
                isBufferShader = true;
            }
            else if (selectedTabIndex == 4)
            {
                shaderName = m_shaderName + "-BufferC";
                isFullScreenShader = false;
                isBufferShader = true;
            }
            else if (selectedTabIndex == 5)
            {
                shaderName = m_shaderName + "-BufferD";
                isFullScreenShader = false;
                isBufferShader = true;
            }

            string outputShaderContent = Convert(inputShaderContent, shaderName, mainTexType, secondTexType, thirdTexType, fourthTexType, isFullScreenShader, isBufferShader, m_extractProperties);
            SetOutputShaderContent(selectedTabIndex, outputShaderContent);
        }
        
        protected string Convert(string inputShaderContent, string shaderName, int mainTexType, int secondTexType, int thirdTexType, int fourthTexType, bool isFullScreenShader, bool isBufferShader, bool extractProperties)
        {
            if (string.IsNullOrEmpty(inputShaderContent) || string.IsNullOrWhiteSpace(inputShaderContent))
            {
                Debug.LogWarning("Warning: Please input or paste working shader source code first.");
                return "";
            }

            string renderType = m_renderTypeArray[m_renderType];
            string renderQueue = m_renderQueueArray[m_renderQueue];
            string cullType = "Cull " + m_cullTypeArray[m_cullType];
            string zWriteType = "ZWrite " + m_zWriteTypeArray[m_zWriteType];
            string zTestType = "ZTest " + m_zTestTypeArray[m_zTestType];
            string blendType = "Blend " + m_blendTypeArray[m_blendType];
            string pragmaTarget = m_pragmaTargetArray[m_pragmaTarget];
            string outputShaderContent = Convert(inputShaderContent, shaderName, m_inputShaderStage,
                renderType, renderQueue, cullType, zWriteType, zTestType, blendType, pragmaTarget,
                mainTexType, secondTexType, thirdTexType, fourthTexType, isFullScreenShader, isBufferShader, extractProperties);
            
            return outputShaderContent;
        }

        protected string Convert(string inputShaderContent, string shaderName, ShaderStage inputShaderStage,
            string renderType, string renderQueue, string cullType, string zWriteType, string zTestType, string blendType, string pragmaTarget,
            int mainTexType, int secondTexType, int thirdTexType, int fourthTexType, bool isFullScreenShader, bool isBufferShader, bool extractProperties)
        {
            string outputShaderContent = string.Empty;
            if (m_inputShaderType == ShaderLangType.ShaderToy)
            {
                outputShaderContent = ImageShaderConverter.Convert(inputShaderContent, shaderName,
                    renderType, renderQueue, cullType, zWriteType, zTestType, blendType, pragmaTarget, m_saveDebugFile,
                    mainTexType, secondTexType, thirdTexType, fourthTexType, isFullScreenShader, isBufferShader, extractProperties);
            }
            //else if (m_inputShaderType == ShaderLangType.OpenGLShader)
            //{
            //    outputShaderContent = GLSLConverter.Convert(inputShaderContent, shaderName, inputShaderStage,
            //        renderType, renderQueue, cullType, zWriteType, zTestType, blendType, pragmaTarget, m_saveDebugFile);
            //}

            return outputShaderContent;
        }

        protected void SaveAllShaders(string filePathName)
        {
            string filePath = Path.GetDirectoryName(filePathName);
            string fileName = Path.GetFileNameWithoutExtension(filePathName);

            bool ret = false;
            if (!string.IsNullOrEmpty(m_outputBufferAContent))
            {
                string bufferShaderFilePathName = filePath + "/" + fileName + "-BufferA.shader";
                ret = SaveShader(m_outputBufferAContent, bufferShaderFilePathName);
            }

            if (!string.IsNullOrEmpty(m_outputBufferBContent))
            {
                string bufferShaderFilePathName = filePath + "/" + fileName + "-BufferB.shader";
                ret = SaveShader(m_outputBufferBContent, bufferShaderFilePathName);
            }

            if (!string.IsNullOrEmpty(m_outputBufferCContent))
            {
                string bufferShaderFilePathName = filePath + "/" + fileName + "-BufferC.shader";
                ret = SaveShader(m_outputBufferCContent, bufferShaderFilePathName);
            }

            if (!string.IsNullOrEmpty(m_outputBufferDContent))
            {
                string bufferShaderFilePathName = filePath + "/" + fileName + "-BufferD.shader";
                ret = SaveShader(m_outputBufferDContent, bufferShaderFilePathName);
            }

            if (!string.IsNullOrEmpty(m_outputMainShaderContent))
            {
                ret = SaveShader(m_outputMainShaderContent, filePathName);
            }

            if (ret)
            {
                Debug.Log("All Shaders are saved successfully.");
            }
        }

        protected bool SaveShader(string shaderContent, string filePathName)
        {
            if (string.IsNullOrEmpty(shaderContent) || string.IsNullOrEmpty(filePathName))
            {
                Debug.LogWarning("Warning: Please convert shader source code first.");
                return false;
            }

            bool ret = ShaderConverter.Save(shaderContent, filePathName);
            if (!ret)
            {
                Debug.LogWarning("Warning: Failed to save shader: " + filePathName);
                return false;
            }
            
            AssetDatabase.Refresh();
            Debug.Log("Shader is saved to file successfully: " + filePathName);

            return true;
        }

        protected bool CreateAllMaterials(string shaderName, string matFilePathName)
        {
            string filePath = Path.GetDirectoryName(matFilePathName);
            string fileName = Path.GetFileNameWithoutExtension(matFilePathName);
            
            if (!string.IsNullOrEmpty(m_outputBufferAContent))
            {
                string bufferShaderFilePathName = shaderName + "-BufferA";
                string bufferMatFilePathName = filePath + "/" + fileName + "-BufferA.mat";
                CreateMaterial(bufferShaderFilePathName, bufferMatFilePathName);
            }

            if (!string.IsNullOrEmpty(m_outputBufferBContent))
            {
                string bufferShaderFilePathName = shaderName + "-BufferB";
                string bufferMatFilePathName = filePath + "/" + fileName + "-BufferB.mat";
                CreateMaterial(bufferShaderFilePathName, bufferMatFilePathName);
            }

            if (!string.IsNullOrEmpty(m_outputBufferCContent))
            {
                string bufferShaderFilePathName = shaderName + "-BufferC";
                string bufferMatFilePathName = filePath + "/" + fileName + "-BufferC.mat";
                CreateMaterial(bufferShaderFilePathName, bufferMatFilePathName);
            }

            if (!string.IsNullOrEmpty(m_outputBufferDContent))
            {
                string bufferShaderFilePathName = shaderName + "-BufferD";
                string bufferMatFilePathName = filePath + "/" + fileName + "-BufferD.mat";
                CreateMaterial(bufferShaderFilePathName, bufferMatFilePathName);
            }
            
            if (!string.IsNullOrEmpty(m_outputMainShaderContent))
            {
                CreateMaterial(shaderName, matFilePathName);
            }

            Debug.Log("All Materials are created successfully.");
            return true;
        }

        protected bool CreateMaterial(string shaderName, string matFilePathName)
        {
            bool ret = ShaderConverter.CreateMaterial(shaderName, matFilePathName);
            if (!ret)
            {
                Debug.LogError("Error: Failed to create material: " + matFilePathName);
                return false;
            }

            Debug.Log("Material is created successfully: " + matFilePathName);
            return true;
        }

        protected void ClearAllContents()
        {
            m_inputMainShaderContent = string.Empty;
            m_outputMainShaderContent = string.Empty;

            m_inputCommonShaderContent = string.Empty;
            m_outputCommonShaderContent = string.Empty;

            m_inputBufferAContent = string.Empty;
            m_outputBufferAContent = string.Empty;
            m_inputBufferBContent = string.Empty;
            m_outputBufferBContent = string.Empty;
            m_inputBufferCContent = string.Empty;
            m_outputBufferCContent = string.Empty;
            m_inputBufferDContent = string.Empty;
            m_outputBufferDContent = string.Empty;

            m_selectedTabIndex = 0;
            m_mainShaderType = 0;
            
            for (int i = 0; i < 6; ++i)
            {
                m_iChannel0Type[i] = 0;
                m_iChannel1Type[i] = 0;
                m_iChannel2Type[i] = 0;
                m_iChannel3Type[i] = 0;
            }
        }
    }
}
