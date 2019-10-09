// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:2595,x:32719,y:32712,varname:node_2595,prsc:2|custl-8428-OUT;n:type:ShaderForge.SFN_Tex2d,id:5217,x:31918,y:32745,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:030be6c3a3f614d9daa1530790d392e7,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:3080,x:31914,y:33058,ptovrint:False,ptlb:Overlay,ptin:_Overlay,varname:_Overlay,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:8428,x:32472,y:33037,varname:node_8428,prsc:2|A-1517-OUT,B-5969-OUT;n:type:ShaderForge.SFN_Lerp,id:5969,x:32293,y:33075,varname:node_5969,prsc:2|A-596-OUT,B-3749-OUT,T-3080-A;n:type:ShaderForge.SFN_Vector1,id:596,x:32239,y:32998,varname:node_596,prsc:2,v1:1;n:type:ShaderForge.SFN_Add,id:3749,x:32114,y:33075,varname:node_3749,prsc:2|A-3080-RGB,B-2589-OUT;n:type:ShaderForge.SFN_Vector1,id:2589,x:32114,y:33220,varname:node_2589,prsc:2,v1:0.2;n:type:ShaderForge.SFN_SwitchProperty,id:5564,x:32204,y:32653,ptovrint:False,ptlb:Invert,ptin:_Invert,varname:_Invert,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True|A-5217-RGB,B-7893-OUT;n:type:ShaderForge.SFN_RemapRange,id:7893,x:32204,y:32828,varname:node_7893,prsc:2,frmn:0,frmx:1,tomn:1,tomx:0|IN-5217-RGB;n:type:ShaderForge.SFN_Multiply,id:1343,x:32472,y:32751,varname:node_1343,prsc:2|A-9714-OUT,B-5564-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9714,x:32472,y:32642,ptovrint:False,ptlb:Brightness,ptin:_Brightness,varname:_Brightness,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1.1;n:type:ShaderForge.SFN_Add,id:1517,x:32472,y:32889,varname:node_1517,prsc:2|A-7458-OUT,B-1343-OUT;n:type:ShaderForge.SFN_Subtract,id:7458,x:32472,y:32466,varname:node_7458,prsc:2|A-9714-OUT,B-7064-OUT;n:type:ShaderForge.SFN_Vector1,id:7064,x:32204,y:32561,varname:node_7064,prsc:2,v1:1;proporder:5217-3080-5564-9714;pass:END;sub:END;*/

Shader "Custom/CanvasAndOverlay" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _Overlay ("Overlay", 2D) = "white" {}
        [MaterialToggle] _Invert ("Invert", Float ) = 1
        _Brightness ("Brightness", Float ) = 1.1
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        LOD 200
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x xboxone ps4 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _Overlay; uniform float4 _Overlay_ST;
            uniform fixed _Invert;
            uniform float _Brightness;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float node_596 = 1.0;
                float4 _Overlay_var = tex2D(_Overlay,TRANSFORM_TEX(i.uv0, _Overlay));
                float3 finalColor = (((_Brightness-1.0)+(_Brightness*lerp( _MainTex_var.rgb, (_MainTex_var.rgb*-1.0+1.0), _Invert )))*lerp(float3(node_596,node_596,node_596),(_Overlay_var.rgb+0.2),_Overlay_var.a));
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
