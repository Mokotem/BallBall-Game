using FriteCollection.Entity;
using FriteCollection.Scripting;
using FriteCollection.Tools.Pen;
using FriteCollection.Math;
using FriteCollection.Graphics;

namespace RocketLike;

public class Cube3D : Script
{
    public Cube3D() : base(Scenes.Game) { }
    class Point
    {
        public readonly bool b;
        public float x, y, z;
        public Point(float x, float y, float z, bool big)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.b = big;
        }

        public Vector Get2DPos()
        {
            float a = b ? 1f / (z - 1.5f) : 5f / (z - 4);
            return new Vector(x * a * (b ? 5 : 2f), y * a * (b ? 3f : 1f)) * (b ? 50f : 20f);
        }
    }

    Point[] points;
    Vector[] result;
    int[,] edges;
    bool room;

    public override void Start()
    {
        room = PlayerManager.random.Next(2) == 0;
        Layer = -15;
        points = new Point[8]
        {
            new Point(-1, 1, -1, true),
            new Point(1, 1, -1, true),
            new Point(1, -1, -1, true),
            new Point(-1, -1, -1, true),

            new Point(-1, 1, 1, true),
            new Point(1, 1, 1, true),
            new Point(1, -1, 1, true),
            new Point(-1, -1, 1, true)
        };
        edges = new int[12, 2]
        {
            {0, 1},
            {1, 2},
            {2, 3},
            {0, 3},

            {4, 5},
            {5, 6},
            {6, 7},
            {4, 7},

            {0, 4},
            {1, 5},
            {2, 6},
            {3, 7}
        };
    }

    public const float r = 10;
    public static float roty = r;

    private static readonly float sqrt2 = 1.41421356237f;
    public static Color color = BackGround.color2;

    delegate float GetAttr(Point p);

    private Vector MakeRot(Point p, GetAttr x, GetAttr y, float add)
    {
        float angle = Math.Atan(y(p) / x(p));
        if (x(p) < 0)
            angle += 180;
        Vector dir = new Vector(
            Math.Cos(angle + (add * Time.FixedFrameTime)),
            Math.Sin(angle + (add * Time.FixedFrameTime))) * sqrt2;
        return dir;
    }

    public override void AfterUpdate()
    {
        for (byte i = 0; i < 8; i++)
        {
            Vector dir;

            //rot Y
            dir = MakeRot(points[i], (Point p) => p.x, (Point p) => p.z, roty * (room ? 1 : -1) * 2);
            points[i].x = dir.x;
            points[i].z = dir.y;
        }
        result = new Vector[8];
        for (byte i = 0; i < 8; i++)
        {
            result[i] = points[i].Get2DPos();
        }
    }

    public override void BeforeDraw()
    {
        for (byte i = 0; i < 12; i++)
        {
            if (points[edges[i, 0]].z < 0.2f || points[edges[i, 1]].z < 0.2f)
            {
                Pen.Line(result[edges[i, 0]], result[edges[i, 1]], color: color);
            }
        }
    }
}