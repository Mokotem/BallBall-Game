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

public class Manager : Script
{
    public const float GRAVITY = 400f;
    public const short floor = -16 * 5 - 12, roof = 64 + 32 - 4, walls = 16 * 14 - 20;
    public static readonly SpriteSheet sheet =
new SpriteSheet(Open.Texture("Game/PlayerManager/player"), 10, 10);

    public static readonly Color ColorPlayer1 = Pico8.Green, Color2Player1 = Pico8.DarkGreen;
    public static readonly Color ColorPlayer2 = Pico8.Red, Color2Player2 = Pico8.DarkPurple;

    private MapManager mapManager;

    private float shakeTime = 0f;
    private System.Random random = new System.Random();

    public void Shake()
    {
        shakeTime = 0.2f;
    }

    public Manager() : base(Scenes.Game) { }

    Player p1, p2;

    public override void Start()
    {
        mapManager = GameManager.GetScript("MapManager") as MapManager;
        Layer = 10;
        p2 = new Player(mapManager.tilemap.GetPos(1, 6), ColorPlayer2, Color2Player2,
            Keys.NumPad5, Keys.NumPad1, Keys.NumPad3, Keys.Left);
        p1 = new Player(mapManager.tilemap.GetPos(0, 6), ColorPlayer1, Color2Player1,
            Keys.Z, Keys.Q, Keys.D, Keys.V);
        Pen.GridOrigin = Bounds.Center;
    }

    public override void AfterUpdate()
    {
        shakeTime += -Time.FrameTime;
        if (shakeTime > 0)
        {
            Camera.Position = new Vector(
            (float)(random.NextDouble() * 4) - 2,
            (float)(random.NextDouble() * 4) - 2);
        }
        else
            Camera.Position = Vector.Zero;
    }

    public override void Dispose()
    {
        p1.Dispose();
        sheet.Dispose();
    }
}

public class Player : Clone
{
    private const float cayoteTime = 0.07f;
    private const float jumpForce = 180;
    private const float vitesseXMax = 170;
    private const int radius = 25;

    private static readonly Ball ball = GameManager.GetScript("Ball") as Ball;

    private Keys up, left, right, tap;
    private bool Up => Input.IsKeyDown(up);
    private bool Left => Input.IsKeyDown(left);
    private bool Right => Input.IsKeyDown(right);
    private bool Tap => Input.IsKeyDown(tap);

    private readonly Object p = new Object();
    private readonly Color circleColor;

    public Player(Vector pos, Color color, Color color2, Keys up, Keys left, Keys right, Keys tap) : base()
    {
        this.up = up;
        this.left = left;
        this.right = right;
        this.tap = tap;
        p.Renderer.Texture = Manager.sheet[0, 0];
        hit = new HitBox.Rectangle(p.Space);
        hit.LockSize = new(8, 8);
        p.Space.Scale = new Vector(10, 10);
        p.Renderer.Color = color;
        p.Space.Position = pos;

        circleColor = color2;

        fx.Parameters["amount"].SetValue(power);
    }

    public override void Start()
    {
        Layer = 0;
        fx.Parameters["thick"].SetValue(0.1f);
        circleMesh.Renderer.Color = Pico8.White;
    }

    Vector vitesse = Vector.Zero;
    private bool walledLeft = false, walledRight = false;

    private float cayoteTimeCount = 0f;

    private HitBox.Rectangle hit;
    private HitBox lastHit;
    bool grounded;
    float frappeCouldown = 0.5f;

