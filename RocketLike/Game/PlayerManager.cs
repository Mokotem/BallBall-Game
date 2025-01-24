using FriteCollection.Entity;
using FriteCollection.Scripting;
using FriteCollection.Graphics;
using FriteCollection.Tools.SpriteSheet;
using Microsoft.Xna.Framework.Graphics;
using FriteCollection.Math;
using FriteCollection.Tools.Pen;
using Microsoft.Xna.Framework.Input;
using FriteCollection.Audio;

namespace RocketLike;

public class PlayerManager : Script
{
    public const float GRAVITY = 288f;
    public const short floor = -108, roof = 108, walls = 220;
    public static SpriteSheet sheet;

    public static readonly Color ColorPlayer1 = Pico8.Green, Color2Player1 = Pico8.DarkGreen;
    public static readonly Color ColorPlayer2 = Pico8.Red, Color2Player2 = Pico8.DarkPurple;

    public static readonly System.Random random = new System.Random();

    private SoundEffect jet;

    public PlayerManager() : base(Scenes.Game, active: false) { }

    public Player p1, p2;
    Texture2D t;
    private ReplayManager replay;

    public override void Start()
    {
        t = Open.Texture("Game/PlayerManager/player");
        sheet = new SpriteSheet(t, 10, 10);
        jet = Open.SoundEffect("Sfx/jet");
        Player.foot = Open.SoundEffect("Sfx/foot");
        Player.foot.Volume = 0.5f;
        Player.ball = GameManager.GetScript("Ball") as Ball;
        Player.fx = GameManager.instance.Content.Load<Effect>("Shaders/CircleBarr");
        Screen.backGround = Pico8.Lavender;
        Layer = 1;
        p1 = new Player(MapManager.tilemap.GetPos(0, 5)[0], ColorPlayer1, Color2Player1,
            GameData.SAVE.i[0], GameData.SAVE.i[1], GameData.SAVE.i[2], GameData.SAVE.i[3], true);
        p2 = new Player(MapManager.tilemap.GetPos(1, 5)[0], ColorPlayer2, Color2Player2,
            GameData.SAVE.i[4], GameData.SAVE.i[5], GameData.SAVE.i[6], GameData.SAVE.i[7], false);
        Pen.GridOrigin = Bounds.Center;
        (GameManager.GetScript("StartAnimation") as StartAnimation).START += GameStart;
        Player.ball.BUT += But;

        replay = GameManager.GetScript("ReplayManager") as ReplayManager;
        replay.RESTART += OnRestart;
    }

    void OnRestart()
    {
        if (last > 7)
        {
            string m;
            do
            {
                m = "map" + (random.Next(12) + 1).ToString();
            }
            while (m == MapManager.map);
            MapManager.map = m;
            GameData.SAVE.t += (ulong)(Time.TargetTimer);
            GameManager.CurrentScene = Scenes.Game;
        }
        else
        {
            p1.Reset();
            p2.Reset();
        }
    }

    byte last;

    void But(bool sideRight, Ball.States state, bool csc, byte last, bool combo)
    {
        this.last = last;
        if (sideRight)
        {
            p2.locked = true;
        }
        else
        {
            p1.locked = true;
        }
    }

    void GameStart()
    {
        p1.locked = false;
        p2.locked = false;
    }

    bool pre = false;

    public override void BeforeUpdate()
    {
        Player.state = Keyboard.GetState();
    }

    public override void Update()
    {
        bool p1Col = p1.CheckCollisionBall();
        bool p2Col = p2.CheckCollisionBall();
        if (p1.Frapping && p2.Frapping && p1Col && p2Col)
        {
            if (random.Next(2) == 0)
            {
                p1.Bump(true);
                p2.Bump(false);
            }
            else
            {
                p1.Bump(false);
                p2.Bump(true);
            }
        }
        else
        {
            if (p1.Frapping)
            {
                p1.Bump(p1Col);
            }
            if (p2.Frapping)
            {
                p2.Bump(p2Col);
            }
        }

        if (!pre && (p1.Jetting || p2.Jetting))
        {
            jet.Play(loop: true);
        }
        else if (!p1.Jetting && !p2.Jetting)
        {
            jet.Stop();
        }

        pre = p1.Jetting || p2.Jetting;
    }

    public override void Dispose()
    {
        p1.Dispose();
        p2.Dispose();
        sheet.Dispose();
        Player.fx.Dispose();
        Player.foot.Dispose();
        jet.Dispose();
        Player.fx = null;
        sheet = null;
        jet = null;
        p1 = null;
        p2 = null;
        t.Dispose();
        Player.ball = null;
    }
}

