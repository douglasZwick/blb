Shader "BLB/TileOutliner"
{
    Properties
    {
        _TileGridTex("Tile Grid", 2D) = "black" {}
        _TileGridStartPosition("Tile Grid Start Position", Vector) = (0, 0, 0, 0)
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
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11
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

            static const float2 TILE_SIZE = float2(0.5, 0.5);
            static const float MAX_DISTANCE = float(1e6);
            static const int N = 16;

            sampler2D _TileGridTex;
            float4 _TileGridTex_TexelSize;
            // The world position of the tile grid's origin cell
            float2 _TileGridStartPosition;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float sdPolygon(float2 v[N], float2 p)
            {
                float d = dot(p - v[0], p - v[0]);
                float s = 1.0;

                for (int i = 0, j = N - 1; i < N; j = i, i++)
                {
                    float2 e = v[j] - v[i];
                    float2 w = p - v[i];
                    float2 b = w - e * clamp(dot(w, e) / max(dot(e, e), 0.0001), 0.0, 1.0);
                    d = min(d, dot(b, b));

                    bool3 c = bool3(p.y >= v[i].y, p.y < v[j].y, e.x * w.y > e.y * w.x);
                    if (all(c) || all(!c)) s *= -1.0;
                }

                return s * sqrt(d);
            }

            float4 getTile(float2 cellcoord)
            {
                if (any(cellcoord < 0.0) || any(cellcoord >= _TileGridTex_TexelSize.zw)) {
                    return float4(0.0, 0.0, 0.0, 0.0);
                }

                float2 tileUv = (cellcoord + 0.5) * _TileGridTex_TexelSize.xy;
                return tex2D(_TileGridTex, tileUv);
            }

            bool isTileValidPath(float4 tile, float pathIndex)
            {
                return abs(tile.g - pathIndex) < 0.001;
            }

            bool isTileValidType(float4 tile, float typeValue)
            {
                return abs(tile.g - typeValue) < 0.001;
            }


            void appendPoly(inout int index, inout float2 polyPoints[N], float2 a, float2 b)
            {
                polyPoints[index++] = a;
                polyPoints[index++] = b;
            }

            float2[N] getSdfPolygon(float4 currentTile, float2 cellcoord)
            {
                // Look at each 8 directional neighbor and create a polygon from them.
                // Start at the top-left corner and loop clockwise back to the origin.
                const float pathIndex = currentTile.g;
                float2 polyPoints[N];
                int index = 0;

                // Top left corner valid tiles are solid or slope bottom right
                float4 checkingTile = getTile(cellcoord + float2(-1.0, -1.0));
                if (isTileValidPath(checkingTile, pathIndex) && (isTileValidType(checkingTile, 1) || isTileValidType(checkingTile, 5)))
                {
                    appendPoly(index, polyPoints, float2(-1.5, -0.5), float2(-0.5, -1.5));
                }
                else
                {
                    polyPoints[index++] = float2(-0.5, -0.5);
                }

                // Top middle valid tiles are solid or slope bottom right and bottom left
                float4 checkingTile = getTile(cellcoord + float2(-1.0, -1.0));
                if (isTileValidPath(checkingTile, pathIndex))
                {
                    if (isTileValidType(checkingTile, 1))
                        appendPoly(index, polyPoints, float2(-0.5, -1.5), float2(0.5, -1.5));
                    else if (isTileValidType(checkingTile, 2))
                        appendPoly(index, polyPoints, float2(-0.5, -1.5), float2(0.5, -0.5));
                    else if (isTileValidType(checkingTile, 5))
                        appendPoly(index, polyPoints, float2(-0.5, -0.5), float2(0.5, -1.5));
                }
                else
                {
                    polyPoints[index++] = float2(-0.5, -0.5);
                    polyPoints[index++] = float2(0.5, -0.5);
                }
      

                checkingTile = getTile(cellcoord + float2(1.0, -1.0));
                if (isTileValidPath(checkingTile, pathIndex) && (isTileValidType(checkingTile, 1) || isTileValidType(checkingTile, 2)))
                {
                    appendPoly(index, polyPoints, float2(0.5, -1.5), float2(1.5, -0.5));
                }
                else
                {
                    polyPoints[index++] = float2(0.5, -0.5);
                }

                checkingTile = getTile(cellcoord + float2(1.0, 0.0));
                if (isTileValidPath(checkingTile, pathIndex))
                {
                    if (isTileValidType(checkingTile, 1))
                        appendPoly(index, polyPoints, float2(1.5, -0.5), float2(1.5, 0.5));
                    else if (isTileValidType(checkingTile, 2))
                        appendPoly(index, polyPoints, float2(0.5, -0.5), float2(1.5, 0.5));
                    else if (isTileValidType(checkingTile, 3))
                        appendPoly(index, polyPoints, float2(1.5, -0.5), float2(0.5, 0.5));
                }
                else
                {
                    polyPoints[index++] = float2(0.5, -0.5);
                    polyPoints[index++] = float2(0.5, 0.5);
                }

                checkingTile = getTile(cellcoord + float2(1.0, 1.0));
                if (isTileValidPath(checkingTile, pathIndex) && (isTileValidType(checkingTile, 1) || isTileValidType(checkingTile, 3)))
                {
                    appendPoly(index, polyPoints, float2(1.5, 0.5), float2(0.5, 1.5));
                }
                else
                {
                    polyPoints[index++] = float2(0.5, 0.5);
                }

                checkingTile = getTile(cellcoord + float2(0.0, 1.0));
                if (isTileValidPath(checkingTile, pathIndex))
                {
                    if (isTileValidType(checkingTile, 1))
                        appendPoly(index, polyPoints, float2(0.5, 1.5), float2(-0.5, 1.5));
                    else if (isTileValidType(checkingTile, 3))
                        appendPoly(index, polyPoints, float2(0.5, 0.5), float2(-0.5, 1.5));
                    else if (isTileValidType(checkingTile, 4))
                        appendPoly(index, polyPoints, float2(0.5, 1.5), float2(-0.5, 0.5));
                }
                else
                {
                    polyPoints[index++] = float2(0.5, 0.5);
                    polyPoints[index++] = float2(-0.5, 0.5);
                }

                checkingTile = getTile(cellcoord + float2(-1.0, 1.0));
                if (isTileValidPath(checkingTile, pathIndex) && (isTileValidType(checkingTile, 1) || isTileValidType(checkingTile, 4)))
                {
                    appendPoly(index, polyPoints, float2(-0.5, 1.5), float2(-1.5, 0.5));
                }
                else
                {
                    polyPoints[index++] = float2(-0.5, 0.5);
                }

                checkingTile = getTile(cellcoord + float2(1.0, 0.0));
                if (isTileValidPath(checkingTile, pathIndex))
                {
                    if (isTileValidType(checkingTile, 1))
                        appendPoly(index, polyPoints, float2(-1.5, 0.5), float2(-1.5, -0.5));
                    else if (isTileValidType(checkingTile, 4))
                        appendPoly(index, polyPoints, float2(-0.5, 0.5), float2(-1.5, -0.5));
                    else if (isTileValidType(checkingTile, 5))
                        appendPoly(index, polyPoints, float2(-1.5, 0.5), float2(-0.5, -0.5));
                }
                else
                {
                    polyPoints[index++] = float2(-0.5, 0.5);
                    polyPoints[index++] = float2(-0.5, -0.5);
                }

                return polyPoints;
            }

            float sdAtUv(float2 uv)
            {
                // Convert UV to coords on grid

                // unity_OrthoParams.xy is the camera's visible world width and height.
                float2 cameraWorldSize = unity_OrthoParams.xy;
                // Map this fragment's UV to a world position centered on the camera.
                float2 worldPosition = _WorldSpaceCameraPos.xy +
                (uv - 0.5) * (cameraWorldSize * 2) + 0.5;
                // Convert the uv to be scaled by the tile size and ofset by the _TileGridStartPosition and world position
                float2 gridPosition = worldPosition - _TileGridStartPosition;
                // Get the cell position our world position then check it and the neighbor cells
                float2 cellCoord = floor(gridPosition);
                // Get the position of this pixel in the cell since the sdfs are defined in the range [-0.5, 0.5] for each cell
                float2 gridCellUv = gridPosition - cellCoord - 0.5;

                float4 currentTile = getTile(cellCoord);

                // If there is no tile here, return a large distance so we don't draw anything
                if (currentTile.r <= 0.01)
                    return MAX_DISTANCE;

                float pathIndex = currentTile.g;


                return sdPolygon(getSdfPolygon(currentTile, cellCoord), gridCellUv);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float d = sdAtUv(i.uv);
                // alpha cut all the positive values outside of the shape but gradient blend 0.01 units outside of the shape till the proper edge
                float alpha = smoothstep(-0.01, 0.00, -d);
                // Cut out the inside of the shape to only get a 0.05 pixel thickness inside
                alpha = min(alpha, smoothstep(-0.11, -0.1, d));
                return fixed4(float3(1.0,1.0,1.0), alpha);
            }
            ENDCG
        }
    }
}
