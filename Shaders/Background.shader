Shader "Skybox/Background"
{
    Properties
    {
        _Color1 ("Color", Color) = (0, 0, 0, 0)
        _Color2 ("Color", Color) = (0, 0, 0, 0)
        _Edge ("Edge", Float) = 1
        _Smooth("Smooth", Float) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float3 _Color1;
            float3 _Color2;
            float _Edge;
            float _Smooth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 sceneUV = GetNormalizedScreenSpaceUV(i.vertex);
                sceneUV -= 0.5;
                sceneUV *= 2;
                float dis = smoothstep(_Edge, _Edge + _Smooth, length(sceneUV));
                return half4(lerp(_Color1, _Color2, dis), 1);
            }
            ENDHLSL
        }
    }
}
