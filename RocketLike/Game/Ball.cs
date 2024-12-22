using FriteCollection.Entity;
using FriteCollection.Math;
using FriteCollection.Scripting;

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
        gravity = false;
    }

    public override void Start()
    {
        (GameManager.GetScript("StartAnimation") as StartAnimation).START += start;
        b = new Object();
        Layer = 12;
        texture = Open.Texture("Game/Circle");
        b.Renderer.Texture = texture;
        hit = new HitBox.Rectangle(b.Space);
        hit.LockSize = new Vector(12, 12);
        hitGoal = new HitBox.Rectangle(b.Space);
        hitGoal.LockSize = new Vector(12, 12);
        hitGoal.Layer = 1;
        Activer();
    }

    void start() { gravity = true; }

    public void Collide(Vector point, Vector _vitesse)
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

        float vit = NormeVitesse;
        if (vit >= vitesseGolden)
        {
            vitesse *= 1.1f;
            if (vit >= vitesseCyan && (state == States.Golden || state == States.Cyan))
            {
                state = States.Cyan;
                vitesse *= 1.1f;
            }
            else state = States.Golden;
            canClone = true;
            new ParticleWink(ref b.Space.Position);
            FRAPPE(dir, state);
        }
        else
            canClone = false;
    }

    private const float deceleration = 0.8f;
    private float partTimer = 0f;

    private void Collisions()
    {
        if (hitGoal.Check(out HitBox.Sides s, out HitBox c, out ushort nombre))
        {
            if (BUT is not null) BUT(b.Space.Position.x > 0, state);
            vitesse = new Vector(0, 0);
            Desactiver();
            new Sequence(() => 4f, () => { Activer(); return 0; });
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
                vitesse.y += -PlayerManager.GRAVITY * Time.FrameTime * 1f;
            }
            else vitesse = new Vector(0, 0);
            b.Space.Position += vitesse * Time.FrameTime * 1f;
            Collisions();
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
            if (canClone)
            {
                timer += Time.FrameTime;
                if (state == States.Cyan)
                {
                    partTimer += Time.FrameTime;
                    b.Renderer.Color = Pico8.Pink;
                    if (partTimer > 0.1f)
                    {
                        partTimer = 0f;
                        new ParticleFlaque(ref b.Space.Position, 12);
                    }
                }
                else
                {
                    b.Renderer.Color = Pico8.Yellow;
                }
                if (timer > 0.01f)
                {
                    new ParticleBall(ref b.Space.Position, ref vitesse, ref b.Renderer.Color);
                    timer = 0f;
                }
            }
            if (state == States.Cyan ? scale < vitesseCyan / 2f : scale < vitesseGolden / 1.5f)
            {
                partTimer = 0f;
                timer = 1f;
                canClone = false;
                state = States.None;
            }
        }
    }

    public override void Draw()
    {
        b.Draw();
    }

    public override void Dispose()
    {
        texture.Dispose();
    }
}