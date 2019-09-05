Shader "OutlineShader" {
	Properties 
	{
		_Outline ("Outline", Range(0,0.1)) = 0
		_OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
	}

	SubShader 
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 200
		
		Pass
		{
			Name "BASE"
			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f 
			{
				float4 pos : SV_POSITION;
			};

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};


			float _Outline;
			float4 _OutlineColor;

			v2f vert(appdata v)
			{
				v2f o;

				o.pos = UnityObjectToClipPos(v.vertex);

				float3 normal = mul((float3x3) UNITY_MATRIX_MV, v.normal);
				normal.x *= UNITY_MATRIX_P[0][0];
				normal.y *= UNITY_MATRIX_P[1][1];

				float outline = _Outline;
				o.pos.xy += normal.xy * outline;
				return o;
			}

			fixed4 frag(v2f i) :  SV_Target
			{
				float4 outlineColor = _OutlineColor;

				return outlineColor;
			}

			ENDCG
		}
	}

		FallBack "Mobile/VertexLit"
}
