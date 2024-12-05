using FriteCollection.Graphics;
using FriteCollection.Scripting;

namespace RocketLike
{
    public class Settings : GetSeetings, ISetGameSettings
    {
        public void SetGameSettings()
        {
            Settings.WindowWidth = 384 * 3;
            Settings.WindowHeight = 256 * 3;
            Settings.GameFixeWidth = 384 + 16 * 4;
            Settings.GameFixeHeight = 256;
            Settings.FPS = 165;
            Settings.StartScene = Scenes.Game;
            Settings.FullScreen = true;
        }
    }

    public enum Scenes
    {
        Game
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
