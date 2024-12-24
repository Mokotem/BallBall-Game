using FriteCollection.UI;
using FriteCollection.Scripting;
using FriteCollection.Graphics;

namespace RocketLike;

public class UIManager : Script
{
    public UIManager() : base(Scenes.Game) { }

    Panel panel;
    FriteCollection.Tools.TileMap.TileSet tile;
    Microsoft.Xna.Framework.Graphics.Texture2D texture;

    Text p1, p2, p3;
    Text escape;
    Text time;
    Text mode;

    public override void AfterStart()
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
            p1 = new Text(GameData.sp1.ToString(),
                new Space(Bounds.Center, Extend.None, new Vector(0, 0), new Vector(-9, 0)), panel);
            p2 = new Text("(   -   )",
                new Space(Bounds.Center, Extend.None, new Vector(0, 0), new Vector(0, 0)), panel);
            p3 = new Text(GameData.sp2.ToString(),
                new Space(Bounds.Center, Extend.None, new Vector(0, 0), new Vector(9, 0)), panel);

            escape = new Text("echap  -->  menu principal",
                new Space(Bounds.Left, Extend.Full, new Vector(64, 14), new Vector(4, 0)), panel);
            time = new Text("00:00",
                new Space(Bounds.Center, Extend.Full, new Vector(64, 14), new Vector(226, 0)), panel);

            if (Ball.AttireMode)
            {
                mode = new Text("target mode",
                new Space(Bounds.Center, Extend.None, new Vector(0, 0), new Vector(115, 0)), panel);
                mode.Color = Pico8.Pink;
            }
            else
            {
            mode = new Text("normal mode",
                new Space(Bounds.Center, Extend.None, new Vector(0, 0), new Vector(115, 0)), panel);
                mode.Color = Pico8.Blue * 1.5f;
            }
        }
        p1.Color = Color.White;
        p3.Color = Color.White;

        (GameManager.GetScript("Ball") as Ball).BUT += onBut;
    }

    void onBut(bool sideRight, Ball.States ballState)
    {
        if (sideRight)
        {
            GameData.sp1++;
            p3.Color = Color.White;
            p1.Color = Pico8.Yellow;
        }
        else
        {
            GameData.sp2++;
            p3.Color = Pico8.Yellow;
            p1.Color = Color.White;
        }
        p1.Edit = GameData.sp1.ToString();
        p3.Edit = GameData.sp2.ToString();
    }

    public override void Update()
    {
        string r = ((int)(Time.TargetTimer) % 60).ToString();
        if (r.Length < 2)
        {
            r = "0" + r;
        }
        string l = ((int)(Time.TargetTimer) / 60).ToString();
        if (l.Length < 2)
        {
            l = "0" + l;
        }
        time.Edit = l + ":" + r;
    }

    public override void DrawUI()
    {
        panel.Draw();
        p1.Draw();
        p2.Draw();
        p3.Draw();
        escape.Draw();
        time.Draw();
        mode.Draw();
    }

    public override void Dispose()
    {
        tile.Dispose();
    }
}