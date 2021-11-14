Shader "Unlit/PanoramaShader"
{
    Properties
    {
       _Color("Main Color", Color) = (1,1,1,1)
       _MainTex("Base (RGB)", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        //LOD 200

        // Render the texture inside the sphere.
        Cull Front

        CGPROGRAM
        // Physically based standard lighting model, and enable shadows on all light types
        #pragma surface surf SimpleLambert
        half4 LightingSimpleLambert(SurfaceOutput s, half3 lightDir, half atten)
      {
         half4 c;
         c.rgb = s.Albedo;
         c.a = s.Alpha;
         return c;
      }


    // Use shader model 3.0 target, to get nicer looking lighting
    #pragma target 3.0

    sampler2D _MainTex;

    struct Input
    {
        float2 uv_MainTex;
        float4 myColor : COLOR;
    };

    half _Glossiness;
    half _Metallic;
    fixed3 _Color;

    // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
    // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
    // #pragma instancing_options assumeuniformscaling
    UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
    UNITY_INSTANCING_BUFFER_END(Props)

    void surf(Input IN, inout SurfaceOutput o)
    {
        // Correct the automatic mirroring of the image that occurs when rendering it inside the sphere.
        IN.uv_MainTex.x = 1 - IN.uv_MainTex.x;

        fixed3 result = tex2D(_MainTex, IN.uv_MainTex) * _Color;
        o.Albedo = result.rgb;
        o.Alpha = 1;
    }
    ENDCG
    }
        FallBack "Diffuse"
}
