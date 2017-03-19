Shader "Custom/GPUParticleRender" {
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
	SubShader{
		ZWrite Off
		Cull Off
		Blend SrcAlpha One

		Pass{
		CGPROGRAM

#pragma target 5.0

#pragma vertex vert
#pragma geometry geom
#pragma fragment frag

#include "UnityCG.cginc"
#include "Assets/MeshVertexTexture/Shaders/MeshVertexTexture.cginc"
#include "GPUParticleData.cginc"

		sampler2D _MainTex;

		float _ParticleScale;
		float _PositionRatio;

		StructuredBuffer<ParticleData> _ParticleBuffer;

		struct VSOut {
			float4 pos : SV_POSITION;
			float2 tex : TEXCOORD0;
			float4 col : COLOR;
		};

		VSOut vert(uint id : SV_VertexID)
		{
			VSOut output;
			output.pos = float4(lerp(_ParticleBuffer[id].orgPosition, _ParticleBuffer[id].position, _PositionRatio), 1);
			output.tex = float2(0, 0);
			output.col = _ParticleBuffer[id].color;

			return output;
		}

		[maxvertexcount(4)]
		void geom(point VSOut input[1], inout TriangleStream<VSOut> outStream)
		{
			VSOut output;

			float4 pos = input[0].pos;
			float4 col = input[0].col;

			float4x4 billboardMatrix = UNITY_MATRIX_V;
			billboardMatrix._m03 =
			billboardMatrix._m13 =
			billboardMatrix._m23 =
			billboardMatrix._m33 = 0;

			for (int x = 0; x < 2; x++)
			{
				for (int y = 0; y < 2; y++)
				{
					float2 tex = float2(x, y);
					output.tex = tex;

					output.pos = pos + mul(float4((tex * 2 - float2(1, 1)) * _ParticleScale, 0, 1), billboardMatrix);
					output.pos = mul(UNITY_MATRIX_VP, output.pos);

					output.col = col;

					outStream.Append(output);
				}
			}

			outStream.RestartStrip();
		}

		fixed4 frag(VSOut i) : COLOR
		{
			return tex2D(_MainTex, i.tex) * i.col;
		}

		ENDCG
		}
	}
}