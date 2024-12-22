using FriteCollection.Entity;
using FriteCollection.Scripting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FriteCollection.Tools.TileMap;

public struct Vector
{
    public Vector()
    {
        width = 0; height = 0;
    }
    public Vector(int width, int height)
    {
        this.width = width; this.height = height;
    }

    public int width, height;
}

public class TileSet : IDisposable
{
    private HitBox.Rectangle[,] _hitReplaces;
    private Entity.Object[,] _entReplaces;
    public readonly List<Entity.Object> entities;
    public List<int> _dontDraw = new List<int>();
    public int Xlenght { get; private set; }
    public int Ylenght { get; private set; }

    public HitBox.Rectangle[,] ReplaceHitbox
    {
        get
        {
            return _hitReplaces;
        }
    }

    public Entity.Object[,] ReplaceEntity
    {
        get
        {
            return _entReplaces;
        }
    }

    public void DontDraw(ushort i, ushort j)
    {
        _dontDraw.Add(i + (j * Xlenght));
    }

    public TileSet(Texture2D texture)
    {
        entities = new List<Entity.Object>();
        this._texture = texture;
        sheet.width = texture.Width;
        sheet.height = texture.Height;
    }
    private Texture2D _texture;
    public Texture2D Texture
    {
        get { return _texture; }
    }

    private void Apply()
    {
        Xlenght =
        (sheet.width + _tileSeparation.width) / (_tileSize.width + _tileSeparation.width);

        Ylenght =
            (sheet.height + _tileSeparation.height) / (_tileSize.height + _tileSeparation.height);

        _hitReplaces = new HitBox.Rectangle[Xlenght, Ylenght];
        _entReplaces = new Entity.Object[Xlenght, Ylenght];
    }
    public readonly Vector sheet;
    private Vector _tileSize;
    public Vector TileSize
    {
        get { return _tileSize; }
        set
        {
            _tileSize = value;
            Apply();
        }
    }

    private Vector _tileSeparation = new(0, 0);
    public Vector TileSeparation
    {
        get { return _tileSeparation; }
        set
        {
            _tileSeparation = value;
            Apply();
        }
    }
    private Vector _tileMargin = new(0, 0);
    public Vector TileMargin
    {
        get { return _tileMargin; }
        set
        {
            _tileMargin = value;
            Apply();
        }
    }

    public Rectangle GetRectangle(int index)
    {
        Vector positon = new Vector
        (
            (index * (_tileSize.width + _tileSeparation.width)) % (sheet.width + _tileSeparation.width),
            Math.Math.Quotient(index, ((sheet.width + _tileSeparation.width) / (_tileSize.width + _tileSeparation.width))) * (_tileSize.height + _tileSeparation.height)
        );
        return new Rectangle
        (
            positon.width,
            positon.height,
            _tileSize.width,
            _tileSize.height
        );
    }

    public void Dispose()
    {
        _texture.Dispose();
    }
}

public class TileMap : IDisposable
{
    int xCount;
    int yCount;

    public delegate void DoAt(Vector pos);

    public TileMap(TileSet sheet,
        OgmoFile file,
        Graphics.Color background = null,
        Texture2D backgroundTexture = null,
        bool mergeHitBoxes = true)
    {
        HitBox.Rectangle[,] _hitboxData;
        Graphics.Color bg;
        if (background != null)
        {
            bg = background;
        }
        else { bg = new(0, 0, 0); }
        _sheet = sheet;
        _file = file;

        SpriteBatch batch = FriteModel.MonoGame.instance.SpriteBatch;

        xCount = (file.width / file.layers[0].gridCellWidth);
        yCount = (file.height / file.layers[0].gridCellHeight);

        _renderTarget = new RenderTarget2D
        (
            FriteModel.MonoGame.instance.GraphicsDevice,
            xCount * sheet.TileSize.width,
            yCount * sheet.TileSize.height
        );

        FriteModel.MonoGame.instance.GraphicsDevice.SetRenderTarget(_renderTarget);
        FriteModel.MonoGame.instance.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);
        batch.Begin(samplerState: SamplerState.PointClamp);
        if (backgroundTexture != null)
        {
            batch.Draw
            (
                backgroundTexture,
                new Rectangle(0, 0,
                xCount * sheet.TileSize.width,
                yCount * sheet.TileSize.height),
                null,
                new Color(bg.RGB.R, bg.RGB.G, bg.RGB.B)
            );
        }

