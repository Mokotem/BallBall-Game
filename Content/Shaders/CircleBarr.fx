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

float amount;
float thick;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float PI = 3.141592653589793;
    float2 co = input.TextureCoordinates;
    float x = co.r - 0.5;
    float y = co.g - 0.5;
    float dist = sqrt((x * x) + (y * y));
    float dir = atan(y / x);
    if (x < 0)
        dir += PI;
    if ((1 - thick) / 2 <= dist && dist <= 0.5)
    {
        if (dir + (PI / 2) >= amount * PI * 2)
        {
            return input.Color;
        }
    }
    return float4(0, 0, 0, 0);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};