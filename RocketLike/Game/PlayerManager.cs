using FriteCollection.Entity;
using FriteCollection.Scripting;
using FriteCollection.Graphics;
using FriteCollection.Tools.SpriteSheet;
using FriteCollection.Input;
using Microsoft.Xna.Framework.Graphics;
using FriteCollection.Math;
using FriteCollection.Tools.Pen;
using Microsoft.Xna.Framework.Input;

namespace RocketLike;

public class PlayerManager : Script
{
    public const float GRAVITY = 288f;
    public const short floor = -108, roof = 108, walls = 220;
    public static SpriteSheet sheet;

    public static readonly Color ColorPlayer1 = Pico8.Green, Color2Player1 = Pico8.DarkGreen;
    public static readonly Color ColorPlayer2 = Pico8.Red, Color2Player2 = Pico8.DarkPurple;

    public static readonly System.Random random = new System.Random();

    public PlayerManager() : base(Scenes.Game) { }

    public Player p1, p2;

    public override void BeforeStart()
    {
        GameManager.GameFont = Open.Font("Game/fritefont");
    }

    public override void Start()
    {
        sheet = new SpriteSheet(Open.Texture("Game/PlayerManager/player"), 10, 10);
        Player.ball = GameManager.GetScript("Ball") as Ball;
        Player.fx = FriteModel.MonoGame.instance.Content.Load<Effect>("Shaders/CircleBarr");
        Screen.backGround = Pico8.Lavender;
        Layer = 1;
        p2 = new Player(MapManager.tilemap.GetPos(1, 6), ColorPlayer2, Color2Player2,
            Keys.NumPad5, Keys.NumPad1, Keys.NumPad3, Keys.Left);
        p1 = new Player(MapManager.tilemap.GetPos(0, 6), ColorPlayer1, Color2Player1,
            Keys.Z, Keys.Q, Keys.D, Keys.S);
        Pen.GridOrigin = Bounds.Center;
        (GameManager.GetScript("StartAnimation") as StartAnimation).START += GameStart;
        (GameManager.GetScript("Ball") as Ball).BUT += But;
    }

    void But(bool sideRight, Ball.States state)
    {
        new Sequence(
            () =>
            {
                if (sideRight)
                {
                    p2.locked = true;
                }
                else
                {
                    p1.locked = true;
                }
                return 4f;
            },
            () =>
            {
                p1.Reset();
                p2.Reset();
                return 0;
            });
    }

    void GameStart()
    {
        p1.locked = false;
        p2.locked = false;
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
    }

    public override void AfterUpdate()
    {
        if (Input.KeyBoard.Escape)
        {
            GameManager.CurrentScene = Scenes.Menu;
        }
    }

    public override void Dispose()
    {
        p1.Dispose();
        sheet.Dispose();
        Player.fx.Dispose();
    }
}

public class Player : Clone
{
    private const float cayoteTime = 0.07f;
    private const float jumpForce = 171;
    private const float vitesseXMax = 170;
    private const int radius = 26;
    private static readonly float gravity = PlayerManager.GRAVITY * 1.5f;

    public static Ball ball;

    private Keys up, left, right, tap;
    private bool Up => Input.IsKeyDown(up) && !locked;
    private bool Left => Input.IsKeyDown(left) && !locked;
    private bool Right => Input.IsKeyDown(right) && !locked;
    private bool Tap => Input.IsKeyDown(tap) && !locked;

    private readonly Object p = new Object();
    private readonly Color circleColor;
    private readonly Vector startPos;
    private Object _lock;
    public bool locked = true;

