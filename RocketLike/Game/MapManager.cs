using FriteCollection.Scripting;
using FriteCollection.Tools.TileMap;
using FriteCollection.Graphics;
using FriteCollection.Entity;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace RocketLike;

public class MapManager : Script
{
    public static string map;

    public MapManager() : base(Scenes.Game, false)
    {

    }


    private static TileSet tiles;
    Texture2D texture;

    public const byte numberOfMaps = 18;

    private static OgmoFile ogmo;
    public static TileMap tilemap;
    public static bool hide = false;
    public const byte camPosY = 7;

    Object grad;
    Effect fx;
    BlendState multiplyBlend;

    Object cache;

    public override void BeforeStart()
    {
        texture = Open.Texture("Game/MapManager/tileSet");
        if (GameData.custom)
        {
            ogmo = GameData.file;
            GameData.custom = false;
        }
        else
        {
            ogmo = Open.OgmoTileMap("Maps\\" + map + ".json");
        }
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

        HitBox.Rectangle hitBottom = new HitBox.Rectangle(new Space());
        hitBottom.Active = false;
        hitBottom.Layer = 1;
        hitBottom.LockSize = new(16, 18);
        hitBottom.PositionOffset.y = -15;


        tiles.ReplaceHitbox[2, 6] = hit;
        tiles.ReplaceHitbox[0, 9] = hitLeft;
        tiles.ReplaceHitbox[1, 9] = hitRight;
        tiles.ReplaceHitbox[0, 11] = hitBottom;
        tiles.ReplaceHitbox[1, 11] = hitBottom;

        tiles.DontDraw(0, 5);
        tiles.DontDraw(1, 5);

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

        fx = GameManager.instance.Content.Load<Effect>("Shaders/Gradient");

        multiplyBlend = new BlendState()
        {
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.DestinationAlpha,
            AlphaSourceBlend = Blend.DestinationAlpha,
            ColorBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.DestinationColor,
            ColorDestinationBlend = Blend.DestinationAlpha,
        };

        cache = new Object();
        cache.Space.Scale = new(Screen.widht + 20, Screen.height);
        cache.Space.Position = new(0, -Screen.height + 7);
        cache.Renderer.Color = Pico8.Lavender;
    }

    public override void Update()
    {
        if (Time.TargetTimer > 3)
        {
            cache = null;
        }
    }

    public override void Draw()
    {
        if (!hide)
        {
            tilemap.Draw();
            if (Time.TargetTimer <= 3)
                cache.Draw();

        }
    }

    public override void Draw(ref SpriteBatch _batch)
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
        tiles = null;
        tilemap = null;
        fx = null;
        texture = null;
    }
}

class BackGround : Script
{
    public static Color color1, color2;


    public BackGround() : base(Scenes.Game, false)
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

        shader = GameManager.instance.Content.Load<Effect>("Shaders/ShaderTahLesFous");
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
        if (Time.TargetTimer > 0.2f && FriteCollection.Input.Input.KeyBoard.H && FriteCollection.Input.Input.KeyBoard.CtrlLeft)
        HitBox.Debug();
    }

    public override void Dispose()
    {
        shader.Dispose();
        shader = null;
    }
}