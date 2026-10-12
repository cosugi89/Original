Shader "UI/Tutorial Alpha Backdrop"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _MaskTex ("Alpha Mask", 2D) = "black" {}
        _MaskRect ("Mask Rectangle", Vector) = (0,0,1,1)
        _MaskUV ("Mask UV Rectangle", Vector) = (0,0,1,1)
        _MaskOpacity ("Mask Opacity", Range(0,1)) = 1
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct appdata { float4 vertex : POSITION; fixed4 color : COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 local : TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };
            sampler2D _MaskTex;
            float4 _MaskRect, _MaskUV, _ClipRect;
            float _MaskOpacity;
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.local = v.vertex.xy;
                o.color = v.color;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = (i.local - _MaskRect.xy) / _MaskRect.zw;
                float inside = step(0,uv.x) * step(0,uv.y) * step(uv.x,1) * step(uv.y,1);
                fixed4 color = i.color;
                color.a *= 1 - tex2D(_MaskTex, uv * _MaskUV.zw + _MaskUV.xy).a * inside * _MaskOpacity;
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(i.local, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif
                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}