public class Player : Clone
{
    private const float cayoteTime = 0.07f;
    private const float jumpForce = 185;
    private const float vitesseXMax = 170;
    private readonly int radius;
    private static readonly float gravity = PlayerManager.GRAVITY * 1.6f;
    public static KeyboardState state;

    public static SoundEffect foot;

    public static Ball ball;
    private readonly bool isGreen;

    private Keys up, left, right, tap;
    private bool Up => state.IsKeyDown(up) && !locked && GameManager.instance.IsActive;
    private bool Left => state.IsKeyDown(left) && !locked && GameManager.instance.IsActive;
    private bool Right => state.IsKeyDown(right) && !locked && GameManager.instance.IsActive;
    private bool Tap => state.IsKeyDown(tap) && !locked && GameManager.instance.IsActive;

    private readonly Object p = new Object();
    private readonly Color circleColor;
    private readonly Vector startPos;
    private Object _lock;
    public bool locked = true;
    private readonly byte sheetIndex;

    public Player(Vector pos, Color color, Color color2, Keys up, Keys left, Keys right, Keys tap, bool ig) : base()
    {
        radius = GameData.bigTapCircles ? 39 : 26;
        if (GameData.dalton)
        {
            sheetIndex = (byte)(ig ? 1 : 2);
        }
        else
            sheetIndex = 0;
        this.isGreen = ig;
        this.up = up;
        this.left = left;
        this.right = right;
        this.tap = tap;
        p.Renderer.Texture = PlayerManager.sheet[0, sheetIndex];
        hit = new HitBox.Rectangle(p.Space);
        hit.LockSize = new(8, 8);
        p.Space.Scale = new Vector(10, 10);
        if (GameData.dalton)
        {
            p.Renderer.Color = color + 0.3f;
        }
        else
        {
            p.Renderer.Color = color;
        }
        p.Space.Position = pos;
        startPos = pos;

        circleColor = color2;
        _lock = new Object();
        _lock.Space.Scale = new Vector(10, 10);
        _lock.Renderer.Texture = PlayerManager.sheet[4, sheetIndex];

        fx.Parameters["amount"].SetValue(power);
    }

    public void Reset()
    {
        p.Space.Position = startPos;
        vitesse = Vector.Zero;
        frappeCouldown = 0.5f;
        canBump = false;
        lastSpace = false;
        bumpDelay = 0f;
        canUp = false;
        power = 0;
        locked = true;
    }

    public override void Start()
    {
        Layer = 10;
        fx.Parameters["thick"].SetValue(0.1f);
        circleMesh = new Object();
        circleMesh.Space.Scale = new Vector(radius, radius);
        circleMesh.Renderer.Color = Pico8.White;
        power = maxpower;
    }

    Vector vitesse = Vector.Zero;
    private bool walledLeft = false, walledRight = false;

    private float cayoteTimeCount = 0f;

    public bool Frapping { get; private set; }
    private HitBox.Rectangle hit;
    private HitBox lastHit;
    bool grounded, preGrounded = false;
    float frappeCouldown = 0.5f;
    float circleAlpha = -1f;
    public bool CheckCollisionBall()
    {
        if (ball.Active)
        return Math.GetDistance(p.Space.Position, ball.b.Space.Position) < radius + 10
            && Ball.IsActive;
        else
            return false;
    }

    public void Bump(bool collision)
    {
        frappeCouldown = 0.3f;
        canBump = false;
        bumpDelay = 0.1f;
        if (collision)
        {
            ball.Collide(p.Space.Position, vitesse, isGreen);
            GameData.SAVE.nte++;
        }
        else if (Math.GetDistance(p.Space.Position, ball.b.Space.Position) < radius + 20)
        {
            GameData.SAVE.nta++;
        }
        vitesse = new Vector(0, 0);
    }

