Shader "BLB/TileOutliner"
{
    Properties
    {
        _TileDistanceTex("Tile Distance Field", 2D) = "white" {}
        _TileGridStartPosition("Tile Grid Start Position", Vector) = (0, 0, 0, 0)
        _TileDistanceScale("Tile Distance Scale", Float) = 5
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            static const float OUTSIDE_FADE_DISTANCE = 0.5;

            sampler2D _TileDistanceTex;
            float4 _TileDistanceTex_TexelSize;
            // The world position of the tile grid's origin cell
            float2 _TileGridStartPosition;
            float _TileDistanceScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float sdAtUv(float2 uv)
            {
                // unity_OrthoParams.xy is the camera's visible world width and height.
                float2 cameraWorldSize = unity_OrthoParams.xy;
                // Map this fragment's UV to a world position centered on the camera.
                float2 worldPosition = _WorldSpaceCameraPos.xy +
                (uv - 0.5) * (cameraWorldSize * 2) + 0.5;
                float2 gridPosition = worldPosition - _TileGridStartPosition;
                float2 distanceUv = (gridPosition * _TileDistanceScale + 0.5)
                    * _TileDistanceTex_TexelSize.xy;
                return tex2D(_TileDistanceTex, distanceUv).r;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float d = sdAtUv(i.uv);
                float3 col = (d>0.0) ? float3(0.9,0.6,0.3) : float3(0.65,0.85,1.0);
	            col *= 1.0 - exp2(-20.0*abs(d));
	            col *= 0.8 + 0.2*cos(120.0*abs(d));
	            col = lerp(col, float3(1.0,1.0,1.0), 1.0-smoothstep(0.0,0.01,abs(d)));
                float alpha = 1.0 - smoothstep(0.0, OUTSIDE_FADE_DISTANCE, max(d, 0.0));
                return fixed4(col, alpha);
            }
            ENDCG
        }
    }
}
