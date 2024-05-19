Shader "Custom/ShellExperimentalNoShadow" {
	SubShader{
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
		#pragma geometry geometry_shader
		#pragma fragment fragment_shader


		// The input struct of the vertex shader
		struct appdata
		{
			float4 vertexPos : POSITION;
			float4 normal : NORMAL;
			float2 uv : TEXCOORD0;
		};

		struct v2g
		{
			float4 vertexPos : POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
		};
		struct g2f {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			SHADOW_COORDS(1)
				float3 normal : TEXCOORD2;
			float3 worldPos : TEXCOORD3;
			float shellIndex : TEXCOORD4;
		};

		// constant for hash function
		#define M1 1597334677U     //1719413*929
		#define M2 3812015801U     //140473*2467*11


		// ----- SHADER PARAMS -----

		// --- vars for vertex extrusion
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
		v2g vertex_shader(appdata v) {
			v2g o;
			o.vertexPos = v.vertexPos;
			o.uv = v.uv;
			o.normal = v.normal;
			return o;
		}

		// Geometry shader
		[maxvertexcount(48)] // 3 vertices per triangle * 16 shells = 48 vertices max
		void geometry_shader(triangle v2g input[3], inout TriangleStream<g2f> triStream) {
			for (int height = 0; height < _ShellCount; height++) {
				float shellHeight = (float)height / (float)_ShellCount;
				shellHeight = pow(shellHeight, _ShellDistanceAttenuation);

				for (int j = 0; j < 3; j++) {
					g2f o;

					// Extrude the shells based on normal, shell height, and length
					input[j].vertexPos.xyz += input[j].normal * _TotalShellLength * shellHeight;

					o.normal = normalize(UnityObjectToWorldNormal(input[j].normal));

					// Displace shells based on curvature and displacement strength
					float k = pow(shellHeight, _Curvature);
					input[j].vertexPos.xyz += _ShellDisplacementDir * k * _DisplacementStrength;

					o.worldPos = mul(unity_ObjectToWorld, input[j].vertexPos).xyz;
					o.pos = UnityObjectToClipPos(input[j].vertexPos);
					o.uv = input[j].uv;
					o.shellIndex = (float)height / (float)_ShellCount;

					TRANSFER_SHADOW(o)

					triStream.Append(o);
				}
			}
		}

		// In the fragment shader we discard fragments that do not belong to any strand of hair/fur/grass
		// we also do the calculation
		float4 fragment_shader(g2f frag) : SV_TARGET {

			// ----- DISCARDING FRAGMENTS -----

			// By taking the fractional component of the uv coordinates, 
			// we get uvs in the local space of the individual strand
			// = we split the space into many small spaces, one for each strand

			// Make the uvs bigger -> we get more strands
			float2 newUV = frag.uv * _ShellDensity;
			// Shift the local uvs to the center of the strand (-1 - 1 range)
			float2 localUV = frac(newUV) * 2 - 1;
			// Now we can get the distance from the center of the strand
			float localDistanceFromCenter = length(localUV);

			// Introduce length variance to the strands
			fnl_state random_state = fnlCreateState(42);
			random_state.noise_type = FNL_NOISE_VALUE;
			// We use the newUV as a seed for determining the strand length
			// use the hash function to generate a random number between 0, 1 based on the uvs
			float lerp_val = fnlGetNoise2D(random_state, (100 * newUV.x), (100 * newUV.y)); // -1 to 1
			lerp_val = lerp_val * 0.5 + 0.5; // 0 to 1
			float random = lerp(_MinNormalizedLength, _MaxNormalizedLength, lerp_val);

			// Normalized shell height
			float height = frag.shellIndex / (float)_ShellCount;
			// if the distance from the center is greater than thickness, we discard it, the thickness decreases with height
			int isOutside = (localDistanceFromCenter) > (_Thickness * (random - height));

			// we can only discard the fragment if we are not in the lowest shell
			if (isOutside && frag.shellIndex > 0) discard;

			// Now we know that the pixel is not discarded
			// Lets color it

			// ----- LIGHTING -----

			// We use the non physically based model - Valve's half lambert
			// _WorldSpaceLightPos0 is the direction of the main light in world space

			float3 normalDirection = normalize(frag.normal);
			float3 viewDirection = normalize(
				_WorldSpaceCameraPos - frag.worldPos.xyz);
			float3 lightDirection;
			float attenuation;

			if (0.0 == _WorldSpaceLightPos0.w) // directional light?
			{
				attenuation = 1.0; // no attenuation
				lightDirection =
					normalize(_WorldSpaceLightPos0.xyz);
			}
			else // point or spot light
			{
				float3 vertexToLightSource =
					_WorldSpaceLightPos0.xyz - frag.worldPos.xyz;
				float distance = length(vertexToLightSource);
				attenuation = 1.0 / distance; // linear attenuation 
				lightDirection = normalize(vertexToLightSource);
			}

			// Perform the half lambert shading
			// take the dot product of the normal and the light direction
			// clamp it to 0, 1
			// convert it to 0.5, 1
			// square it
			float halfDot = DotClamped(frag.normal, lightDirection);
			halfDot = halfDot * 0.5f + 0.5f;
			halfDot = halfDot * halfDot;

			// Fake ambient occlusion effect

			// the lower the shell, the less light we get (note h is in 0-1 range)
			float ambientOcclusion = pow(height, _Attenuation);

			// Add a bias to allow to not go to full black/dark in the lowest layer
			ambientOcclusion += _OcclusionBias;
			// then clamp it to 0, 1
			ambientOcclusion = saturate(ambientOcclusion);

			// compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
			float shadow = SHADOW_ATTENUATION(frag);

			// Put it all together and include ambient light
			//return float4(shadow * attenuation * _LightColor0 * _ShellColor * halfDot * ambientOcclusion, 1.0);
			return saturate(float4(shadow * attenuation * _LightColor0 * _ShellColor * halfDot * ambientOcclusion, 1.0) + float4(unity_IndirectSpecColor.xyz * _ShellColor * halfDot * ambientOcclusion, 1.0));
		}

		ENDCG
	} // end of ForwardBase pass
	}
}
