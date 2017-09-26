#include "Common.cginc"

sampler2D _MainTex;

struct Input
{
    float2 uv_MainTex;
};

void Setup()
{
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
    Transform anim = CalculateAnimation(unity_InstanceID, _CurrentTime);
    unity_ObjectToWorld = mul(_LocalToWorld, anim.instanceToObject);
    unity_WorldToObject = mul(anim.objectToInstance, _WorldToLocal);
#endif
}

void Surf(Input IN, inout SurfaceOutputStandard o)
{
    o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
}
