using FriteCollection.Entity;
using FriteCollection.Scripting;
using FriteCollection.Graphics;
using FriteCollection.Tools.Shaders;
using FriteCollection.Tools.Pen;

namespace RocketLike;

public class GameEffects : Script
{
    public GameEffects() : base(Scenes.Game) { }

    private Ball ball;
    private List<Vector> locations1, locations2;

    public override void BeforeStart()
    {
        ball = GameManager.GetScript("Ball") as Ball;
        ball.FRAPPE += Frappe;
        ball.BUT += But;
    }

    public override void Start()
    {
        locations1 = new List<Vector>();
        locations1 =
            (MapManager.tilemap.GetPos(0, 7)
            + MapManager.tilemap.GetPos(0, 8)
            + MapManager.tilemap.GetPos(0, 9)
            + MapManager.tilemap.GetPos(0, 13)
            + MapManager.tilemap.GetPos(1, 13)
            + MapManager.tilemap.GetPos(2, 13));
        locations2 = new List<Vector>();
        locations2 =
            (MapManager.tilemap.GetPos(1, 7)
            + MapManager.tilemap.GetPos(1, 8)
            + MapManager.tilemap.GetPos(1, 9)
            + MapManager.tilemap.GetPos(0, 14)
            + MapManager.tilemap.GetPos(1, 14)
            + MapManager.tilemap.GetPos(2, 14));
    }

    private readonly System.Random random = new System.Random();

    void But(bool sideRight, Ball.States state)
    {
        if ((int)state > 0)
            new ButShockWave(!sideRight);
    }

    byte force;
    float timerShake;
    float timerShakeMemo;

    public void Shake(byte f, float t)
    {
        force = f;
        timerShake = t;
        timerShakeMemo = t;
    }

    Vector frappeDir;
    float frappeTimerMemo, frappeTimer;
    Ball.States frappeState;

    void Frappe(Vector _dir, Ball.States state)
    {
        frappeState = state;
        if (state == Ball.States.Golden)
        {
            frappeDir = _dir;
            frappeTimer = 0.2f;
            frappeTimerMemo = 0.2f;
            new FrappeBulge(ref ball.b.Space.Position, 2f);
        }
        if (state == Ball.States.Cyan)
        {
            frappeDir = _dir;
            frappeTimer = 1f;
            frappeTimerMemo = 1f;
            new FrappeBulge(ref ball.b.Space.Position, 0.5f);
            Shake(4, 1);
        }
    }

    float partTimer = 1f, windTimer = 1f;

    public override void Update()
    {
        Camera.Position = new Vector(0, MapManager.camPosY);
        if (frappeTimer > 0)
        {
            //  1f -> 0f
            float dt = (frappeTimer / frappeTimerMemo);
            float dtt = (1 - dt) * (1 - dt) * (1 - dt) * (1 - dt) * (1 - dt) * (1 - dt);
            if (frappeState == Ball.States.Golden)
            {
                float c = 5 * (frappeTimer / frappeTimerMemo);
                Camera.Position = new Vector(frappeDir.x * c, (frappeDir.y * c) + MapManager.camPosY);
                Time.SpaceTime = (dtt + 2) / 3f;
            }
            else if (frappeState == Ball.States.Cyan)
            {
                Time.SpaceTime = dtt;
                Cube3D.color = (Pico8.DarkBlue / 4f * (1 - dtt)) + (Pico8.DarkBlue * dtt);
                Screen.backGround = (Pico8.Lavender / 4f * (1 - dtt)) + (Pico8.Lavender * dtt);
                MapManager.tilemap.Color = (Color.White / 4f * (1 - dtt)) + (Color.White * dtt);
            }
            frappeTimer += -Time.FixedFrameTime;
        }
        else
        {
            Cube3D.color = Pico8.DarkBlue;
            Screen.backGround = Pico8.Lavender;
            MapManager.tilemap.Color = Color.White;
            if (ball.Activee)
            Time.SpaceTime = 1f;
        }
        if (timerShake > 0)
        {
            float f = force * (timerShake / timerShakeMemo);
            Camera.Position = new Vector(random.Next(-1, 1) * f, (random.Next(-1, 1) * f) + MapManager.camPosY);
            timerShake += -Time.FixedFrameTime;
        }

        partTimer += Time.FrameTime;
        windTimer += Time.FrameTime;
        if (partTimer > 0.1f)
        {
            foreach (Vector pos in locations1)
            {
                new ParticleGlow(pos, Pico8.Green);
            }
            foreach (Vector pos in locations2)
            {
                new ParticleGlow(pos, Pico8.Red);
            }
            partTimer = 0f;
        }

        if (windTimer > 0.01f)
        {
            new WindParticle();
            windTimer = 0f;
        }
    }

    public override void Dispose()
    {

    }
}

public class WindParticle : Clone
{
    private readonly Color c;

    private float rnd => PlayerManager.random.NextSingle();

    private Vector position;
    private readonly bool isBg;
    private Vector vitesse, acc;
    private float alpha;

    public WindParticle()
    {
        Layer = -15;
        position = new Vector(
            ((rnd * 2) - 1) * Screen.widht,
            -(Screen.widht / 2f) - 10);

        vitesse.x = (rnd - 0.1f) * 30 + 10;
        vitesse.y = (rnd * 100) + 5;
        alpha = 1f;
        isBg = rnd > 0.5f;
        acc = new Vector((rnd * 2) - 1, (rnd * 2) - 1);
        c = Pico8.Yellow;
    }

    public override void Update()
    {
        vitesse += acc * Time.FrameTime * 20;
        position += vitesse * Time.FrameTime;
        alpha += -Time.FrameTime * 0.25f;
        if (alpha < 0f)
            Destroy();
    }

    public override void Draw()
    {
        if (isBg)
            Pen.Point(position, color: c, alpha: alpha);
    }

    public override void DrawAdditive()
    {
        if (!isBg)
            Pen.Point(position, color: c, alpha: alpha);
    }
}


public class FrappeBulge : Clone
{
    private Shader.Bulge wave;
    private readonly float vol;
    public FrappeBulge(ref Vector pos, float v)
    {
        wave = new Shader.Bulge();
        wave.Position = pos;
        wave.Volume = 1f;
        wave.Value = v;
        Shader.Start();
        timer = 0f;
        vol = v;
    }

    private float timer;

    public override void Update()
    {
        timer += Time.FrameTime;
        wave.Value = (1 - (timer / 0.5f)) / 4f / vol;
        if (Shader.Stopped)
            Destroy();
        if (timer > 0.5f)
        {
            Shader.Stop();
            Destroy();
        }
    }
}

public class ButShockWave : Clone
{
    private readonly bool sideLeft;
    private Shader.ShockWave2 wave;
    public ButShockWave(bool sideLeft)
    { 
        this.sideLeft = sideLeft;
        wave = new Shader.ShockWave2();
        if (sideLeft)
        {
            wave.Value = 0;
            timer = 0;
        }
        else
        {
            wave.Value = 1;
            timer = 1f;
        }
        Shader.Start();
    }

    private float timer;

    public override void Update()
    {
        Shader.Start();
        if (sideLeft)
        {
            timer += Time.FrameTime;
            wave.Value = timer;
            if (timer > 2)
            {
                Shader.Stop();
                Destroy();
            }
        }
        else
        {
            timer -= Time.FrameTime;
            wave.Value = timer;
            if (timer < -1)
            {
                Shader.Stop();
                Destroy();
            }
        }
        if (IsDestroyed)
            Shader.Stop();
    }
}