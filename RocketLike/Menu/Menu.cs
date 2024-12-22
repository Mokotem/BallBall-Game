using FriteCollection.UI;
using FriteCollection.Scripting;
using Microsoft.Xna.Framework.Input;

namespace RocketLike;

class Inputs
{
    public static void CheckInputs(KeyboardState previous)
    {
        if (Time.TargetTimer > 0.1f)
        {
            KeyboardState ks = Keyboard.GetState();
            Keys[] lk = ks.GetPressedKeys();
            if (lk.Length > 0)
            {
                Keys k = lk[0];
                if (k.ToString()[0] == 'F'
                    && previous.GetPressedKeys().Length <= 0)
                {
                    MapManager.map = k.ToString()[1];
                    GameManager.CurrentScene = Scenes.Game;
                }
            }
        }
    }
}

public class Menu : Script
{
    public Menu() : base(Scenes.Menu) { }

    Text txt;

    public override void BeforeStart()
    {
        GameManager.GameFont = Open.Font("Game/fritefont");
    }
    public override void Start()
    {
        txt = new Text("INPUTS\n" +
            "f1 f2 f3 f4 f5 f6 f7 f8 f9  -->  choisir une map\n"
            + "Ctrl + R  -->  reset les scores" +
            "\n\n" +
            "SCORES\n" + 
            GameData.sp1.ToString() + " - " + GameData.sp2.ToString(),
                new Space(Bounds.TopLeft, Extend.Full));
        ps = Keyboard.GetState();
    }

    KeyboardState ps;

    public override void AfterUpdate()
    {
        Inputs.CheckInputs(ps);
        ps = Keyboard.GetState();
    }

    public override void Draw()
    {
        txt.Draw();
    }
}

public class Changing : Script
{
    public Changing() : base(Scenes.Game) { }

    public override void Start()
    {
        ps = Keyboard.GetState();
    }

    KeyboardState ps;

    public override void AfterUpdate()
    {
        Inputs.CheckInputs(ps);
        ps = Keyboard.GetState();
    }
}
