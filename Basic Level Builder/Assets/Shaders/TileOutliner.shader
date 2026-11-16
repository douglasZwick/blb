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

            float sdPolygon(float2 v[20], float2 p)
            {
                float d = dot(p - v[0], p - v[0]);
                float s = 1.0;
                
                for (int i = 0, j = N - 1; i < N; j = i, i++)
                {
                    float2 e = v[j] - v[i];
                    float2 w = p - v[i];
                    float2 b = w - e * clamp(dot(w, e) / dot(e, e), 0.0, 1.0);
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

            float getSdTile(float2 cellcoord, float2 uv, float pathIndex)
            {
                float4 tile = getTile(cellcoord);
                // If we have a tile here and the path indexs match
                if (tile.r > 0.01 && abs(tile.g - pathIndex) < 0.001) {
                    return sdBox(uv, TILE_SIZE);
                }
                return MAX_DISTANCE;
            }

            float getNeighborSdTile(float2 cellcoord, float2 uv, float pathIndex, float2 dir)
            {
                float4 tile = getTile(cellcoord + dir);
                // If we have a tile here and the path indexs match
                if (tile.r > 0.01 && abs(tile.g - pathIndex) < 0.001) {
                    return sdBox(uv - (dir * TILE_SIZE), TILE_SIZE + TILE_SIZE * abs(dir));
                }
                return MAX_DISTANCE;
            }

            float getCornerNeighborSdTile(float2 cellcoord, float2 uv, float pathIndex, float2 dir)
            {
                float4 horizontalTile = getTile(cellcoord + float2(dir.x, 0));
                float4 verticalTile = getTile(cellcoord + float2(0, dir.y));
                if (horizontalTile.r > 0.01 && abs(horizontalTile.g - pathIndex) < 0.001 &&
                    verticalTile.r > 0.01 && abs(verticalTile.g - pathIndex) < 0.001) {
                    return getNeighborSdTile(cellcoord, uv, pathIndex, dir);
                }
                return MAX_DISTANCE;
            }

            bool isTileValid(float4 tile, float pathIndex)
            {
                return tile.r > 0.01 && abs(tile.g - pathIndex) < 0.001;
            }

            float2[20] getSdfPolygon(float4 currentTile, float2 cellcoord)
            {
                // Look at each 8 directional neighbor and create a polygon from them
                // Start at top left and loop back
                float pathIndex = currentTile.g;


                float2[20] polyPoints;
                float4 checkingTile = getTile(cellCoord + float2(-1,-1));
                int index = 0
                if (isTileValid(checkingTile, pathIndex))
                {
                    // Slope solid bottom left
                    if (abs(checkingTile.r - 2) < 0.001)
                    {
                        polyPoints[index++] = float2(-1.5, -0.5);
                        polyPoints[index++] = float2(-1.5, -1.5);
                        polyPoints[index++] = float2(-0.5, -0.5);
                    }
                    // Slope solid top left. Ignore, not part of this shape
                    else if (abs(checkingTile.r - 3) < 0.001)
                    {
                        polyPoints[index++] = float2(-0.5, -0.5);
                    }
                    // Slope solid top right.
                    else if (abs(checkingTile.r - 4) < 0.001)
                    {
                        polyPoints[index++] = float2(-0.5, -0.5);
                        polyPoints[index++] = float2(-1.5, -1.5);
                        polyPoints[index++] = float2(-0.5, -1.5);
                    }
                    // Slope solid bottom right.
                    else if (abs(checkingTile.r - 5) < 0.001)
                    {
                        polyPoints[index++] = float2(-1.5, -0.5);
                        polyPoints[index++] = float2(-0.5, -1.5);
                    }
                    // Solid tile
                    else
                    {
                        polyPoints[index++] = float2(-1.5, -0.5);
                        polyPoints[index++] = float2(-1.5, -1.5);
                        polyPoints[index++] = float2(-0.5, -1.5);
                    }
                }
                else
                {
                    polyPoints[index++] = float2(-0.5, -0.5);
                }
                checkingTile = getTile(cellCoord + float2(0,-1));
                if (isTileValid(checkingTile, pathIndex))
                {
                    // Slope solid bottom left
                    if (abs(checkingTile.r - 2) < 0.001)
                    {
                        polyPoints[index++] = float2(-0.5, -1.5);
                        polyPoints[index++] = float2(0.5, -0.5);
                    }
                    // Slope solid top left. Unconnected, default case
                    else if (abs(checkingTile.r - 3) < 0.001)
                    {
                        polyPoints[index++] = float2(-0.5, -0.5);
                        polyPoints[index++] = float2(0.5, -0.5);
                    }
                    // Slope solid top right. Unconnected, default case
                    else if (abs(checkingTile.r - 4) < 0.001)
                    {
                        polyPoints[index++] = float2(-0.5, -0.5);
                        polyPoints[index++] = float2(0.5, -0.5);
                    }
                    // Slope solid bottom right.
                    else if (abs(checkingTile.r - 5) < 0.001)
                    {
                        polyPoints[index++] = float2(-0.5, -0.5);
                        polyPoints[index++] = float2(0.5, -1.5);
                    }
                    // Solid tile
                    else
                    {
                        polyPoints[index++] = float2(-0.5, -1.5);
                        polyPoints[index++] = float2(0.5, -1.5);
                    }
                }
                else
                {
                    polyPoints[index++] = float2(-0.5, -0.5);
                    polyPoints[index++] = float2(0.5, -0.5);
                }
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

                float distance = getSdTile(cellCoord, gridCellUv, pathIndex);
                distance = min(distance, getNeighborSdTile(cellCoord, gridCellUv, pathIndex, float2(1,0)));
                distance = min(distance, getNeighborSdTile(cellCoord, gridCellUv, pathIndex, float2(-1,0)));
                distance = min(distance, getNeighborSdTile(cellCoord, gridCellUv, pathIndex, float2(0,1)));
                distance = min(distance, getNeighborSdTile(cellCoord, gridCellUv, pathIndex, float2(0,-1)));
                distance = min(distance, getCornerNeighborSdTile(cellCoord, gridCellUv, pathIndex, float2(1,1)));
                distance = min(distance, getCornerNeighborSdTile(cellCoord, gridCellUv, pathIndex, float2(-1,1)));
                distance = min(distance, getCornerNeighborSdTile(cellCoord, gridCellUv, pathIndex, float2(1,-1)));
                distance = min(distance, getCornerNeighborSdTile(cellCoord, gridCellUv, pathIndex, float2(-1,-1)));

                return distance;
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