    private bool Collisions()
    {
        walledLeft = false;
        walledRight = false;
        bool grounded = false;
        if (p.Space.Position.x >=Manager.walls)
        {
            p.Space.Position.x = Manager.walls;
            vitesse.x = 0;
            walledRight = true;
        }
        if (p.Space.Position.x <= -Manager.walls)
        {
            p.Space.Position.x = -Manager.walls;
            vitesse.x = 0;
            walledLeft = true;
        }
        if (p.Space.Position.y < Manager.floor)
        {
            p.Space.Position.y = Manager.floor;
            grounded = true;
            vitesse.y = 0;
        }
        if (p.Space.Position.y > Manager.roof)
        {
            p.Space.Position.y = Manager.roof;
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
                        p.Renderer.Texture = Manager.sheet[0, 0];
                        grounded = true;
                        break;

                    case HitBox.Sides.Up:
                        p.Space.Position.y =
                            collider.PositionOffset.y - ((collider.LockSize.y + 8) / 2f);
                        if (vitesse.y >= 0)
                            vitesse.y *= -0.25f;
                        p.Renderer.Texture = Manager.sheet[0, 0];
                        break;

                    case HitBox.Sides.Left:
                        p.Space.Position.x =
                            collider.PositionOffset.x + ((collider.LockSize.x + 8) / 2f);
                        vitesse.x = 0;
                        walledLeft = true;
                        break;

                    case HitBox.Sides.Right:
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

    private void jump()
    {
        cayoteTimeCount = 1f;
        canUp = false;
        vitesse.y = jumpForce;
        vitesse.y += -Manager.GRAVITY * Time.FrameTime;
        p.Space.Position.y += vitesse.y * Time.FrameTime;
    }

    float bumpDelay = 0f;
    private void Bump()
    {
        canBump = false;
        bumpDelay = 0.1f;
        if (Math.GetDistance(p.Space.Position, ball.b.Space.Position) < radius + 9)
        {
            ball.Collide(p.Space.Position, vitesse);
        }
        vitesse = new Vector(0, 0);
    }

    float partDelay = 1f;
    private bool CanFrappe => !lastSpace && canBump && frappeCouldown <= 0;
    private System.Random rand = new System.Random();

    public override void Update()
    {
        frappeCouldown += -Time.FrameTime;
        vitesse.y += -Manager.GRAVITY * Time.FrameTime;
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

        p.Renderer.Texture = Manager.sheet[0, 0];
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
            power = 1;
            cayoteTimeCount = 0f;
            walledLeft = false;
            walledRight = false;
            if (Up && canUp)
            {
                jump();
            }

            if (walledLeft)
            {
                p.Renderer.Texture = Manager.sheet[0, 0];
            }
            else
            {
                if (Right || Left)
                {
                    p.Renderer.Texture = Manager.sheet[1, 0];
                }
            }
        }
        else
        {
            if (walledLeft || walledRight)
            {
                cayoteTimeCount = 0f;
                p.Renderer.Texture = Manager.sheet[2, 0];
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
                    vitesse.y += Manager.GRAVITY * 1.1f * Time.FrameTime;
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
                    p.Renderer.Texture = Manager.sheet[3, 0];
                }
                else if (Math.Abs(vitesse.x) > 70)
                {
                    p.Renderer.Texture = Manager.sheet[1, 0];
                }
            }
        }

        if (Tap && CanFrappe)
        {
            frappeCouldown = 0.4f;
            Bump();
        }

        preLeft = Left;
        preRight = Right;
        lastSpace = this.Tap;

        circleMesh.Space.Position = p.Space.Position;
    }

    public override void Draw()
    {
        p.Draw();
        if (bumpDelay > 0f)
        {
            Pen.Circle(p.Space.Position, bumpDelay / 0.1f * radius, color: p.Renderer.Color);
        }
        else if (CanFrappe)
        {
            Pen.Circle(p.Space.Position, radius, color: circleColor);
        }
    }

    Object circleMesh = new Object()
    {
        Space = new Space()
        {
            Scale = new Vector(radius, radius)
        }
    };

    float power = 0.5f;
    private Effect fx = FriteModel.MonoGame.instance.Content.Load<Effect>("Shaders/CircleBarr");

    public override void Draw(SpriteBatch spriteBatch)
    {
        fx.Parameters["amount"].SetValue(1 - power);
        spriteBatch.Begin(effect: fx);

        circleMesh.Draw();

        spriteBatch.End();
    }

    public override void Dispose()
    {
        fx.Dispose();
    }
}