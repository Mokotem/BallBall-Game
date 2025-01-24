﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using FriteCollection.Tools.TileMap;
using FriteCollection.Scripting;
using System;

namespace FriteCollection.UI;

public enum Extend
{
    Horizontal, Vertical, Full, None
}

public enum Bounds
{
    TopLeft, Top, TopRight,
    Left, Center, Right,
    BottomLeft, Bottom, BottomRight
}

public struct Vector
{
    public int i;
    public int j;

    public static Vector Zero => new Vector(0, 0);

    public Vector(int width, int height)
    {
        this.i = width;
        this.j = height;
    }

    public Vector()
    {
        i = 0;
        j = 0;
    }

    public static Vector operator + (Vector v1, Vector v2) => new Vector(v1.i + v2.i, v1.j + v2.j);
    public static Vector operator -(Vector v1, Vector v2) => new Vector(v1.i - v2.i, v1.j - v2.j);
    public static Vector operator *(Vector v1, Vector v2) => new Vector(v1.i * v2.i, v1.j * v2.j);
    public static Vector operator /(Vector v1, Vector v2) => new Vector(v1.i / v2.i, v1.j / v2.j);
}

public class Space
{
    public Extend Extend = Extend.None;
    public Vector Position = Vector.Zero;
    public Vector Scale = Vector.Zero;
    public Bounds Origin;

    public Space(Bounds origin, Extend extend)
    {
        this.Origin = origin;
        this.Extend = extend;
    }

    public Space(Bounds origin, Extend extend, Vector scale)
    {
        this.Origin = origin;
        this.Extend = extend;
        this.Scale = scale;
    }

    public Space(Bounds origin, Extend extend, Vector scale, Vector position)
    {
        this.Origin = origin;
        this.Extend = extend;
        this.Scale = scale;
        this.Position = position;
    }
}

public abstract class UI : IDisposable
{
    public virtual void Dispose()
    {

    }

    private protected bool _active = true;
    public delegate void Procedure();
    private protected UI papa;

    public Vector Scale => space.Scale;

    public bool Active
    {
        get => _active;
        set => _active = value;
    }

    private protected Rectangle rect;

    public virtual Vector Position
    {
        get => space.Position;
        set
        {
            this.space.Position = value;
            ApplyPosition(papa is null ? Screen : papa.Rectangle);
        }
    }

    public virtual int PositionY
    {
        get => rect.Y;
        set
        {
            this.space.Position.j = value;
            ApplyPosition(papa is null ? Screen : papa.Rectangle);
        }
    }

    private protected List<UI> childs = new List<UI>();
    public void Add(UI element)
    {
        childs.Add(element);
    }

    public Rectangle Rectangle => rect;
    public static readonly Rectangle Screen = new Rectangle
(0, 0, GetSeetings.Settings.GameFixeWidth * GetSeetings.Settings.UICoef,
       GetSeetings.Settings.GameFixeHeight * GetSeetings.Settings.UICoef);

    public Graphics.Color Color = Graphics.Color.White;

    private protected Space space;

    private protected void ApplyScale(Rectangle parent)
    {
        switch (space.Extend)
        {
            case Extend.None:
                rect.Width = 0;
                rect.Height = 0;
                break;

            case Extend.Full:
                rect.Width = parent.Width;
                rect.Height = parent.Height;
                break;

            case Extend.Horizontal:
                rect.Width = parent.Width;
                rect.Height = 0;
                break;

            case Extend.Vertical:
                rect.Width = 0;
                rect.Height = parent.Height;
                break;
        }

        rect.Width += space.Scale.i;
        rect.Height += space.Scale.j;
    }

    private protected void ApplyPosition(Rectangle parent)
    {
        switch ((int)space.Origin % 3)
        {
            default:
                rect.X = parent.X;
                break;

            case 1:
                rect.X = parent.X + (parent.Width / 2) - (rect.Width / 2);
                break;

            case 2:
                rect.X = parent.X + parent.Width - rect.Width;
                break;
        }

        switch ((int)space.Origin / 3)
        {
            default:
                rect.Y = parent.Y;
                break;

            case 1:
                rect.Y = parent.Y + (parent.Height / 2) - (rect.Height / 2);
                break;

            case 2:
                rect.Y = parent.Y + parent.Height - rect.Height;
                break;
        }

        rect.X += space.Position.i;
        rect.Y += space.Position.j;
    }

    private protected void ApplySpace(Rectangle parent)
    {
        ApplyScale(parent);
        ApplyPosition(parent);
    }

    public virtual void Draw() { }
}

public abstract class ButtonCore : UI
{
    private Text titleText;
    private FriteModel.MonoGame instance => GameManager.instance;

