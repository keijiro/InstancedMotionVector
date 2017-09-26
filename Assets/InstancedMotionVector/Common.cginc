#include "UnityCG.cginc"

float _CurrentTime;
float _DeltaTime;
float4x4 _LocalToWorld;
float4x4 _WorldToLocal;

// Hash function from H. Schechter & R. Bridson, goo.gl/RXiKaH
uint Hash(uint s)
{
    s ^= 2747636419u;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    return s;
}

float Random(uint seed)
{
    return float(Hash(seed)) / 4294967295.0; // 2^32-1
}

float4x4 Translation(float3 v)
{
    return float4x4(
        1, 0, 0, v.x,
        0, 1, 0, v.y,
        0, 0, 1, v.z,
        0, 0, 0, 1
    );
}

float4x4 EulerRotation(float3 v)
{
    float sx, cx;
    float sy, cy;
    float sz, cz;

    sincos(v.x, sx, cx);
    sincos(v.y, sy, cy);
    sincos(v.z, sz, cz);

    float4 row1 = float4(sx*sy*sz + cy*cz, sx*sy*cz - cy*sz, cx*sy, 0);
    float4 row3 = float4(sx*cy*sz - sy*cz, sx*cy*cz + sy*sz, cx*cy, 0);
    float4 row2 = float4(cx*sz, cx*cz, -sx, 0);

    return float4x4(row1, row2, row3, float4(0, 0, 0, 1));
}

struct Transform
{
    float4x4 instanceToObject;
    float4x4 objectToInstance;
};

Transform CalculateAnimation(uint id, float t)
{
    uint seed = id * 8;
    t += 100;

    float3 a1 = float3(Random(seed + 0), Random(seed + 1), Random(seed + 2));
    float3 a2 = float3(Random(seed + 3), Random(seed + 4), Random(seed + 5));

    a1 = (4 * a1 - 2) * t;
    a2 = (4 * a2 - 2) * t;

    float r = lerp(1, 10, sqrt(Random(seed + 6)));

    float3 pos = mul(EulerRotation(a1), float4(r, 0, 0, 1));
    float4x4 rot = EulerRotation(a2);

    Transform transform;
    transform.instanceToObject = mul(Translation(pos), rot);
    transform.objectToInstance = mul(transpose(rot), Translation(-pos));
    return transform;
}
