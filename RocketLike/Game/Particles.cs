using FriteCollection.Entity;
using FriteCollection.Scripting;
using FriteCollection.Tools.SpriteSheet;
using FriteCollection.Math;

namespace RocketLike
{
    public class ParticleSmoke : Clone
    {
        private static readonly SpriteSheet sheet
            = new SpriteSheet(Open.Texture("Game/particles"), 6, 6);
        private static System.Random rnd = new System.Random();

        private Object p;

        private readonly Vector vitesse;
        private float delay = 0.3f;
        private readonly short rotVit;

        public ParticleSmoke(Vector startpos, short direction) : base()
        {
            p = new Object();
            p.Renderer.Texture = sheet[(int)rnd.NextInt64(0, 1), 0];
            p.Renderer.Color = Pico8.Yellow;
            p.Space.Scale = new Vector(3.5f, 3.5f);
            vitesse.x = Math.Cos(direction);
            vitesse.y = Math.Sin(direction);
            p.Space.Position = startpos;
            p.Renderer.Alpha = 1f;
            rotVit = (short)rnd.NextInt64(0, 1);
        }

        public override void Update()
        {
            p.Space.Position += vitesse * 200 * Time.FrameTime;
            p.Space.rotation += rotVit * 100 * Time.FrameTime;
            delay += -Time.FrameTime * 4;

            if (delay <= 0)
                Destroy();
        }

        public override void Draw()
        {
            p.Draw();
        }
    }
    public class ParticleBall : Clone
    {
        Object p = new Object();

        public ParticleBall(Space space)
        {
            p.Space = space.Copy();
            p.Renderer.Texture = Ball.texture;
            p.Renderer.Color = Pico8.Yellow;
            p.Space.Scale.x /= 2f;
        }
        float timer = 0f;

        public override void Update()
        {
            timer+= Time.FrameTime;
            if (timer > 0.5f)
                Destroy();
        }

        public override void Draw()
        {
            p.Draw();
        }
    }
}