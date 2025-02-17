﻿using System;
using Microsoft.Xna.Framework;
using FriteCollection.Scripting;
using Microsoft.Xna.Framework.Graphics;

namespace FriteCollection.Entity
{
    public abstract class Entity
    {
        /// <summary>
        /// Space data of the Entity.
        /// </summary>
        public Space Space = new();

        /// <summary>
        /// Aesthetic data of the Entity.
        /// </summary>
        public Renderer Renderer = new Renderer();

        /// <summary>
        /// Draws the entity on screen.
        /// </summary>
        /// <param name="textureSpacePart">only draw a part of the texture</param>
        public virtual void Draw() { }

        private protected Color GetEntColor()
        {
            return new Color
            (
                Renderer.Color.RGB.R * Renderer.Alpha,
                Renderer.Color.RGB.G * Renderer.Alpha,
                Renderer.Color.RGB.B * Renderer.Alpha,
                Renderer.Alpha
            );
        }
    }

    /// <summary>
    /// Object.
    /// </summary>
    public class Object : Entity, ICopy<Object>
    {
        public Object Copy()
        {
            return new()
            {
                Space = this.Space.Copy(),
                Renderer = this.Renderer.Copy()
            };
        }

        public override void Draw()
        {
            if (Renderer.hide == false)
            {
                Vector entPosi = base.Space.GetScreenPosition(includeCamera: (!Space.UI && !Space.LockCamera));
                Vector s = Space.Copy().Scale;
                float flipFactor = 0f;

                if (Space.GridOrigin == Camera.GridOrigin)
                {
                    s *= Camera.zoom;
                }

                SpriteEffects spriteEffect = SpriteEffects.None;
                if (s.x < 0 && s.y < 0)
                {
                    flipFactor = MathF.PI * 1;
                }
                else
                {
                    if (s.x < 0 && s.y >= 0)
                    {
                        spriteEffect = SpriteEffects.FlipVertically;
                        flipFactor = MathF.PI;
                    }
                    else if (s.y < 0 && s.x >= 0)
                    {
                        spriteEffect = SpriteEffects.FlipHorizontally;
                        flipFactor = MathF.PI;
                    }
                }

                GameManager.instance.SpriteBatch.Draw
                (
                    Renderer.Texture,
                    new Microsoft.Xna.Framework.Rectangle
                    (
                        (int)entPosi.x,
                        (int)entPosi.y,
                        (int)MathF.Abs(s.x),
                        (int)MathF.Abs(s.y)
                    ),
                    null,
                    base.GetEntColor(),
                    Space.rotation * (MathF.PI / 180f) + flipFactor,
                    Renderer.GetTextureBounds()[(int)Space.CenterPoint].ToVector2(),
                    spriteEffect,
                    0
                );
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Object)
            {
                return Space.Equals((obj as Object).Space)
                    && Renderer.Equals((obj as Object).Renderer);
            }
            return false;
        }

        public override string ToString()
        {
            return "Object (" + Space.ToString() + ", " + Renderer.ToString() + ")";
        }
    }

    /// <summary>
    /// Text.
    /// </summary>
    public class Text : Entity, ICopy<Text>
    {
        private int _spacing;
        private static readonly BoundFunc _boundFuncs = new();

        public Text Copy()
        {
            Text t = new(this.font)
            {
                Space = this.Space.Copy(),
                Renderer = Renderer.Copy(),
                Spacing = this._spacing
            };
            return t;
        }

        public override void Draw()
        {
            if (Renderer.hide == false && _text != null)
            {
                Vector entPosi = base.Space.GetScreenPosition(includeCamera: !Space.UI);

                GameManager.instance.SpriteBatch.DrawString
                (
                    font,
                    _text,
                    new Vector2
                    (
                        entPosi.x, entPosi.y
                    ),
                    base.GetEntColor(),
                    Space.rotation * (MathF.PI / 180f),
                    GetTextBounds()[(int)Space.CenterPoint].ToVector2(),
                    1,
                    SpriteEffects.None,
                    0
                );
            }
        }

        private Vector[] _bounds;
        /// <summary>
        /// Gets the 9 bounds of the text.
        /// </summary>
        /// <returns> an array of 9 Vector</returns>
        public Vector[] GetTextBounds()
        {
            return _bounds;
        }

        private string _text = null;

        /// <summary>
        /// Text to show.
        /// </summary>
        public string Write
        {
            get { return _text; }
            set
            {
                _text = value;
                this.Space.Scale = new Vector
                (
                    (font.MeasureString(value).X) + (_spacing * (value.Length - 1)),
                    (font.MeasureString(value).Y)
                );
                this._bounds = _boundFuncs.CreateBounds(
                    (font.MeasureString(value).X) + (_spacing * (value.Length - 1)),
                    (font.MeasureString(value).Y)
                    );
            }
        }

        private SpriteFont font;

        /// <summary>
        /// Gets the font file of the Text.
        /// </summary>
        public SpriteFont Font
        {
            get
            {
                return font;
            }
        }

        private void Constructor()
        {
            _spacing = 0;
            Space.CenterPoint = Bounds.Center;
        }

        /// <summary>
        /// Creates a Text Entity.
        /// </summary>
        /// <param name="font">font file</param>
        public Text(SpriteFont font)
        {
            this.font = font;
            Constructor();
        }

        /// <summary>
        /// Creates a Text Entity.
        /// </summary>
        /// <param name="font">font file</param>
        /// <param name="text">text to show</param>
        public Text(SpriteFont font, string text)
        {
            this.font = font;
            Constructor();
            this.Write = text;
        }

        /// <summary>
        /// Spacing between letters.
        /// </summary>
        public float Spacing
        {
            get { return font.Spacing; }
            set
            {
                font.Spacing = value;
            }
        }

        public override string ToString()
        {
            return "Text " + Write + " : (" + Space.ToString() + ", " + Renderer.ToString() + ")";
        }
    }
}