    private bool clic => _active && Input.Input.Mouse.Left
&& IsInRange(instance.mouseClickedPosition)
&& IsInRange(instance.mousePosition)
&& Time.TargetTimer >= 0.2f && !GameManager.instance.charging;

    private bool IsInRange(Vector pos) => 
        pos.i >= papa.Rectangle.X - 1 && pos.i < papa.Rectangle.X + papa.Rectangle.Width + 1
     && pos.j >= papa.Rectangle.Y - 1 && pos.j < papa.Rectangle.Y + papa.Rectangle.Height + 1;

    private protected bool selected = false;
    private bool previousClic = false;

    private protected Procedure _fonction;

    public override void Dispose()
    {
        instance.buttons.Remove(this);
        _fonction = null;
        papa.Dispose();
    }

    public void Update()
    {
        if (_active)
        {
            selected = IsInRange(instance.mousePosition);

            if (clic)
            {
                papa.Color = new Graphics.Color(0.7f, 0.7f, 0.7f);
                if (titleText is not null)
                    titleText.Color = new Graphics.Color(0.7f, 0.7f, 0.7f);
            }
            else
            {
                papa.Color = Graphics.Color.White;
                if (titleText is not null)
                    titleText.Color = Graphics.Color.White;

                if (previousClic == true && _fonction is not null && IsInRange(instance.mousePosition))
                {
                    instance.previousMouseLeft = false;
                    _fonction();
                }
            }
        }

        previousClic = clic;
    }

    public override int PositionY
    {
        get => papa.Position.j;
        set
        {
            papa.PositionY = value;
            titleText.SetPar(papa.Rectangle);
            titleText.PositionY = 0;
        }
    }

    public string Edit
    {
        set => titleText.Edit = value;
    }

    public ButtonCore(TileSet tileset, Space space, UI parent)
    {
        papa = new Panel(tileset, space, parent);
        instance.buttons.Add(this);
    }

    public ButtonCore(Texture2D image, Space space, UI parent)
    {
        papa = new Image(image, space, parent);
        instance.buttons.Add(this);
    }

    public ButtonCore(string title, TileSet tileset, Space space, UI parent)
    {
        papa = new Panel(tileset, space, parent);
        titleText = new Text(title, new Space(Bounds.Center, Extend.Full), papa);
        papa.Add(titleText);
        instance.buttons.Add(this);
    }

    public ButtonCore(string title, Texture2D image, Space space, UI parent)
    {
        papa = new Image(image, space, parent);
        titleText = new Text(title, new Space(Bounds.Center, Extend.Full), papa);
        papa.Add(titleText);
        instance.buttons.Add(this);
    }

    public ButtonCore(TileSet tileset, Space space)
    {
        papa = new Panel(tileset, space);
        instance.buttons.Add(this);
    }

    public ButtonCore(Texture2D image, Space space)
    {
        papa = new Image(image, space);
        instance.buttons.Add(this);
    }

    public ButtonCore(string title, TileSet tileset, Space space)
    {
        papa = new Panel(tileset, space);
        titleText = new Text(title, new Space(Bounds.Center, Extend.Full), papa);
        papa.Add(titleText);
        instance.buttons.Add(this);
    }

    public ButtonCore(string title, Texture2D image, Space space)
    {
        papa = new Image(image, space);
        titleText = new Text(title, new Space(Bounds.Center, Extend.Full), papa);
        papa.Add(titleText);
        instance.buttons.Add(this);
    }
}

public class Toggle : ButtonCore
{
    public Procedure OnActivate;
    public Procedure OnDeactivate;

    private bool _on = false;

    public void Set(bool value)
    {
        _on = value;
    }

    public bool On => _on;

    public Toggle[] voisins = new Toggle[0];
    public void Deactivate()
    {
        _on = false;
        OnDeactivate();
    }

    public Toggle(TileSet tileset, Space space, UI parent) : base(tileset, space, parent) { _fonction = OnClic; }
    public Toggle(Texture2D image, Space space, UI parent) : base(image, space, parent) { _fonction = OnClic; }
    public Toggle(string title, TileSet tileset, Space space, UI parent) : base(title, tileset, space, parent) { _fonction = OnClic; }
    public Toggle(string title, Texture2D image, Space space, UI parent) : base(title, image, space, parent) { _fonction = OnClic; }
    public Toggle(TileSet tileset, Space space) : base(tileset, space) { _fonction = OnClic; }
    public Toggle(Texture2D image, Space space) : base(image, space) { _fonction = OnClic; }
    public Toggle(string title, TileSet tileset, Space space) : base(title, tileset, space) { _fonction = OnClic; }
    public Toggle(string title, Texture2D image, Space space) : base(title, image, space) { _fonction = OnClic; }