    private bool Collisions()
    {
        // bug de collision joueur
        walledLeft = false;
        walledRight = false;
        bool grounded = false;
        if (p.Space.Position.x >=PlayerManager.walls)
        {
            p.Space.Position.x = PlayerManager.walls;
            vitesse.x = 0;
            walledRight = true;
        }
        if (p.Space.Position.x <= -PlayerManager.walls)
        {
            p.Space.Position.x = -PlayerManager.walls;
            vitesse.x = 0;
            walledLeft = true;
        }
        if (p.Space.Position.y < PlayerManager.floor)
        {
            p.Space.Position.y = PlayerManager.floor;
            grounded = true;
            vitesse.y = 0;
        }
        if (p.Space.Position.y > PlayerManager.roof)
        {
            p.Space.Position.y = PlayerManager.roof;
            if (vitesse.y >= 0)
                vitesse.y *= -0.25f;
        }

        ushort ncolliders = 2;
        byte n = 0;
        while (ncolliders > 0 && n < 3)
        {
            if (hit.Check(out HitBox.Sides side, out HitBox collider, out ncolliders, tag: "plat"))
            {
                switch (side)
                {
                    case HitBox.Sides.Down:
                        if (vitesse.y <= 0)
                        {
                            p.Space.Position.y =
                                collider.PositionOffset.y + ((collider.LockSize.y + 8) / 2f);
                            grounded = true;
                            vitesse.y = 0;
                        }
                        break;

                    case HitBox.Sides.Up:
                        if (vitesse.y >= 0)
                        {
                            p.Space.Position.y =
                                collider.PositionOffset.y - ((collider.LockSize.y + 8) / 2f);
                            if (vitesse.y >= 0)
                                vitesse.y *= -0.25f;
                        }
                        break;

                    case HitBox.Sides.Left:
                        if (vitesse.x <= 0)
                        {
                            p.Renderer.Texture = p.Renderer.Texture = PlayerManager.sheet[0, sheetIndex];
                            p.Space.Position.x =
                                collider.PositionOffset.x + ((collider.LockSize.x + 8) / 2f);
                            vitesse.x = 0;
                            walledLeft = true;
                        }
                        break;

                    case HitBox.Sides.Right:
                        if (vitesse.x >= 0)
                        {
                            p.Renderer.Texture = p.Renderer.Texture = PlayerManager.sheet[0, sheetIndex];
                            p.Space.Position.x =
                            collider.PositionOffset.x - ((collider.LockSize.x + 8) / 2f);
                            vitesse.x = 0;
                            walledRight = true;
                        }
                        break;
                }

                if (collider is not null)
                    lastHit = collider;
            }

            if (lastHit is not null && hit.CheckWith(lastHit, out HitBox.Sides side2))
            {
                switch (side2)
                {
                    case HitBox.Sides.Left:
                        walledLeft = true;
                        break;

                    case HitBox.Sides.Right:
                        walledRight = true;
                        break;
                }
            }

            n++;
        }

        return grounded;
    }

    bool lastIsLeft = false;
    bool preLeft, preRight, canUp;

    bool canBump = false;
    bool lastSpace = false;

    private void AddX(float value)
    {
        if (!locked)
        {
            p.Renderer.Texture = PlayerManager.sheet[1, sheetIndex];
            if (value < 0)
                p.Space.Scale.x = -10;
            else
                p.Space.Scale.x = 10;
            vitesse.x += value * 1500 * Time.FrameTime;
            if (vitesse.x > vitesseXMax)
                vitesse.x = vitesseXMax;
            if (vitesse.x < -vitesseXMax)
                vitesse.x = -vitesseXMax;
        }
    }

    private void jump()
    {
        cayoteTimeCount = 1f;
        canUp = false;
        vitesse.y = jumpForce;
        foot.Play();
    }

    float bumpDelay = 0f;
    

    float partDelay = 1f;
    private bool CanFrappe => !lastSpace && canBump && frappeCouldown <= 0 && !locked;
    private float maxpower = 0.8f;
    private bool jetting = false;

    public bool Jetting => jetting;

