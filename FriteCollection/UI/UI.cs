using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using FriteCollection.Tools.TileMap;
using FriteCollection.Scripting;
using Microsoft.Xna.Framework.Input;

namespace FriteCollection.UI
{
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

    public class Vector
    {
        public int i;
        public int j;

        public static readonly Vector Zero = new Vector(0, 0);

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

    public abstract class UI
    {
        private protected bool _active = true;
        public delegate void Procedure();

        public bool Active
        {
            get => _active;
            set => _active = value;
        }

        private protected Rectangle rect;

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
        private protected UI papa;
        private FriteModel.MonoGame instance => FriteModel.MonoGame.instance;

        private bool clic => Input.Input.Mouse.Left
    && IsInRange(instance.mouseClickedPosition)
    && IsInRange(mousePos);

        private bool IsInRange(Vector pos) => pos.i >= papa.Rectangle.X && pos.i < papa.Rectangle.X + papa.Rectangle.Width
                && pos.j >= papa.Rectangle.Y && pos.j < papa.Rectangle.Y + papa.Rectangle.Height;
        private Vector mousePos
        {
            get
            {
                float ratio = instance.aspectRatio / 2f;
                return new Vector
                (
                    (int)((float)(Mouse.GetState().X - instance.targetGameRectangle.Location.X)
                    / ratio),
                    (int)((float)(Mouse.GetState().Y - instance.targetGameRectangle.Location.Y)
                    / ratio)
                );
            }
        }

        private protected bool selected = false;
        private bool previousClic = false;

        private protected Procedure _fonction;

        public void Update()
        {
            if (_active)
            {
                selected = IsInRange(mousePos);

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

                    if (previousClic == true && _fonction is not null)
                        _fonction();
                }
            }

            previousClic = clic;
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
                    FriteModel.MonoGame.instance.SpriteBatch.Draw
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
                    FriteModel.MonoGame.instance.SpriteBatch.Draw
                        (Entity.Renderer.DefaultTexture, new Rectangle(
                            papa.Rectangle.X - 1,
                        papa.Rectangle.Y - 1, papa.Rectangle.Width + 2, papa.Rectangle.Height + 2),
                        Microsoft.Xna.Framework.Color.White);
                }
                papa.Draw();
            }
        }
    }

    public class Image : UI
    {
        private Texture2D image;

        public Vector Scale
        {
            get => new Vector(rect.Width, rect.Height);
            set
            {
                rect.Width = value.i;
                rect.Height = value.j;
            }
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
                FriteModel.MonoGame.instance.SpriteBatch.Draw(image, rect,
                new(this.Color.RGB.R, this.Color.RGB.G, this.Color.RGB.B));
                foreach (UI element in childs)
                    element.Draw();
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
                    this.text = value;
                    ApplyScale(par);
                    ApplyText(value);
                    ApplyPosition(par);
                }
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
                    if ((result + (i + 1 < txt.Length ? txt[i + 1] : "")).Length * 5
                        > rect.Width)
                    {
                        text += result + (i == txt.Length - 1 ? "" : "\n");
                        result = "";
                    }
                }
            }

            space.Scale.j = 0;

            if ((int)space.Origin % 3 == 1)
                rect.Width = (int)text.Length * 5;
            if ((int)space.Origin / 3 == 1)
                rect.Height = (int)GameManager.GameFont.MeasureString(text).Y;

            if (text == "")
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
                    FriteModel.MonoGame.instance.SpriteBatch.DrawString
                    (GameManager.GameFont, text, new Vector2(rect.X + r.X , rect.Y + r.Y),
                    Microsoft.Xna.Framework.Color.Black, 0, Vector2.Zero, 0.5f,
                    SpriteEffects.None, 0);
                }
                FriteModel.MonoGame.instance.SpriteBatch.DrawString
                    (GameManager.GameFont, text, new Vector2(rect.X, rect.Y),
                    new(this.Color.RGB.R, this.Color.RGB.G, this.Color.RGB.B), 0, Vector2.Zero, 0.5f,
                    SpriteEffects.None, 0);
            }
        }
    }
    

    public class Panel : UI
    {
        private TileSet tileSet;
        private Texture2D texture;

        private void CreateTexture()
        {
            if (rect.Width < tileSet.TileSize.width || rect.Height < tileSet.TileSize.height)
                throw new System.Exception("space trop petit");

            GraphicsDevice gd = FriteModel.MonoGame.instance.GraphicsDevice;
            SpriteBatch sb = FriteModel.MonoGame.instance.SpriteBatch;
            RenderTarget2D rt = new RenderTarget2D(gd, rect.Width, rect.Height);

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
                    FriteModel.MonoGame.instance.SpriteBatch.Draw
                     (texture, rect, new(this.Color.RGB.R, this.Color.RGB.G, this.Color.RGB.B));
                foreach (UI element in childs)
                    element.Draw();
            }
        }
    }
}
