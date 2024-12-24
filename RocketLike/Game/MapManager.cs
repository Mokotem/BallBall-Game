using FriteCollection.Scripting;
using FriteCollection.Tools.TileMap;
using FriteCollection.Graphics;
using FriteCollection.Entity;
using Microsoft.Xna.Framework.Graphics;

namespace RocketLike;

public class MapManager : Script
{
    public static string map;

    public MapManager() : base(Scenes.Game)
    {

    }


    private static TileSet tiles;
    Texture2D texture;

    private static OgmoFile ogmo;
    public static TileMap tilemap;
    public static bool hide = false;
    public const byte camPosY = 7;

    Object grad;
    Effect fx;
    BlendState multiplyBlend;

    public override void BeforeStart()
    {
        texture = Open.Texture("Game/MapManager/tileSet");
        ogmo = Open.OgmoTileMap("Maps\\map" + map + ".json");
        tiles = new TileSet(texture)
        {
            TileSize = new(16, 16),
            TileSeparation = new(1, 1),
            TileMargin = new(-1, -1),
        };

        Layer = -10;
        Camera.Position = new(0, camPosY);

        HitBox.Rectangle hit = new HitBox.Rectangle(new Space());
        hit.Active = false;
        hit.tag = "plat";
        hit.LockSize = new(16, 16);

        HitBox.Rectangle hitLeft = new HitBox.Rectangle(new Space());
        hitLeft.LockSize = new(38, 16);
        hitLeft.PositionOffset.x = -24;
        hitLeft.Active = false;
        hitLeft.Layer = 1;
        HitBox.Rectangle hitRight = new HitBox.Rectangle(new Space());
        hitRight.LockSize = new(38, 16);
        hitRight.PositionOffset.x = 24;
        hitRight.Layer = 1;
        hitRight.Active = false;

        HitBox.Rectangle hitBottomLeft = new HitBox.Rectangle(new Space());
        hitBottomLeft.Active = false;
        hitBottomLeft.Layer = 1;
        hitBottomLeft.LockSize = new(16, 36);
        hitBottomLeft.PositionOffset.y = -24;
        HitBox.Rectangle hitBottomRight = new HitBox.Rectangle(new Space());
        hitBottomRight.Layer = 1;
        hitBottomRight.Active = false;
        hitBottomRight.LockSize = new(16, 36);
        hitBottomRight.PositionOffset.y = -24;

        HitBox.Rectangle hitHalf = new HitBox.Rectangle(new Space());
        hitHalf.Layer = 0;
        hitHalf.tag = "plat";
        hitHalf.Active = false;
        hitHalf.LockSize = new(16, 8);
        hitHalf.PositionOffset.y = 4;

        tiles.ReplaceHitbox[2, 8] = hit;
        tiles.ReplaceHitbox[2, 7] = hitHalf;
        tiles.ReplaceHitbox[0, 10] = hitLeft;
        tiles.ReplaceHitbox[1, 10] = hitRight;
        tiles.ReplaceHitbox[0, 11] = hitBottomLeft;
        tiles.ReplaceHitbox[1, 11] = hitBottomRight;

        tiles.DontDraw(0, 6);
        tiles.DontDraw(1, 6);

        tilemap = new TileMap(tiles, ogmo);
        tilemap.Position.y = 7;

        grad = new Object()
        {
            Space = new Space()
            {
                GridOrigin = Bounds.BottomLeft,
                CenterPoint = Bounds.BottomLeft,
                Position = new(-20, -20),
                Scale = new(Screen.widht + 40, (Screen.height / 1.5f) + 40)
            },
        };
        grad.Renderer.Color = Pico8.Yellow;

        fx = FriteModel.MonoGame.instance.Content.Load<Effect>("Shaders/Gradient");

        multiplyBlend = new BlendState()
        {
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.DestinationAlpha,
            AlphaSourceBlend = Blend.DestinationAlpha,
            ColorBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.DestinationColor,
            ColorDestinationBlend = Blend.DestinationAlpha,
        };
    }

    public override void Draw()
    {
        if (!hide)
        {
            tilemap.Draw();
        }
    }

    public override void DrawAdditive(ref SpriteBatch _batch)
    {
    
        _batch.Begin(effect: fx ,blendState: multiplyBlend, samplerState: SamplerState.PointClamp);
        grad.Draw();
        _batch.End();
    }

    public override void Dispose()
    {
        tiles.Dispose();
        tilemap.Dispose();
        fx.Dispose();
        texture.Dispose();
    }
}

class BackGround : Script
{
    public static Color color1, color2;


    public BackGround() : base(Scenes.Game)
    {

    }

    public Object bg = new Object();
    Effect shader;

    public override void BeforeStart()
    {
        color1 = Pico8.Black;
        color2 = Pico8.DarkBlue;
        Layer = -20;

        bg.Space.Scale = new FriteCollection.Entity.Vector(Screen.widht - 16, Screen.height - (16 * 2));
        bg.Renderer.Color = color1;
        bg.Space.LockCamera = true;
        bg.Space.Position.y = -8;

        shader = FriteModel.MonoGame.instance.Content.Load<Effect>("Shaders/ShaderTahLesFous");
    }

    public override void Update()
    {
        shader.Parameters["timer"].SetValue(Time.TargetTimer);
    }

    public override void BeforeDraw()
    {
        bg.Draw();
    }

    public override void AfterDraw()
    {
        if (Time.TargetTimer > 0.2f && FriteCollection.Input.Input.KeyBoard.H)
        HitBox.Debug();
    }

    public override void Dispose()
    {
        shader.Dispose();
    }
}