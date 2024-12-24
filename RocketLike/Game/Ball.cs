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


    public delegate void MarqueEvent(bool sideRight, States ballState);
    public delegate void FrappeEvent(Vector direction, States ballState);
    public event MarqueEvent BUT;
    public event FrappeEvent FRAPPE;

    private SoundEffect bump1, bump2, bump22, bump3;
    private SoundEffect but1, but2, but3;

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

    public Ball() : base(Scenes.Game) { }
    public Object b;
    HitBox.Rectangle hit;
    HitBox.Rectangle hitGoal;
    Vector vitesse = Vector.Zero;
    public Microsoft.Xna.Framework.Graphics.Texture2D texture;

    private static bool active = true;
    public static bool Active => active;

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
    }

    GameEffects effects;

    public override void Start()
    {
        bump1 = Open.SoundEffect("Sfx/none");
        bump2 = Open.SoundEffect("Sfx/golden");
        bump22 = Open.SoundEffect("Sfx/golden2");
        bump3 = Open.SoundEffect("Sfx/red");

        but1 = Open.SoundEffect("Sfx/nonebut");
        but2 = Open.SoundEffect("Sfx/goldbut");
        but3 = Open.SoundEffect("Sfx/redbut");

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

        if (attiremode)
        {
            List<Vector> p1 = MapManager.tilemap.GetPos(0, 7);
            List<Vector> p2 = MapManager.tilemap.GetPos(0, 9);
            List<Vector> p3 = MapManager.tilemap.GetPos(1, 7);
            List<Vector> p4 = MapManager.tilemap.GetPos(1, 9);

            List<Vector> v1 = MapManager.tilemap.GetPos(0, 13);
            List<Vector> v2 = MapManager.tilemap.GetPos(2, 13);
            List<Vector> v3 = MapManager.tilemap.GetPos(0, 14);
            List<Vector> v4 = MapManager.tilemap.GetPos(2, 14);

            if (p1.Count > 0)
            {
                target1 = (p1[0] + p2[0]) / 2f;
                target2 = (p3[0] + p4[0]) / 2f;
            }
            else
            {
                target1 = (v1[0] + v2[0]) / 2f;
                target2 = (v3[0] + v4[0]) / 2f;
            }

            GameManager.Print(target1,  target2);
        }

        Activer();
    }

    void start() { gravity = true; }

    public void Collide(Vector point, Vector _vitesse, bool shooterIsGreen)
    {
        float force = Math.Sqrt((vitesse.x * vitesse.x) + (vitesse.y * vitesse.y));
        float angle = Math.Atan(point, b.Space.Position);
        float result = (105 + (force * 0.3f));
        Vector dir = new Vector(Math.Cos(angle), Math.Sin(angle));
        vitesse = dir * result;
        if (point.x < b.Space.Position.x)
            vitesse.x = Math.Abs(vitesse.x);
        else
            vitesse.x = -Math.Abs(vitesse.x);
        if (point.y < b.Space.Position.y)
            vitesse.y = Math.Abs(vitesse.y);
        else
            vitesse.y = -Math.Abs(vitesse.y);
        vitesse += _vitesse / 1.5f;

        bump1.Play();
        float vit = NormeVitesse;
        if (vit >= vitesseGolden)
        {
            vitesse *= 1.1f;
            if (vit >= vitesseCyan && (state == States.Golden || state == States.Cyan))
            {
                state = States.Cyan;
                vitesse *= 1.1f;
                bump3.Play();
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
            new ParticleWink(ref b.Space.Position);
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

    private void Collisions()
    {
        if (hitGoal.Check(out HitBox.Sides s, out HitBox c, out ushort nombre))
        {
            active = false;
            vitesse = new Vector(0, 0);
            float t = (state == States.None ? 0 : (state == States.Golden ? 0.2f : 0.4f));
            if (state == States.None)
            {
                but1.Play();
                effects.Shake(4, 0.5f);
                GameData.STATS.nombreDeButsNone++;
            }
            else
            {
                effects.Shake(7, t * 5f);
                if (state == States.Golden)
                {
                    but2.Play();
                    GameData.STATS.nombreDeButsGolden++;
                }
                else
                {
                    but3.Play();
                    GameData.STATS.nombreDeButsRed++;
                }
            }
            new Sequence(() => { Time.SpaceTime = 0; return t; },
                () =>
                { 
                    Time.SpaceTime = 1f; Desactiver();
                    if (BUT is not null) BUT(b.Space.Position.x > 0, state);
                    return 4f; }, () => { Activer(); return 0; 
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

        while (ncolliders > 0)
        {
            if (hit.Check(out HitBox.Sides side, out HitBox collider, out ncolliders, tag: "plat"))
            {
                switch (side)
                {
                    case HitBox.Sides.Down:
                        b.Space.Position.y =
                            collider.PositionOffset.y + ((collider.LockSize.y + hit.LockSize.y) / 2f);
                        vitesse.y *= -deceleration;
                        if (Math.Abs(vitesse.y) < 120 && Math.Abs(vitesse.x) < 120)
                        {
                            vitesse.y = 120;
                        }
                        break;

                    case HitBox.Sides.Up:
                        b.Space.Position.y =
                            collider.PositionOffset.y - ((collider.LockSize.y + hit.LockSize.y) / 2f);
                        vitesse.y *= -deceleration;
                        break;

                    case HitBox.Sides.Left:
                        b.Space.Position.x =
                            collider.PositionOffset.x + ((collider.LockSize.x + hit.LockSize.x) / 2f);
                        if (vitesse.x < 0)
                            vitesse.x *= -deceleration;
                        break;

                    case HitBox.Sides.Right:
                        b.Space.Position.x =
                            collider.PositionOffset.x - ((collider.LockSize.x + hit.LockSize.x) / 2f);
                        if (vitesse.x > 0)
                            vitesse.x *= -deceleration;
                        break;
                }
            }

            n++;
            if (n > 10)
            {
                b.Space.Position = Vector.Zero;
                vitesse = Vector.Zero;
                ncolliders = 0;
            }
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
                        partTimer += Time.FrameTime;
                        if (!attiremode)
                        {
                            b.Renderer.Color = Pico8.Pink;
                        }
                        if (partTimer > 0.1f)
                        {
                            partTimer = 0f;
                            new ParticleFlaque(ref b.Space.Position, 12);
                        }
                    }
                    else if (!attiremode)
                    {
                        b.Renderer.Color = Pico8.Yellow;
                    }
                    if (timer > 0.01f)
                    {
                        new ParticleBall(ref b.Space.Position, ref vitesse, ref b.Renderer.Color);
                        timer = 0f;
                    }
                }
                if (!attiremode && (state == States.Cyan ? scale < vitesseCyan / 2f : scale < vitesseGolden / 1.5f))
                {
                    partTimer = 0f;
                    timer = 1f;
                    canClone = false;
                    state = States.None;
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

    public override void Draw()
    {
        b.Draw();
    }

    public override void Dispose()
    {
        texture.Dispose();
        bump1.Dispose();
        bump2.Dispose();
        bump22.Dispose();
        bump3.Dispose();
    }
}