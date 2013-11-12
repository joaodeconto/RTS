// Stippling Alpha Test shader for Automatic 3D Billboard Imposters
// Assign this to the model material
// By CWKX

Shader "StipplingAlphaTest" {
Properties
{
	// Main
	_MainTex ("Texture (RGB)", 2D) = "white" {}
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	
	// Stippling
	_Stippling ("Stippling", Range(0,1)) = 1
	_StipplingNoise ("Alpha (A)", 2D) = "white" {}
}
SubShader 
{
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
	Cull Off
	
	CGPROGRAM
	#pragma surface surf Lambert

	sampler2D _MainTex;
	sampler2D _StipplingNoise;
	fixed 	  _Cutoff;
	fixed 	  _Stippling;

 	struct Input {
		float2 uv_MainTex;
  	};
  	
	void surf (Input IN, inout SurfaceOutput o)
	{
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
		
		if (c.a < _Cutoff || _Stippling < tex2D(_StipplingNoise, IN.uv_MainTex).a)
			discard;
			
		o.Albedo = c.rgb;
		o.Alpha = c.a;
	}
	
	ENDCG
}
Fallback "Transparent/Cutout/VertexLit"
}