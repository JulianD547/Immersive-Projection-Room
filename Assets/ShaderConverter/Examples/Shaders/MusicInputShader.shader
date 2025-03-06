// Created by inigo quilez - iq/2013
// https://www.youtube.com/c/InigoQuilez
// https://iquilezles.org/


// See also:
//
// Input - Keyboard    : https://www.shadertoy.com/view/lsXGzf
// Input - Microphone  : https://www.shadertoy.com/view/llSGDh
// Input - Mouse       : https://www.shadertoy.com/view/Mss3zH
// Input - Sound       : https://www.shadertoy.com/view/Xds3Rr
// Input - SoundCloud  : https://www.shadertoy.com/view/MsdGzn
// Input - Time        : https://www.shadertoy.com/view/lsXGz8
// Input - TimeDelta   : https://www.shadertoy.com/view/lsKGWV
// Inout - 3D Texture  : https://www.shadertoy.com/view/4llcR4


// This shader is converted by ShaderConverter : 
// https://u3d.as/2Zim 


Shader "Custom/MusicInputShader"
{
    Properties
    {
        _iResolution ("_iResolution", Vector) = (0,0,0,0)
        _MainTex ("_MainTex / iChannel0", 2D) = "black" {}
        //To Add Properties
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderQueue" = "Geometry"}

        Pass
        {
            Cull Back
            ZWrite On
            ZTest LEqual
            Blend Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma target 4.0
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            //////////////////////////////////////////////////////////////////////////

            //Vertex Shader Begin
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            //Vertex Shader End

            //////////////////////////////////////////////////////////////////////////
            
            Texture2D<float4> _MainTex : register(t0);
            SamplerState sampler_MainTex : register(s0);
            uniform float3 _iResolution;

            static float2 _texCoord_;
            static float4 fragColor;

            struct SPIRV_Cross_Input
            {
                float2 _texCoord_ : TEXCOORD0;
                float4 gl_FragCoord : SV_Position;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct SPIRV_Cross_Output
            {
                float4 fragColor : SV_Target0;
            };

            void frag_main()
            {
                float2 uv = _texCoord_;
                int tx = int(uv.x * 512.0f);
                float fft = _MainTex.Load(int3(int2(tx, 0), 0)).x;
                float wave = _MainTex.Load(int3(int2(tx, 1), 0)).x;
                float3 col = float3(fft, (4.0f * fft) * (1.0f - fft), 1.0f - fft) * fft;
                col += (1.0f - smoothstep(0.0f, 0.1500000059604644775390625f, abs(wave - uv.y))).xxx;
                fragColor = float4(col, 1.0f);
            }

            SPIRV_Cross_Output frag(SPIRV_Cross_Input stage_input)
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(stage_input);

                _texCoord_ = stage_input._texCoord_;
                frag_main();
                SPIRV_Cross_Output stage_output;
                stage_output.fragColor = fragColor;
                stage_output.fragColor.r = GammaToLinearSpaceExact(stage_output.fragColor.r);
                stage_output.fragColor.g = GammaToLinearSpaceExact(stage_output.fragColor.g);
                stage_output.fragColor.b = GammaToLinearSpaceExact(stage_output.fragColor.b);
                return stage_output;
            }


            ENDCG
        }
    }
}