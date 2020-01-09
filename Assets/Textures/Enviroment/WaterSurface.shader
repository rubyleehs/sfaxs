//Adapted from Catlike coding tutorial regarding texture distortion.
//https://catlikecoding.com/unity/tutorials/flow/texture-distortion/
//Comments are my own. Adapted to look more like water + transparency.

Shader "Custom/WaterSurface" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset] _DerivHeightMap ("Deriv (AG) Height (B)", 2D) = "black" {}
        [NoScaleOffset] _NormalMap ("Normals", 2D) = "bump" {}
        _UJump ("U jump per phase", Range(-0.25, 0.25)) = 0.25
		_VJump ("V jump per phase", Range(-0.25, 0.25)) = 0.25
        _Tiling ("Tiling", Float) = 1
        _Speed ("Speed", Float) = 1
        _FlowStrength ("Flow Strength", Float) = 1
        _FlowOffset ("Flow Offset", Float) = 0
        _HeightScale ("Height Scale, Constant", Float) =  0.25
        _HeightScaleModulated ("Height Scale, Modulated", Float) = 0.75
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }

		LOD 200

		CGPROGRAM
        #pragma surface surf Standard alpha
		#pragma target 3.0

        #include "FlowUV.cginc"
		sampler2D _MainTex, _FlowMap, _DerivHeightMap;
        float _UJump, _VJump, _Tiling, _Speed, _FlowStrength, _FlowOffset, _HeightScale, _HeightScaleModulated;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

        float3 UnpackDerivativeHeight (float4 textureData) {
			float3 dh = textureData.agb;
			dh.xy = dh.xy * 2 - 1;
			return dh;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
            
            float3 flow = tex2D(_FlowMap, IN.uv_MainTex).rgb; //shift everything to positive values.
			flow.xy = flow.xy * 2 - 1;
			flow *= _FlowStrength;
            float noise = tex2D(_FlowMap, IN.uv_MainTex).a; //noise for flow
            float time = _Time.y *_Speed + noise; 
            float2 jump = float2(_UJump, _VJump);
            
            //we sample twice so we have 2 "flow patterns". when one pattern is about to end, fade to other
            //this prevents sudden transitions
            float3 uvwA = FlowUVW(IN.uv_MainTex, flow.xy, jump, _FlowOffset, _Tiling, time, false);
			float3 uvwB = FlowUVW(IN.uv_MainTex, flow.xy, jump, _FlowOffset, _Tiling, time, true);

            float finalHeightScale = flow.z * _HeightScaleModulated + _HeightScale;
            float3 dhA = UnpackDerivativeHeight(tex2D(_DerivHeightMap, uvwA.xy)) * uvwA.z * finalHeightScale;
			float3 dhB = UnpackDerivativeHeight(tex2D(_DerivHeightMap, uvwB.xy)) * uvwB.z * finalHeightScale;
			o.Normal = normalize(float3(-(dhA.xy + dhB.xy), 1));

			fixed4 texA = tex2D(_MainTex, uvwA.xy) * uvwA.z;
			fixed4 texB = tex2D(_MainTex, uvwB.xy) * uvwB.z;

			fixed4 c = (texA + texB) * _Color;
            o.Albedo = c.rgb;
			//o.Albedo = pow(dhA.z + dhB.z, 2); //convert gamma to approx linear color space
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
}