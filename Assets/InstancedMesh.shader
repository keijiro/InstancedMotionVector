Shader "InstancedMesh"
{
    Properties
    {
        _MainTex("", 2D) = "white" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    float _CurrentTime;
    float _DeltaTime;

    float4 Animate(float4 vp, uint id, float t)
    {
        t = 0.3 * t + 0.1 * (float)id;
        vp.x += sin(t * 8.71) * 6;
        vp.y += sin(t * 1.95) * 4;
        vp.z += sin(t * 1.33) * 3;
        return vp;
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            Tags { "LightMode" = "MotionVectors" }

            ZWrite Off

            CGPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment

            float4x4 _NonJitteredVP;
            float4x4 _PreviousVP;
            float4x4 _PreviousM;

            struct Attributes
            {
                float4 position : POSITION;
                uint instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float4 transfer0 : TEXCOORD0;
                float4 transfer1 : TEXCOORD1;
            };

            Varyings Vertex(Attributes input)
            {
                float4 vp0 = Animate(input.position, input.instanceID, _CurrentTime - _DeltaTime);
                float4 vp1 = Animate(input.position, input.instanceID, _CurrentTime);

                Varyings o;
                o.position = UnityObjectToClipPos(vp1);
                //o.transfer0 = mul(_PreviousVP, mul(_PreviousM, vp0));
                o.transfer0 = mul(_PreviousVP, mul(unity_ObjectToWorld, vp0));
                o.transfer1 = mul(_NonJitteredVP, mul(unity_ObjectToWorld, vp1));
                return o;
            }

            float4 Fragment(Varyings input) : SV_Target
            {
                float3 hp0 = input.transfer0.xyz / input.transfer0.w;
                float3 hp1 = input.transfer1.xyz / input.transfer1.w;

                float2 vp0 = (hp0.xy + 1) / 2;
                float2 vp1 = (hp1.xy + 1) / 2;

            #if UNITY_UV_STARTS_AT_TOP
                //vp0.y = 1 - vp0.y;
                //vp1.y = 1 - vp1.y;
            #endif

                return half4(vp1 - vp0, 0, 1);
            }

            ENDCG
        }
        
        CGPROGRAM

        #pragma surface Surf Standard vertex:Vertex fullforwardshadows addshadow
        #pragma instancing_options procedural:Setup
        #pragma target 3.5

        sampler2D _MainTex;

        float4x4 _LocalToWorld;
        float4x4 _WorldToLocal;

        struct Input
        {
            float2 uv_MainTex;
        };

        void Vertex(inout appdata_full data)
        {
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            data.vertex = Animate(data.vertex, unity_InstanceID, _CurrentTime);
        #endif
        }

        void Setup()
        {
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            unity_ObjectToWorld = _LocalToWorld;
            unity_WorldToObject = _WorldToLocal;
        #endif
        }

        void Surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
        }

        ENDCG
    }
    FallBack "Diffuse"
}