    public override void BeforeUpdate()
    {
        jetting = false;
        circleAlpha += -Time.FrameTime;
        p.Renderer.Texture = PlayerManager.sheet[0, sheetIndex];
        frappeCouldown += -Time.FrameTime;
        vitesse.y += -gravity * Time.FrameTime;
        if (vitesse.y > jumpForce)
            vitesse.y = jumpForce;
        p.Space.Position.y += vitesse.y * Time.FrameTime;

        if (Up == false)
        {
            canUp = true;
        }

        cayoteTimeCount += Time.FrameTime;

        if (Left && preLeft == false)
        {
            vitesse.x = 0;
            if (lastIsLeft == false)
                lastIsLeft = true;
        }
        if (Right && preRight == false)
        {
            vitesse.x = 0;
            if (lastIsLeft)
                lastIsLeft = false;
        }

        p.Renderer.Texture = PlayerManager.sheet[0, sheetIndex];
        if (Left && Right)
        {
            if (lastIsLeft)
                AddX(-1);
            else
                AddX(1);
        }
        else
        {
            if (Left || Right)
            {
                if (Left)
                    AddX(-1);
                else
                    AddX(1);
            }
        }
        if ((Left == false && Right == false)
            || (Left == false && preLeft == true && lastIsLeft)
            || (Right == false && preRight == true && !lastIsLeft))
            vitesse.x = 0f;

        p.Space.Position.x += vitesse.x * Time.FrameTime;
        bumpDelay += -Time.FrameTime;

        grounded = Collisions();  ////////////////////////////
        if (grounded)
        {
            canBump = true;
            if (preGrounded == false && power < maxpower)
            {
                circleAlpha = 0.75f;
            }
            circleMesh.Renderer.Alpha = circleAlpha;
                power = maxpower;
            cayoteTimeCount = 0f;
            walledLeft = false;
            walledRight = false;
            if (Up && canUp)
            {
                jump();
            }
        }
        else
        {
            circleMesh.Renderer.Alpha = 1f;
            if (walledLeft || walledRight)
            {
                canBump = true;
                cayoteTimeCount = 0f;
                p.Renderer.Texture = PlayerManager.sheet[2, sheetIndex];
                if (vitesse.y < -50)
                    vitesse.y = -50;

                if (Up && canUp)
                {
                    jump();
                    if (walledRight)
                        vitesse.x = -jumpForce;
                    else
                        vitesse.x = jumpForce;
                    p.Space.Position.x += vitesse.x * Time.FrameTime;
                }
            }
            else
            {
                if (Up && canUp && cayoteTimeCount < cayoteTime)
                {
                    jump();
                }
                else if (Up)
                {
                    circleAlpha = 0.75f;
                }

                circleMesh.Renderer.hide = true;
                if (Up && canUp && power > 0)
                {
                    jetting = true;
                    circleMesh.Renderer.hide = false;
                    vitesse.y += gravity * 2f * Time.FrameTime;
                    power += -Time.FrameTime;
                    if (GameData.particles)
                    {
                        partDelay += Time.FrameTime;
                        if (partDelay > 0.02f)
                        {
                            new ParticleSmoke(
                                new(p.Space.Position.x + PlayerManager.random.Next(3) - 1, p.Space.Position.y - 5
                                + PlayerManager.random.Next(3) - 1),
                                (short)PlayerManager.random.NextInt64(-90 - 20, -90 + 20));
                            partDelay = 0f;
                        }
                    }
                }

                if (Math.Abs(vitesse.y) > Math.Abs(vitesse.x) && Math.Abs(vitesse.y) > 70)
                {
                    p.Renderer.Texture = PlayerManager.sheet[3, sheetIndex];
                }
            }
        }

        if (Tap && CanFrappe)
        {
            Frapping = true;
        }
        else
            Frapping = false;

        preLeft = Left;
        preRight = Right;
        lastSpace = this.Tap;
        preGrounded = grounded;

        circleMesh.Space.Position = p.Space.Position;
        _lock.Space.Position = new Vector(p.Space.Position.x, p.Space.Position.y + 12);
    }

    public override void Draw()
    {
        p.Draw();
        if (locked) _lock.Draw();
    }

    Object circleMesh;

    float power;
    public static Effect fx;

    public override void Draw(ref SpriteBatch spriteBatch)
    {
        if (!grounded)
        {
            if (Time.TargetTimer > 0.1f)
                fx.Parameters["amount"].SetValue(1 - (power / maxpower));
            spriteBatch.Begin(effect: fx, samplerState: SamplerState.PointClamp, blendState: BlendState.Additive);

            circleMesh.Draw();
            spriteBatch.End();
        }
    }

    public override void DrawAdditive()
    {
        if (circleAlpha > 0)
        {
            if (grounded)
            {
                Pen.Circle(p.Space.Position, 14, color: Color.White, alpha: circleAlpha, thickness: 2);
            }
            else if (power <= 0)
            {
                Pen.Circle(p.Space.Position, 14, color: new(1, 0, 0), alpha: circleAlpha, thickness: 2);
            }
        }

        if (bumpDelay > 0f)
        {
            Pen.Circle(p.Space.Position, bumpDelay / 0.1f * radius, color: p.Renderer.Color);
        }
        else if (CanFrappe)
        {
            Pen.Circle(p.Space.Position, radius, color: circleColor);
        }
    }

    public override void Dispose()
    {
        hit = null;
        lastHit = null;
    }
}