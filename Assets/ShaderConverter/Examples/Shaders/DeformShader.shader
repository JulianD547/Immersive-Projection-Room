// Copyright Inigo Quilez, 2015 - https://iquilezles.org/
// I am the sole copyright owner of this Work.
// You cannot host, display, distribute or share this Work in any form,
// including physical and digital. You cannot use this Work in any
// commercial or non-commercial product, website or project. You cannot
// sell this Work and you cannot mint an NFTs of it.
// I share this Work for educational purposes, and you can link to it,
// through an URL, proper attribution and unmodified screenshot, as part
// of your educational material. If these conditions are too restrictive
// please contact me and we'll definitely work it out.

// This shader is converted by ShaderConverter : 
// https://u3d.as/2Zim 


Shader "Custom/DeformShader"
{
    Properties
    {
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

            #include "UnityCG.cginc"

            //////////////////////////////////////////////////////////////////////////

            //Vertex Shader Begin
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            //Vertex Shader End

            //////////////////////////////////////////////////////////////////////////
            
            Texture2D<float4> _MainTex : register(t2);
            SamplerState sampler_MainTex : register(s2);

            static float4 gl_FragCoord;
            static float4 fragColor;
            static float2 _texCoord_;

            struct SPIRV_Cross_Input
            {
                float2 _texCoord_ : TEXCOORD0;
                float4 gl_FragCoord : SV_Position;
            };

            struct SPIRV_Cross_Output
            {
                float4 fragColor : SV_Target0;
            };

            void frag_main()
            {
                float2 q = ((gl_FragCoord.xy * 2.0f) - _ScreenParams.xy) / _ScreenParams.y.xx;
                float2 p = q;
                float time = _Time.y * 0.100000001490116119384765625f;
                p += (cos(((p.yx * 1.5f) + (1.0f * time).xx) + float2(0.100000001490116119384765625f, 1.10000002384185791015625f)) * 0.20000000298023223876953125f);
                p += (cos(((p.yx * 2.400000095367431640625f) + (1.60000002384185791015625f * time).xx) + float2(4.5f, 2.599999904632568359375f)) * 0.20000000298023223876953125f);
                p += (cos(((p.yx * 3.2999999523162841796875f) + (1.2000000476837158203125f * time).xx) + float2(3.2000000476837158203125f, 3.400000095367431640625f)) * 0.20000000298023223876953125f);
                p += (cos(((p.yx * 4.19999980926513671875f) + (1.7000000476837158203125f * time).xx) + float2(1.7999999523162841796875f, 5.19999980926513671875f)) * 0.20000000298023223876953125f);
                p += (cos(((p.yx * 9.1000003814697265625f) + (1.10000002384185791015625f * time).xx) + float2(6.30000019073486328125f, 3.900000095367431640625f)) * 0.20000000298023223876953125f);
                float r = length(p);
                float3 col1 = _MainTex.SampleBias(sampler_MainTex, float2(r, 0.0f), 0.0f).zyx;
                float3 col2 = _MainTex.SampleBias(sampler_MainTex, float2(r + 0.039999999105930328369140625f, 0.0f), 0.0f).zyx;
                float3 col = col1;
                col += 0.100000001490116119384765625f.xxx;
                col *= (1.0f.xxx + (sin(r.xxx + float3(0.0f, 3.0f, 3.0f)) * 0.4000000059604644775390625f));
                col -= (4.0f * max(0.0f.xxx, col1 - col2).x).xxx;
                col += ((1.0f * max(0.0f.xxx, col2 - col1).x) - 0.100000001490116119384765625f).xxx;
                col *= (1.7000000476837158203125f - (0.5f * length(q)));
                fragColor = float4(col, 1.0f);
            }

            SPIRV_Cross_Output frag(SPIRV_Cross_Input stage_input)
            {
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