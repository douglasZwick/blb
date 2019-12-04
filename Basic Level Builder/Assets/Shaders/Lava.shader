// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "BLB/Lava"
{
  Properties
  {
    [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
    [HDR] _ColorA("Color A", Color) = (1, 1, 1, 1)
    [HDR] _ColorB("Color B", Color) = (1, 1, 1, 1)
    _Period("Period", Float) = 1
    _Exponent("Exponent", Float) = 4
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
      Blend SrcAlpha OneMinusSrcAlpha

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

        fixed4 _ColorA;
        fixed4 _ColorB;
        float _Period;
        float _Exponent;
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

        fixed4 frag(v2f i) : SV_Target
        {
          fixed4 color = tex2D(_MainTex, i.uv);
          
          float time = _Time.y;
          float t = fmod(time, _Period);
          float fraction = t / _Period;
          float innerPart = 2.0f * abs(fraction - 0.5f);
          float interpolant = pow(innerPart, _Exponent);
          fixed4 lerpedColor = lerp(_ColorA, _ColorB, interpolant);

          color *= lerpedColor;
          color *= i.color;

          return color;
        }
        ENDCG
      }
    }
}
