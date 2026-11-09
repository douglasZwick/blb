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

            // Returns a 0-to-1 mask for grid lines at the given world-space spacing.
            float GridLine(float2 worldPosition, float gridSize)
            {
                // In grid-cell units, integer coordinates mark grid lines.
                float2 gridPosition = worldPosition / gridSize;

                // Find the distance to the nearest grid line on each axis.
                float2 distanceToLine = min(frac(gridPosition), 1.0 - frac(gridPosition));

                // Estimate coordinate change across a pixel to keep line edges
                // anti-aliased as the camera zooms. The minimum prevents zero width.
                float2 lineWidth = max(fwidth(gridPosition) * 1.5, float2(0.001, 0.001));

                // Fade from the line center to the background over the pixel-scaled width.
                float2 lineMask = 1.0 - smoothstep(0.0, lineWidth, distanceToLine);

                // A line is visible when either axis is close to a grid line.
                return max(lineMask.x, lineMask.y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // unity_OrthoParams.xy is the camera's visible world width and height.
                float2 cameraWorldSize = unity_OrthoParams.xy;
                // Map this fragment's UV to a world position centered on the camera.
                float2 worldPosition = _WorldSpaceCameraPos.xy +
                    (i.uv - 0.5) * (cameraWorldSize / 0.5) + (TILE_SIZE * 0.5);

                // Move between grid sizes by powers of ten as the camera zooms.
                float gridLevel = max(log10(max(cameraWorldSize.x / (TILE_SIZE * 10.0), 0.0001)), 0.0);
                float fineGridSize = TILE_SIZE * pow(10.0, floor(gridLevel));
                float coarseGridSize = fineGridSize * 10.0;

                // Cross-fade between fine and coarse grids to avoid abrupt level changes.
                float coarseGridOpacity = frac(gridLevel);
                float fineGridOpacity = 1.0 - coarseGridOpacity;
                float fineGrid = GridLine(worldPosition, fineGridSize) * fineGridOpacity;
                float coarseGrid = GridLine(worldPosition, coarseGridSize) * coarseGridOpacity;
                float grid = fineGrid + coarseGrid * (1.0 - fineGrid);

                // Use the combined grid mask to blend the background and line colors.
                return lerp(_BackgroundColor, _GridColor, grid);
            }
            ENDCG
        }
    }
}
