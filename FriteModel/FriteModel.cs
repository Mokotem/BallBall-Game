using System;
using System.Linq;
using System.Reflection;
using FriteCollection.Audio;
using FriteCollection.Entity;
using FriteCollection.Scripting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RocketLike;

namespace FriteModel;

public class MonoGame : Game
{
    private GraphicsDeviceManager graphics;
    public SpriteBatch SpriteBatch;
    private Settings onSettings;

    public List<FriteCollection.UI.ButtonCore> buttons = new List<FriteCollection.UI.ButtonCore>();

    private bool changingScene = false;
    public bool charging = false;
    public Effect oFilter;
    public SoundEffect shutsfx;

    public MonoGame()
    {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        graphics = new GraphicsDeviceManager(this);
        Window.AllowAltF4 = false;
        Window.AllowUserResizing = false;
    }

    public float _timer = 0f, _delta = 0f, _targetTimer = 0f;
    public bool oldFilter = false;
    public void SetFps(byte fps)
    {
        GetSeetings.Settings.FPS = fps;
        TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0f / fps);
    }

    public bool FPS60 => GetSeetings.Settings.FPS < 100;

    private static Texture2D CreateTexture(GraphicsDevice device, int w, int h, Color color)
    {
        Texture2D texture = new Texture2D(device, w, h);

        Color[] data = new Color[w * h];
        for (int pixel = 0; pixel < w * h; pixel++)
        {
            data[pixel] = color;
        }

        texture.SetData(data);

        return texture;
    }

    public List<Executable> CurrentExecutables = new List<Executable>();
    public FriteCollection.UI.Vector
        mouseClickedPosition = FriteCollection.UI.Vector.Zero,
        mousePosition = FriteCollection.UI.Vector.Zero;

    public FriteCollection.Tools.Shaders.Shader CurrentShader = null;

    protected override void Initialize()
    {
        Music.Volume = 0.5f;

        GameData.firstConnection = true;

        Window.AllowUserResizing = false;
        onSettings = new RocketLike.Settings();
        onSettings.SetGameSettings();
        Window.Title = GetSeetings.Settings.WindowName;

        graphics.PreferredBackBufferWidth = GetSeetings.Settings.WindowWidth;
        graphics.PreferredBackBufferHeight = GetSeetings.Settings.WindowHeight;
        FullScreen = GetSeetings.Settings.FullScreen;

        TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0f / GetSeetings.Settings.FPS);
        screenBounds = _bf.CreateBounds(GetSeetings.Settings.GameFixeWidth, GetSeetings.Settings.GameFixeHeight);

        shutsfx = Open.SoundEffect("Sfx/start");
        Renderer.DefaultTexture = CreateTexture(GraphicsDevice, 2, 2, Color.White);

        base.Initialize();
        UpdateScriptToScene();
    }


    public static Music music;

    public void UpdateScriptToScene()
    {
        Content.Unload();
        Content.Dispose();

        if (replay is not null) DisposeReplay();
        replay = new List<Texture2D>();

        Content = new Microsoft.Xna.Framework.Content.ContentManager(Services);
        Content.RootDirectory = "Content";

        if (oldFilter)
        {
            oFilter = Content.Load<Effect>("Shaders/oldFilter");
        }

        if (music is not null)
        {
            music.Dispose();
        }
        shutsfx.Dispose();
        shutsfx = Open.SoundEffect("Sfx/start");
        shutsfx.Volume = 0.2f;
        if (GameManager.CurrentScene == Scenes.Game)
        {
            music = Open.Music("Game/587501_Last-Tile-");
            Music.Volume = 0.2f;
            music.Start();
        }
        else
        {
            music = Open.Music("Game/363931_Sick_Life_Inc.");
            Music.Volume = 0.4f;
            music.Start();
        }
        changingScene = true;
        HitBox.ClearHitboxes();
        if (SpriteBatch is not null)
            SpriteBatch.Dispose();

        GameManager.GameFont = Open.Font("Game/fritefont");

        SpriteBatch = new SpriteBatch(GraphicsDevice);

        buttons.Clear();

        foreach (Executable exe in CurrentExecutables)
        {
            if (exe is Clone)
                (exe as Clone).Destroy();
            exe.Dispose();
        }

        CurrentExecutables.Clear();

        Camera.Position = new FriteCollection.Entity.Vector(0, 0);
        Screen.backGround = new FriteCollection.Graphics.Color(0.1f, 0.2f, 0.3f);

        _timer = 0;
        _targetTimer = 0;

        GameManager.instance = null;
        GameManager.instance = this;

        Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();
        var childTypesScript = allTypes.Where(t => t.IsSubclassOf(typeof(Script)) && !t.IsAbstract);

        foreach (Type type in childTypesScript)
        {
            Script instance = (Script)Activator.CreateInstance(type);
            if (instance.AttributedScenes == GameManager.CurrentScene)
            {
                CurrentExecutables.Add(instance);
            }
            else instance = null;
        }

        foreach (Executable script in CurrentExecutables.Copy())
        {
            script.BeforeStart();
        }

        foreach (Executable script in CurrentExecutables.Copy())
        {
            script.Start();
        }

        foreach (Executable script in CurrentExecutables.Copy())
        {
            script.AfterStart();
        }

        foreach (Executable script in CurrentExecutables.Copy())
        {
            script.BeforeUpdate();
        }

        foreach (Executable script in CurrentExecutables.Copy())
        {
            script.Update();
        }

        foreach (Executable script in CurrentExecutables.Copy())
        {
            script.AfterUpdate();
        }
        scaleY = 2f;
        GC.Collect();
    }

    private bool _fullScreen;
    public float aspectRatio;
    DisplayMode display => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;

    public bool FullScreen
    {
        get
        {
            return _fullScreen;
        }
        set
        {
            _fullScreen = value;

            renderTarget = new RenderTarget2D(GraphicsDevice, GetSeetings.Settings.GameFixeWidth, GetSeetings.Settings.GameFixeHeight);

            if (value == false)
            {
                float ratioW = GetSeetings.Settings.WindowWidth / (float)GetSeetings.Settings.GameFixeWidth;
                float ratioH = GetSeetings.Settings.WindowHeight / (float)GetSeetings.Settings.GameFixeHeight;

                aspectRatio = MathF.Min(ratioW, ratioH);

                targetGameRectangle = new Rectangle
                (
                    (int)((GetSeetings.Settings.WindowWidth - (GetSeetings.Settings.GameFixeWidth * aspectRatio)) / 2f),
                    (int)((GetSeetings.Settings.WindowHeight - (GetSeetings.Settings.GameFixeHeight * aspectRatio)) / 2f),
                    (int)(GetSeetings.Settings.GameFixeWidth * aspectRatio),
                    (int)(GetSeetings.Settings.GameFixeHeight * aspectRatio)
                );
            }
            else
            {
                float ratioW = display.Width / (float)GetSeetings.Settings.GameFixeWidth;
                float ratioH = display.Height / (float)GetSeetings.Settings.GameFixeHeight;

                aspectRatio = MathF.Min(ratioW, ratioH);

                targetGameRectangle = new Rectangle
                (
                    (int)((display.Width - GetSeetings.Settings.GameFixeWidth * aspectRatio) / 2f),
                    (int)((display.Height - GetSeetings.Settings.GameFixeHeight * aspectRatio) / 2f),
                    (int)(GetSeetings.Settings.GameFixeWidth * aspectRatio),
                    (int)(GetSeetings.Settings.GameFixeHeight * aspectRatio)
                );
            }

            renderTargetUI = new RenderTarget2D(GraphicsDevice, renderTarget.Width, renderTarget.Height);
            UIscreenBounds = _bf.CreateBounds(renderTarget.Width, renderTarget.Height);
            graphics.HardwareModeSwitch = !value;
            graphics.IsFullScreen = value;
            graphics.ApplyChanges();
        }
    }

    public Rectangle targetGameRectangle;

    private BoundFunc _bf = new BoundFunc();
    public Vector[] screenBounds, UIscreenBounds;
    public bool previousMouseLeft;
    public float scaleY = 2f;

    public const byte REPLAY_FPS = 30;
    public const ushort REPLAY_LENGHT = REPLAY_FPS * 5;
    private List<Texture2D> replay;
    public Texture2D[] Replay => replay.ToArray();

    protected override void Update(GameTime gameTime)
    {
        _timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000f;
        _targetTimer += (1f / GetSeetings.Settings.FPS);
        _delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000f;

        MouseState mstate = Mouse.GetState();
        FriteCollection.UI.Vector v = new(
            (mstate.Position.X) * GetSeetings.Settings.GameFixeWidth / (_fullScreen ? display.Width : GetSeetings.Settings.WindowWidth),
            (mstate.Position.Y) * GetSeetings.Settings.GameFixeHeight / (_fullScreen ? display.Height : GetSeetings.Settings.WindowHeight));
        if (Mouse.GetState().LeftButton == ButtonState.Pressed && !previousMouseLeft)
        {
            mouseClickedPosition = v;
        }
        mousePosition = v;
        if (!this.IsActive)
        {
            mouseClickedPosition = new(-10, -10);
            mousePosition = new(-10, -10);
        }

        if (!changingScene)
        {
            if (!changingScene)
            {
                foreach (Executable script in CurrentExecutables.Copy())
                {
                    script.BeforeUpdate();
                    if (changingScene) break;
                }
            }
            if (!changingScene)
            {
                foreach (Executable script in CurrentExecutables.Copy())
                {
                    script.Update();
                    if (changingScene) break;
                }
            }
            if (!changingScene)
            {
                foreach (FriteCollection.UI.ButtonCore but in buttons)
                {
                    if (!changingScene)
                    {
                        but.Update();
                    }
                    else break;
                }
            }
            if (!changingScene)
            {
                foreach (Executable script in CurrentExecutables.Copy())
                {
                    script.AfterUpdate();
                    if (changingScene) break;
                }
            }
        }

        previousMouseLeft = Mouse.GetState().LeftButton == ButtonState.Pressed;
        z = MathF.Min(1, _targetTimer * 10);
        base.Update(gameTime);
    }

    RenderTarget2D renderTarget, renderTargetUI;
    float z;
    private int SX => GetSeetings.Settings.GameFixeWidth;
    private int SY => GetSeetings.Settings.GameFixeHeight;
    byte av = 0;

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(renderTarget);
        GraphicsDevice.Clear(
            new Color(Screen.backGround.RGB.R, Screen.backGround.RGB.G, Screen.backGround.RGB.B));
        SpriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
        foreach (Executable script in CurrentExecutables)
        {
                script.BeforeDraw();
        }
            foreach (Executable script in CurrentExecutables)
            {
                    script.Draw();
            }
            foreach (Executable script in CurrentExecutables)
            {
                    script.AfterDraw();
            }
        SpriteBatch.End();

        if (!changingScene)
        {
            if (GameData.replay && GameManager.CurrentScene == Scenes.Game && _targetTimer - Ball.LastGoal <= 1)
            {
                av++;
                if (av > GetSeetings.Settings.FPS / REPLAY_FPS)
                {
                    Texture2D tex = new Texture2D(GraphicsDevice, SX / 2, SY / 2 - 8);
                    Color[] pd = new Color[SX * SY];
                    renderTarget.GetData(pd);

                    Color[] pixelFinal = new Color[SX * (SY - 16) / 4];
                    for (int i = 0; i < SX * (SY - 16) / 4; i++)
                    {
                        pixelFinal[i] = pd[(i * 2) + ((((i) * 2) / SX) * SX) + (SX * 16)];
                    }
                    tex.SetData(pixelFinal);

                    replay.Add(tex);

                    if (replay.Count > REPLAY_LENGHT)
                    {
                        replay[0].Dispose();
                        replay.RemoveIndex(0);
                    }

                    av = 0;
                }
            }
        }

        if (SpriteBatch.IsDisposed == false)
        {
            foreach (Executable script in CurrentExecutables)
            {
                script.BeforeDraw(ref SpriteBatch);
            }
            foreach (Executable script in CurrentExecutables)
            {
                script.Draw(ref SpriteBatch);
            }
            foreach (Executable script in CurrentExecutables)
            {
                script.AfterDraw(ref SpriteBatch);
            }
        }

        SpriteBatch.Begin(blendState: BlendState.Additive, samplerState: SamplerState.PointClamp);
        foreach (Executable script in CurrentExecutables)
        {
            if (!changingScene)
            {
                script.DrawAdditive();
            }
        }
        SpriteBatch.End();

        GraphicsDevice.SetRenderTarget(renderTargetUI);

        GraphicsDevice.Clear(Color.Black);
        if (CurrentShader is null || CurrentShader.GetEffect.IsDisposed == true || FriteCollection.Tools.Shaders.Shader.Stopped)
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        else
        {
            CurrentShader.ApllySettings();
            SpriteBatch.Begin(effect: CurrentShader.GetEffect, samplerState: SamplerState.PointClamp);
        }
        SpriteBatch.Draw(
            renderTarget,
            new Rectangle(0, 0, renderTarget.Width, renderTarget.Height),
            Color.White);

        SpriteBatch.End();
        SpriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
        foreach (Executable script in CurrentExecutables)
        {
            script.DrawUI();
        }
        SpriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        if (oldFilter)
        {
            oFilter.Parameters["value"].SetValue(_timer / 25f);
            SpriteBatch.Begin(effect: oFilter, samplerState: SamplerState.PointClamp);
        }
        else
        {
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        }
        if (scaleY < 1.1f)
        {
            if (scaleY > 0)
            {
                SpriteBatch.Draw(renderTargetUI, new Rectangle(
                    0,
                    (int)(targetGameRectangle.Height * (1 - scaleY) / 2),
                    targetGameRectangle.Width,
                    (int)(targetGameRectangle.Height * scaleY)
                    ), Color.White);
            }
        }
        else
            SpriteBatch.Draw(renderTargetUI, targetGameRectangle, Color.White * z);
        SpriteBatch.End();

        changingScene = false;
        charging = false;
        base.Draw(gameTime);
    }

    public void DisposeReplay()
    {
        foreach (Texture2D tex in replay)
        {
            tex.Dispose();
        }
        replay.Clear();
    }
}