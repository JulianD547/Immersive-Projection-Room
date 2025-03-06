
// This shader is converted by ShaderConverter : 
// https://u3d.as/2Zim 


Shader "Custom/ChocolateShader"
{
    Properties
    {
        _iResolution ("_iResolution", Vector) = (0,0,0,0)
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

            static float4 gl_FragCoord;
            static float4 fragColor;
            static float2 _texCoord_;

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
                float2 p = (((gl_FragCoord.xy - (_iResolution.xy * 0.5f)) / _iResolution.y.xx) * 6.0f) - 0.5f.xx;
                float2 i = p;
                float c = 0.0f;
                float r = length(p + (float2(sin(_Time.y), sin((_Time.y * 0.300000011920928955078125f) + 5.0f)) * 0.5f));
                float d = length(p);
                float rot = (d + _Time.y) + (p.x * 0.699999988079071044921875f);
                for (float n = 0.0f; n < 4.0f; n += 1.0f)
                {
                    p = mul(float2x2(float2(cos(rot - sin(_Time.y / 5.0f)), sin(rot)), float2(-sin(cos(rot) - _Time.y), cos(rot))) * (-0.20000000298023223876953125f), p);
                    float t = r - (_Time.y / (n + 3.0f));
                    i -= (p + float2(cos((t - i.x) - r) + sin(t + i.y), (sin(t - i.y) + cos(t + i.x)) + r));
                    c += (1.2000000476837158203125f / length(float2(sin(i.x + t) / 0.1500000059604644775390625f, cos(i.y + t) / 0.1500000059604644775390625f)));
                }
                c /= 6.0f;
                fragColor = float4((c.xxx * float3(3.0f, 2.0f, 1.10000002384185791015625f)) - 0.3499999940395355224609375f.xxx, 0.100000001490116119384765625f);
            }

            SPIRV_Cross_Output frag(SPIRV_Cross_Input stage_input)
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(stage_input);

                gl_FragCoord = stage_input.gl_FragCoord;
                gl_FragCoord.w = 1.0 / gl_FragCoord.w;
                _texCoord_ = stage_input._texCoord_;
                frag_main();
                SPIRV_Cross_Output stage_output;
                stage_output.fragColor = fragColor;
                return stage_output;
            }


            ENDCG
        }
    }
}