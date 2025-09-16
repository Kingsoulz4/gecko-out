// Made with Amplify Shader Editor v1.9.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Shader_Hai/FULL"
{
	Properties
	{
		_scalenormal("scale normal", Float) = 1
		[Normal]_Normal("Normal", 2D) = "bump" {}
		_MainTex("_MainTex", 2D) = "white" {}
		_Hue("Hue", Float) = 1
		_Saturation("Saturation", Float) = 1
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Opacity("Opacity", Range( 0 , 1)) = 1
		_BaseTint("Base Tint", Color) = (1,1,1,1)
		[Toggle(_GRADIENT_ON)] _Gradient("Gradient", Float) = 0
		_Max("Max", Float) = 1
		_Min("Min ", Float) = 0
		_Color2("Color 2", Color) = (0,0,0,0)
		[KeywordEnum(X,Y,Z)] _Position("Position", Float) = 0
		_constrat("constrat", Float) = 1
		_Shadow("Shadow ", Range( 0 , 1)) = 0.5
		_Intensity("Intensity", Float) = 1
		_Colorrim("Color rim", Color) = (1,1,1,0)
		_rim("rim", Float) = 0
		_Float1("Float 1", Float) = 0.03
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _GRADIENT_ON
		#pragma shader_feature_local _POSITION_X _POSITION_Y _POSITION_Z
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif

		struct appdata_full_custom
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			float4 texcoord3 : TEXCOORD3;
			float4 color : COLOR;
			UNITY_VERTEX_INPUT_INSTANCE_ID
			float4 ase_texcoord7 : TEXCOORD7;
		};
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float2 ase_texcoord8;
			float2 uv_texcoord;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform float4 _Colorrim;
		uniform float _rim;
		uniform float _Opacity;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform float _scalenormal;
		uniform float _constrat;
		uniform float _Shadow;
		uniform float _Intensity;
		uniform float _Hue;
		uniform float _Saturation;
		uniform float4 _BaseTint;
		uniform float4 _Color2;
		uniform float _Min;
		uniform float _Max;
		uniform float _Float1;
		uniform float _Cutoff = 0.5;


		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}


		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		void vertexDataFunc( inout appdata_full_custom v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			o.ase_texcoord8 = v.ase_texcoord7;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float2 uv7_MainTex = i.ase_texcoord8.xy * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2DNode123 = tex2D( _MainTex, uv7_MainTex );
			float alpha_texture330 = tex2DNode123.a;
			float temp_output_331_0 = alpha_texture330;
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float temp_output_114_0 = ( 1.0 - ( ( 1.0 - ase_lightAtten ) * _WorldSpaceLightPos0.w ) );
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			float3 newWorldNormal105 = normalize( (WorldNormalVector( i , UnpackScaleNormal( tex2D( _Normal, uv_Normal ), _scalenormal ) )) );
			float3 normalizeResult108 = normalize( newWorldNormal105 );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult111 = dot( normalizeResult108 , ase_worldlightDir );
			float lerpResult120 = lerp( temp_output_114_0 , ( saturate( dotResult111 ) * ( ase_lightAtten * ( _constrat + 1.0 ) ) ) , _Shadow);
			float3 ligting358 = ( ( ase_lightColor.rgb * lerpResult120 ) + ( ( ase_lightColor.a * _Intensity ) * temp_output_114_0 ) );
			float3 hsvTorgb248 = RGBToHSV( tex2DNode123.rgb );
			float3 hsvTorgb247 = HSVToRGB( float3(( _Hue * hsvTorgb248.x ),( hsvTorgb248.y * _Saturation ),hsvTorgb248.z) );
			float4 base_color324 = ( float4( hsvTorgb247 , 0.0 ) * _BaseTint );
			float4 temp_output_128_0 = ( float4( ligting358 , 0.0 ) * base_color324 );
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			#if defined(_POSITION_X)
				float staticSwitch227 = ase_vertex3Pos.x;
			#elif defined(_POSITION_Y)
				float staticSwitch227 = ase_vertex3Pos.y;
			#elif defined(_POSITION_Z)
				float staticSwitch227 = ase_vertex3Pos.z;
			#else
				float staticSwitch227 = ase_vertex3Pos.x;
			#endif
			float smoothstepResult229 = smoothstep( _Min , _Max , ( staticSwitch227 + _Float1 ));
			float4 lerpResult232 = lerp( temp_output_128_0 , _Color2 , smoothstepResult229);
			#ifdef _GRADIENT_ON
				float4 staticSwitch242 = lerpResult232;
			#else
				float4 staticSwitch242 = temp_output_128_0;
			#endif
			c.rgb = staticSwitch242.rgb;
			c.a = ( _Opacity * alpha_texture330 );
			clip( temp_output_331_0 - _Cutoff );
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float fresnelNdotV223 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode223 = ( 0.0 + _rim * pow( 1.0 - fresnelNdotV223, 5.0 ) );
			float4 lerpResult138 = lerp( float4( 0,0,0,0 ) , _Colorrim , fresnelNode223);
			float4 Rim362 = lerpResult138;
			o.Emission = Rim362.rgb;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full_custom v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.ase_texcoord8;
				o.customPack1.zw = customInputData.uv_texcoord;
				o.customPack1.zw = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.ase_texcoord8 = IN.customPack1.xy;
				surfIN.uv_texcoord = IN.customPack1.zw;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19100
Node;AmplifyShaderEditor.CommentaryNode;357;-2331.031,-1445.76;Inherit;False;2782.661;1063.916;lighting;67;134;131;135;204;106;283;292;136;105;284;293;294;107;108;281;111;282;110;295;109;113;112;130;118;114;115;302;296;288;287;303;297;301;299;286;256;300;285;298;116;120;121;306;290;308;309;307;291;311;304;132;305;310;289;124;122;318;315;319;314;313;317;312;316;149;163;156;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;131;-816.524,-1395.76;Inherit;False;Property;_constrat;constrat;14;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;134;-825.2122,-1306.479;Inherit;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;204;-2281.031,-1062.598;Inherit;False;Property;_scalenormal;scale normal;0;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;135;-662.5533,-1372.125;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightAttenuation;106;-1395.519,-708.9835;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;292;-470.1785,-1343.009;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;283;-1153.221,-703.6615;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;136;-2129.574,-1112.177;Inherit;True;Property;_Normal;Normal;1;1;[Normal];Create;True;0;0;0;True;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;2;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;293;-470.1948,-1342.5;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;284;-1152.729,-705.8947;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;105;-1855.828,-1070.044;Inherit;False;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;108;-1652.999,-1039.485;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;294;-472.1723,-932.0883;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;107;-1871.158,-879.7697;Inherit;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WireNode;281;-1148.528,-952.3728;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;109;-1083.76,-694.4144;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;111;-1501.222,-1041.208;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;282;-1150.534,-952.3629;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightPos;110;-1445.752,-611.6989;Inherit;True;0;3;FLOAT4;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.WireNode;295;-471.1723,-932.0883;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;113;-936.681,-678.5139;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;112;-1276.015,-1044.543;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;130;-397.9874,-984.271;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;115;-461.6989,-890.0869;Float;False;Property;_Shadow;Shadow ;15;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;114;-807.9515,-621.095;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;118;-263.8678,-1045.46;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;288;-625.1516,-592.9841;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;296;-152.9189,-1012.866;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;302;-138.9189,-855.8661;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;287;-623.1846,-592.9841;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;356;-2328.262,-354.2076;Inherit;False;1682.346;476.6641;base color;35;255;336;337;335;334;253;338;339;250;123;330;248;254;342;343;345;344;340;341;333;332;346;347;247;348;349;350;351;222;221;352;353;354;355;324;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WireNode;303;-138.9189,-855.8661;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;297;-152.9189,-1013.866;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;299;-136.9188,-1037.866;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;286;-626.6942,-1084.014;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;301;-151.9189,-1063.866;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;298;-150.9189,-1064.866;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;256;-461.5287,-803.6268;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;116;-463.1899,-639.6484;Inherit;False;Property;_Intensity;Intensity;16;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;300;-134.9188,-1036.866;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RGBToHSVNode;248;-1962.855,-198.6636;Inherit;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WireNode;285;-626.6942,-1085.981;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;120;-93.65019,-1114.261;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;335;-1663.468,-145.2595;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;121;-285.4399,-710.4775;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;290;-673.6672,-572.5554;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;334;-1663.468,-147.2595;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;308;61.68913,-1086.787;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;306;-22.4175,-677.3104;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;309;63.68913,-1084.787;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;307;-22.4175,-677.3104;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;336;-1662.468,-238.2592;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;291;-672.6672,-574.5554;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;337;-1663.468,-240.2592;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;311;65.68912,-743.7866;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;250;-1779.905,-304.2076;Inherit;False;Property;_Hue;Hue;3;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;253;-1758.82,-2.602402;Inherit;False;Property;_Saturation;Saturation;4;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;304;-24.63503,-544.0117;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;132;-676.8112,-509.596;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;310;65.68912,-741.7866;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;305;-23.4175,-542.3104;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;338;-1621.468,31.74059;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;255;-1577.158,-295.2293;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;345;-1760.341,-121.9194;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;289;-673.4849,-509.3556;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;221;-1335.662,-89.54384;Float;False;Property;_BaseTint;Base Tint;7;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.5566038,0.4174528,0.4174528,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;122;106.3685,-571.4976;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;344;-1759.341,-121.9194;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;124;132.2397,-797.3573;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;339;-1622.468,29.74059;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;349;-1385.34,-262.9193;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;333;-1418.536,-92.55977;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;342;-1761.341,-171.9194;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;352;-1111.791,-55.7522;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;341;-1622.468,-148.2595;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;348;-1384.34,-260.9193;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;318;254.9269,-539.511;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;332;-1416.536,-91.55977;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;315;261.9269,-767.511;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;346;-1416.34,-148.9194;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;343;-1761.341,-174.9194;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;314;261.9269,-767.511;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;278;-835.1859,187.472;Inherit;False;982.4505;640.1243;forg;16;225;227;257;226;228;269;270;272;271;229;274;273;275;276;258;231;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WireNode;351;-1386.34,-207.9194;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;319;257.9269,-539.511;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;340;-1621.468,-148.2595;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;353;-1111.791,-55.7522;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;317;259.9269,-626.511;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;313;259.9269,-651.511;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;350;-1387.34,-206.9194;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;254;-1575.669,-200.6248;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;347;-1417.34,-149.9194;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;355;-1105.791,-166.7522;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;228;-734.5787,621.1783;Inherit;False;Property;_Min;Min ;11;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;226;-732.7278,711.4615;Inherit;False;Property;_Max;Max;10;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;316;260.9269,-626.511;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;312;259.9269,-652.511;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.HSVToRGBNode;247;-1335.745,-227.6143;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WireNode;354;-1104.791,-167.7522;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;272;-253.4131,656.5959;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;270;-213.413,747.5959;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;360;1177.916,-1270.04;Inherit;False;692.2173;506.5939;rim;4;223;139;138;193;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;222;-996.6176,-222.7379;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;269;-211.413,748.5959;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;225;-785.1859,426.6094;Inherit;True;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;149;299.6298,-682.6786;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0.5;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;271;-253.4131,656.5959;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;324;-869.9177,-226.6338;Inherit;False;base_color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;227;-571.9743,442.4393;Inherit;False;Property;_Position;Position;13;0;Create;True;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;3;X;Y;Z;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;358;472.4179,-693.7976;Inherit;False;ligting;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;193;1227.916,-948.2374;Inherit;False;Property;_rim;rim;18;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;257;-566.8942,565.7486;Inherit;False;Property;_Float1;Float 1;22;0;Create;True;0;0;0;False;0;False;0.03;0.03;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;274;-212.413,524.5959;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;276;-250.4131,504.596;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;328;1138.52,106.8188;Inherit;False;324;base_color;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;273;-212.413,526.5959;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;139;1406.488,-1220.04;Inherit;False;Property;_Colorrim;Color rim;17;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;275;-251.4131,502.596;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;223;1387.493,-1016.445;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0.22;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;258;-362.3353,448.8163;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;359;1146.463,39.08636;Inherit;False;358;ligting;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SmoothstepOpNode;229;-131.6313,447.7334;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;-1.24;False;2;FLOAT;3.42;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;330;-1964.819,-47.14124;Inherit;False;alpha_texture;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;231;-745.2848,234.4138;Inherit;False;Property;_Color2;Color 2;12;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;138;1688.133,-1123.238;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;128;1398.472,29.16924;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;331;2845.938,-279.8907;Inherit;False;330;alpha_texture;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;232;1680.334,289.3695;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;362;1892.816,-1125.981;Inherit;False;Rim;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;244;2785.333,-358.8756;Inherit;False;Property;_Opacity;Opacity;6;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;327;-609.9789,-307.7893;Inherit;False;1400.3;425.788;spacular;9;236;233;234;235;237;280;279;325;326;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;163;-1662.273,-1161.445;Float;False;world;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CustomStandardSurface;236;-279.8981,-179.0848;Inherit;False;Metallic;World;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;233;-557.523,-173.3997;Inherit;False;Property;_Metallic;Metallic;20;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;279;538.3214,-231.6829;Inherit;False;Property;_on_Spacular;on_Spacular;9;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;326;379.9886,-235.9698;Inherit;False;324;base_color;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;325;-51.7478,-257.7894;Inherit;False;324;base_color;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;234;-559.9788,-85.6356;Inherit;False;Property;_Smootheness;Smootheness;19;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;235;-557.3389,1.998672;Inherit;False;Property;_Occlusion;Occlusion;21;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;361;3230.508,-497.3954;Inherit;False;362;Rim;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCGrayscale;237;-56.90576,-183.8839;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;280;273.135,-201.6895;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;156;-1625.654,-772.8735;Inherit;False;dir;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;242;2840.954,-193.3353;Inherit;False;Property;_Gradient;Gradient;8;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;245;3062.242,-345.0861;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;58;3447.303,-526.4725;Float;False;True;-1;2;ASEMaterialInspector;0;0;CustomLighting;Shader_Hai/FULL;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;1;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;True;Transparent;;AlphaTest;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;2;10;25;False;1;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;1;False;0.0007;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;5;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SamplerNode;123;-2278.262,-151.8509;Inherit;True;Property;_MainTex;_MainTex;2;0;Create;True;0;0;0;False;0;False;-1;None;None;True;7;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;135;0;131;0
WireConnection;135;1;134;0
WireConnection;292;0;135;0
WireConnection;283;0;106;0
WireConnection;136;5;204;0
WireConnection;293;0;292;0
WireConnection;284;0;283;0
WireConnection;105;0;136;0
WireConnection;108;0;105;0
WireConnection;294;0;293;0
WireConnection;281;0;284;0
WireConnection;109;0;106;0
WireConnection;111;0;108;0
WireConnection;111;1;107;0
WireConnection;282;0;281;0
WireConnection;295;0;294;0
WireConnection;113;0;109;0
WireConnection;113;1;110;2
WireConnection;112;0;111;0
WireConnection;130;0;282;0
WireConnection;130;1;295;0
WireConnection;114;0;113;0
WireConnection;118;0;112;0
WireConnection;118;1;130;0
WireConnection;288;0;114;0
WireConnection;296;0;118;0
WireConnection;302;0;115;0
WireConnection;287;0;288;0
WireConnection;303;0;302;0
WireConnection;297;0;296;0
WireConnection;299;0;303;0
WireConnection;286;0;287;0
WireConnection;301;0;297;0
WireConnection;298;0;301;0
WireConnection;300;0;299;0
WireConnection;248;0;123;0
WireConnection;285;0;286;0
WireConnection;120;0;285;0
WireConnection;120;1;298;0
WireConnection;120;2;300;0
WireConnection;335;0;248;1
WireConnection;121;0;256;2
WireConnection;121;1;116;0
WireConnection;290;0;114;0
WireConnection;334;0;335;0
WireConnection;308;0;120;0
WireConnection;306;0;121;0
WireConnection;309;0;308;0
WireConnection;307;0;306;0
WireConnection;336;0;334;0
WireConnection;291;0;290;0
WireConnection;337;0;336;0
WireConnection;311;0;309;0
WireConnection;304;0;307;0
WireConnection;132;0;291;0
WireConnection;310;0;311;0
WireConnection;305;0;304;0
WireConnection;338;0;253;0
WireConnection;255;0;250;0
WireConnection;255;1;337;0
WireConnection;345;0;248;2
WireConnection;289;0;132;0
WireConnection;122;0;305;0
WireConnection;122;1;289;0
WireConnection;344;0;345;0
WireConnection;124;0;256;1
WireConnection;124;1;310;0
WireConnection;339;0;338;0
WireConnection;349;0;255;0
WireConnection;333;0;248;3
WireConnection;342;0;344;0
WireConnection;352;0;221;0
WireConnection;341;0;339;0
WireConnection;348;0;349;0
WireConnection;318;0;122;0
WireConnection;332;0;333;0
WireConnection;315;0;124;0
WireConnection;346;0;332;0
WireConnection;343;0;342;0
WireConnection;314;0;315;0
WireConnection;351;0;348;0
WireConnection;319;0;318;0
WireConnection;340;0;341;0
WireConnection;353;0;352;0
WireConnection;317;0;319;0
WireConnection;313;0;314;0
WireConnection;350;0;351;0
WireConnection;254;0;343;0
WireConnection;254;1;340;0
WireConnection;347;0;346;0
WireConnection;355;0;353;0
WireConnection;316;0;317;0
WireConnection;312;0;313;0
WireConnection;247;0;350;0
WireConnection;247;1;254;0
WireConnection;247;2;347;0
WireConnection;354;0;355;0
WireConnection;272;0;228;0
WireConnection;270;0;226;0
WireConnection;222;0;247;0
WireConnection;222;1;354;0
WireConnection;269;0;270;0
WireConnection;149;0;312;0
WireConnection;149;1;316;0
WireConnection;271;0;272;0
WireConnection;324;0;222;0
WireConnection;227;1;225;1
WireConnection;227;0;225;2
WireConnection;227;2;225;3
WireConnection;358;0;149;0
WireConnection;274;0;269;0
WireConnection;276;0;271;0
WireConnection;273;0;274;0
WireConnection;275;0;276;0
WireConnection;223;2;193;0
WireConnection;258;0;227;0
WireConnection;258;1;257;0
WireConnection;229;0;258;0
WireConnection;229;1;275;0
WireConnection;229;2;273;0
WireConnection;330;0;123;4
WireConnection;138;1;139;0
WireConnection;138;2;223;0
WireConnection;128;0;359;0
WireConnection;128;1;328;0
WireConnection;232;0;128;0
WireConnection;232;1;231;0
WireConnection;232;2;229;0
WireConnection;362;0;138;0
WireConnection;163;0;105;0
WireConnection;236;3;233;0
WireConnection;236;4;234;0
WireConnection;236;5;235;0
WireConnection;279;1;326;0
WireConnection;279;0;280;0
WireConnection;237;0;236;0
WireConnection;280;0;325;0
WireConnection;280;1;237;0
WireConnection;156;0;107;0
WireConnection;242;1;128;0
WireConnection;242;0;232;0
WireConnection;245;0;244;0
WireConnection;245;1;331;0
WireConnection;58;2;361;0
WireConnection;58;9;245;0
WireConnection;58;10;331;0
WireConnection;58;13;242;0
ASEEND*/
//CHKSM=23D32DF0F7D4A8A8E3375B818B2ACF7B84A8E76C