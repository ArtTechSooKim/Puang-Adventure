Shader "UI/CircularMiniMapMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _InnerRadius ("Inner Radius", Range(0, 1)) = 0.9
        _OuterRadius ("Outer Radius", Range(0, 1)) = 1.0
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
        ZTest [unity_GUIZTestMode]
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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _InnerRadius;
            float _OuterRadius;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 텍스처 샘플링
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                // 중심으로부터 거리 계산 (0.5, 0.5가 중심)
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center) * 2.0; // 0~1 범위로 정규화

                // 원형 페이드 적용
                float alpha = 1.0;

                if (dist > _InnerRadius)
                {
                    // InnerRadius와 OuterRadius 사이에서 부드럽게 페이드
                    float fadeRange = _OuterRadius - _InnerRadius;
                    alpha = 1.0 - smoothstep(_InnerRadius, _OuterRadius, dist);
                }

                // 알파 적용
                col.a *= alpha;

                // 완전 투명한 픽셀은 버림
                if (col.a < 0.01)
                    discard;

                return col;
            }
            ENDCG
        }
    }
}
