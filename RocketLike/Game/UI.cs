using FriteCollection.UI;
using FriteCollection.Scripting;
using FriteCollection.Tools.SpriteSheet;
using FriteCollection.Input;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace RocketLike;

public class Back : Script
{
    public Back() : base(Scenes.Game) { }

    bool t;
    public override void Start()
    {
         t = false;
    }
    public override void AfterUpdate()
    {
        KeyboardState state = Keyboard.GetState();
        if (t == false && Input.KeyBoard.Escape && state.GetPressedKeyCount() < 2 && Time.TargetTimer > 0.2f)
        {
            GameData.SAVE.t += (ulong)(Time.TargetTimer);
            SaveManager.Save(GameData.SAVE);
            GameManager.CurrentScene = Scenes.Menu;
            t = true;
        }
    }
}

public class UIManager : Script
{
    public UIManager() : base(Scenes.Game, false) { }

    Panel panel;
    public static FriteCollection.Tools.TileMap.TileSet tile;
    SpriteSheet sheet;

    static Text score;
    Text escape;
    Text time;

    Texture2D t;

    private static string ts(int i)
    {
        return i < 10 ? "0" + i.ToString() : i.ToString();
    }
    private static DateTime T => DateTime.Now;
    private static string TimeSTR => ts(T.Hour) + ":" + ts(T.Minute);
    private static string Score => GameData.cscCounter ? (
        "(own goal: " + GameData.cp1.ToString() + ")      " + GameData.sp1.ToString()
        + "  -  " +
        GameData.sp2.ToString() + "      (own goal: " + GameData.cp2.ToString() + ")")
    :
        (
        GameData.sp1.ToString()
        + "  -  " +
        GameData.sp2.ToString()
        )
    ;

    public override void Start()
    {
        Layer = 50;
        t = Open.Texture("Game/ui");
        sheet = new SpriteSheet(t, 24, 24);
        tile = new(sheet[0, 0])
        {
            TileSize = new(8, 8),
            TileSeparation = new(0, 0),
            TileMargin = new(0, 0)
        };
        panel = new Panel(tile, new Space(Bounds.Top, Extend.Horizontal, new Vector(0, 14)));
        {
            score = new Text(Score,
                new Space(Bounds.Center, Extend.None, new Vector(0, 0), new Vector(0, 0)), panel);

            escape = new Text(">escape<",
                new Space(Bounds.Left, Extend.Full, new Vector(64, 14), new Vector(4, 0)), panel);
            time = new Text(TimeSTR,
                new Space(Bounds.Center, Extend.Full, new Vector(64, 14), new Vector(226, 0)), panel);
        }

        (GameManager.GetScript("Ball") as Ball).BUT += OnBut;
    }

    public static void setScore()
    {
        score.Edit = Score;
    }

    void OnBut(bool sideRight, Ball.States ballState, bool csc, byte t, bool combo)
    {
        if (sideRight)
        {
            GameData.sp1++;
            if (csc && GameData.cscCounter)
                GameData.cp2++;
        }
        else
        {
            GameData.sp2++;
            if (csc && GameData.cscCounter)
                GameData.cp1++;
        }
        score.Edit = Score;
    }

    public override void Update()
    {
        time.Edit = TimeSTR;
    }

    public override void DrawUI()
    {
        panel.Draw();
        score.Draw();
        escape.Draw();
        time.Draw();
    }

    public override void Dispose()
    {
        tile.Dispose();
        sheet.Dispose();
        panel = null;
        tile = null;
        t.Dispose();

        escape = null;
        time = null;
    }
}