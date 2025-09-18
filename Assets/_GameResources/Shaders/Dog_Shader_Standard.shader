Shader "Custom/EnhancedToonSaturated"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (1,1,1,1)
        
        [Header(Ramp Settings)]
        _Ramp ("Ramp Texture", 2D) = "white" {}
        _RampSmoothness ("Ramp Smoothness", Range(0, 1)) = 0.5
        _RampSteps ("Ramp Steps", Range(1, 10)) = 3
        
        [Header(Color Controls)]
        _HighlightColor ("Highlight Color", Color) = (1,1,1,1)
        _HighlightStrength ("Highlight Strength", Range(0, 3)) = 1.0
        _ShadowColor ("Shadow Color", Color) = (0.3,0.3,0.3,1)
        _ShadowStrength ("Shadow Strength", Range(0, 3)) = 1.0
        _Saturation ("Saturation", Range(0, 3)) = 1.5
        
        [Header(Outline Settings)]
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0, 0.1)) = 0.03
        
        [Header(Edge Settings)]
        _EdgeDarkness ("Edge Darkness", Range(0, 1)) = 0.5
        _EdgeFalloff ("Edge Falloff", Range(0.1, 10)) = 2.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        LOD 100

        // Outline pass
        Pass
        {
            Name "OUTLINE"
            Cull Front
            ZWrite On
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
            };
            
            float _OutlineThickness;
            float4 _OutlineColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // Convert to world space
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 worldNormal = normalize(mul(v.normal, (float3x3)unity_WorldToObject));
                
                // Offset position along normal
                float3 offset = worldNormal * _OutlineThickness;
                worldPos += offset;
                
                // Convert back to clip space
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }

        // Main pass
        Pass
        {
            Name "MAIN"
            Cull Back
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                SHADOW_COORDS(3)
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _HighlightColor;
            fixed4 _ShadowColor;
            sampler2D _Ramp;
            float _RampSteps;
            float _RampSmoothness;
            float _Saturation;
            float _EdgeDarkness;
            float _EdgeFalloff;
            float _HighlightStrength;
            float _ShadowStrength;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                TRANSFER_SHADOW(o);
                return o;
            }
            
            // Smooth step function for transitions
            float smoothStep(float edge0, float edge1, float x)
            {
                float t = saturate((x - edge0) / (edge1 - edge0));
                return t * t * (3.0 - 2.0 * t);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Basic lighting
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float NdotL = dot(i.worldNormal, lightDir);
                
                // Sample shadow
                fixed shadow = SHADOW_ATTENUATION(i);
                
                // Create smooth stepped lighting
                float toon;
                if (_RampSmoothness > 0)
                {
                    // Smooth transitions between steps
                    float stepSize = 1.0 / _RampSteps;
                    float currentStep = floor(NdotL * _RampSteps) / _RampSteps;
                    float nextStep = currentStep + stepSize;
                    
                    // Smooth interpolation between steps
                    toon = lerp(currentStep, nextStep, 
                               smoothStep(currentStep, nextStep, NdotL) * _RampSmoothness);
                }
                else
                {
                    // Hard steps (original behavior)
                    toon = floor(NdotL * _RampSteps) / _RampSteps;
                }
                
                toon = max(0, toon) * shadow;
                
                // Apply ramp texture if needed
                fixed4 rampCol = tex2D(_Ramp, float2(toon, 0.5));
                
                // Sample texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // Calculate edge darkening based on view direction
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float VdotN = dot(viewDir, i.worldNormal);
                float edge = 1 - saturate(VdotN);
                edge = pow(edge, _EdgeFalloff);
                
                // Apply edge darkening
                col.rgb *= lerp(1.0, 1.0 - _EdgeDarkness, edge);
                
                // Apply toon lighting with custom highlight and shadow colors
                float3 highlight = lerp(col.rgb, _HighlightColor.rgb * col.rgb, _HighlightStrength);
                float3 shadowTint = lerp(col.rgb, _ShadowColor.rgb * col.rgb, _ShadowStrength);
                
                // Blend between shadow and highlight based on lighting
                float3 shadedColor = lerp(shadowTint, highlight, toon);
                
                // Apply saturation
                float3 saturatedColor = lerp(shadedColor, shadedColor * (1.0 + _Saturation), toon);
                
                // Final color composition
                col.rgb = saturatedColor * rampCol.rgb * _LightColor0.rgb;
                
                // Add ambient light
                col.rgb += col.rgb * UNITY_LIGHTMODEL_AMBIENT.rgb;
                
                return col;
            }
            ENDCG
        }
        
        // Shadow pass
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
    
    FallBack "Diffuse"
}