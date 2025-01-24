using FriteCollection.Entity;
using FriteCollection.Math;
using FriteCollection.Scripting;
using FriteCollection.Audio;

namespace RocketLike;

public class Ball : Script
{
    private const ushort vitesseGolden = 290;
    private const ushort vitesseCyan = 350;
    private States state = States.None;


    public delegate void MarqueEvent(bool sideRight, States ballState, bool csc, byte delay, bool combo);
    public delegate void FrappeEvent(Vector direction, States ballState);
    public event MarqueEvent BUT;
    public event FrappeEvent FRAPPE;

    private static float _lastGoal;
    public static float LastGoal => _lastGoal;

    private SoundEffect bump1, bump2, bump22;
    public static SoundEffect bump3;
    private SoundEffect but1, but2, but3, fly;

    private static bool attiremode = false;
    private Vector target1, target2;
    private bool lastShootIsGreen;

    public static void SetAttireMode(bool value)
    {
        attiremode = value;
    }

    public static bool AttireMode => attiremode;

    public enum States
    {
        None, Golden, Cyan
    }

    public Ball() : base(Scenes.Game, active: false) { }
    public Object b;
    HitBox.Rectangle hit;
    HitBox.Rectangle hitGoal;
    Vector vitesse = Vector.Zero;
    public Microsoft.Xna.Framework.Graphics.Texture2D texture;

    private static bool active = true;
    public static bool IsActive => active;

    private bool gravity = false;


    public void Desactiver()
    {
        b.Renderer.hide = true;
        hit.Active = false;
        hitGoal.Active = false;
        active = false;
        gravity = false;
    }
    public void Activer()
    {
        b.Renderer.hide = false;
        hit.Active = true;
        hitGoal.Active = true;
        active = true;
        b.Space.Position = new Vector(0, 32);
        b.Space.Scale = new Vector(12, 12);
        vitesse = Vector.Zero;
        state = States.None;
        canClone = false;
        gravity = false;
        _lastGoal = float.MaxValue;
    }

    GameEffects effects;

    struct ButPos
    {
        public Vector Down;
        public Vector Up;
        public Vector Center;
    }

    private ButPos b1, b2;
    private ReplayManager replay;

    private bool volley = false;

    public override void Start()
    {
        bump1 = Open.SoundEffect("Sfx/none");
        bump2 = Open.SoundEffect("Sfx/golden");
        bump22 = Open.SoundEffect("Sfx/golden2");
        bump3 = Open.SoundEffect("Sfx/red");

        but1 = Open.SoundEffect("Sfx/nonebut");
        but2 = Open.SoundEffect("Sfx/goldbut");
        but3 = Open.SoundEffect("Sfx/redbut");

        fly = Open.SoundEffect("Sfx/flying");

        bump2.Volume = 0.75f;
        bump22.Volume = 0.75f;
        bump22.Volume = 0.75f;

        effects = (GameManager.GetScript("GameEffects") as GameEffects);
        (GameManager.GetScript("StartAnimation") as StartAnimation).START += start;
        b = new Object();
        Layer = 12;
        texture = Open.Texture("Game/Circle");
        b.Renderer.Texture = texture;
        hit = new HitBox.Rectangle(b.Space);
        hit.LockSize = new Vector(12, 12);
        hitGoal = new HitBox.Rectangle(b.Space);
        hitGoal.LockSize = new Vector(14, 14);
        hitGoal.Layer = 1;

        if (attiremode || GameData.p1visee || GameData.p2visee)
        {
            List<Vector> p1 = MapManager.tilemap.GetPos(0, 6);
            List<Vector> p2 = MapManager.tilemap.GetPos(0, 8);
            List<Vector> p3 = MapManager.tilemap.GetPos(1, 6);
            List<Vector> p4 = MapManager.tilemap.GetPos(1, 8);

            List<Vector> v1 = MapManager.tilemap.GetPos(0, 13);
            List<Vector> v2 = MapManager.tilemap.GetPos(2, 13);
            List<Vector> v3 = MapManager.tilemap.GetPos(0, 14);
            List<Vector> v4 = MapManager.tilemap.GetPos(2, 14);

            if (p1.Count > 0 && p2.Count > 0 && p3.Count > 0 && p4.Count > 0)
            {
                volley = false;
                target1 = (p1[0] + p2[0]) / 2f;
                target2 = (p3[0] + p4[0]) / 2f;

                if (GameData.p2visee)
                {
                    b1.Down = p2[0];
                    b1.Up = p1[0];
                    b1.Center = (p1[0] + p2[0]) / 2f;
                }
                if (GameData.p1visee)
                {
                    b2.Down = p4[0];
                    b2.Up = p3[0];
                    b2.Center = (p3[0] + p4[0]) / 2f;
                }
            }
            else if (v1.Count > 0 && v2.Count > 0 && v3.Count > 0 && v4.Count > 0)
            {
                volley = true;
                target1 = (v1[0] + v2[0]) / 2f;
                target2 = (v3[0] + v4[0]) / 2f;

                if (GameData.p2visee)
                {
                    b1.Down = v1[0];
                    b1.Up = v2[0];
                    b1.Center = (v1[0] + v2[0]) / 2f;
                }
                if (GameData.p1visee)
                {
                    b2.Down = v3[0];
                    b2.Up = v4[0];
                    b2.Center = (v3[0] + v4[0]) / 2f;
                }
            }
            else
            {
                new MessageBox(false, "this map is missing goal cages.");
                target1 = new Vector(-Screen.widht / 2f, 0);
                target2 = new Vector(Screen.widht / 2f, 0);
            }
        }

        Activer();
        butsp1 = 0;
        butsp2 = 0;

        replay = GameManager.GetScript("ReplayManager") as ReplayManager;
        replay.RESTART += OnStart;
    }

