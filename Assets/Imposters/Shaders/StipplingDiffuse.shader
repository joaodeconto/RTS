// Stippling Diffuse shader for Automatic 3D Billboard Imposters
// Assign this to the model material
// By CWKX

Shader "StipplingDiffuse" {
Properties
{
	// Main
	_MainTex ("Texture (RGB)", 2D) = "white" {}
	
	// Stippling
	_Stippling ("Stippling", Range(0,1)) = 1
	_StipplingNoise ("Alpha (A)", 2D) = "white" {}
}
SubShader 
{
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
	
	CGPROGRAM
	#pragma surface surf Lambert

	sampler2D _MainTex;
	sampler2D _StipplingNoise;
	fixed 	  _Stippling;

 	struct Input {
		float2 uv_MainTex;
  	};
  	
	void surf (Input IN, inout SurfaceOutput o)
	{
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
		
		if (_Stippling < tex2D(_StipplingNoise, IN.uv_MainTex).a)
			discard;
			
		o.Albedo = c.rgb;
		o.Alpha = c.a;
	}
	
	ENDCG
}
Fallback "VertexLit"
}