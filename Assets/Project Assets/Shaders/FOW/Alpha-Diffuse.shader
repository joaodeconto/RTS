// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Mobile/Fog of War/Alpha Diffuse"
{
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
	    _MainTex ("Texture", 2D) = "white" { }
	}
	SubShader {
	    Pass {
		Blend SrcAlpha One
		Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
		
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		 
		#include "UnityCG.cginc"
		 
		float4 _Color;
		sampler2D _MainTex;
		 
		//begin regin FOG_OF_WAR
		sampler2D _FogTex0;
		sampler2D _FogTex1;
		uniform float4 _Params;
		uniform half4 _Unexplored;
		uniform half4 _Explored;
		//end regin
		 
		struct v2f {
		    float4  pos : SV_POSITION;
		    float2  uv : TEXCOORD0;
		   
		    float2 fog : TEXCOORD2; // this is the new line to add
		};
		 
		float4 _MainTex_ST;
		 
		v2f vert (appdata_base v)
		{
		    v2f o;
		    o.pos = mul (unit_MATRIX_MVP, v.vertex);
		    o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
		   
		    float4 worldPos = mul (_Object2World, v.vertex);
		    o.fog.xy = worldPos.xz * _Params.z + _Params.xy;
		   
		    return o;
		}
		 
		half4 frag (v2f i) : COLOR
		{
		    half4 c = tex2D (_MainTex, i.uv);
		   
		    half4 fog = lerp(tex2D(_FogTex0, i.fog), tex2D(_FogTex1, i.fog), _Params.w);
		    c= lerp(lerp(c * _Unexplored, c * _Explored, fog.g), c, fog.r); // where c is the fixed4 that you will return
		   
		    return c * _Color;
		}
		
		ENDCG
	    }
	}
	Fallback "Transparent/VertexLit"
}