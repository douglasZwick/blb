Shader "BLB/TileOutliner"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        [PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
        _Color("Tint", Color) = (1, 1, 1, 1)
        _TileGridTex("Tile Grid", 2D) = "black" {}
        _TileGridStartPosition("Tile Grid Start Position", Vector) = (0, 0, 0, 0)
        [PerRendererData] _TileWorldPositionOffset("Tile World Position Offset", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "CanUseSpriteAtlas" = "True" }

        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #include "UnityCG.cginc"

            #define POLYGON_MAX_POINTS 16
            #define FLOAT_EQ(a, b) (abs((a) - (b)) < 1e-5)
            #define FLOAT_EQ_STEP(a, b) (1.0 - step(1e-5, abs((a) - (b))))

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 worldPosition : TEXCOORD1;
                fixed4 color : COLOR;
            };

            static const float MAX_DISTANCE = float(1e6);
            static const float BG_OUTLINE_ALPHA_CAP = 0.1;
            static const float SOLID_OUTLINE_ALPHA_CAP = 1.0;
            sampler2D _MainTex;
            sampler2D _AlphaTex;
            fixed4 _Color;
            sampler2D _TileGridTex;
            float4 _TileGridTex_TexelSize;
            float2 _TileGridStartPosition;
            float2 _TileWorldPositionOffset;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex).xy + _TileWorldPositionOffset;
                o.color = v.color;
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

            // If tile is solid/slope/falseSolid. Ie not a bg tile
            bool isTileSolid(float2 cellcoord)
            {
                return getTile(cellcoord).b < 0.001;
            }

            bool isTileValidPath(float4 tile, float pathIndex)
            {
                return FLOAT_EQ(tile.g, pathIndex);
            }

            bool isTileValidType(float4 tile, float typeValue)
            {
                return FLOAT_EQ(tile.r, typeValue);
            }

            void appendPoly(inout int index, inout float2 polyPoints[POLYGON_MAX_POINTS], float2 a, float2 b)
            {
                polyPoints[index++] = a;
                polyPoints[index++] = b;
            }

            void appendCorner(float2 cellcoord, float4 currentTile, float2 direction,
                inout float2 polyPoints[POLYGON_MAX_POINTS], inout int index)
            {
                const float pathIndex = currentTile.g;
                const float isCenterBg = currentTile.b;
                float4 tile = getTile(cellcoord + direction);
                float slopeType = direction.y < 0.0
                    ? (direction.x < 0.0 ? 4.0 : 3.0)
                    : (direction.x > 0.0 ? 2.0 : 5.0);

                if (isTileValidPath(tile, pathIndex) &&
                    (isTileValidType(tile, 1.0) || isTileValidType(tile, slopeType)) && tile.b <= isCenterBg)
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

            void appendEdge(float2 cellcoord, float4 currentTile, float2 direction,
                inout float2 polyPoints[POLYGON_MAX_POINTS], inout int index)
            {
                const float pathIndex = currentTile.g;
                const float isCenterBg = currentTile.b;
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
                if (isTileValidPath(tile, pathIndex) && tile.b <= isCenterBg)
                {
                    if (isTileValidType(tile, 1.0))
                    {
                        a = float2(-0.5, -1.5);
                        b = float2(0.5, -1.5);
                    }
                    else if (FLOAT_EQ(typeValue, 3.0))
                    {
                        a = float2(-0.5, -1.5);
                        b = float2(0.5, -0.5);
                    }
                    else if (FLOAT_EQ(typeValue, 4.0))
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
                index = 0;
                if (isTileValidType(currentTile, 2.0))
                {
                    appendCorner(cellcoord, currentTile, float2(-1.0, -1.0), polyPoints, index);
                    appendEdge(cellcoord, currentTile, float2(0.0, -1.0), polyPoints, index);
                    appendPoly(index, polyPoints, float2(0.5, -0.5), float2(-0.5, 0.5));
                    appendEdge(cellcoord, currentTile, float2(-1.0, 0.0), polyPoints, index);
                }
                else if (isTileValidType(currentTile, 3.0))
                {
                    appendCorner(cellcoord, currentTile, float2(-1.0, 1.0), polyPoints, index);
                    appendEdge(cellcoord, currentTile, float2(-1.0, 0.0), polyPoints, index);
                    appendPoly(index, polyPoints, float2(-0.5, -0.5), float2(0.5, 0.5));
                    appendEdge(cellcoord, currentTile, float2(0.0, 1.0), polyPoints, index);
                }
                else if (isTileValidType(currentTile, 4.0))
                {
                    appendCorner(cellcoord, currentTile, float2(1.0, 1.0), polyPoints, index);
                    appendEdge(cellcoord, currentTile, float2(0.0, 1.0), polyPoints, index);
                    appendPoly(index, polyPoints, float2(-0.5, 0.5), float2(0.5, -0.5));
                    appendEdge(cellcoord, currentTile, float2(1.0, 0.0), polyPoints, index);
                }
                else if (isTileValidType(currentTile, 5.0))
                {
                    appendCorner(cellcoord, currentTile, float2(1.0, -1.0), polyPoints, index);
                    appendEdge(cellcoord, currentTile, float2(1.0, 0.0), polyPoints, index);
                    appendPoly(index, polyPoints, float2(0.5, 0.5), float2(-0.5, -0.5));
                    appendEdge(cellcoord, currentTile, float2(0.0, -1.0), polyPoints, index);
                }
            }

            void getSdfPolygonFromSquare(float4 currentTile, float2 cellcoord, inout float2 polyPoints[POLYGON_MAX_POINTS], inout int index)
            {
                index = 0;
                appendCorner(cellcoord, currentTile, float2(-1.0, -1.0), polyPoints, index);
                appendEdge(cellcoord, currentTile, float2(0.0, -1.0), polyPoints, index);
                appendCorner(cellcoord, currentTile, float2(1.0, -1.0), polyPoints, index);
                appendEdge(cellcoord, currentTile, float2(1.0, 0.0), polyPoints, index);
                appendCorner(cellcoord, currentTile, float2(1.0, 1.0), polyPoints, index);
                appendEdge(cellcoord, currentTile, float2(0.0, 1.0), polyPoints, index);
                appendCorner(cellcoord, currentTile, float2(-1.0, 1.0), polyPoints, index);
                appendEdge(cellcoord, currentTile, float2(-1.0, 0.0), polyPoints, index);
            }

            void getSdfPolygon(float4 currentTile, float2 cellcoord, out float2 polyPoints[POLYGON_MAX_POINTS], out int index)
            {
                index = 0;
                for (int i = 0; i < POLYGON_MAX_POINTS; i++)
                    polyPoints[i] = float2(0.0, 0.0);

                // If solid tile
                if (FLOAT_EQ(currentTile.r, 1.0))
                    getSdfPolygonFromSquare(currentTile, cellcoord, polyPoints, index);
                else
                    getSdfPolygonFromSlope(currentTile, cellcoord, polyPoints, index);
            }

            float sdAtWorldPosition(float2 worldPosition, out float outlineAlpha)
            {
                float2 gridPosition = worldPosition + 0.5 - _TileGridStartPosition;
                float2 cellCoord = floor(gridPosition);
                // Get the position of this pixel in the cell since the sdfs are defined in the range [-0.5, 0.5] for each cell
                float2 gridCellUv = gridPosition - cellCoord - 0.5;

                float4 currentTile = getTile(cellCoord);
                
                // If there is no tile here, return a large distance so we don't draw anything
                if (currentTile.r <= 0.01)
                {
                    outlineAlpha = 0.0;
                    return MAX_DISTANCE;
                }
                
                // Get the alpha for the outline from the tile type. 1 = bg 0 = solid
                outlineAlpha = lerp(SOLID_OUTLINE_ALPHA_CAP, BG_OUTLINE_ALPHA_CAP, currentTile.b);
                
                float2 polyPoints[POLYGON_MAX_POINTS];
                int pointCount;
                getSdfPolygon(currentTile, cellCoord, polyPoints, pointCount);
                return sdPolygon(polyPoints, gridCellUv, pointCount);
            }

            fixed4 sampleSprite(float2 uv)
            {
                fixed4 sprite = tex2D(_MainTex, uv);
                #if ETC1_EXTERNAL_ALPHA
                sprite.a = tex2D(_AlphaTex, uv).r;
                #endif
                return sprite;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float outlineAlpha = 1;
                float d = sdAtWorldPosition(i.worldPosition, outlineAlpha);
                // alpha cut all the positive values outside of the shape but gradient blend 0.01 units outside of the shape till the proper edge
                float alpha = smoothstep(-0.01, 0.00, -d);
                // Cut out the inside of the shape to only get a 0.05 pixel thickness inside
                alpha = min(alpha, smoothstep(-0.11, -0.06, d));
                float value = smoothstep(-0.01, 0.05, -d) + 0.7;
                fixed4 outline = fixed4(value, value, value, min(alpha, outlineAlpha));
                fixed4 sprite = sampleSprite(i.uv) * i.color * _Color;
                fixed4 result;
                result.a = outline.a + sprite.a * (1.0 - outline.a);
                result.rgb = (outline.rgb * outline.a + sprite.rgb * sprite.a * (1.0 - outline.a)) / max(result.a, 1e-5);
                return result;

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