        _hitboxData = new HitBox.Rectangle[xCount, yCount];

        foreach (OgmoLayer layer in file.layers)
        {
            for (int i = 0; i < layer.data.Length; i++)
            {
                if (layer.data[i] >= 0)
                {
                    int x = i % xCount;
                    int y = i / xCount;

                    int sx = layer.data[i] % _sheet.Xlenght;
                    int sy = layer.data[i] / _sheet.Xlenght;

                    if (sheet.ReplaceHitbox[sx, sy] is null
                        && sheet.ReplaceEntity[sx, sy] is null
                        && sheet._dontDraw.Contains(layer.data[i]) == false)
                    {
                        batch.Draw
                        (
                            sheet.Texture,
                            new Rectangle
                            (
                                x * sheet.TileSize.width,
                                y * sheet.TileSize.height,
                                sheet.TileSize.width,
                                sheet.TileSize.height
                            ),
                            sheet.GetRectangle(layer.data[i]),
                            Microsoft.Xna.Framework.Color.White
                        );
                    }
                    else
                    {
                        if (!(sheet.ReplaceHitbox[sx, sy] is null))
                        {
                            _hitboxData[x, y] = sheet.ReplaceHitbox[sx, sy];
                        }

                        if (!(sheet.ReplaceEntity[sx, sy] is null))
                        {
                            Entity.Vector pos = new Entity.Vector();
                            pos = new Entity.Vector
                            (
                            -(file.width / 2f) + (layer.gridCellWidth / 2f),
                                (file.height / 2f) - (layer.gridCellHeight / 2f)
                            );
                            Entity.Object obj = sheet.ReplaceEntity[x, y].Copy();
                            obj.Space.Position += Position;
                            obj.Space.GridOrigin = Bounds.Center;
                            obj.Space.Position += pos;
                            obj.Space.Position.x += x % (file.width / layer.gridCellWidth) * layer.gridCellWidth;
                            obj.Space.Position.y -= System.Math.DivRem(x * layer.gridCellWidth, file.width).Quotient * sheet.TileSize.height;
                            obj.Renderer.hide = false;
                            sheet.entities.Add(obj);
                        }
                    }
                }
            }
        }
        batch.End();