    private void OnClic()
    {
        foreach(Toggle tog in voisins)
            tog.Deactivate();
        _on = !_on;
        if (_on)
            OnActivate();
        else
            OnDeactivate();
    }

    public override void Draw()
    {
        if (_active)
        {
            if (_on)
            {
                GameManager.instance.SpriteBatch.Draw
                    (Entity.Renderer.DefaultTexture, new Rectangle(
                        papa.Rectangle.X - 1,
                    papa.Rectangle.Y - 1, papa.Rectangle.Width + 2, papa.Rectangle.Height + 2),
                    Microsoft.Xna.Framework.Color.White);
            }
            papa.Draw();
        }
    }
}

public class Button : ButtonCore
{
    public Button(TileSet tileset, Space space, UI parent) : base(tileset, space, parent) { }
    public Button(Texture2D image, Space space, UI parent) : base(image, space, parent) { }
    public Button(string title, TileSet tileset, Space space, UI parent) : base(title, tileset, space, parent) { }
    public Button(string title, Texture2D image, Space space, UI parent) : base(title, image, space, parent) { }
    public Button(TileSet tileset, Space space) : base(tileset, space) { }
    public Button(Texture2D image, Space space) : base(image, space) { }
    public Button(string title, TileSet tileset, Space space) : base(title, tileset, space) { }
    public Button(string title, Texture2D image, Space space) : base(title, image, space) { }

    public Procedure Fonction
    {
        set
        {
            base._fonction = value;
        }
    }

    public override void Draw()
    {
        if (_active)
        {
            if (selected && !Input.Input.Mouse.Left)
            {
                GameManager.instance.SpriteBatch.Draw
                    (Entity.Renderer.DefaultTexture, new Rectangle(
                        papa.Rectangle.X - 1,
                    papa.Rectangle.Y - 1, papa.Rectangle.Width + 2, papa.Rectangle.Height + 2),
                    Microsoft.Xna.Framework.Color.White);
            }
            papa.Draw();
        }
    }

    public override void Dispose()
    {
        if (papa is not null)
        {
            papa.Dispose();
            papa = null;
        }
    }
}

public class Image : UI
{
    private Texture2D image;

    public Texture2D Texture
    {
        get => image;
        set => image = value;
    }

    public Image(Texture2D image, Space space)
    {
        this.image = image;
        this.space = space;
        base.ApplyScale(Screen);
        if (space.Extend == Extend.Horizontal || space.Extend == Extend.None)
            rect.Height = image.Height;
        if (space.Extend == Extend.Vertical || space.Extend == Extend.None)
            rect.Width = image.Width;
        base.ApplyPosition(Screen);
    }

    public Image(Texture2D image, Space space, UI parent)
    {
        this.image = image;
        this.space = space;
        base.ApplyScale(parent.Rectangle);
        if (space.Extend == Extend.Horizontal || space.Extend == Extend.None)
            rect.Height = image.Height;
        if (space.Extend == Extend.Vertical || space.Extend == Extend.None)
            rect.Width = image.Width;
        base.ApplyPosition(parent.Rectangle);
    }

    public override void Draw()
    {
        if (_active)
        {
            GameManager.instance.SpriteBatch.Draw(image, rect,
            new(this.Color.RGB.R, this.Color.RGB.G, this.Color.RGB.B));
            foreach (UI element in childs)
                element.Draw();
        }
    }

    public override void Dispose()
    {
        if (image is not null)
        {
            image.Dispose();
            image = null;
        }
    }
}

public class Text : UI
{
    private Rectangle par;
    private string text;

    public string Edit
    {
        get => text;
        set
        {
            if (value != text)
            {
                int posy = rect.Y;
                this.text = value;
                ApplyScale(par);
                ApplyText(value);
                ApplyPosition(par);
                rect.Y = posy;
            }
        }
    }

    public void SetPar(Rectangle rect)
    {
        par = rect;
    }

    public override int PositionY
    {
        get => papa.Position.j;
        set
        {
            space.Position.j = value;
            ApplyPosition(par);
        }
    }

    private void ApplyText(string input)
    {
        text = "";
        string[] txt = input.Split(" ");
        string result = "";
        if (txt.Length == 1)
        {
            text = txt[0];
        }
        else
        {
            for (int i = 0; i < txt.Length; i++)
            {
                result += txt[i] + (i == txt.Length - 1 ? "" : " ");
                if ((result + (i + 1 < txt.Length ? txt[i + 1] : "")).Length * 4
                    > rect.Width)
                {
                    text += result + (i == txt.Length - 1 ? "" : "\n");
                    result = "";
                }
            }
        }

        space.Scale.j = 0;

        if ((int)space.Origin % 3 == 1)
            rect.Width = (int)(input.Length * 4);
        if ((int)space.Origin / 3 == 1)
            rect.Height = 12;

        text = input;
    }

