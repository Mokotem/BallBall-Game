using FriteCollection.Graphics;
using FriteCollection.Scripting;

namespace RocketLike
{
    public class Settings : GetSeetings, ISetGameSettings
    {
        public void SetGameSettings()
        {
            Settings.GameFixeWidth = 480;
            Settings.GameFixeHeight = 270;
            Settings.FPS = 165;
            Settings.StartScene = Scenes.Menu;
            Settings.FullScreen = true;
        }
    }
    public class GameData
    {
        public static uint sp1 = 0, sp2 = 0;
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

        public override void Update()
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
    }
}
