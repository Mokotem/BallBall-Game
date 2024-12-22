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
    float2 newposi = float2(0, 0);
    newposi.x = (posi.x + (sin(timer + (posi.x * 5)) * 0.1));
	if (newposi.x < 0)
    {
        newposi.x += 1;
    }

    newposi.x = newposi.x % 1;
    return tex2D(SpriteTextureSampler, newposi);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};