    public Text(string txt, Space space)
    {
        this.space = space;
        base.ApplyScale(Screen);
        ApplyText(txt);
        base.ApplyPosition(Screen);
        par = Screen;
    }

    public Text(string txt, Space space, UI parent)
    {
        this.space = space;
        base.ApplyScale(Screen);
        ApplyText(txt);
        base.ApplyPosition(parent.Rectangle);
        par = parent.Rectangle;
    }

    public override void Draw()
    {
        if (_active)
        {
            foreach (Vector2 r in new Vector2[1]
            {
                new(1, 1)
            })
            {
                GameManager.instance.SpriteBatch.DrawString
                (GameManager.GameFont, text, new Vector2(rect.X + r.X , rect.Y + r.Y),
                Microsoft.Xna.Framework.Color.Black, 0, Vector2.Zero, 1f,
                SpriteEffects.None, 0);
            }
            GameManager.instance.SpriteBatch.DrawString
                (GameManager.GameFont, text, new Vector2(rect.X, rect.Y),
                new(this.Color.RGB.R, this.Color.RGB.G, this.Color.RGB.B), 0, Vector2.Zero, 1f,
                SpriteEffects.None, 0);
        }
    }
}


public class Panel : UI, IDisposable
{
    private TileSet tileSet;
    private Texture2D texture;
    private RenderTarget2D rt;

    private void CreateTexture()
    {
        if (rect.Width < tileSet.TileSize.width || rect.Height < tileSet.TileSize.height)
            throw new System.Exception("space trop petit");

        GraphicsDevice gd = GameManager.instance.GraphicsDevice;
        SpriteBatch sb = GameManager.instance.SpriteBatch;
        rt = new RenderTarget2D(gd, rect.Width, rect.Height);

        gd.SetRenderTarget(rt);
        gd.Clear(new(this.Color.RGB.R, this.Color.RGB.G, this.Color.RGB.B));
        sb.Begin(samplerState: SamplerState.PointClamp);

        for (int x = 0; x < 3; x++)
        {
            int width;
            if (x == 0 || x == 2)
                width = tileSet.TileSize.width;
            else
                width = rect.Width - (tileSet.TileSize.width * GetSeetings.Settings.UICoef);

            int posX;
            if (x == 0)
                posX = 0;
            else if (x == 1)
                posX = tileSet.TileSize.width;
            else
                posX = rect.Width - tileSet.TileSize.width;


            for (int y = 0; y < 3; y++)
            {
                int height;
                if (y == 0 || y == 2)
                    height = tileSet.TileSize.height;
                else
                    height = rect.Height - (tileSet.TileSize.height * GetSeetings.Settings.UICoef);

                int posY;
                if (y == 0)
                    posY = 0;
                else if (y == 1)
                    posY = tileSet.TileSize.height;
                else
                    posY = rect.Height - tileSet.TileSize.height;

                sb.Draw(tileSet.Texture,
                    new Rectangle(posX, posY, width, height),
                    tileSet.GetRectangle(x + (y * 3)),
                    new(this.Color.RGB.R, this.Color.RGB.G, this.Color.RGB.B));
            }
        }

        sb.End();
        texture = rt;
    }

    public override int PositionY
    {
        get => papa.Position.j;
        set
        {
            space.Position.j = value;
            ApplyPosition(papa is null ? Screen : papa.Rectangle);
        }
    }

    public void Clear()
    {
        foreach(UI c in childs)
        {
            c.Active = false;
        }
        this.childs.Clear();
    }

    public Panel(Space space)
    {
        this.space = space;
        ApplySpace(Screen);
    }

    public Panel(Space space, UI parent)
    {
        this.space = space;
        ApplySpace(parent.Rectangle);
    }

    public Panel(TileSet tileSet, Space space)
    {
        this.space = space;
        this.tileSet = tileSet;
        ApplySpace(Screen);
        CreateTexture();
    }

    public Panel(TileSet tileSet, Space space, UI parent)
    {
        this.space = space;
        this.tileSet = tileSet;
        ApplySpace(parent.Rectangle);
        CreateTexture();
    }

    public override void Draw()
    {
        if (_active)
        {
            if (texture != null)
                GameManager.instance.SpriteBatch.Draw
                 (texture, rect, new(this.Color.RGB.R, this.Color.RGB.G, this.Color.RGB.B));
            foreach (UI element in childs)
                element.Draw();
        }
    }

    public override void Dispose()
    {
        if (texture is not null)
        texture.Dispose();
        if (rt is not null)
            rt.Dispose();
        rt = null;
        texture = null;
        foreach (UI ui in childs)
        {
            ui.Dispose();
        }
    }
}
