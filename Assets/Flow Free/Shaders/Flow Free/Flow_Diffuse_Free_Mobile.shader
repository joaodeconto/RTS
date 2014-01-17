Shader "Vacuum/Flow Free/Flow_Diffuse_mobile" {
	Properties {
	    _BaseColor ("Base Color (RGB)", Color) = (1, 1, 1, 1)
		_MainTex ("Base Texture", 2D) = "" {}
		_FlowColor ("Flow Color (A)", Color) = (1, 1, 1, 1)
		_FlowTexture ("Flow Texture(RGB) Specular (A)", 2D) = ""{}
		_FlowMap ("FlowMap (RG) Alpha (B) Gradient (A)", 2D) = ""{}
	}
	SubShader {
		Tags { "RenderType"="Opaque" "FlowTag"="Flow" }
		LOD 200

		CGPROGRAM
		#pragma surface surf MobileBlinnPhong exclude_path:prepass noforwardadd nolightmap halfasview novertexlights

		inline fixed4 LightingMobileBlinnPhong (SurfaceOutput s, fixed3 lightDir, fixed3 halfDir, fixed atten)
		{
			fixed diff = max (0, dot (s.Normal, lightDir));
			fixed nh = max (0, dot (s.Normal, halfDir));
			fixed spec = pow (nh, s.Specular*128) * s.Gloss;
	
			fixed4 c;
			c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * (atten*2);
			c.a = 0.0;
			return c;
		}


		fixed4 _BaseColor;
		sampler2D _MainTex;
		fixed4 _FlowColor;
		sampler2D _FlowTexture;
		sampler2D _FlowMap;
		half _PhaseLength;

		half4 _FlowMapOffset;


	struct Input {
			half2 uv_MainTex;
			half2 uv_FlowTexture;
			half2 uv_FlowMap;
		}; 
 
	void surf (Input IN, inout SurfaceOutput o)
	{
			half4 mainColor = tex2D (_MainTex, IN.uv_MainTex);
			mainColor.rgb *= _BaseColor;
			
			half4 flowMap = tex2D (_FlowMap, IN.uv_FlowMap);
			flowMap.r = flowMap.r * 2.0f - 1.011765;
			flowMap.g = flowMap.g * 2.0f - 1.003922;

			////////////////////////////////////////

			half phase1 = _FlowMapOffset.x;
			half phase2 = _FlowMapOffset.y;
			
			half4 t1 = tex2D (_FlowTexture, IN.uv_FlowTexture + flowMap.rg * phase1); 		 	
			half4 t2 = tex2D (_FlowTexture, IN.uv_FlowTexture + flowMap.rg * phase2); 	
			
			half blend = abs(_PhaseLength - _FlowMapOffset.z) / _PhaseLength;
			blend = max(0, blend);
			half4 final = lerp( t1, t2, blend );

			final.rgb *= _FlowColor.rgb;
			

			o.Albedo = lerp(mainColor.rgb, final.rgb, flowMap.b * _FlowColor.a);

			o.Alpha = 0;
	}

ENDCG
}

Fallback "Mobile/VertexLit"
}