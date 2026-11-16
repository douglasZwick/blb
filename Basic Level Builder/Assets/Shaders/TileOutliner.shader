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

            // uv is the position of this pixel in uv space, wh is the width and height of the box
            float sdBox(float2 uv, float2 wh)
            {
                float2 d = abs(uv)-wh;
                return length(max(d,0.0)) + min(max(d.x,d.y),0.0);
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
                distance = min(distance, getSdTile(cellCoord + float2(1,0), gridCellUv + float2(-1,0), pathIndex));
                distance = min(distance, getSdTile(cellCoord + float2(-1,0), gridCellUv + float2(1,0), pathIndex));
                distance = min(distance, getSdTile(cellCoord + float2(0,1), gridCellUv + float2(0,-1), pathIndex));
                distance = min(distance, getSdTile(cellCoord + float2(0,-1), gridCellUv + float2(0,1), pathIndex));

                return distance;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float d = sdAtUv(i.uv);
                float3 col = (d>0.0) ? float3(0.9,0.6,0.3) : float3(0.65,0.85,1.0);
	            col *= 1.0 - exp2(-20.0*abs(d));
	            col *= 0.8 + 0.2*cos(120.0*abs(d));
	            col = lerp(col, float3(1.0,1.0,1.0), 1.0-smoothstep(0.0,0.01,abs(d)));
                // Keep empty cells transparent so path textures can layer.
                //float alpha = smoothstep(0.0, 0.5, abs(d));
                return fixed4(col, 1.0);
            }
            ENDCG
        }
    }
}
