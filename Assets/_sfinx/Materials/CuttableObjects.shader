Shader "SfinxGames/CuttableObjects"
{
    Properties
    {
        _SliceColor ("Slice Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay"}
        LOD 100

        // write 2 to the buffer wherever front faces are visible
        Pass
        {
            ZWrite On
            Cull Back
            ColorMask 0
            
            Stencil
            {
                Ref 2
                Comp Always
                Pass Replace
            }
		}

        // check the buffer for values that are 2 and only render those which are greater or equal
        Pass
        {
            Cull Front

            Stencil
            {
                Ref 2
                Comp GEqual
                Pass Keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _SliceColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _SliceColor;
            }
            ENDCG
        }
    }
}
