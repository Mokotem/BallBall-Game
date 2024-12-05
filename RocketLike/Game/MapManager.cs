using FriteCollection.Scripting;
using FriteCollection.Tools.TileMap;
using FriteCollection.Graphics;
using FriteCollection.Entity;

namespace RocketLike;

public class MapManager : Script
{
    public MapManager() : base(Scenes.Game)
    {
        List<int> lst = new List<int>(1, 2, 3, 4, 5, 6);
        GameManager.Print(lst, lst.Count);
        lst.RemoveIndex(2);
        GameManager.Print(lst, lst.Count);
    }

    private static readonly TileSet tiles = new TileSet(Open.Texture("Game/MapManager/tileSet"))
    {
        TileSize = new(16, 16),
        TileSeparation = new(1, 1),
        TileMargin = new(-1, -1),
    };

    private static readonly OgmoFile ogmo = Open.OgmoTileMap("Maps\\map1.json");
    public TileMap tilemap;

    public override void BeforeStart()
    {
        Screen.backGround = Color.Black;

        HitBox.Rectangle hit = new HitBox.Rectangle(new Space());
        hit.Active = false;
        hit.tag = "plat";
        hit.LockSize = new(16, 16);
        HitBox.Rectangle hitLeft = new HitBox.Rectangle(new Space());
        hitLeft.Layer = 1;
        hitLeft.PositionOffset.x = -16;
        hitLeft.Active = false;
        hitLeft.LockSize = new(28, 16);
        HitBox.Rectangle hitRight = new HitBox.Rectangle(new Space());
        hitRight.Layer = 1;
        hitRight.PositionOffset.x = 16;
        hitRight.Active = false;
        hitRight.LockSize = new(28, 16);

        tiles.ReplaceHitbox[2, 3] = hit;
        tiles.ReplaceHitbox[0, 10] = hitLeft;
        tiles.ReplaceHitbox[1, 10] = hitRight;

        tiles.DontDraw(0, 6);
        tiles.DontDraw(1, 6);

        tilemap = new TileMap(tiles, ogmo, background: Pico8.DarkGrey);
    }

    public override void Update()
    {

    }

    public override void BeforeDraw()
    {
        tilemap.Draw();
    }

    public override void Dispose()
    {
        tiles.Dispose();
    }
}