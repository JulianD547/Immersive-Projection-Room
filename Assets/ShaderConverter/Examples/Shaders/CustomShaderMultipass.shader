// ***********************************************************
// Alcatraz / Rhodium 4k Intro liquid carbon
// by Jochen "Virgill" Feldkötter
//
// 4kb executable: http://www.pouet.net/prod.php?which=68239
// Youtube: https://www.youtube.com/watch?v=YK7fbtQw3ZU
// ***********************************************************

// This shader is converted by ShaderConverter : 
// https://u3d.as/2Zim 


Shader "Custom/CustomShaderMultipass"
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
            
            uniform float3 _iResolution;
            Texture2D<float4> _MainTex : register(t1);
            SamplerState sampler_MainTex : register(s1);

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

            float3 dof(Texture2D<float4> tex, SamplerState samplertex, float2 uv, inout float rad)
            {
                float3 acc = 0.0f.xxx;
                float2 pixel = float2((0.00200000009499490261077880859375f * _iResolution.y) / _iResolution.x, 0.00200000009499490261077880859375f);
                float2 angle = float2(0.0f, rad);
                rad = 1.0f;
                for (int j = 0; j < 80; j++)
                {
                    rad += (1.0f / rad);
                    angle = mul(float2x2(float2(-0.736717879772186279296875f, 0.676200211048126220703125f), float2(-0.676200211048126220703125f, -0.736717879772186279296875f)), angle);
                    float4 col = tex.Sample(samplertex, uv + ((pixel * (rad - 1.0f)) * angle));
                    acc += col.xyz;
                }
                return acc / 80.0f.xxx;
            }

            void frag_main()
            {
                float2 uv = (_texCoord_ * _iResolution.xy) / _iResolution.xy;
                float2 param = uv;
                float param_1 = _MainTex.Sample(sampler_MainTex, uv).w;
                float3 _116 = dof(_MainTex, sampler_MainTex, param, param_1);
                fragColor = float4(_116, 1.0f);
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