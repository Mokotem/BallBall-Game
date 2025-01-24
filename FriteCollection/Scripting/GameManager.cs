using FriteCollection.Audio;
using FriteCollection.Entity;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using RocketLike;
using System;
using System.IO;
using System.Windows.Forms.Design;

namespace FriteCollection.Scripting;

public abstract class GameManager
{
    public static FriteModel.MonoGame instance;

    public static void RESET()
    {
        GameData.firstConnection = true;
        CurrentScene = GetSeetings.Settings.StartScene;
    }

    public static void Print(params object[] listText)
    {
        string finalTxt = "";
        foreach (object s in listText) { finalTxt += s.ToString() + "  "; }
        System.Diagnostics.Debug.WriteLine(finalTxt);
    }

    public static SpriteFont GameFont;

    private static Scenes _currentScene = Scenes.Menu;
    /// <summary>
    /// Scène en cour d'execution.
    /// </summary>
    public static Scenes CurrentScene
    {
        get
        {
            return _currentScene;
        }

        set
        {

            instance.charging = true;
            _currentScene = value;
            Music.StopAllMusics();
            if (value != Scenes.Game)
            {
                instance.shutsfx.Play();
                new Transition();
                new Sequence(() => 0.5f, () =>
                {
                    instance.UpdateScriptToScene();
                    return 0f;
                });
            }
            else
                instance.UpdateScriptToScene();
        }
    }

    /// <summary>
    /// Acceder à un autre script de la meme scène.
    /// </summary>
    public static Script GetScript(string name)
    {
        foreach (Executable script in GameManager.instance.CurrentExecutables)
        {
            if (script is Script)
            {
                if (script.GetType().Name == name)
                    return script as Script;
            }
        }

        throw new Exception("Le script '"+name+"' n'existe pas.");
    }
}

/// <summary>
/// Musique. Les variables 'Music' ne peuvent pas être joué en même temps.
/// </summary>
public static class Open
{
    /// <summary>
    /// Ouvrir une texture. (png, jpg,...)
    /// </summary>
    public static Texture2D Texture(string path)
    {
        return GameManager.instance.Content.Load<Texture2D>(path);
    }

    /// <summary>
    /// Ouvrir un son. (mp3, wma, ogg)
    /// </summary>
    public static Music Music(string path)
    {
        return new Music(GameManager.instance.Content.Load<Microsoft.Xna.Framework.Media.Song>(path));
    }

    /// <summary>
    /// Ouvrir un son. (wav)
    /// </summary>
    public static FriteCollection.Audio.SoundEffect SoundEffect(string path)
    {
        return new FriteCollection.Audio.SoundEffect
            (GameManager.instance.Content.Load<Microsoft.Xna.Framework.Audio.SoundEffect>(path));
    }

    /// <summary>
    /// Ouvrir une police. (.ttf)
    /// </summary>
    public static SpriteFont Font(string path)
    {
        return GameManager.instance.Content.Load<SpriteFont>(path);
    }

    /// <summary>
    /// [pas dans Content] Ouvrir une tileMap (.Json)
    /// </summary>
    public static Tools.TileMap.OgmoFile OgmoTileMap(string path)
    {
        string file;
        using (StreamReader sr = new StreamReader(AppContext.BaseDirectory + path))
            file = sr.ReadToEnd();
        return JsonConvert.DeserializeObject<Tools.TileMap.OgmoFile>(file);
    }
}

public abstract class SaveManager
{
    private const string foldername = "BallBallGame";
    private static readonly string folder = Path.Combine
        (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        @foldername);
    private static readonly string path = Path.Combine
        (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        @"BallBallGame\Save_BallBallGame.json");
    private static FileInfo _info;

    public static string SavePath
    {
        get
        {
            return path;
        }
    }

    public static bool SaveExist
    {
        get
        {
            return Directory.Exists(folder) && File.Exists(path);
        }
    }

    /// <summary>
    /// bytes.
    /// </summary>
    public static long SpaceTaking => _info is null ? 0 : _info.Length;

    public static void Save(object _struct)
    {
        bool noError = true;
        try
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            string save = JsonConvert.SerializeObject(_struct);
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(save);
            }
            _info = new FileInfo(path);
        }
        catch (Exception ex)
        {
            new MessageBox(false, "failed to save data : " + ex.Message);
            noError = false;
        }

        if (noError)
        {
            GameData.saved = true;
            new MessageBox(true, "data successfully saved.");
        }
    }

    public static void Delete()
    {
        bool noError = true;
        try
        {
            File.Delete(path);
            Directory.Delete(folder);
            _info = null;
        }
        catch
        {
            if (Directory.Exists(folder))
            {
                new MessageBox(false, "folder can't be deleted. delete it at " + folder);
            }
            noError = false;
        }

        if (noError)
        {
            new MessageBox(true, "data successfully deleted.");
        }
    }

    public static SAVEDATA Load()
    {
        bool noError = true;
        try
        {
            string file;
            using (StreamReader sr = new StreamReader(path))
                file = sr.ReadToEnd();
            _info = new FileInfo(path);
            SAVEDATA data = JsonConvert.DeserializeObject<SAVEDATA>(file);
            if (data.i.Length != 9)
                throw new Exception("input list size not valid");
            else
                return data;
        }
        catch (Exception ex)
        {
            new MessageBox(false, "failed to load data : " + ex.Message);
            noError = false;
        }

        if (noError)
        {
            new MessageBox(true, "data successfully loaded.");
        }
        return new SAVEDATA();
    }
}

/// <summary>
/// Caméra
/// </summary>
public abstract class Camera
{
    /// <summary>
    /// Position de la caméra.
    /// </summary>
    private static Vector _position = Vector.Zero;

    public static Vector Position
    {
        get => _position;
        set => _position = value;
    }

    /// <summary>
    /// Les objets de la meme origine que la caméra bougent.
    /// </summary>
    public static Bounds GridOrigin = Bounds.Center;

    /// <summary>
    /// Facteur de zoom.
    /// </summary>
    public static float zoom = 1f;
}

/// <summary>
/// Données sur la fenêtre du projet.
/// </summary>
public abstract class Screen
{
    /// <summary>
    /// Couleur d'arrière plan.
    /// </summary>
    public static FriteCollection.Graphics.Color backGround = new(0.1f, 0.2f, 0.3f);
    public static readonly int widht = GetSeetings.Settings.GameFixeWidth, height = GetSeetings.Settings.GameFixeHeight;
    public static Tools.Shaders.Shader Shader
    {
        set
        {

        }
    }
}
