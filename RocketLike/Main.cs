using FriteCollection.Graphics;
using FriteCollection.Scripting;
using FriteCollection.Tools.TileMap;

namespace RocketLike
{
    /*
    X ---- bug de collision x avec le joueur
    X ---- afficher l'heure en haut à gauche
    X ---- points au mec qui fait x3
    X ---- compteur de csc + popup
    X ---- 2 nouvels maps
    X ---- aim assist dans le menu touches
    X ---- replay
    X ---- bug de sauvegarde de temps.

    */


    public class Settings : GetSeetings, ISetGameSettings
    {
        public void SetGameSettings()
        {
            Settings.WindowWidth = 480 * 2;
            Settings.WindowHeight = 270 * 2;
            Settings.GameFixeWidth = 480;
            Settings.GameFixeHeight = 270;
            Settings.FPS = 240;
            Settings.StartScene = Scenes.Menu;
            Settings.FullScreen = true;
            Settings.WindowName = "BallBall Game";
        }
    }
    public static class GameData
    {
        public const string VERSION = "1.2.0";  // deja modifié /!\
        public static uint sp1 = 0, sp2 = 0;
        public static uint cp1 = 0, cp2 = 0;
        public static SAVEDATA SAVE;
        public static bool firstConnection = true;
        public static bool particles = true;
        public static bool randomMode = false;
        public static bool p1visee = false, p2visee = false;
        public static bool cscCounter = false;
        public static bool bonusOnCombo = true;
        public static bool bigTapCircles = false;
        public static bool dalton = false;
        public static bool replay = true;
        public static bool saved = false;
        public static float defaultdt = 1f;
        public static OgmoFile file;
        public static bool custom = false;
    }

    public delegate float Task(); // retourne le delai


    public class Sequence : Clone
    {
        private readonly Task[] _tasks;
        public Sequence(params Task[] tasks)
        {
            _tasks = tasks;
        }

        float timer = -1;
        uint i = 0;

        public override void BeforeUpdate()
        {
            if (i < _tasks.Length)
            {
                if (timer < 0)
                {
                    timer = _tasks[i]();
                    i++;
                }
                else
                    timer += -Time.FixedFrameTime;
            }
            else
                Destroy();
        }
    }

    public enum Scenes
    {
        Game, Menu
    }

    public abstract class Pico8
    {
        public static readonly Color Black = new Color(0, 0, 0);
        public static readonly Color DarkBlue = new Color() { RGB = new RGB(29, 46, 83) };
        public static readonly Color DarkPurple = new Color() { RGB = new RGB(126, 37, 83) };
        public static readonly Color DarkGreen = new Color() { RGB = new RGB(0, 135, 81) };
        public static readonly Color Brown = new Color() { RGB = new RGB(171, 82, 54) };
        public static readonly Color DarkGrey = new Color() { RGB = new RGB(95, 87, 79) };
        public static readonly Color LightGrey = new Color() { RGB = new RGB(194, 195, 199) };
        public static readonly Color White = new Color() { RGB = new RGB(255, 241, 232) };
        public static readonly Color Red = new Color() { RGB = new RGB(255, 0, 77) };
        public static readonly Color Orange = new Color() { RGB = new RGB(255, 163, 0) };
        public static readonly Color Yellow = new Color() { RGB = new RGB(255, 236, 39) };
        public static readonly Color Green = new Color() { RGB = new RGB(0, 228, 54) };
        public static readonly Color Blue = new Color() { RGB = new RGB(41, 173, 255) };
        public static readonly Color Lavender = new Color() { RGB = new RGB(131, 118, 156) };
        public static readonly Color Pink = new Color() { RGB = new RGB(255, 119, 168) };
        public static readonly Color LightPeach = new Color() { RGB = new RGB(255, 204, 170) };
        public static readonly Color[] COLORS = new Color[16]
        {
            Black,
        DarkBlue,
        DarkPurple,
        DarkGreen,
        Brown,
        DarkGrey,
        LightGrey,
        White,
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Lavender,
        Pink,
        LightPeach
        };
    }
}
