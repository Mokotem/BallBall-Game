#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
float value;

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

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 pos = input.TextureCoordinates;
    float4 c = tex2D(SpriteTextureSampler, pos);
    if ((pos.y + value) % 0.02f < 0.004)
    {
        c.rg *= 1.1;
    }
    c.b = (c.b + tex2D(SpriteTextureSampler, float2(pos.x, pos.y + 0.003)).b) / 2;
    c.g = (c.g + tex2D(SpriteTextureSampler, float2(pos.x, pos.y - 0.003)).b) / 2;
    return c;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};