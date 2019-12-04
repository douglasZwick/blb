// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/Additive"
{
  Properties
  {
    [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
    [HDR] _Color("Additive Tint", Color) = (1, 1, 1, 1)
    [MaterialToggle] PixelSnap("Pixel Snap", Float) = 0
  }

    SubShader
    {
      Tags
      {
        "Queue" = "Transparent"
        "IgnoreProjector" = "True"
        "RenderType" = "Transparent"
        "PreviewType" = "Plane"
        "CanUseSpriteAtlas" = "True"
      }

      Cull Off
      Lighting Off
      ZWrite Off
      Blend One OneMinusSrcAlpha

      Pass
      {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma multi_compile _ PIXELSNAP_ON
        #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
        #include "UnityCG.cginc"

        struct appdata
        {
          float4 vertex : POSITION;
          float4 color : COLOR;
          float2 uv : TEXCOORD0;
        };

        struct v2f
        {
          fixed4 color : COLOR;
          float2 uv : TEXCOORD0;
          float4 vertex : SV_POSITION;
        };

        fixed4 _Color;
        sampler2D _MainTex;
        sampler2D _AlphaTex;

        v2f vert(appdata v)
        {
          v2f o;
          o.vertex = UnityObjectToClipPos(v.vertex);
          o.uv = v.uv;
          o.color = v.color;

          #ifdef PIXELSNAP_ON
          o.vertex = UnityPixelSnap(o.vertex);
          #endif

          return o;
        }

        fixed4 SampleSpriteTexture(float2 uv)
        {
          fixed4 color = tex2D(_MainTex, uv);
          #if ETC1_EXTERNAL_ALPHA
          color.a = tex2D(_AlphaTex, uv).r;
          #endif
          return color;
        }

        fixed4 frag(v2f i) : SV_Target
        {
          fixed4 color = SampleSpriteTexture(i.uv) * i.color;

          color.rgb = color.rgb * _Color.rgb;
          color.rgb *= color.a;

          return color;
        }
        ENDCG
      }
    }
}