    void start()
    {
        gravity = true;
    }

    void OnStart()
    {
        gravity = false;
        vitesse = Vector.Zero;
        Activer();
    }

    private bool lastShooterIsGreen;

    public void Collide(Vector point, Vector _vitesse, bool shooterIsGreen)
    {
        lastShooterIsGreen = shooterIsGreen;
        float angle = Math.Atan(point, b.Space.Position);
        Vector dir = new Vector(Math.Cos(angle), Math.Sin(angle));
        if (point.x < b.Space.Position.x)
            dir.x = Math.Abs(dir.x);
        else
            dir.x = -Math.Abs(dir.x);
        if (point.y < b.Space.Position.y)
            dir.y = Math.Abs(dir.y);
        else
            dir.y = -Math.Abs(dir.y);

        bool aimChanged = false;
        Vector t = Vector.Zero;
        if (!volley)
        {
            if (shooterIsGreen)
            {
                float dist = GetDistanceBut(b2);
                if (GameData.p1visee && dir.x > 0 && dist < 240)
                {
                    aimChanged = true;
                    if (point.y < b2.Down.y)
                        t = b2.Up;
                    else if (point.y > b2.Up.y)
                        t = b2.Down;
                    else
                        t = b2.Center;
                    if (!attiremode)
                        t.y += d(dist);

                    angle = Math.Atan(b.Space.Position, t);
                    dir = new Vector(Math.Cos(angle), Math.Sin(angle));
                }
            }

            if (!shooterIsGreen)
            {
                float dist = GetDistanceBut(b1);
                if (GameData.p2visee && dir.x < 0 && dist < 240)
                {
                    aimChanged = true;
                    if (point.y < b1.Down.y)
                        t = b1.Up;
                    else if (point.y > b1.Up.y)
                        t = b1.Down;
                    else
                        t = b1.Center;
                    if (!attiremode)
                        t.y += d(dist);

                    angle = Math.Atan(b.Space.Position, t);
                    dir = new Vector(Math.Cos(angle), Math.Sin(angle));
                }
            }
        }
        else
        {
            if (shooterIsGreen && GameData.p1visee && dir.y < 0)
            {
                aimChanged = true;
                if (point.x < b2.Down.x)
                    t = b2.Up;
                else if (point.x > b2.Up.x)
                    t = b2.Down;
                else
                    t = b2.Center;

                angle = Math.Atan(b.Space.Position, t);
                dir = new Vector(Math.Cos(angle), Math.Sin(angle));
            }

            if (!shooterIsGreen && GameData.p2visee && dir.y < 0)
            {
                aimChanged = true;
                if (point.x < b1.Down.x)
                    t = b1.Up;
                else if (point.x > b1.Up.x)
                    t = b1.Down;
                else
                    t = b1.Center;

                angle = Math.Atan(b.Space.Position, t);
                dir = new Vector(Math.Cos(angle), Math.Sin(angle));
            }
        }

        if (aimChanged)
        {
            if (t.x > b.Space.Position.x)
                dir.x = Math.Abs(dir.x);
            else
                dir.x = -Math.Abs(dir.x);
            if (t.y > b.Space.Position.y)
                dir.y = Math.Abs(dir.y);
            else
                dir.y = -Math.Abs(dir.y);
        }

        float force = Math.Sqrt((vitesse.x * vitesse.x) + (vitesse.y * vitesse.y));
        float result = (105 + (force * 0.3f));

        if (aimChanged)
        {
            float f = Math.Sqrt((_vitesse.x * _vitesse.x) + (_vitesse.y * _vitesse.y)) / 1.5f;
            vitesse = dir * (result + f);
        }
        else
        {
            vitesse = dir * result;
            vitesse += _vitesse / 1.5f;
        }


        bump1.Play();
        fly.Stop();
        float vit = NormeVitesse;
        if (vit >= vitesseGolden)
        {
            vitesse *= 1.1f;
            if (vit >= vitesseCyan && (state == States.Golden || state == States.Cyan))
            {
                state = States.Cyan;
                vitesse *= 1.1f;
                bump3.Play();
                fly.Volume = 0.5f;
                fly.Play();
            }
            else
            {
                state = States.Golden;
                if (PlayerManager.random.Next(2) == 0)
                    bump2.Play();
                else
                    bump22.Play();
            }
            canClone = true;
            if (GameData.particles)
            {
                new ParticleWink(ref b.Space.Position);
            }
            FRAPPE(dir, state);
        }
        else
            canClone = false;
        if (attiremode && (int)state > 0)
        {
            state = States.Golden;

            this.lastShootIsGreen = shooterIsGreen;
            if (shooterIsGreen)
            {
                b.Renderer.Color = PlayerManager.ColorPlayer1;
            }
            else
            {
                b.Renderer.Color = PlayerManager.ColorPlayer2;
            }
        }
    }

