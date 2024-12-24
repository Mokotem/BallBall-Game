using FriteCollection.Entity;
using MonoGame.Extended;

namespace FriteCollection.Tools.Pen
{
    /// <summary>
    /// Draws basic shapes : Line, Rectangle, Circle, Point
    /// </summary>
    public abstract class Pen
    {
        /// <summary>
        /// Drawing settings
        /// </summary>
        public static float thickness = 1;
        public static float layer;
        public static Graphics.Color Color = Graphics.Color.White;
        public static Entity.Bounds GridOrigin;
        public static bool UI = false;

        /// <summary>
        /// Draws a line from a point to an other
        /// </summary>
        public static void Line(Entity.Vector v1, Entity.Vector v2, float? thickness = null, Graphics.Color color = null)
        {
            float th = thickness is null ? Pen.thickness : thickness.Value;
            Graphics.Color co = color is null ? Pen.Color : color;
            Vector offset = Pen.UI ? FriteModel.MonoGame.instance.UIscreenBounds[(int)Pen.GridOrigin]
                                     :
                                     FriteModel.MonoGame.instance.screenBounds[(int)Pen.GridOrigin];

            FriteModel.MonoGame.instance.SpriteBatch.DrawLine
            (
                    v1.x + offset.x - Scripting.Camera.Position.x,
                    -v1.y + offset.y + Scripting.Camera.Position.y,
                    v2.x + offset.x - Scripting.Camera.Position.x,
                    -v2.y + offset.y + Scripting.Camera.Position.y,
                    new Microsoft.Xna.Framework.Color
                    (
                        co.RGB.R, co.RGB.G, co.RGB.B
                    ),
                    th,
                    0
            );
        }

        /// <summary>
        /// Draws a rectangle. (Rectangle center point : TopLeft)
        /// </summary>
        public static void Rectangle(Entity.Vector v, float width, float height, float? thickness = null, Graphics.Color color = null)
        {
            float th = thickness is null ? Pen.thickness : thickness.Value;
            Graphics.Color co = color is null ? Pen.Color : color;
            Vector offset = Pen.UI ? FriteModel.MonoGame.instance.UIscreenBounds[(int)Pen.GridOrigin]
                         :
                         FriteModel.MonoGame.instance.screenBounds[(int)Pen.GridOrigin];

            FriteModel.MonoGame.instance.SpriteBatch.DrawRectangle
            (
                new RectangleF
                (
                    v.x + offset.x, -v.y + offset.y, width, height
                ),
                new Microsoft.Xna.Framework.Color
                (
                        co.RGB.R, co.RGB.G, co.RGB.B
                ),
                th,
                0
            );
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        public static void Circle(Entity.Vector v, float radius, float? thickness = null, Graphics.Color color = null, float alpha = 1f)
        {
            float th = thickness is null ? Pen.thickness : thickness.Value;
            Graphics.Color co = color is null ? Pen.Color : color;
            Vector offset = Pen.UI ? FriteModel.MonoGame.instance.UIscreenBounds[(int)Pen.GridOrigin]
                         :
                         FriteModel.MonoGame.instance.screenBounds[(int)Pen.GridOrigin];

            FriteModel.MonoGame.instance.SpriteBatch.DrawCircle
            (
                new CircleF
                (
                    new Microsoft.Xna.Framework.Vector2(
                    v.x + offset.x - Scripting.Camera.Position.x,
                    -v.y + offset.y + Scripting.Camera.Position.y),
                    radius
                ),
                (int)(radius),
                new Microsoft.Xna.Framework.Color
                (
                        co.RGB.R * alpha, co.RGB.G * alpha, co.RGB.B * alpha, alpha
                ),
                th,
                0
            );
        }

        /// <summary>
        /// Draws a point.
        /// </summary>
        public static void Point(Entity.Vector v, float? thickness = null, Graphics.Color color = null, float alpha = 1)
        {
            float th = thickness is null ? Pen.thickness : thickness.Value;
            Graphics.Color co = color is null ? Pen.Color : color;
            Vector offset = Pen.UI ? FriteModel.MonoGame.instance.UIscreenBounds[(int)Pen.GridOrigin]
                         :
                         FriteModel.MonoGame.instance.screenBounds[(int)Pen.GridOrigin];

            FriteModel.MonoGame.instance.SpriteBatch.DrawPoint
            (
                v.x + offset.x,
                -v.y + offset.y,
                new Microsoft.Xna.Framework.Color
                (
                        co.RGB.R * alpha, co.RGB.G * alpha, co.RGB.B * alpha, alpha
                ),
                th,
                0
            );
        }
    }
}