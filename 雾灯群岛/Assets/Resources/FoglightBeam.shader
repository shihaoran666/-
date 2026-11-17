Shader "FoglightIsles/Beam"
{
    Properties
    {
        _Color ("Beam color", Color) = (1, 0.55, 0.16, 0.2)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct VertexInput
            {
                float4 vertex : POSITION;
            };
            struct VertexOutput
            {
                float4 position : SV_POSITION;
                float distanceAlongBeam : TEXCOORD0;
            };

            fixed4 _Color;

            VertexOutput vert(VertexInput input)
            {
                VertexOutput output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.distanceAlongBeam = saturate(input.vertex.z / 58.0);
                return output;
            }

            fixed4 frag(VertexOutput input) : SV_Target
            {
                fixed fade = 1.0 - 0.6 * input.distanceAlongBeam;
                return fixed4(_Color.rgb, _Color.a * fade);
            }
            ENDCG
        }
    }
}