    private const float deceleration = 0.8f;
    private float partTimer = 0f;
    public bool Activee => active;
    private byte butsp1, butsp2;

    private void Collisions()
    {
        if (hitGoal.Check(out HitBox.Sides s, out HitBox c, out ushort nombre))
        {
            active = false;
            vitesse = new Vector(0, 0);
            float t = (state == States.None ? 0 : (state == States.Golden ? 0.2f : 0.4f));
            fly.Stop();
            if (state == States.None)
            {
                but1.Play();
                effects.Shake(4, 0.5f);
                GameData.SAVE.nbbn++;
            }
            else
            {
                effects.Shake(7, t * 5f);
                if (state == States.Golden)
                {
                    but2.Play();
                    GameData.SAVE.nbbg++;
                }
                else
                {
                    but3.Play();
                    GameData.SAVE.nbbr++;
                }
            }
            new Sequence(() => { Time.SpaceTime = 0; return t; },
                () =>
                { 
                    Time.SpaceTime = 1f; Desactiver();
                    if (b.Space.Position.x > 0)
                    {
                        butsp1++;
                    }
                    else
                    {
                        butsp2++;
                    }
                    byte dt = (byte)(GameData.randomMode ? ((butsp1 + butsp2) > 2 ? 8 : 4) : 2);
                    BUT(b.Space.Position.x > 0, state,
                        b.Space.Position.x > 0 != lastShooterIsGreen,
                        dt, GameData.randomMode && (butsp1 >= 3 || butsp2 >= 3));
                    _lastGoal = Time.TargetTimer;
                    return 0;
                });
            return;
        }

        if (b.Space.Position.x >= PlayerManager.walls)
        {
            b.Space.Position.x = PlayerManager.walls;
            vitesse.x *= -deceleration;
        }
        if (b.Space.Position.x <= -PlayerManager.walls)
        {
            b.Space.Position.x = -PlayerManager.walls;
            vitesse.x *= -deceleration;
        }
        if (b.Space.Position.y < PlayerManager.floor)
        {
            b.Space.Position.y = PlayerManager.floor;
            vitesse.y *= -deceleration;
            if (Math.Abs(vitesse.y) < 120 && Math.Abs(vitesse.x) < 120)
            {
                vitesse.y = 120;
            }
        }
        if (b.Space.Position.y > PlayerManager.roof)
        {
            b.Space.Position.y = PlayerManager.roof;
            vitesse.y *= -deceleration;
        }

        ushort ncolliders = 1;
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
                            b.Space.Position.y =
                                collider.PositionOffset.y + ((collider.LockSize.y + hit.LockSize.y) / 2f);
                            vitesse.y *= -deceleration;
                            if (Math.Abs(vitesse.y) < 120 && Math.Abs(vitesse.x) < 120)
                            {
                                vitesse.y = 120;
                            }
                        }
                        break;

                    case HitBox.Sides.Up:
                        if (vitesse.y >= 0)
                        {
                            b.Space.Position.y =
                            collider.PositionOffset.y - ((collider.LockSize.y + hit.LockSize.y) / 2f);
                            vitesse.y *= -deceleration;
                        }
                        break;

                    case HitBox.Sides.Left:
                        if (vitesse.x <= 0)
                        {
                            b.Space.Position.x =
                            collider.PositionOffset.x + ((collider.LockSize.x + hit.LockSize.x) / 2f);
                            if (vitesse.x < 0)
                                vitesse.x *= -deceleration;
                        }
                        break;