        if (mergeHitBoxes)
        {
            MergeHitBoxes(ref _hitboxData);
        }
        else
        {
            for (int x = 0; x < xCount; x++)
            {
                for (int y = 0; y < yCount; y++)
                {
                    if (_hitboxData[x, y] is not null)
                    {
                        HitBox.Rectangle hit = _hitboxData[x, y].Copy();
                        hit.Active = true;
                        hit.PositionOffset += new Entity.Vector
                        (
                            -(_file.width / 2f) + (_sheet.TileSize.width / 2f),
                            (_file.height / 2f) - (_sheet.TileSize.height / 2f)
                        );
                        hit.PositionOffset.x += x * _sheet.TileSize.width;
                        hit.PositionOffset.y -= y * _sheet.TileSize.height;
                        hit.LockSize = new Entity.Vector(
                            _hitboxData[x, y].LockSize.x,
                            _hitboxData[x, y].LockSize.y);
                    }
                }
            }
        }
        Color = Graphics.Color.White;
    }

    public Entity.Vector GetPos(ushort i, ushort j)
    {
        int target = i + (j * _sheet.Xlenght);
        foreach (OgmoLayer layer in _file.layers)
        {
            for(int k = 0; k < layer.data.Length; k++)
            {
                if (layer.data[k] == target)
                {
                    return new Entity.Vector
                        (
                           (k % xCount) * _sheet.TileSize.width - (_file.width - _sheet.TileSize.width) / 2f,
                           -(k / xCount) * _sheet.TileSize.height + (_file.height + _sheet.TileSize.height) / 2f
                        );
                }
            }
        }

        return Entity.Vector.Zero;
    }

    private List<HitBox.Rectangle> MergeHitBoxes(ref HitBox.Rectangle[,] lst)
    {
        List<HitBox.Rectangle> result = new List<HitBox.Rectangle>();
        int i = -1;
        while (i + 1 < xCount * yCount)
        {
            i++;
            long x = i % xCount;
            long y = i / xCount;

            HitBox.Rectangle hit1 = lst[x, y];

            if (hit1 is not null)
            {
                int width = 1;
                int height = 1;

                while (x + width < xCount
                    && lst[x + width, y] is not null
                    && lst[x + width, y].tag == hit1.tag
                    && lst[x + width, y].Layer == hit1.Layer
                    && lst[x + width, y].LockSize.y == hit1.LockSize.y)
                {
                    lst[x + width, y] = null;
                    width++;
                }

                bool Cond(ref HitBox.Rectangle[,] h)
                {
                    if (y + height >= yCount)
                        return false;
                    HitBox.Rectangle h2 = hit1;
                    for (int k = 0; k < width; k++)
                    {
                        HitBox.Rectangle h1 = h[x + k, y + height];
                        if (h1 is null
                           || h1.tag != h2.tag
                           || h1.Layer != h2.Layer
                           || h1.LockSize.x != h2.LockSize.x)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                while (Cond(ref lst))
                {
                    for (int k = 0; k < width; k++)
                    {
                        lst[x + k, y + height] = null;
                    }
                    height++;
                }

                HitBox.Rectangle hit = hit1.Copy();
                hit.Layer = hit1.Layer;
                hit.Active = true;
                hit.PositionOffset += new Entity.Vector
                (
                    -(_file.width / 2f) + (_sheet.TileSize.width / 2f),
                    (_file.height / 2f) - (_sheet.TileSize.height / 2f)
                );
                hit.PositionOffset.x += (x + ((width - 1) / 2f)) * _sheet.TileSize.width;
                hit.PositionOffset.y -= (y + ((height - 1) / 2f)) * _sheet.TileSize.height;
                hit.LockSize = new Entity.Vector(
                    hit1.LockSize.x * width,
                    hit1.LockSize.y * height);
                lst[x, y] = null;

                i = -1;
            }
        }

        return result;
    }

    private readonly TileSet _sheet;
    private readonly OgmoFile _file;

    private RenderTarget2D _renderTarget;
    public Texture2D Texture
    {
        get
        {
            return _renderTarget;
        }
    }

    private BoundFunc _boundFunc = new();

    public Entity.Vector Position;

    public Graphics.Color Color { get; set; }

    public void Draw()
    {
        FriteModel.MonoGame.instance.SpriteBatch.Draw
        (
            _renderTarget,
            new Microsoft.Xna.Framework.Rectangle
            (
                (int)(Position.x - Camera.Position.x),
                (int)(Position.y + Camera.Position.y),
                (int)(_file.width),
                256
            ),
            null,
            new Microsoft.Xna.Framework.Color(Color.RGB.R, Color.RGB.G, Color.RGB.B),
            0,
            Vector2.Zero,
            SpriteEffects.None,
            0
        );

        foreach (Entity.Object obj in _sheet.entities)
        {
            obj.Draw();
        }
    }

    public void Dispose()
    {
        _renderTarget.Dispose();
    }
}

public struct OgmoFile
{
    public string ogmoVersion;
    public int width;
    public int height;
    public int offsetX;
    public int offsetY;
    public OgmoLayer[] layers;
}

public struct OgmoLayer
{
    public string name;
    public int _eid;
    public int offsetX;
    public int offsetY;
    public int gridCellWidth;
    public int gridCellHeight;
    public int gridCellsX;
    public int gridCellsY;
    public string tileset;
    public int[] data;
    public int arrayMode;
}