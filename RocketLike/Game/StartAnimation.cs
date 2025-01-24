using FriteCollection.Entity;
using FriteCollection.Scripting;
using FriteCollection.Audio;
using System;

namespace RocketLike;

public class StartAnimation : Script
{
    public StartAnimation() : base(Scenes.Game, false) { }

    private GameEffects effects;

    public event Action START;

    private SoundEffect count, top;
    private bool first = true;
    private bool started = false;

    private float f(float x)
    {
        return -35 * ((x - 3) * (x - 3)) + MapManager.camPosY;
    }

    void But(bool sideRight, Ball.States state, bool csc, byte t, bool combo)
    {
        if (combo)
        {
            new Sequence(() => t * 0.1f, () =>
            {
                new Num(sideRight ? "player 1 : COMBO (+1)" : "player 2 : COMBO (+1)", 3);
                if (sideRight)
                    GameData.sp1++;
                else
                    GameData.sp2++;
                UIManager.setScore();
                return 0;
            });
        }
    }

    private ReplayManager replay;

    public override void BeforeStart()
    {
        (GameManager.GetScript("Ball") as Ball).BUT += But;
        effects = GameManager.GetScript("GameEffects") as GameEffects;
        Layer = 20;
        replay = GameManager.GetScript("ReplayManager") as ReplayManager;
        replay.RESTART += AfterStart;
    }

    float Decompte(byte num)
    {
        new Num(num.ToString(), 0.5f);
        return 0.25f;
    }
    void shake()
    {
        effects.Shake(3, 0.3f);
    }

    public override void Start()
    {
        count = Open.SoundEffect("Sfx/countDown");
        count.Volume = 0.5f;
        top = Open.SoundEffect("Sfx/start");
    }

    bool done = false;

    public override void AfterStart()
    {
        if (!first)
        {
            Camera.Position = new Vector(0, MapManager.camPosY);
            new Sequence(
                () => 1f,
        () => Decompte(3),
        () =>
        {
            shake();
            count.Play();
            return 0.5f;
        },
        () => Decompte(2),
        () =>
        {
            shake();
            count.Play();
            return 0.5f;
        },
        () => Decompte(1),
        () =>
        {
            shake();
            count.Play();
            return 0.75f;
        },
        () => { if (START is not null) START(); top.Play(); return 0; });
        }
    }

    public override void BeforeUpdate()
    {
        if (first)
        {
            if (Time.TargetTimer <= 3)
            Camera.Position = new Vector(0, f(Time.TargetTimer));
            else if (!done)
            {
                first = false;
                done = true;
                new Sequence(
        () => 1f,
        () => Decompte(3),
        () =>
        {
            shake();
            count.Play();
            return 0.5f;
        },
        () => Decompte(2),
        () =>
        {
            shake();
            count.Play();
            return 0.5f;
        },
        () => Decompte(1),
        () =>
        {
            shake();
            count.Play();
            return 0.75f;
        },
        () => { if (START is not null) START(); top.Play(); return 0; });
            }
        }
    }

    public override void Draw()
    {

    }

    public override void Dispose()
    {
        effects.Dispose();
        effects = null;
        count.Dispose();
        top.Dispose();
    }
}

public class Num : Clone
{
    Text t;
    readonly float target;
    readonly float delay;
    public Num(string txt, float delay)
    {
        target = -(Screen.widht / 2f) - 30;
        Layer = 20;
        t = new Text(GameManager.GameFont);
        t.Write = txt;
        t.Space.Position.x = (Screen.widht / 2f);
        this.delay = delay;
    }

    const float dt = 0.25f;
    float timer = 0f;
    public override void Update()
    {
        if (t is not null)
        {
            timer += Time.FrameTime;
            if (timer < dt)
            {
                t.Space.Position.x = ((Screen.widht / 2f) + 30) * (1 - (timer / dt));
                t.Renderer.Alpha = timer / 0.25f;
            }
            else if (timer >= dt && timer <= delay + dt)
            {
                t.Space.Position.x = 0;
                t.Renderer.Alpha = 1;
            }
            else
            {
                t.Space.Position.x = target * ((timer - (delay + dt)) * 4);
                t.Renderer.Alpha = (1 - ((timer - (delay + dt)) * 4));
            }
            if (timer > delay + (dt * 2))
                Destroy();
        }
    }

    public override void AfterDraw()
    {
        if (t is not null)
            t.Draw();
    }

    public override void Dispose()
    {
        t = null;
    }
}