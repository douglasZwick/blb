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

            #define POLYGON_MAX_POINTS 16

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

            float sdPolygon(float2 v[POLYGON_MAX_POINTS], float2 p, int count)
            {
                float d = dot(p - v[0], p - v[0]);
                float s = 1.0;

                for (int i = 0, j = count - 1; i < count; j = i, i++)
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
                return abs(tile.r - typeValue) < 0.001;
            }

            void appendPoly(inout int index, inout float2 polyPoints[POLYGON_MAX_POINTS], float2 a, float2 b)
            {
                polyPoints[index++] = a;
                polyPoints[index++] = b;
            }

            void appendCorner(float2 cellcoord, float pathIndex, float2 direction,
                inout float2 polyPoints[POLYGON_MAX_POINTS], inout int index)
            {
                float4 tile = getTile(cellcoord + direction);
                float slopeType = direction.y < 0.0
                    ? (direction.x < 0.0 ? 4.0 : 3.0)
                    : (direction.x > 0.0 ? 2.0 : 5.0);

                if (isTileValidPath(tile, pathIndex) &&
                    (isTileValidType(tile, 1.0) || isTileValidType(tile, slopeType)))
                {
                    float2 xEdge = float2(direction.x * 1.5, direction.y * 0.5);
                    float2 yEdge = float2(direction.x * 0.5, direction.y * 1.5);
                    if (direction.x * direction.y > 0.0)
                        appendPoly(index, polyPoints, xEdge, yEdge);
                    else
                        appendPoly(index, polyPoints, yEdge, xEdge);
                }
                else
                {
                    polyPoints[index++] = direction * 0.5;
                }
            }

            float2 rotateEdgePoint(float2 edgePoint, float2 direction)
            {
                float2 tangent = float2(-direction.y, direction.x);
                return tangent * edgePoint.x - direction * edgePoint.y;
            }

            void appendEdge(float2 cellcoord, float pathIndex, float2 direction,
                inout float2 polyPoints[POLYGON_MAX_POINTS], inout int index)
            {
                float4 tile = getTile(cellcoord + direction);
                float typeValue = tile.r;

                // Map each edge's slope orientation to the bottom-edge orientation.
                if (direction.x > 0.0)
                {
                    if (isTileValidType(tile, 2.0)) typeValue = 3.0;
                    else if (isTileValidType(tile, 3.0)) typeValue = 4.0;
                    else if (isTileValidType(tile, 4.0)) typeValue = 0.0;
                }
                else if (direction.y > 0.0)
                {
                    if (isTileValidType(tile, 5.0)) typeValue = 3.0;
                    else if (isTileValidType(tile, 2.0)) typeValue = 4.0;
                    else if (isTileValidType(tile, 3.0) || isTileValidType(tile, 4.0)) typeValue = 0.0;
                }
                else if (direction.x < 0.0)
                {
                    if (isTileValidType(tile, 4.0)) typeValue = 3.0;
                    else if (isTileValidType(tile, 5.0)) typeValue = 4.0;
                    else if (isTileValidType(tile, 3.0)) typeValue = 0.0;
                }

                float2 a = float2(-0.5, -0.5);
                float2 b = float2(0.5, -0.5);
                if (isTileValidPath(tile, pathIndex))
                {
                    if (isTileValidType(tile, 1.0))
                    {
                        a = float2(-0.5, -1.5);
                        b = float2(0.5, -1.5);
                    }
                    else if (abs(typeValue - 3.0) < 0.001)
                    {
                        a = float2(-0.5, -1.5);
                        b = float2(0.5, -0.5);
                    }
                    else if (abs(typeValue - 4.0) < 0.001)
                    {
                        a = float2(-0.5, -0.5);
                        b = float2(0.5, -1.5);
                    }
                }

                appendPoly(index, polyPoints,
                    rotateEdgePoint(a, direction), rotateEdgePoint(b, direction));
            }

            void getSdfPolygonFromSlope(float4 currentTile, float2 cellcoord, inout float2 polyPoints[POLYGON_MAX_POINTS], inout int index)
            {
                const float pathIndex = currentTile.g;
                index = 0;

                if (isTileValidType(currentTile, 2.0))
                {
                    appendCorner(cellcoord, pathIndex, float2(-1.0, -1.0), polyPoints, index);
                    appendEdge(cellcoord, pathIndex, float2(0.0, -1.0), polyPoints, index);
                    appendPoly(index, polyPoints, float2(0.5, -0.5), float2(-0.5, 0.5));
                    appendEdge(cellcoord, pathIndex, float2(-1.0, 0.0), polyPoints, index);
                }
                else if (isTileValidType(currentTile, 3.0))
                {
                    appendCorner(cellcoord, pathIndex, float2(-1.0, 1.0), polyPoints, index);
                    appendEdge(cellcoord, pathIndex, float2(-1.0, 0.0), polyPoints, index);
                    appendPoly(index, polyPoints, float2(-0.5, -0.5), float2(0.5, 0.5));
                    appendEdge(cellcoord, pathIndex, float2(0.0, 1.0), polyPoints, index);
                }
                else if (isTileValidType(currentTile, 4.0))
                {
                    appendCorner(cellcoord, pathIndex, float2(1.0, 1.0), polyPoints, index);
                    appendEdge(cellcoord, pathIndex, float2(0.0, 1.0), polyPoints, index);
                    appendPoly(index, polyPoints, float2(-0.5, 0.5), float2(0.5, -0.5));
                    appendEdge(cellcoord, pathIndex, float2(1.0, 0.0), polyPoints, index);
                }
                else if (isTileValidType(currentTile, 5.0))
                {
                    appendCorner(cellcoord, pathIndex, float2(1.0, -1.0), polyPoints, index);
                    appendEdge(cellcoord, pathIndex, float2(1.0, 0.0), polyPoints, index);
                    appendPoly(index, polyPoints, float2(0.5, 0.5), float2(-0.5, -0.5));
                    appendEdge(cellcoord, pathIndex, float2(0.0, -1.0), polyPoints, index);
                }
            }

            void getSdfPolygonFromSquare(float pathIndex, float2 cellcoord, inout float2 polyPoints[POLYGON_MAX_POINTS], inout int index)
            {
                index = 0;
                appendCorner(cellcoord, pathIndex, float2(-1.0, -1.0), polyPoints, index);
                appendEdge(cellcoord, pathIndex, float2(0.0, -1.0), polyPoints, index);
                appendCorner(cellcoord, pathIndex, float2(1.0, -1.0), polyPoints, index);
                appendEdge(cellcoord, pathIndex, float2(1.0, 0.0), polyPoints, index);
                appendCorner(cellcoord, pathIndex, float2(1.0, 1.0), polyPoints, index);
                appendEdge(cellcoord, pathIndex, float2(0.0, 1.0), polyPoints, index);
                appendCorner(cellcoord, pathIndex, float2(-1.0, 1.0), polyPoints, index);
                appendEdge(cellcoord, pathIndex, float2(-1.0, 0.0), polyPoints, index);
            }

            void getSdfPolygon(float4 currentTile, float2 cellcoord, out float2 polyPoints[POLYGON_MAX_POINTS], out int index)
            {
                const float pathIndex = currentTile.g;
                index = 0;
                for (int i = 0; i < POLYGON_MAX_POINTS; i++)
                    polyPoints[i] = float2(0.0, 0.0);

                // If solid tile
                if (abs(currentTile.r - 1.0) < 0.001)
                    getSdfPolygonFromSquare(pathIndex, cellcoord, polyPoints, index);
                else
                    getSdfPolygonFromSlope(currentTile, cellcoord, polyPoints, index);
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
                // Testing: sample the distance relative to tile (0, 0) for every fragment.
                //float2 cellCoord = float2(1.0, 1.0);
                float2 cellCoord = floor(gridPosition);
                // Get the position of this pixel in the cell since the sdfs are defined in the range [-0.5, 0.5] for each cell
                float2 gridCellUv = gridPosition - cellCoord - 0.5;

                float4 currentTile = getTile(cellCoord);

                // If there is no tile here, return a large distance so we don't draw anything
                if (currentTile.r <= 0.01)
                   return MAX_DISTANCE;

                float2 polyPoints[POLYGON_MAX_POINTS];
                int pointCount;
                getSdfPolygon(currentTile, cellCoord, polyPoints, pointCount);
                return sdPolygon(polyPoints, gridCellUv, pointCount);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float d = sdAtUv(i.uv);
                // alpha cut all the positive values outside of the shape but gradient blend 0.01 units outside of the shape till the proper edge
                float alpha = smoothstep(-0.01, 0.00, -d);
                // Cut out the inside of the shape to only get a 0.05 pixel thickness inside
                alpha = min(alpha, smoothstep(-0.11, -0.06, d));
                float value = smoothstep(-0.01, 0.05, -d) + 0.7;
                return fixed4(float3(value,value,value), alpha);



                // float3 col = (d>0.0) ? float3(0.9,0.6,0.3) : float3(0.65,0.85,1.0);
	            // col *= 1.0 - exp2(-20.0*abs(d));
	            // col *= 0.8 + 0.2*cos(120.0*abs(d));
	            // col = lerp(col, float3(1.0,1.0,1.0), 1.0-smoothstep(0.0,0.01,abs(d)));
                // return fixed4(col, 1.0);
            }
            ENDCG
        }
    }
}
