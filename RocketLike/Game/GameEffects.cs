using FriteCollection.Entity;
using FriteCollection.Scripting;
using FriteCollection.Graphics;

namespace RocketLike;

public class GameEffects : Script
{
    public GameEffects() : base(Scenes.Game) { }

    public override void BeforeStart()
    {
        (GameManager.GetScript("Ball") as Ball).FRAPPE += Frappe;
        (GameManager.GetScript("Ball") as Ball).BUT += But;
    }

    private readonly System.Random random = new System.Random();


    void But(bool sideRight, Ball.States state)
    {
        Shake(5, 0.5f);
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
        }
        if (state == Ball.States.Cyan)
        {
            frappeDir = _dir;
            frappeTimer = 1f;
            frappeTimerMemo = 1f;
            Shake(4, 1);
        }
    }

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
            Time.SpaceTime = 1f;
        }
        if (timerShake > 0)
        {
            float f = force * (timerShake / timerShakeMemo);
            Camera.Position = new Vector(random.Next(-1, 1) * f, (random.Next(-1, 1) * f) + MapManager.camPosY);
            timerShake += -Time.FixedFrameTime;
        }
    }

    public override void Dispose()
    {

    }
}