    public Player(Vector pos, Color color, Color color2, Keys up, Keys left, Keys right, Keys tap) : base()
    {
        this.up = up;
        this.left = left;
        this.right = right;
        this.tap = tap;
        p.Renderer.Texture = PlayerManager.sheet[0, 0];
        hit = new HitBox.Rectangle(p.Space);
        hit.LockSize = new(8, 8);
        p.Space.Scale = new Vector(10, 10);
        p.Renderer.Color = color;
        p.Space.Position = pos;
        startPos = pos;

        circleColor = color2;
        _lock = new Object();
        _lock.Space.Scale = new Vector(10, 10);
        _lock.Renderer.Texture = PlayerManager.sheet[4, 0];

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
    bool grounded;
    float frappeCouldown = 0.5f;
    public bool CheckCollisionBall()
    {
        return Math.GetDistance(p.Space.Position, ball.b.Space.Position) < radius + 10
            && Ball.Active;
    }

    public void Bump(bool collision)
    {
        frappeCouldown = 0.3f;
        canBump = false;
        bumpDelay = 0.1f;
        if (collision)
        {
            ball.Collide(p.Space.Position, vitesse);
        }
        vitesse = new Vector(0, 0);
    }

    private bool Collisions()
    {
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
        while (ncolliders > 0)
        {
            if (hit.Check(out HitBox.Sides side, out HitBox collider, out ncolliders, tag: "plat"))
            {
                switch (side)
                {
                    case HitBox.Sides.Down:
                        p.Space.Position.y =
                            collider.PositionOffset.y + ((collider.LockSize.y + 8) / 2f);
                        vitesse.y = 0;
                        p.Renderer.Texture = PlayerManager.sheet[0, 0];
                        grounded = true;
                        break;

                    case HitBox.Sides.Up:
                        p.Space.Position.y =
                            collider.PositionOffset.y - ((collider.LockSize.y + 8) / 2f);
                        if (vitesse.y >= 0)
                            vitesse.y *= -0.25f;
                        p.Renderer.Texture = PlayerManager.sheet[0, 0];
                        break;

                    case HitBox.Sides.Left:
                        p.Renderer.Texture = p.Renderer.Texture = PlayerManager.sheet[0, 0];
                        p.Space.Position.x =
                            collider.PositionOffset.x + ((collider.LockSize.x + 8) / 2f);
                        vitesse.x = 0;
                        walledLeft = true;

                        break;

                    case HitBox.Sides.Right:
                        p.Renderer.Texture = p.Renderer.Texture = PlayerManager.sheet[0, 0];
                        p.Space.Position.x =
                        collider.PositionOffset.x - ((collider.LockSize.x + 8) / 2f);
                        vitesse.x = 0;
                        walledRight = true;

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
            if (n > 10)
            {
                p.Space.Position = Vector.Zero;
                vitesse = Vector.Zero;
                ncolliders = 0;
            }
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
            p.Renderer.Texture = PlayerManager.sheet[1, 0];
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
    }

    float bumpDelay = 0f;
    

    float partDelay = 1f;
    private bool CanFrappe => !lastSpace && canBump && frappeCouldown <= 0 && !locked;
    private System.Random rand = new System.Random();
    private float maxpower = 0.8f;

    public override void Update()
    {
        p.Renderer.Texture = PlayerManager.sheet[0, 0];
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

        p.Renderer.Texture = PlayerManager.sheet[0, 0];
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

        grounded = Collisions();
        if (grounded)
        {
            canBump = true;
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
            if (walledLeft || walledRight)
            {
                canBump = true;
                cayoteTimeCount = 0f;
                p.Renderer.Texture = PlayerManager.sheet[2, 0];
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

                circleMesh.Renderer.hide = true;
                if (Up && canUp && power > 0)
                {
                    circleMesh.Renderer.hide = false;
                    vitesse.y += gravity * 2f * Time.FrameTime;
                    power += -Time.FrameTime;
                    partDelay += Time.FrameTime;
                    if (partDelay > 0.02f)
                    {
                        new ParticleSmoke(
                            new(p.Space.Position.x, p.Space.Position.y - 5),
                            (short)rand.NextInt64(-90 - 20, -90 + 20));
                        partDelay = 0f;
                    }
                }

                if (Math.Abs(vitesse.y) > Math.Abs(vitesse.x) && Math.Abs(vitesse.y) > 70)
                {
                    p.Renderer.Texture = PlayerManager.sheet[3, 0];
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

        circleMesh.Space.Position = p.Space.Position;
        _lock.Space.Position = new Vector(p.Space.Position.x, p.Space.Position.y + 12);
    }

    public override void BeforeDraw()
    {
        if (bumpDelay > 0f)
        {
            Pen.Circle(p.Space.Position, bumpDelay / 0.1f * radius, color: p.Renderer.Color);
        }
        else if (CanFrappe)
        {
            Pen.Circle(p.Space.Position, radius, color: circleColor);
        }
    }

    public override void Draw()
    {
        p.Draw();
        if (locked) _lock.Draw();
    }

    Object circleMesh;

    float power;
    public static Effect fx;

    public override void Draw(SpriteBatch spriteBatch)
    {
        fx.Parameters["amount"].SetValue(1 - (power / maxpower));
        spriteBatch.Begin(effect: fx, samplerState: SamplerState.PointClamp);

        circleMesh.Draw();

        spriteBatch.End();
    }
}