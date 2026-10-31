Shader "BLB/GridDraw"
{
    Properties
    {
        _GridColor ("Grid Color", Color) = (0.25, 0.25, 0.25, 1)
        _BackgroundColor ("Background Color", Color) = (0, 0, 0, 0)
    }
    SubShader
    {
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

            static const float TILE_SIZE = 1;

            fixed4 _GridColor;
            fixed4 _BackgroundColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float GridLine(float2 worldPosition, float gridSize)
            {
                float2 gridPosition = worldPosition / gridSize;
                float2 distanceToLine = min(frac(gridPosition), 1.0 - frac(gridPosition));
                float2 lineWidth = max(fwidth(gridPosition) * 1.5, float2(0.001, 0.001));
                float2 lineMask = 1.0 - smoothstep(0.0, lineWidth, distanceToLine);
                return max(lineMask.x, lineMask.y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // unity_OrthoParams.xy is the camera's visible world width and height.
                float2 cameraWorldSize = unity_OrthoParams.xy;
                float2 worldPosition = _WorldSpaceCameraPos.xy +
                    (i.uv - 0.5) * (cameraWorldSize / 0.5) + (TILE_SIZE * 0.5);
                float gridLevel = max(log10(max(cameraWorldSize.x / (TILE_SIZE * 10.0), 0.0001)), 0.0);
                float fineGridSize = TILE_SIZE * pow(10.0, floor(gridLevel));
                float coarseGridSize = fineGridSize * 10.0;
                float coarseGridOpacity = frac(gridLevel);
                float fineGridOpacity = 1.0 - coarseGridOpacity;
                float fineGrid = GridLine(worldPosition, fineGridSize) * fineGridOpacity;
                float coarseGrid = GridLine(worldPosition, coarseGridSize) * coarseGridOpacity;
                float grid = fineGrid + coarseGrid * (1.0 - fineGrid);

                return lerp(_BackgroundColor, _GridColor, grid);
            }
            ENDCG
        }
    }
}
