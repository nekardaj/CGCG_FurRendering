Shader "Custom/ShadowCaster" {
	SubShader {
		// ForwardBase rendering pass
		Pass {
			Tags {
				"LightMode" = "ForwardBase"
			}
			// Backface culling turned off as we can see through the fur
			Cull Off

			CGPROGRAM

			// Unity built in graphics functions
			#include "UnityPBSLighting.cginc"
			#include "AutoLight.cginc"

			#pragma multi_compile_fwdbase

			// Set shader stages for rendering pipeline
			#pragma vertex vertex_shader
			#pragma fragment fragment_shader


			// The input struct of the vertex shader
			struct VertexIn {
				float4 vertexPos : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			// Struct for the output of the vertex shader, passed to the fragment shader
			struct VertOutFracIn {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
			};

			// ----- SHADER PARAMS -----
			float _MoveDist;
			bool _Discard;

			// In the vertex shader we extrude the shells
			VertOutFracIn vertex_shader(VertexIn v) {
				// define output
				VertOutFracIn frag;

				// Extrude the shells based on normal, shell height and length
				v.vertexPos.xyz -= v.normal.xyz * _MoveDist;

				// set the output values
				frag.normal = normalize(UnityObjectToWorldNormal(v.normal));


				// Displace shells based on the cpu input
					// curvature and displacement strength used as tuning parameters
					// highger curvature -> more displacement
					// due to k, only the tips of the hair are be displaced

				frag.worldPos = mul(unity_ObjectToWorld, v.vertexPos);
				frag.pos = UnityObjectToClipPos(v.vertexPos);
				frag.uv = v.uv;

				return frag;
			}

			// In the fragment shader we discard fragments that do not belong to any strand of hair/fur/grass
			// we also do the calculation
			float4 fragment_shader(VertOutFracIn frag) : SV_TARGET{
				//if (_Discard == 1) discard;
				discard;
				return float4(0, 0, 0, 1);
			}

			ENDCG
		} // end of ForwardBase pass

		

		// shadow caster rendering pass, implemented manually
		// using macros from UnityCG.cginc
		Pass
		{
			Tags {"LightMode" = "ShadowCaster"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		} // end of ShadowCaster pass
	}
}
