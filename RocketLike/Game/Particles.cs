using FriteCollection.Entity;
using FriteCollection.Scripting;
using FriteCollection.Tools.SpriteSheet;
using FriteCollection.Tools.Pen;
using FriteCollection.Math;
using FriteCollection.Graphics;

namespace RocketLike
{
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
        private const byte number = 2;
        readonly Object[] p;
        readonly Vector[] vitesse;
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
                Destroy();
        }

        public override void Draw()
        {
            foreach (Object obj in p)
                obj.Draw();
        }
    }

    public class ParticleSmoke : Clone
    {
        private Object p;

        private readonly Vector vitesse;
        private float delay = 0.3f;

        public ParticleSmoke(Vector startpos, short direction) : base()
        {
            p = new Object();
            p.Renderer.Color = Pico8.Yellow;
            p.Space.Scale = new Vector(1f, 1f);
            vitesse.x = Math.Cos(direction);
            vitesse.y = Math.Sin(direction);
            p.Space.Position = startpos;
            p.Renderer.Alpha = 1f;
        }

        public override void Update()
        {
            p.Space.Position += vitesse * 200 * Time.FrameTime;
            delay += -Time.FrameTime * 4;

            if (delay <= 0)
                Destroy();
        }

        public override void DrawAdditive()
        {
            p.Draw();
        }
    }
    public class ParticleBall : Clone
    {
        Object p = new Object();
        readonly Vector vitesse;
        public ParticleBall(ref Vector pos, ref Vector vit, ref Color color)
        {
            p.Space.Position = pos + new Vector(
                PlayerManager.random.NextInt64(-3, 3),
                PlayerManager.random.NextInt64(-3, 3));
            p.Renderer.Color = color;
            p.Space.Scale = new Vector(1, 1);
            vitesse = new Vector(
                -vit.x + PlayerManager.random.NextInt64(-100, 100),
                -vit.y + PlayerManager.random.NextInt64(-100, 100)) / 4f;
        }
        float timer = 0f;

        public override void Update()
        {
            p.Space.Position += vitesse * Time.FrameTime;
            timer += Time.FrameTime;
            p.Renderer.Alpha = 1 -(timer / 0.5f);
            if (timer > 0.5f)
                Destroy();
        }

        public override void DrawAdditive()
        {
            p.Draw();
        }
    }
}