                    case HitBox.Sides.Right:
                        if (vitesse.x >= 0)
                        {
                            b.Space.Position.x =
                            collider.PositionOffset.x - ((collider.LockSize.x + hit.LockSize.x) / 2f);
                            if (vitesse.x > 0)
                                vitesse.x *= -deceleration;
                        }
                        break;
                }
            }

            n++;
        }
    }
    bool canClone = false;
    float timer = 0f;

    private float NormeVitesse => Math.Sqrt((vitesse.x * vitesse.x) + (vitesse.y * vitesse.y));

    public override void AfterUpdate()
    {
        if (active)
        {
            float scale = NormeVitesse;

            if (gravity)
            {
                if (!attiremode)
                {
                    vitesse.y += -PlayerManager.GRAVITY * Time.FrameTime * 1f;
                }
            }
            else vitesse = new Vector(0, 0);

            if (attiremode && (int)state > 0)
            {
                if (lastShootIsGreen)
                {
                    vitesse.x += (target2.x - b.Space.Position.x) * Time.FrameTime;
                    vitesse.y += (target2.y - b.Space.Position.y) * Time.FrameTime;
                }
                else
                {
                    vitesse.x += (target1.x - b.Space.Position.x) * Time.FrameTime;
                    vitesse.y += (target1.y - b.Space.Position.y) * Time.FrameTime;
                }
            }

            b.Space.Position += vitesse * Time.FrameTime * 1f;
            Collisions();
            if (active)
            {
                b.Space.Scale = new Vector(12 - (scale / 150), 12 + (scale / 150));
                if (vitesse.x != 0)
                {
                    b.Space.rotation = -Math.Atan(vitesse.y / vitesse.x) + 90;
                }
                else
                {
                    b.Space.rotation = 0;
                }
                b.Renderer.Color = Pico8.White;
                if (attiremode && (int)state > 0)
                {
                    if (lastShootIsGreen)
                    {
                        b.Renderer.Color = PlayerManager.ColorPlayer1;
                    }
                    else
                    {
                        b.Renderer.Color = PlayerManager.ColorPlayer2;
                    }
                }
                if (canClone)
                {
                    timer += Time.FrameTime;
                    if (state == States.Cyan)
                    {
                        if (!attiremode)
                        {
                            b.Renderer.Color = Pico8.Pink;
                        }
                        if (GameData.particles)
                        {
                            partTimer += Time.FrameTime;
                            if (partTimer > 0.1f)
                            {
                                partTimer = 0f;
                                new ParticleFlaque(ref b.Space.Position, 12);
                            }
                        }
                    }
                    else if (!attiremode)
                    {
                        b.Renderer.Color = Pico8.Yellow;
                    }
                    if (GameData.particles)
                    {
                        if (timer > 0.01f)
                        {
                            new ParticleBall(ref b.Space.Position, ref vitesse, ref b.Renderer.Color);
                            timer = 0f;
                        }
                    }
                }
                if (!attiremode && (state == States.Cyan ? scale < vitesseCyan / 2f : scale < vitesseGolden / 1.5f))
                {
                    partTimer = 0f;
                    timer = 1f;
                    canClone = false;
                    state = States.None;
                    fly.Stop();
                }
            }
        }

        if (state == States.Cyan)
        {
            Cube3D.roty = 40;
        }
        else if (state == States.Golden)
        {
            Cube3D.roty = 20;
        }
        else
        {
            Cube3D.roty = 10;
        }
    }

    private float d(float x)
    {
        return Math.Min(1, Math.Max(0, (x / 16f) - 5)) * 16;
    }

    private float GetDistanceBut(ButPos pos)
    {
        return Math.Min(Math.Min(
            Math.GetDistance(b.Space.Position, pos.Up),
            Math.GetDistance(b.Space.Position, pos.Down)),
            Math.GetDistance(b.Space.Position, pos.Center));
    }

    public override void Draw()
    {
        b.Draw();
    }

    public override void Dispose()
    {
        if (texture is not null)
        {
            hit.Destroy();
            hitGoal.Destroy();
            texture.Dispose();
            bump1.Dispose();
            bump2.Dispose();
            bump22.Dispose();
            bump3.Dispose();

            but1.Dispose();
            but2.Dispose();
            but3.Dispose();

            fly.Dispose();

            effects = null;

            hitGoal = null;
            fly = null;
            hit = null;
            texture = null;
            bump1 = null;
            bump2 = null;
            bump22 = null;
            bump3 = null;
            but1 = null;
            but2 = null;
            but3 = null;
        }
    }
}