Shader "Voxel/Block Shader"{

	Properties{
		_MainTex("Block Texture Atlas", 2D) = "white" {}
	}

		SubShader{
		//Tags {"Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout"}
		Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100
		Lighting Off

		Pass
			{
				CGPROGRAM
					#pragma vertex vertFunction
					#pragma fragment fragFunction
					#pragma target 2.0

					#include "UnityCG.cginc"

					struct appdata
					{
						float4 vertex : POSITION;
						float2 uv : TEXCOORD0;
						float4 color : COLOR;
						float4 worldPos : TEXCOORD1;
					};

					struct v2f
					{
						float4 vertex : SV_POSITION;
						float2 uv : TEXCOORD0;
						float4 color : COLOR;
						float4 worldPos : TEXCOORD1;
					};

					sampler2D _MainTex;
					float globalLightLevel;
					float minGlobalLightLevel;
					float maxGlobalLightLevel;
					float fogRadius;

					float4 skyTint;

					float4 daySkyColor;


					v2f vertFunction(appdata v) 
					{
						v2f o;

						o.vertex = UnityObjectToClipPos(v.vertex);
						o.uv = v.uv;
						o.color = v.color;
						o.worldPos = mul(unity_ObjectToWorld, v.vertex);

						return o;
					}

					fixed4 fragFunction(v2f i) : SV_Target
					{
						fixed4 col = tex2D(_MainTex, i.uv);

						
						float shade = globalLightLevel;
						shade *= i.color.a;
						shade = clamp(1 - shade, minGlobalLightLevel, maxGlobalLightLevel);

						float globalLightLevelHighlights = clamp(globalLightLevel, .2, 1);
						shade -= i.color.r * globalLightLevelHighlights; // highlights

						clip(col.a - 1);

						col = lerp(col, float4(0, 0, 0, 1), shade);

						// sky tint
						col.x *= skyTint.x;
						col.y *= skyTint.y;
						col.z *= skyTint.z;

						// fog
						float dist = distance(i.worldPos, _WorldSpaceCameraPos);
						float distByFog = (dist / fogRadius);
						float dF = clamp(distByFog, 0, 1);
						col = lerp(col, float4(daySkyColor.x, daySkyColor.y, daySkyColor.z, 1), saturate(dF));

						return col;
					}

						ENDCG
			}

	}
}