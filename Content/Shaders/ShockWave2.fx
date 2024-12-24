#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float timer;

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 posi = input.TextureCoordinates;
    
    posi.y += pow(2, -pow((3 * posi.x) - (timer * 7) + 2, 2)) * sin((3 * posi.x) - (timer * 7) + 3) * 0.1;
    
    if (posi.y > 1)
    {
        return tex2D(SpriteTextureSampler, float2(posi.x % 1, 1));
    } 
    if (posi.y < 0)
    {
        return tex2D(SpriteTextureSampler, float2(posi.x % 1, 0));
    }
    return tex2D(SpriteTextureSampler, posi);
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};