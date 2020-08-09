Shader "Unlit/PointCloud"
{
    Properties
    {
        _Size("Size",Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float3 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            float _Size;

            v2f vert (appdata v)
            {
                v2f o;
                float4x4 m = UNITY_MATRIX_V;
                m[3] = float4(0, 0, 0, 1);
                float3 normal = mul(m, v.normal);

                float3 pos = v.vertex + normal * _Size;
                o.vertex = UnityObjectToClipPos(pos);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
