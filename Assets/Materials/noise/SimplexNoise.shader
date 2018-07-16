Shader "Custom/SimplexNoise"
{
	Properties
	{
		_Speed("Speed", Range(0,10)) = 1.0
		_Velocity("Velocity", Vector) = (0,0,0,0)
	}

		CGINCLUDE

#include "UnityCG.cginc"
#include "SimplexNoise3D.hlsl"

		float _Speed;
	float4 _Velocity;

	v2f_img vert(appdata_base v)
	{
		v2f_img o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		return o;
	}

	float4 frag(v2f_img i) : SV_Target
	{
		const float epsilon = 0.0001;
	//float2 uv = i.uv * 4.0 + float2(0.2, 1) * _Time.y *_Offset;
	float2 uv = i.uv * 4.0 + _Velocity.xy * _Time.y * _Speed;
	//float2 uv = i.uv * 4.0 + float2(0.2, 1) * _Offset;// _Time.y;

	float o = 0.5;
	float s = 1.0;
	float w = 0.25;

	for (int i = 0; i < 6; i++)
	{
		float3 coord = float3(uv * s, _Time.y * _Speed);// *_Offset);
														//float3 coord = float3(uv * s, _Offset);
														//float3 period = float3(s, s, 1.0) * 2.0;

		o += snoise(coord) * w;

		s *= 2.0;
		w *= 0.5;
	}

	return float4(o, o, o, 1);
	}

		ENDCG

		SubShader
	{
		Pass
		{
			CGPROGRAM
#pragma target 3.0
#pragma vertex vert
#pragma fragment frag
			ENDCG
		}
	}
}