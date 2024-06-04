Shader "Custom/VertexExtruding" {
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
			#include "FastNoiseLite.hlsl"

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
				SHADOW_COORDS(1)
				float3 normal : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
			};

			// constant for hash function
			#define M1 1597334677U     //1719413*929
			#define M2 3812015801U     //140473*2467*11


			// ----- SHADER PARAMS -----

			// --- vars for vertex extrusion
			// index of the current shell layer; 0 to _ShellCount-1
			int _ShellIndex;
			// total number of shell layers
			int _ShellCount;
			// distance that all shells combined cover (in world space units)
			float _TotalShellLength;
			// density of individual strands

			// --- vars for tuning fur behavior/looks
			// minimum and maximum normalized length of individual strands
			float _MinNormalizedLength, _MaxNormalizedLength;
			float _ShellDensity;
			float _Thickness;
			// how much the shells are pushed outwards
			float _ShellDistanceAttenuation;
			float3 _ShellColor; // TODO: sample the color from the model texture (not uniform color everywhere)

			// --- vars for lightning
			// bias for the ambient occlusion
			float _OcclusionBias;
			// attenuation of the shell height for lighting calculations to fake ambient occlusion
			float _Attenuation;

			// --- vars for displacement/movement
			// controls the hair stiffness (how much the strands are curved)
			float _Curvature;
			// strength of the displacement
			float _DisplacementStrength;
			// direction of the displacement; updated each frame
			float3 _ShellDisplacementDir;

			// hash function source: https://www.shadertoy.com/view/MdcfDj
			float hash12(uint2 q)
			{
				q *= uint2(M1, M2);
				uint n = (q.x ^ q.y) * M1;
				return float(n) * (1.0 / float(0xffffffffU));
			}

			// In the vertex shader we extrude the shells
			VertOutFracIn vertex_shader(VertexIn v) {
				// define output
				VertOutFracIn frag;

				// apply shell distance attenuation
				float shellHeight = (float)_ShellIndex / (float)_ShellCount;
				shellHeight = pow(shellHeight, _ShellDistanceAttenuation);

				// Extrude the shells based on normal, shell height and length
				v.vertexPos.xyz += v.normal.xyz * _TotalShellLength * shellHeight;

				// set the output values
				frag.normal = normalize(UnityObjectToWorldNormal(v.normal));


				// Displace shells based on the cpu input
					// curvature and displacement strength used as tuning parameters
					// highger curvature -> more displacement
					// due to k, only the tips of the hair are be displaced
				float k = pow(shellHeight, _Curvature);
				v.vertexPos.xyz += _ShellDisplacementDir * k * _DisplacementStrength;

				frag.worldPos = mul(unity_ObjectToWorld, v.vertexPos);
				frag.pos = UnityObjectToClipPos(v.vertexPos);
				frag.uv = v.uv;

				TRANSFER_SHADOW(frag)

				return frag;
			}

			float4 fragment_shader(VertOutFracIn i) : SV_Target
			{
				// Sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv) * _Color;
				float3 col = _ShellColor;

				// Calculate lighting
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz); // Example directional light direction
				float diffuse = max(0, dot(i.normal, lightDir));

				// Apply lighting to the color
				col.rgb *= diffuse;

				return (col, 1);
			}

			// In the fragment shader we discard fragments that do not belong to any strand of hair/fur/grass
			// we also do the calculation
			//float4 fragment_shader(VertOutFracIn i) : SV_TARGET {				

			//	// Vector
			//	half3 normal = normalize(i.normal);
			//	half3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
			//	half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
			//	half3 halfDir = normalize(lightDir + viewDir);

			//	// Dot
			//	half NdotL = saturate(dot(normal, lightDir));
			//	half NdotH = saturate(dot(normal, halfDir));

			//	// Color
			//	fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb * _ShellColor.rgb;
			//	fixed3 diffuse = _LightColor0.rgb * _ShellColor.rgb * NdotL;
			//	//fixed3 specular = _LightColor0.rgb * _ShellColor.rgb * pow(NdotH, 1);
			//	//fixed4 color = fixed4(ambient + diffuse + specular, 1.0);
			//	fixed4 color = fixed4(diffuse, 1.0);

			//	return color;
			//}

			ENDCG
		} // end of ForwardBase pass
	}
}
