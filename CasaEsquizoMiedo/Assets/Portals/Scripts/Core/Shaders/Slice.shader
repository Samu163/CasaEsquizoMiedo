Shader "URP/Custom/Slice"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        sliceNormal ("Slice Normal", Vector) = (0,1,0,0)
        sliceCentre ("Slice Centre", Vector) = (0,0,0,0)
        sliceOffsetDst ("Slice Offset", Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Glossiness;
            float _Metallic;

            float3 sliceNormal;
            float3 sliceCentre;
            float sliceOffsetDst;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float4 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.worldPos = worldPos.xyz;
                OUT.positionHCS = TransformWorldToHClip(worldPos.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Slicing logic
                float3 adjustedCentre = sliceCentre + sliceNormal * sliceOffsetDst;
                float3 offset = adjustedCentre - IN.worldPos;
                if (dot(offset, sliceNormal) < 0)
                    discard;

                float4 texColor = tex2D(_MainTex, IN.uv) * _Color;
                return texColor;
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}