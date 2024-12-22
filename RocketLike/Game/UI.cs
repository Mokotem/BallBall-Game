using FriteCollection.UI;
using FriteCollection.Scripting;

namespace RocketLike;

public class UIManager : Script
{
    public UIManager() : base(Scenes.Game) { }

    Panel panel;
    FriteCollection.Tools.TileMap.TileSet tile;
    Microsoft.Xna.Framework.Graphics.Texture2D texture;

    Text p1;

    public override void Start()
    {
        Layer = 50;
        texture = Open.Texture("Game/ui");
        tile = new(texture)
        {
            TileSize = new(8, 8),
            TileSeparation = new(0, 0),
            TileMargin = new(0, 0)
        };
        panel = new Panel(tile, new Space(Bounds.Top, Extend.Horizontal, new Vector(0, 14)));
        {
            p1 = new Text(GameData.sp1.ToString() + " - " + GameData.sp2.ToString(),
                new Space(Bounds.Center, Extend.Full, new Vector(64, 14), new Vector(0, 0)), panel);
        }

        (GameManager.GetScript("Ball") as Ball).BUT += onBut;
    }

    void onBut(bool sideRight, Ball.States ballState)
    {
        if (sideRight)
        {
            GameData.sp1++;
        }
        else
        {
            GameData.sp2++;
        }
        p1.Edit = GameData.sp1.ToString() + " - " + GameData.sp2.ToString();
    }

    public override void AfterDraw()
    {
        panel.Draw();
        p1.Draw();
    }

    public override void Dispose()
    {
        tile.Dispose();
    }
}