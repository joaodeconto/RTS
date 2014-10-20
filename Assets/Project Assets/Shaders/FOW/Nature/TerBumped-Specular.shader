Shader "Mobile/Fog of War/Nature/Terrain/Diffuse" {
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
	Pass
	{
		CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f_surf members lightDirection)
#pragma exclude_renderers d3d11 xbox360
		#pragma vertex vert_surf
		#pragma fragment frag_surf
	
		#include "UnityCG.cginc"
	
		struct v2f_surf {
		    float4 pos : SV_POSITION;
			float2 uv_Control : TEXCOORD0;
			float2 uv_Splat0 : TEXCOORD1;
			float2 uv_Splat1 : TEXCOORD2;
			float2 uv_Splat2 : TEXCOORD3;
			float2 uv_Splat3 : TEXCOORD4;
		    float2 fog : TEXCOORD5;
            float3 lightDirection;
		};
		
        uniform float4 _LightColor0;
        
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
		
		v2f_surf vert_surf (appdata_full v) {
			v2f_surf o;
            TANGENT_SPACE_ROTATION;
		  
            o.lightDirection = mul(rotation, ObjSpaceLightDir(v.vertex));
		    o.pos = mul (unit_MATRIX_MVP, v.vertex);
		    o.uv_Control = TRANSFORM_TEX (v.texcoord, _MainTex);
		   
		    float4 worldPos = mul (_Object2World, v.vertex);
		    o.fog.xy = worldPos.xz * _Params.z + _Params.xy;
		    
			return o;
		}
		
		half4 frag_surf (v2f_surf IN) : COLOR
		{
			half4 c = tex2D (_MainTex, IN.uv_Control);
			
          	float3 lightColor = float3(0);
 
            float lengthSq = dot(IN.lightDirection, IN.lightDirection);
            float atten = 1.0 / (1.0 + lengthSq * unity_LightAtten[0].z);
            //Angle to the light
            float diff = saturate (normalize(IN.lightDirection));
            lightColor += _LightColor0.rgb * (diff * atten);
            c.rgb = lightColor * c.rgb * 2;
            
			fixed4 splat_control = tex2D (_Control, IN.uv_Control);
			fixed3 col;
			col = splat_control.r * tex2D (_Splat0, IN.uv_Splat0).rgb;
			col += splat_control.g * tex2D (_Splat1, IN.uv_Splat1).rgb;
			col += splat_control.b * tex2D (_Splat2, IN.uv_Splat2).rgb;
			col += splat_control.a * tex2D (_Splat3, IN.uv_Splat3).rgb;
			c.rgb = col;
			c.a = 0.0;
            
			half4 fog = lerp(tex2D(_FogTex0, IN.fog), tex2D(_FogTex1, IN.fog), _Params.w);
			c= lerp(lerp(c * _Unexplored, (0,0,0,0), fog.g), c * 2, fog.r);
			
			return c;
		}
		ENDCG
	}
}

Dependency "AddPassShader" = "Hidden/TerrainEngine/Splatmap/Lightmap-AddPass"
Dependency "BaseMapShader" = "Diffuse"

Fallback "Mobile/Fog of War/Diffuse"
}