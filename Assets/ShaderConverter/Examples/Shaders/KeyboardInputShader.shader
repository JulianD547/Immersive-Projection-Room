// Created by inigo quilez - iq/2013


// An example showing how to use the keyboard input.
//
// Row 0: contain the current state of the 256 keys. 
// Row 1: contains Keypress.
// Row 2: contains a toggle for every key.
//
// Texel positions correspond to ASCII codes in Javascript. Press arrow keys to test.


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


// Inner solid disk: visible when key is currently down ("state"); lookup at (keycode, 0)
// Thin outer circle: visible when key has been newly pressed in the last frame ("keypress"); lookup at (keycode, 1). This also fires repeatedly when keyboard autorepeats.
// Thick border of disk: visible when key has been pressed an odd number of times ("toggle"); lookup at (keycode, 2). This also counts autorepeats.


// This shader is converted by ShaderConverter : 
// https://u3d.as/2Zim 


Shader "Custom/KeyboardInputShader"
{
    Properties
    {
        _iResolution ("_iResolution", Vector) = (0,0,0,0)
        KEY_LEFT ("KEY_LEFT", Int) = 37
        KEY_UP ("KEY_UP", Int) = 38
        KEY_RIGHT ("KEY_RIGHT", Int) = 39
        KEY_DOWN ("KEY_DOWN", Int) = 40
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
            
            uniform float3 _iResolution;
            Texture2D<float4> _MainTex : register(t1);
            SamplerState sampler_MainTex : register(s1);
            uniform int KEY_LEFT;
            uniform int KEY_UP;
            uniform int KEY_RIGHT;
            uniform int KEY_DOWN;

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
                float2 uv = ((-_iResolution.xy) + ((_texCoord_ * _iResolution.xy) * 2.0f)) / _iResolution.y.xx;
                float3 col = 0.0f.xxx;
                col = lerp(col, float3(1.0f, 0.0f, 0.0f), ((1.0f - smoothstep(0.300000011920928955078125f, 0.310000002384185791015625f, length(uv - float2(-0.75f, 0.0f)))) * (0.300000011920928955078125f + (0.699999988079071044921875f * _MainTex.Load(int3(int2(KEY_LEFT, 0), 0)).x))).xxx);
                col = lerp(col, float3(1.0f, 1.0f, 0.0f), ((1.0f - smoothstep(0.300000011920928955078125f, 0.310000002384185791015625f, length(uv - float2(0.0f, 0.5f)))) * (0.300000011920928955078125f + (0.699999988079071044921875f * _MainTex.Load(int3(int2(KEY_UP, 0), 0)).x))).xxx);
                col = lerp(col, float3(0.0f, 1.0f, 0.0f), ((1.0f - smoothstep(0.300000011920928955078125f, 0.310000002384185791015625f, length(uv - float2(0.75f, 0.0f)))) * (0.300000011920928955078125f + (0.699999988079071044921875f * _MainTex.Load(int3(int2(KEY_RIGHT, 0), 0)).x))).xxx);
                col = lerp(col, float3(0.0f, 0.0f, 1.0f), ((1.0f - smoothstep(0.300000011920928955078125f, 0.310000002384185791015625f, length(uv - float2(0.0f, -0.5f)))) * (0.300000011920928955078125f + (0.699999988079071044921875f * _MainTex.Load(int3(int2(KEY_DOWN, 0), 0)).x))).xxx);
                col = lerp(col, float3(1.0f, 0.0f, 0.0f), ((1.0f - smoothstep(0.0f, 0.00999999977648258209228515625f, abs(length(uv - float2(-0.75f, 0.0f)) - 0.3499999940395355224609375f))) * _MainTex.Load(int3(int2(KEY_LEFT, 1), 0)).x).xxx);
                col = lerp(col, float3(1.0f, 1.0f, 0.0f), ((1.0f - smoothstep(0.0f, 0.00999999977648258209228515625f, abs(length(uv - float2(0.0f, 0.5f)) - 0.3499999940395355224609375f))) * _MainTex.Load(int3(int2(KEY_UP, 1), 0)).x).xxx);
                col = lerp(col, float3(0.0f, 1.0f, 0.0f), ((1.0f - smoothstep(0.0f, 0.00999999977648258209228515625f, abs(length(uv - float2(0.75f, 0.0f)) - 0.3499999940395355224609375f))) * _MainTex.Load(int3(int2(KEY_RIGHT, 1), 0)).x).xxx);
                col = lerp(col, float3(0.0f, 0.0f, 1.0f), ((1.0f - smoothstep(0.0f, 0.00999999977648258209228515625f, abs(length(uv - float2(0.0f, -0.5f)) - 0.3499999940395355224609375f))) * _MainTex.Load(int3(int2(KEY_DOWN, 1), 0)).x).xxx);
                col = lerp(col, float3(1.0f, 0.0f, 0.0f), ((1.0f - smoothstep(0.0f, 0.00999999977648258209228515625f, abs(length(uv - float2(-0.75f, 0.0f)) - 0.300000011920928955078125f))) * _MainTex.Load(int3(int2(KEY_LEFT, 2), 0)).x).xxx);
                col = lerp(col, float3(1.0f, 1.0f, 0.0f), ((1.0f - smoothstep(0.0f, 0.00999999977648258209228515625f, abs(length(uv - float2(0.0f, 0.5f)) - 0.300000011920928955078125f))) * _MainTex.Load(int3(int2(KEY_UP, 2), 0)).x).xxx);
                col = lerp(col, float3(0.0f, 1.0f, 0.0f), ((1.0f - smoothstep(0.0f, 0.00999999977648258209228515625f, abs(length(uv - float2(0.75f, 0.0f)) - 0.300000011920928955078125f))) * _MainTex.Load(int3(int2(KEY_RIGHT, 2), 0)).x).xxx);
                col = lerp(col, float3(0.0f, 0.0f, 1.0f), ((1.0f - smoothstep(0.0f, 0.00999999977648258209228515625f, abs(length(uv - float2(0.0f, -0.5f)) - 0.300000011920928955078125f))) * _MainTex.Load(int3(int2(KEY_DOWN, 2), 0)).x).xxx);
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