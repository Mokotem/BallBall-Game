using FriteCollection.Entity;
using FriteCollection.Scripting;
using System;
using System.Runtime.InteropServices.WindowsRuntime;

namespace RocketLike;

public class StartAnimation : Script
{
    public StartAnimation() : base(Scenes.Game) { }

    private GameEffects effects;

    public event Action START;

    void But(bool sideRight, Ball.States state)
    {
        new Sequence(() => 4, () => { Start(); return 0; });
    }

    public override void BeforeStart()
    {
        (GameManager.GetScript("Ball") as Ball).BUT += But;
        effects = GameManager.GetScript("GameEffects") as GameEffects;
        Layer = 20;
    }

    float Decompte(byte num)
    {
        new Num(num);
        return 0.25f;
    }
    float shake()
    {
        effects.Shake(2, 0.3f);
        return 0.75f;
    }

    public override void Start()
    {
        new Sequence(
            () => 1f,
            () => Decompte(3),
            shake,
            () => Decompte(2),
            shake,
            () => Decompte(1),
            shake,
            () => { START(); return 0; });
    }

    public override void Draw()
    {

    }

    public override void Dispose()
    {

    }
}

public class Num : Clone
{
    Text t;
    float target;
    public Num(byte num)
    {
        target = -(Screen.widht / 2f) - 30;
        Layer = 20;
        t = new Text(GameManager.GameFont, num.ToString());
        t.Space.Position.x = (Screen.widht / 2f) + 30;
    }

    const float dt = 1f / 4f;
    float timer = 0f;
    public override void Update()
    {
        if (t is not null)
        {
            timer += Time.FrameTime;
            if (timer < dt)
            {
                t.Space.Position.x = ((Screen.widht / 2f) + 30) * (1 - (timer / dt));
            }
            else if (timer >= dt && timer <= 3 * dt)
            {
                t.Space.Position.x = 0;
            }
            else
            {
                t.Space.Position.x = target * ((timer - (dt * 3)) / dt);
            }
            if (timer > 1)
                Destroy();
        }
    }

    public override void AfterDraw()
    {
        if (t is not null)
            t.Draw();
    }
}