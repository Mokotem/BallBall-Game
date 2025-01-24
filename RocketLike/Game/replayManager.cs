using FriteCollection.Scripting;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FriteCollection.UI;

namespace RocketLike;

public class ReplayManager : Script
{
    public ReplayManager() : base(Scenes.Game) { }
    private Ball ball;
    private Texture2D[] textures;
    private uint current;

    private byte REPLAY_FPS => FriteModel.MonoGame.REPLAY_FPS;
    private ushort REPLAY_LENGHT => FriteModel.MonoGame.REPLAY_LENGHT;

    public delegate void Restart();
    public event Restart RESTART;

    public override void AfterStart()
    {
        if (GameData.replay)
        {
            Layer = 60;
            show = false;
            ball = GameManager.GetScript("Ball") as Ball;
            ball.BUT += OnBut;

            panel = new Panel(UIManager.tile, new Space(
                Bounds.Center,
                Extend.None,
                new(
                    GetSeetings.Settings.GameFixeWidth / 2 + 10,
                    GetSeetings.Settings.GameFixeHeight / 2 + 15
                ),
                new(0, -10)
                ));
            replay = new Image(FriteCollection.Entity.Renderer.DefaultTexture,
                new Space(Bounds.Top, Extend.Full, new(-10, -28), new(0, 5)), panel);
            replay.Color = FriteCollection.Graphics.Color.Black;
            key = new Text(">" + GameData.SAVE.i[8].ToString() + "<   to skip", new Space(Bounds.Bottom, Extend.Horizontal,
                new(0, 10), new(0, -8)), panel);
            panel.Add(replay);
            panel.Add(key);
        }
    }

    private void OnBut(bool side, Ball.States state, bool csc, byte delay, bool combo)
    {
        if (GameData.replay)
        {
            new Sequence(() => 2f, () =>
        {
            textures = GameManager.instance.Replay;
            AskForReplay();
            return 0;
        });
        }
        else
        {
            new Sequence(() => 2f, () =>
            {
                RESTART();
                return 0;
            });
        }
    }

    Panel panel;
    Image replay;
    Text key;
    private static bool show;

    public static bool REPLAYING => show;

    private void AskForReplay()
    {
        current = 0;
        replay.Color = FriteCollection.Graphics.Color.White;
        replay.Texture = textures[0];
        show = true;
    }
    bool pre = false;
    byte av = 0;
    bool restarted = false;

    public override void BeforeDraw()
    {
        if (GameData.replay)
        {
            if (show)
            {
                av++;
                if (av > GetSeetings.Settings.FPS / REPLAY_FPS)
                {
                    current++;
                    if (current >= textures.Length)
                    {
                        current = 0;
                    }
                    replay.Texture = textures[current];
                    av = 0;
                }

                KeyboardState ks = Keyboard.GetState();
                if (ks.GetPressedKeyCount() == 1 && ks.IsKeyDown(GameData.SAVE.i[8]) && !pre)
                {
                    restarted = true;
                }
                pre = ks.IsKeyDown(GameData.SAVE.i[8]);
            }
        }
    }

    public override void BeforeUpdate()
    {
        if (restarted == true)
        {
            restarted = false;
            show = false;
            Time.SpaceTime = 1;
            RESTART();
        }
    }

    public override void DrawUI()
    {
        if (GameData.replay)
        {
            if (show)
            {
                panel.Draw();
            }
        }
    }

    public override void Dispose()
    {
        ball = null;
    }
}