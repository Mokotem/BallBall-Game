using FriteCollection.UI;
using FriteCollection.Scripting;
using Microsoft.Xna.Framework.Input;
using FriteCollection.Input;
using FriteCollection.Audio;

namespace RocketLike;

public class STATS
{
    public ulong tempsDeJeu = 0;
    public ulong nombreDeButsNone = 0, nombreDeButsGolden = 0, nombreDeButsRed = 0;
    public ulong nombreTirReussi = 0, nombreTirRate = 0;
}

class Inputs
{
    public static void CheckInputs(KeyboardState previous)
    {
        if (Time.TargetTimer > 0.2f)
        {
            KeyboardState ks = Keyboard.GetState();
            Keys[] lk = ks.GetPressedKeys();
            if (lk.Length > 0)
            {
                Keys k = lk[0];
                if (k.ToString().Length > 1 && (k.ToString()[0] == 'F'
                    || k.ToString() == "F10"
                    || k.ToString() == "F11"
                    || k.ToString() == "F12")
                    && previous.GetPressedKeys().Length <= 1)
                {
                    if (lk.Length > 1 && (lk[1] == Keys.RightControl || lk[1] == Keys.LeftControl))
                    {
                        Ball.SetAttireMode(true);
                    }
                    else
                    {
                        Ball.SetAttireMode(false);
                    }
                    if (GameManager.CurrentScene != Scenes.Menu)
                        GameData.STATS.tempsDeJeu += (uint)(Time.TargetTimer);
                    if (k.ToString().Length < 3)
                    MapManager.map = k.ToString()[1].ToString();
                    else
                    {
                        MapManager.map = k.ToString()[1].ToString() + k.ToString()[2].ToString();
                    }
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

    private string remove(string s)
    {
        string res = "";
        for(int i = 0; i < s.Length; i++)
        {
            if (s[i] == ',')
            {
                return res + "." + s[i + 1] + s[i + 2];
            }
            else
            {
                res += s[i];
            }
        }
        return res;
    }

    private string texte =>
        "                                 BALL BALL GAME\n\n" +
        "    CONTROLES\n" +
            "         f1  f2  f3  f4  f5  f6  f7  f8  f9  -->  choisir une map, mode normal\n" +
            "ctrl  +  f1  f2  f3  f4  f5  f6  f7  f8  f9  -->  choisir une map, mode cible\n\n" +

            "gauche droite haut shift  -->  player1\n" +
            "d      g      r    q      -->  player2\n\n" +
            "h  -->  hitboxes pour les nerds\n\n" +
            "echap  -->  revenir ici\n" +
            "\n" +
            "    SCORES\n" +
            GameData.sp1.ToString() + " - " + GameData.sp2.ToString() + "\n" +
            "ctrl + r  -->  reset les scores\n\n" +
            "    STATS\n" +
        "temps de jeu = " + remove((GameData.STATS.tempsDeJeu / 3600f).ToString()) + " heures\n\n" +
        "nombre total de buts = " + (GameData.STATS.nombreDeButsNone + GameData.STATS.nombreDeButsGolden + GameData.STATS.nombreDeButsRed).ToString() + "\n" +
        "neutre > " + (GameData.STATS.nombreDeButsNone).ToString() + "\n" +
        "jaune  > " + (GameData.STATS.nombreDeButsGolden).ToString() + "\n" +
        "rouge  > " + (GameData.STATS.nombreDeButsRed).ToString() + "\n\n" +
        "nombre total de tirs = " + (GameData.STATS.nombreTirRate + GameData.STATS.nombreTirReussi).ToString() + "\n" +
        ((GameData.STATS.nombreTirRate + GameData.STATS.nombreTirReussi) > 0 ?
        "tirs reussi > "
        + ((uint)((GameData.STATS.nombreTirReussi / (float)(GameData.STATS.nombreTirRate + GameData.STATS.nombreTirReussi)) * 100))
        .ToString() + "%" : "") + "\n\n" +
        "ctrl + suppr + entrer  -->  supprimer les stats\n" +
        "                                                                                       Frite";

    public override void BeforeStart()
    {
        Screen.backGround = Pico8.Lavender / 2f;
        GameManager.GameFont = Open.Font("Game/fritefont");
    }
    public override void Start()
    {
        if (GameData.firstConnection == false)
        {
            SaveManager.Save(GameData.STATS);
        }
        else if (SaveManager.FileExist)
        {
            GameData.STATS = SaveManager.Load<STATS>();
        }
        else
        {
            GameData.STATS = new STATS();
        }
        txt = new Text(texte,
                new Space(Bounds.TopLeft, Extend.Full, new(), new Vector(75, 10)));
        ps = Keyboard.GetState();
        GameData.firstConnection = false;
    }

    KeyboardState ps;

    public override void AfterUpdate()
    {
        Inputs.CheckInputs(ps);
        ps = Keyboard.GetState();
        if (Input.KeyBoard.R && (Input.KeyBoard.CtrlLeft || Input.KeyBoard.CtrlRight))
        {
            GameData.sp1 = 0;
            GameData.sp2 = 0;
            txt.Edit = texte;
        }
        if (ps.IsKeyDown(Keys.LeftControl) && ps.IsKeyDown(Keys.Delete) && ps.IsKeyDown(Keys.Enter)
            && ps.GetPressedKeyCount() < 4)
        {
            SaveManager.Delete();
            GameData.STATS = new STATS();
            txt.Edit = texte;
        }
    }

    public override void Draw()
    {
        txt.Draw();
    }
}

public class Changing : Script
{
    public Changing() : base(Scenes.Game) { }

    Music music;
    
    public override void Start()
    {
        music = Open.Music("Game/587501_Last-Tile-");
        music.Loop = true;
        Music.Volume = 0.5f;
        music.Start();

        ps = Keyboard.GetState();
    }

    KeyboardState ps;

    public override void AfterUpdate()
    {
        Inputs.CheckInputs(ps);
        ps = Keyboard.GetState();
    }

    public override void Dispose()
    {
        music.Dispose();
    }
}
