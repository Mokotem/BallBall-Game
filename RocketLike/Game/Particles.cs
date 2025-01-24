using FriteCollection.Entity;
using FriteCollection.Scripting;
using FriteCollection.Tools.Pen;
using FriteCollection.Math;
using FriteCollection.Graphics;

namespace RocketLike;

public class MessageBox : Clone
{
    private Object bande;
    private Text t;
    private float timer;

    public MessageBox(bool success, string message)
    {
        bande = new Object()
        {
            Space = new Space()
            {
                UI = true,
                CenterPoint = Bounds.BottomLeft,
                GridOrigin = Bounds.BottomLeft,
                Scale = new Vector(message.Length > 40 ? Screen.widht : (Screen.widht / 2), message.Length > 40 ? 15 : 7)
            },
            Renderer = new Renderer()
            {
                Color = success ? new Color(0, 0.7f, 0) : new Color(0.75f, 0, 0),
            }
        };
        t = new Text(GameManager.GameFont, message);
        t.Space = new Space()
        {
            UI = true,
            CenterPoint = Bounds.Left,
            GridOrigin = Bounds.Left,
            Position = new Vector(1, message.Length > 40 ? 126 : 130)
        };
        timer = 0f;
    }

    public override void Update()
    {
        timer += Time.FrameTime * (t.Write.Length > 40 ? 0.5f : 1f);
        float a = Math.Min(1, -timer + (t.Write.Length > 40 ? 5 : 4));
        t.Renderer.Alpha = a;
        bande.Renderer.Alpha = a;
        if (a < 0)
        {
            Destroy();
        }
    }

    public override void AfterDraw()
    {
        bande.Draw();
        t.Draw();
    }
}


public class ParticleFlaque : Clone
{
    private float radius;
    private readonly Vector pos;
    public ParticleFlaque(ref Vector pos, float startRad)
    {
        radius = startRad;
        Layer = -10;
        this.pos = pos;
    }

    public override void Update()
    {
        radius += 200 * Time.FrameTime;
        if (radius > 50)
            Destroy();
    }

    public override void DrawAdditive()
    {
        Pen.Circle(pos, radius, color: Pico8.Pink, alpha: 1f - (radius / 50f));
    }
}

public class ParticleWink : Clone
{
    private readonly byte number = 2;
    Object[] p;
    Vector[] vitesse;
    public ParticleWink(ref Vector pos)
    {
        float rnd = PlayerManager.random.NextInt64(0, 180);
        p = new Object[number];
        vitesse = new Vector[number];
        for (byte i = 0; i < number; i++)
        {
            p[i] = new Object();
            p[i].Space.Scale = new Vector(8, 1);
            float rot = ((float)i / (float)number * 360) + rnd;
            p[i].Space.rotation = rot;
            vitesse[i] = new Vector(Math.Cos(-rot), Math.Sin(-rot));
            p[i].Space.Position = pos + vitesse[i] * 5;
        }
    }

    private const float time = 0.1f;
    private float timer = 0f;
    public override void Update()
    {
        for (byte i = 0; i < number; i++)
        {
            p[i].Space.Position +=
                vitesse[i] * Time.FrameTime * 100;
        }
        timer += Time.FrameTime;
        if (timer > time)
        {
            p = null;
            vitesse = null;
            Destroy();
        }
    }

    public override void Draw()
    {
        foreach (Object obj in p)
            obj.Draw();
    }
}

public class ParticleSmoke : Clone
{
    private Vector pos;
    private float alpha = 1f;
    private readonly Vector vitesse;
    private float delay = 0.5f;

    public ParticleSmoke(Vector startpos, short direction) : base()
    {
        vitesse.x = Math.Cos(direction);
        vitesse.y = Math.Sin(direction);
        pos = startpos;
    }

    public override void Update()
    {
        pos += vitesse * 200 * Time.FrameTime;
        delay += -Time.FrameTime * 3;
        alpha = delay / 0.5f;
        if (delay <= 0)
            Destroy();
    }

    public override void BeforeDraw()
    {
        Pen.Point(pos, alpha: alpha, color: Pico8.Yellow);
    }
}

public class ParticleGlow : Clone
{
    readonly Vector vitesse;
    Vector pos;
    readonly Color color;
    float alpha = 1f;

    public ParticleGlow(Vector pos, Color color)
    {
        this.pos = pos + new Vector(
            PlayerManager.random.NextInt64(-6, 6),
            PlayerManager.random.NextInt64(-6, 6));
        vitesse = new Vector(
            PlayerManager.random.NextInt64(-100, 100),
            PlayerManager.random.NextInt64(-100, 100)) / 8f;
        this.color = color;
    }
    float timer = 0f;

    public override void Update()
    {
        pos += vitesse * Time.FrameTime;
        timer += Time.FrameTime;
        alpha = 1 - (timer / 0.5f);
        if (timer > 0.5f)
            Destroy();
    }

    public override void DrawAdditive()
    {
        Pen.Point(pos - Camera.Position, color: color, alpha: alpha);
    }
}


public class ParticleBall : Clone
{
    readonly Vector vitesse;
    Vector pos;
    Color color;
    float alpha = 1f;

    public ParticleBall(ref Vector pos, ref Vector vit, ref Color color)
    {
        this.pos = pos + new Vector(
            PlayerManager.random.NextInt64(-3, 3),
            PlayerManager.random.NextInt64(-3, 3));
        vitesse = new Vector(
            -vit.x + PlayerManager.random.NextInt64(-100, 100),
            -vit.y + PlayerManager.random.NextInt64(-100, 100)) / 4f;
        this.color = color;
    }

    public ParticleBall(Vector pos, Vector vit, Color color)
    {
        this.pos = pos + new Vector(
            PlayerManager.random.NextInt64(-3, 3),
            PlayerManager.random.NextInt64(-3, 3));
        vitesse = new Vector(
            -vit.x + PlayerManager.random.NextInt64(-100, 100),
            -vit.y + PlayerManager.random.NextInt64(-100, 100)) / 4f;
        this.color = color;
    }
    float timer = 0f;

    public override void Update()
    {
        pos += vitesse * Time.FrameTime;
        timer += Time.FrameTime;
        alpha = 1 -(timer / 0.5f);
        if (timer > 0.5f)
        {
            color = null;
            Destroy();
        }
    }

    public override void DrawAdditive()
    {
        Pen.Point(pos - Camera.Position, color: color, alpha: alpha);
    }
}