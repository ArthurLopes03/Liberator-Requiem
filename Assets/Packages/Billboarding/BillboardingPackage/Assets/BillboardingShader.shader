Shader "Unlit/BillboardingShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AnimationSpeed ("Animation Speed", float) = 1
		_Color ("Color Tint", Color) = (0.5, 0.5, 0.5, 0.5)
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "DisableBatching" = "True" }

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		ZTest Always

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
				UNITY_FOG_COORDS(1)
				float4 pos : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;

			float _AnimationSpeed;

			float4 _Color;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv.xy;

				// billboard mesh towards camera
				float3 vpos = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
				float4 worldCoord = float4(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23, 1);
				float4 viewPos = mul(UNITY_MATRIX_V, worldCoord) + float4(vpos, 0);
				float4 outPos = mul(UNITY_MATRIX_P, viewPos);

				o.pos = outPos;
				
				return o;
			}

			float BlendMode_Overlay(float base, float blend)
			{
				return (base <= 0.5) ? 2*base*blend : 1 - 2*(1-base)*(1-blend);
			}
			
			float4 frag (v2f i) : SV_Target
			{

				
				

				float tiling = _MainTex_TexelSize.x/_MainTex_TexelSize.y;
				

				float ammountFrames = _MainTex_TexelSize.y/_MainTex_TexelSize.x;
				float frame = trunc((_Time.y * _AnimationSpeed)% ammountFrames);

				float offset = tiling * frame;
				
				float2 tiledUv = float2(i.uv.x * tiling + offset, i.uv.y);
				float4 col = tex2D(_MainTex, tiledUv);

				return col;
			}
			ENDCG
		}
	}
}
