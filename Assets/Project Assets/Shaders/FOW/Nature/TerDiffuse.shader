Shader "Mobile/Fog of War/Nature/Terrain/Diffuse 2" {
Properties {
	[HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
	[HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
	[HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
	[HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
	[HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
	// used in fallback on old cards & base map
	[HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
	[HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
}
	
SubShader {
	Tags {
		"SplatCount" = "4"
		"Queue" = "Geometry-100"
		"RenderType" = "Opaque"
	}
CGPROGRAM
#pragma surface surf Lambert vertex:vert

struct Input {
	float2 uv_Control : TEXCOORD0;
	float2 uv_Splat0 : TEXCOORD1;
	float2 uv_Splat1 : TEXCOORD2;
	float2 uv_Splat2 : TEXCOORD3;
	float2 uv_Splat3 : TEXCOORD4;
    float2 fog : TEXCOORD5;
};

float4 _Color;
sampler2D _MainTex;

//begin regin FOG_OF_WAR
sampler2D _FogTex0;
sampler2D _FogTex1;
uniform float4 _Params;
uniform half4 _Unexplored;
uniform half4 _Explored;
//end regin

float4 _MainTex_ST;

sampler2D _Control;
sampler2D _Splat0,_Splat1,_Splat2,_Splat3;

void vert (inout appdata_full v, out Input IN)
{
	UNITY_INITIALIZE_OUTPUT(Input,IN);
    float4 worldPos = mul (_Object2World, v.vertex);
    IN.fog.xy = worldPos.xz * _Params.z + _Params.xy;
}

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 splat_control = tex2D (_Control, IN.uv_Control);
	fixed3 col;
	col  = splat_control.r * tex2D (_Splat0, IN.uv_Splat0).rgb;
	col += splat_control.g * tex2D (_Splat1, IN.uv_Splat1).rgb;
	col += splat_control.b * tex2D (_Splat2, IN.uv_Splat2).rgb;
	col += splat_control.a * tex2D (_Splat3, IN.uv_Splat3).rgb;
	
	half4 fog = lerp(tex2D(_FogTex0, IN.fog), tex2D(_FogTex1, IN.fog), _Params.w);
	half4 cFog = lerp(lerp(_Color * _Unexplored, _Color * _Explored, fog.g), _Color, fog.r);

	o.Albedo = col * _Color.rgb;
	o.Alpha = 0.0;
}

//half4 frag_surf (Input IN) : COLOR
//{
//	half4 fog = lerp(tex2D(_FogTex0, IN.fog), tex2D(_FogTex1, IN.fog), _Params.w);
//	half4 cFog = lerp(lerp(_Color * _Unexplored, _Color * _Explored, fog.g), _Color, fog.r);
//	
//	return cFog;
//}
ENDCG  
}

Dependency "AddPassShader" = "Hidden/TerrainEngine/Splatmap/Lightmap-AddPass"
Dependency "BaseMapShader" = "Diffuse"

// Fallback to Diffuse
Fallback "Diffuse"
}
