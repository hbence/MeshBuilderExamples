// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/Ground" {
Properties {
    _TopMaxColor ("Top Max Color", Color) = (1,1,1,1)
    _TopMinColor ("Top Min Color", Color) = (1,1,1,1)

    _TopMaxY("Top Max Y", float) = 1
    _TopMinY("Top Min Y", float) = 0

    _SideMaxColor ("Side Max Color", Color) = (1,1,1,1)
    _SideMinColor ("Side Min Color", Color) = (1,1,1,1)

    _SideMaxY("Side Max Y", float) = 1
    _SideMinY("Side Min Y", float) = 0

    _Emission ("Emissive Color", Color) = (0,0,0,0)
    _TopTex ("Top (RGB)", 2D) = "white" {}
    _SideTex ("Side (RGB)", 2D) = "white" {}
}

SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 100
    Pass
    {
        Tags{ "LIGHTMODE" = "ForwardBase" "RenderType" = "Opaque" }

        CGPROGRAM

        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        #include "UnityCG.cginc"
        #pragma multi_compile_fog
        #define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))

        float4 _TopTex_ST;
        float4 _SideTex_ST;

        struct appdata
        {
            float3 pos : POSITION;
            float3 uv0 : TEXCOORD0;
            float3 normal : NORMAL;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f
        {
            float2 uv0 : TEXCOORD0;
            half3 worldNormal : TEXCOORD1;
            float4 worldPos : TEXCOORD2;

       //     fixed4 color : COLOR;
            half3 light : TEXCOORD3;
#if USING_FOG
            fixed fog : TEXCOORD4;
#endif
            float4 pos : SV_POSITION;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        
        fixed4 _SideMaxColor;
        fixed4 _SideMinColor;

        fixed _SideMaxY;
        fixed _SideMinY;

        fixed4 _TopMaxColor;
        fixed4 _TopMinColor;

        fixed _TopMaxY;
        fixed _TopMinY;

        uniform fixed4 _LightColor0;

        v2f vert(appdata IN)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(IN);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

            o.uv0 = IN.uv0.xy;
            o.worldNormal = UnityObjectToWorldNormal(IN.normal);
            o.pos = UnityObjectToClipPos(IN.pos);

            o.worldPos = mul( unity_ObjectToWorld, float4( IN.pos, 1.0 ) );
        //    float ratio = (o.worldPos.y - _MinY) / (_MaxY - _MinY); 
         //   o.worldPos.w = ratio;
         // the color lerp should be clamped, but if it is it can't be done in the vertex shader
         // or the different meshes at different height would clash
         // o.color = lerp(_SideMinColor, _SideMaxColor, clamp(ratio, 0, 1));
         // o.color = lerp(_SideMinColor, _SideMaxColor, ratio);

            //Lighting (directional light)*/
            float3 normalDirection = o.worldNormal;
            float3 viewDirection = normalize( _WorldSpaceCameraPos.xyz - o.worldPos.xyz );
            float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
            float3 diffuseReflection = _LightColor0.xyz * saturate(dot(normalDirection, lightDirection));
            o.light = UNITY_LIGHTMODEL_AMBIENT.xyz + diffuseReflection;

#if USING_FOG
            float3 eyePos = UnityObjectToViewPos(IN.pos);
            float fogCoord = length(eyePos.xyz);
            UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
            o.fog = saturate(unityFogFactor);
#endif

            return o;
        }

        sampler2D _TopTex;
        sampler2D _SideTex;
        
        fixed4 _Emission;

        fixed4 frag(v2f IN) : SV_Target
        {
            fixed4 col = fixed4(IN.light + _Emission, 1);
  /*          
            if (abs(IN.worldNormal.z) > 0.1)
            {
                fixed4 tex = tex2D(_SideTex, IN.worldPos.xy * _SideTex_ST.xy + _SideTex_ST.zw);
                col.rgb *= tex.rgb * IN.color;
            }
            else
            {
                fixed4 tex = tex2D(_TopTex, IN.worldPos.xz * _TopTex_ST.xy + _TopTex_ST.zw);
                col.rgb *= tex.rgb * _TopColor;
            }
 */           

            float ratio = (IN.worldPos.y - _SideMinY) / (_SideMaxY - _SideMinY);
            fixed4 sideColor = lerp(_SideMinColor, _SideMaxColor, clamp(ratio, 0, 1));
            ratio = (IN.worldPos.y - _TopMinY) / (_TopMaxY - _TopMinY);
            fixed4 topColor = lerp(_TopMinColor, _TopMaxColor, clamp(ratio, 0, 1));
            fixed4 top = tex2D(_TopTex, IN.worldPos.xz * _TopTex_ST.xy + _TopTex_ST.zw) * topColor;
            fixed4 side = tex2D(_SideTex, IN.worldPos.xy * _SideTex_ST.xy + _SideTex_ST.zw) * sideColor;
            col *= lerp(side, top, abs(IN.worldNormal.y));

#if USING_FOG
            col.rgb = lerp(unity_FogColor.rgb, col.rgb, IN.fog);
#endif

            return col;
        }

        ENDCG
    }

    // Pass to render object as a shadow caster
    Pass {
        Name "ShadowCaster"
        Tags { "LightMode" = "ShadowCaster" }

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        #pragma multi_compile_shadowcaster
        #pragma multi_compile_instancing // allow instanced shadow pass for most of the shaders
        #include "UnityCG.cginc"

        struct v2f {
            V2F_SHADOW_CASTER;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        v2f vert( appdata_base v )
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
            return o;
        }

        float4 frag( v2f i ) : SV_Target
        {
            SHADOW_CASTER_FRAGMENT(i)
        }
        ENDCG

    }

}

}
