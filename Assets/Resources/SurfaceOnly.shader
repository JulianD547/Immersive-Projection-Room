Shader "Custom/ProjectorSurfaceWithDepth"
{
    Properties
    {
        _ProjectorTex ("Projector Texture", 2D) = "white" {}
        _MainTex ("Base Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _ProjectorTex;
            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            float4x4 _ProjectorMatrix;
            float3 _ProjectorCamPos;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 projUV : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos.xyz;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.projUV = mul(_ProjectorMatrix, worldPos);
                return o;
            }

            float LinearEyeDepth(float z)
            {
                return 1.0 / (_ProjectionParams.z * z + _ProjectionParams.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 projUV = i.projUV.xy / i.projUV.w;
                if (projUV.x < 0 || projUV.x > 1 || projUV.y < 0 || projUV.y > 1)
                    discard;

                float depthFromProj = i.projUV.z / i.projUV.w;
                float sceneDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, projUV);
                float linearSceneDepth = LinearEyeDepth(sceneDepth);

                if (depthFromProj > linearSceneDepth + 0.001)
                    discard;

                fixed4 projColor = tex2Dproj(_ProjectorTex, i.projUV);
                fixed4 baseColor = tex2D(_MainTex, i.uv);

                return baseColor + projColor;
            }
            ENDCG
        }
    }
}
