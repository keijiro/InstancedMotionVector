Shader "InstancedMesh"
{
    Properties
    {
        _MainTex("", 2D) = "white" {}
    }
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
            #pragma target 3.5
            #include "Motion.cginc"
            ENDCG
        }

        CGPROGRAM
        #pragma surface Surf Standard fullforwardshadows addshadow
        #pragma instancing_options procedural:Setup
        #pragma target 3.5
        #include "Surface.cginc"
        ENDCG
    }
    FallBack "Diffuse"
}
