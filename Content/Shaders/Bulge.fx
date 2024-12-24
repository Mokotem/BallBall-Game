#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
float volume;
float value;
float cx;
float cy;

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
	if (value > 0.5)
    {
        value = 0.5;
    }
    if (value < 0)
    {
        value = 0;
    }
	float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates);
    float2 pos = input.TextureCoordinates;
	
    float thing = pow(cx - pos.r, 2) + pow(cy - pos.g, 2);
    float dist = sqrt(thing);
    float mult = pow(dist, 2) * value;
	
    pos.r += (cx - pos.r) * mult;
    pos.g += (cy - pos.g) * mult;
	
    return (tex2D(SpriteTextureSampler, pos) * volume)
	+ (tex2D(SpriteTextureSampler, input.TextureCoordinates) * (1 - volume));

}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};