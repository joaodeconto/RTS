Shader "Mobile/Fog of War/Bumped Diffuse" {
    Properties {
        _Color ("Color", color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Bump ("Bump", 2D) = "bump" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
 
        Pass {
 
            Tags { "LightMode"="ForwardBase" }
            Cull Back
            Lighting On
 
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members lightDirection)
#pragma exclude_renderers d3d11 xbox360
            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"
            uniform float4 _LightColor0;
            
            float4 _Color;
            sampler2D _MainTex;
            sampler2D _Bump;
            float4 _MainTex_ST;
            float4 _Bump_ST;
            
            //begin regin FOG_OF_WAR
			sampler2D _FogTex0;
			sampler2D _FogTex1;
			uniform float4 _Params;
			uniform half4 _Unexplored;
			uniform half4 _Explored;
			//end regin
 
            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                float4 tangent : TANGENT;
 
            };
 
            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float3 lightDirection;
			    float2 fog : TEXCOORD2;
            };
 
            v2f vert (a2v v)
            {
                v2f o;
                TANGENT_SPACE_ROTATION;
 
                o.lightDirection = mul(rotation, ObjSpaceLightDir(v.vertex));
                o.pos = mul( unit_MATRIX_MVP, v.vertex);
                o.uv = TRANSFORM_TEX (v.texcoord, _MainTex); 
                o.uv2 = TRANSFORM_TEX (v.texcoord, _Bump);
                
			    float4 worldPos = mul (_Object2World, v.vertex);
			    o.fog.xy = worldPos.xz * _Params.z + _Params.xy;
                
                return o;
            }
 
            float4 frag(v2f i) : COLOR 
            {
                float4 c = tex2D (_MainTex, i.uv); 
                float3 n =  UnpackNormal(tex2D (_Bump, i.uv2));
 
                float3 lightColor = unit_LIGHTMODEL_AMBIENT.xyz;
 
                float lengthSq = dot(i.lightDirection, i.lightDirection);
                float atten = 1.0 / (1.0 + lengthSq);
                //Angle to the light
                float diff = saturate (dot (n, normalize(i.lightDirection)));  
                lightColor += _LightColor0.rgb * (diff * atten);
                
			    half4 fog = lerp(tex2D(_FogTex0, i.fog), tex2D(_FogTex1, i.fog), _Params.w);
			    c= lerp(lerp(c * _Unexplored, c * _Explored, fog.g), c, fog.r); // where c is the fixed4 that you will return
                
                c.rgb = lightColor * c.rgb * 2;
                return c * _Color;
            }
 
            ENDCG
        }
 
        Pass {
 
            Cull Back
            Lighting On
            Tags { "LightMode"="ForwardAdd" }
            Blend One One
 
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members lightDirection)
#pragma exclude_renderers d3d11 xbox360
            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"
            uniform float4 _LightColor0;
 
            float4 _Color;
            sampler2D _MainTex;
            sampler2D _Bump;
            float4 _MainTex_ST;
            float4 _Bump_ST;
            
            //begin regin FOG_OF_WAR
			sampler2D _FogTex0;
			sampler2D _FogTex1;
			uniform float4 _Params;
			uniform half4 _Unexplored;
			uniform half4 _Explored;
			//end regin
 
            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                float4 tangent : TANGENT;
 
            };
 
            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float3 lightDirection;
			    float2 fog : TEXCOORD2;
            };
 
            v2f vert (a2v v)
            {
                v2f o;
                TANGENT_SPACE_ROTATION;
 
                o.lightDirection = mul(rotation, ObjSpaceLightDir(v.vertex));
                o.pos = mul( unit_MATRIX_MVP, v.vertex);
                o.uv = TRANSFORM_TEX (v.texcoord, _MainTex); 
                o.uv2 = TRANSFORM_TEX (v.texcoord, _Bump);
                
			    float4 worldPos = mul (_Object2World, v.vertex);
			    o.fog.xy = worldPos.xz * _Params.z + _Params.xy;
			    
                return o;
            }
 
            float4 frag(v2f i) : COLOR 
            {
                float4 c = tex2D (_MainTex, i.uv); 
                float3 n =  UnpackNormal(tex2D (_Bump, i.uv2));
 
                float3 lightColor = float3(0);
 
                float lengthSq = dot(i.lightDirection, i.lightDirection);
                float atten = 1.0 / (1.0 + lengthSq * unity_LightAtten[0].z);
                //Angle to the light
                float diff = saturate (dot (n, normalize(i.lightDirection)));
                lightColor += _LightColor0.rgb * (diff * atten);
                c.rgb = lightColor * c.rgb * 2;
                
			    half4 fog = lerp(tex2D(_FogTex0, i.fog), tex2D(_FogTex1, i.fog), _Params.w);
			    c= lerp(lerp(c * _Unexplored, c * _Explored, fog.g), c, fog.r); // where c is the fixed4 that you will return
                
                return c * _Color;
            }
 
            ENDCG
        }
    }
    FallBack "Mobile/Fog of War/Diffuse"
}