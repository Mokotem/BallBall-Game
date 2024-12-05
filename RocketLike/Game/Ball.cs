using FriteCollection.Entity;
using FriteCollection.Math;
using FriteCollection.Scripting;
using SharpDX.Direct3D9;

namespace RocketLike;

public class Ball : Script
{
    public Ball() : base(Scenes.Game) { }
    public Object b = new Object();
    HitBox.Rectangle hit;
    HitBox.Rectangle hitGoal;
    Vector vitesse = Vector.Zero;
    public static Microsoft.Xna.Framework.Graphics.Texture2D texture;
    private Manager manager;

    System.Random rand;

    public override void Start()
    {
        manager = GameManager.GetScript("Manager") as Manager;
        Layer = 2;
        texture = Open.Texture("Game/Circle");
        b.Renderer.Texture = texture;
        hit = new HitBox.Rectangle(b.Space);
        hit.LockSize = new Vector(12, 12);
        hitGoal = new HitBox.Rectangle(b.Space);
        hitGoal.LockSize = new Vector(12, 12);
        hitGoal.Layer = 1;
        vitesse = new Vector(150, 100);
        rand = new System.Random();
    }

    public void Collide(Vector point, Vector _vitesse)
    {
        Microsoft.Xna.Framework.Input.Keyboard.GetState();

        float force = Math.Sqrt((vitesse.x * vitesse.x) + (vitesse.y * vitesse.y));
        float angle = Math.Atan(point, b.Space.Position);
        float result = (100 + (force * 0.3f));
        vitesse.x = Math.Cos(angle) * result;
        vitesse.y = Math.Sin(angle) * result;
        if (point.x < b.Space.Position.x)
            vitesse.x = Math.Abs(vitesse.x);
        else
            vitesse.x = -Math.Abs(vitesse.x);
        if (point.y < b.Space.Position.y)
            vitesse.y = Math.Abs(vitesse.y);
        else
            vitesse.y = -Math.Abs(vitesse.y);
        vitesse += _vitesse / 1.5f;
        canClone = true;
    }

    private const float deceleration = 0.9f;

    private void Collisions()
    {
        if (hitGoal.Check(out HitBox.Sides s, out HitBox c, out ushort nombre))
        {
            manager.Shake();
            b.Space.Position = new Vector(0, 16 * 3);
            vitesse = new Vector(0, 0);
            return;
        }

        if (b.Space.Position.x >= Manager.walls)
        {
            b.Space.Position.x = Manager.walls;
            vitesse.x *= -deceleration;
        }
        if (b.Space.Position.x <= -Manager.walls)
        {
            b.Space.Position.x = -Manager.walls;
            vitesse.x *= -deceleration;
        }
        if (b.Space.Position.y < Manager.floor)
        {
            b.Space.Position.y = Manager.floor;
            vitesse.y *= -deceleration;
            if (Math.Abs(vitesse.y) < 150 && Math.Abs(vitesse.x) < 100)
            {
                vitesse.y = 150;
            }
            else if (Math.Abs(vitesse.y) < 5)
                vitesse.y = 0;

        }
        if (b.Space.Position.y > Manager.roof)
        {
            b.Space.Position.y = Manager.roof;
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
                        if (Math.Abs(vitesse.y) < 150 && Math.Abs(vitesse.x) < 100)
                        {
                            vitesse.y = 150;
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

    public override void AfterUpdate()
    {
        vitesse.y += -Manager.GRAVITY * Time.FrameTime * 0.5f;
        b.Space.Position += vitesse * Time.FrameTime * 0.5f;
        Collisions();
        vitesse.y += -Manager.GRAVITY * Time.FrameTime * 0.5f;
        b.Space.Position += vitesse * Time.FrameTime * 0.5f;
        float scale = Math.Sqrt((vitesse.x * vitesse.x) + (vitesse.y * vitesse.y));
        b.Space.Scale = new Vector(12 - (scale / 150), 12 + (scale / 150));
        b.Space.rotation = -Math.Atan(vitesse.y / vitesse.x) + 90;
        b.Renderer.Color = Pico8.White;
        if (Math.Sqrt((vitesse.x * vitesse.x) + (vitesse.y * vitesse.y)) > 220)
        {
            timer += Time.FrameTime;
            if (canClone)
            {
                b.Renderer.Color = Pico8.Yellow;
                if (timer > 0.025f)
                {
                    new ParticleBall(b.Space);
                    timer = 0f;
                }
            }
        }
        else
        {
            timer = 1f;
            canClone = false;
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