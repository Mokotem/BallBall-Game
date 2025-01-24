using FriteCollection.UI;
using FriteCollection.Tools.SpriteSheet;
using FriteCollection.Scripting;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using FriteCollection.Audio;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace RocketLike;

public class SAVEDATA
{
    public ulong t = 0;
    public ulong nbbn = 0, nbbg = 0, nbbr = 0;
    public ulong nte = 0, nta = 0;
    public Keys[] i = new Keys[9]
    {
        Keys.R, Keys.D, Keys.G, Keys.Q, Keys.Up, Keys.Left, Keys.Right, Keys.M, Keys.Enter
    };
}

public partial class Menu : Script
{
    public Menu() : base(Scenes.Menu) { }

    Texture2D tex;
    SpriteSheet UISheet;

    public static byte scaleY;

    private static Panel mainPanel;
    Texture2D mg;
    private static SpriteSheet iconSheet;

    private Page previousPage = null;
    private static bool inputPage = false;
    private static int keyToChange;

    private static Page takingInput;
    private static List<Page> _back;
    private Page
        princi, quit, credits, settings, inputs, inputsp1, inputsp2,
        stats, play2, play1, delete, graphics, mappack, options, acce, foot, voll;

    private bool inputsChanged = false;
    private bool menuIsSettings = true;
    private bool IMPORTING = false;
    private bool changeScene = false;

    void SetMap(byte map)
    {
        GameData.custom = false;
        MapManager.map = "map" + map.ToString();
        GameManager.CurrentScene = Scenes.Game;
    }

    private static U ktc;

    System.Windows.Forms.OpenFileDialog fileDialog;
    System.Threading.Thread t;

    [STAThread]
    private void OpenFile()
    {
        if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            try
            {
                string file;
                using (StreamReader sr = new StreamReader(fileDialog.FileName))
                    file = sr.ReadToEnd();
                GameData.file = JsonConvert.DeserializeObject<FriteCollection.Tools.TileMap.OgmoFile>(file);
                if (GameData.file.width != 480
                    || GameData.file.height != 256
                    || GameData.file.layers.Count() != 3)
                {
                    new MessageBox(false, "failed to load map.");
                }
                else
                {
                    new MessageBox(true, "map successfully loaded.");
                    Thread.Sleep(1000);
                    GameData.custom = true;
                    changeScene = true;
                }
            }
            catch (System.Exception ex)
            {
                new MessageBox(false, "File not valid : " + ex.Message);
            }
        }

        IMPORTING = false;
    }

    public override void Start()
    {
        Screen.backGround = new(0, 0, 0);

        fileDialog = new System.Windows.Forms.OpenFileDialog();
        fileDialog.Title = "Import a BallBallGame map";
        fileDialog.Multiselect = false;
        fileDialog.Filter = "Fichiers json (*.json)|*.json";
        fileDialog.DefaultExt = "json";
        fileDialog.InitialDirectory = "C://";

        if (GameData.firstConnection == false)
        {
            SaveManager.Save(GameData.SAVE);
        }
        else
        {
            if (!SaveManager.SaveExist)
            {
                GameData.SAVE = new SAVEDATA();
            }
            else
            {
                GameData.SAVE = SaveManager.Load();
            }
        }

        mg = Open.Texture("Game/monogame");
        iconSheet = new SpriteSheet(mg, 22, 22);

        scaleY = 20;
        mainPanel = new Panel(new Space(Bounds.Center, Extend.Vertical, new(270, 0)));
        princi = new Page("BALL  BALL  GAME", false,
            new U(iconSheet[1, 1]),
            new U("2 player game"),
            new U("play", () => play1.Apply()),
            new U("settings", () =>
            {
                menuIsSettings = true;
                settings.Apply();
            }),
            new U("statistics", () =>
            {
                if (stats is not null)
                {
                    stats.Dispose();
                }
                ulong tot1 = GameData.SAVE.nbbn
    + GameData.SAVE.nbbg
    + GameData.SAVE.nbbr;
                if (tot1 < 1) tot1 = 1;
                ulong tot2 = GameData.SAVE.nte + GameData.SAVE.nta;
                if (tot2 < 1) tot2 = 1;
                stats = new Page("STATISTICS", true,
    new U("game time: " + rogner((GameData.SAVE.t / 3600f).ToString()) + " houres"),
    new U("total number of goals scored: " + tot1.ToString()),
    new U("--> neutral ones: " + GameData.SAVE.nbbn.ToString() + " ("
    + rogner((GameData.SAVE.nbbn / (float)tot1 * 100f).ToString()) + "%)"),
    new U("--> golden ones: " + GameData.SAVE.nbbg.ToString() + " ("
    + rogner((GameData.SAVE.nbbg / (float)tot1 * 100f).ToString()) + "%)"),
    new U("--> red ones: " + GameData.SAVE.nbbr.ToString() + " ("
    + rogner((GameData.SAVE.nbbr / (float)tot1 * 100f).ToString()) + "%)"),
    new U("total number of ball shot: " + tot2.ToString()),
    new U("--> successful ones: " + GameData.SAVE.nte.ToString() + " ("
    + rogner((GameData.SAVE.nte / (float)tot2 * 100f).ToString()) + "%)"),
    new U("--> missed ones: " + GameData.SAVE.nta.ToString() + " ("
    + rogner((GameData.SAVE.nta / (float)tot2 * 100f).ToString()) + "%)"),
    new U(""));

            stats.Apply();
            }),
            new U("credits", () => credits.Apply()),
            new U(),
            new U("quit", () => quit.Apply()));

        quit = new Page("quit the game ?", true,
            new U("yes", () =>
            {
                if (GameData.saved)
                {
                    SaveManager.Save(GameData.SAVE);
                }
                Music.Volume = 0f;
                if (fileDialog is not null)
                {
                    fileDialog.Reset();
                    fileDialog.Dispose();
                }
                GameManager.instance.Exit();
            }));
        credits = new Page("credits", true,
            new U("Frite"),
            new U("MonoGame"),
            new U(iconSheet[0, 0]),
            new U(),
            new U("musics : Kommisar"),
            new U());
        inputsp1 = new Page("player 1 controls", true,
            new U("Jump", 0),
            new U("Left", 1),
            new U("Right", 2),
            new U("Shoot", 3),
                        new U("aim assist", GameData.p1visee, () =>
                        {
                            GameData.p1visee = true;
                        }, () =>
                        {
                            GameData.p1visee = false;
                        }),
            new U());
        inputsp2 = new Page("player 2 controls", true,
            new U("Jump", 4),
            new U("Left", 5),
            new U("Right", 6),
            new U("Shoot", 7),
                        new U("aim assist", GameData.p2visee, () =>
                        {
                            GameData.p2visee = true;
                        }, () =>
                        {
                            GameData.p2visee = false;
                        }),
            new U()); ;
        inputs = new Page("controls", false,
            new U("player 1", () => { inputsp1.Apply(); previousPage = inputsp1; inputPage = false; }),
            new U("player 2", () => { inputsp2.Apply(); previousPage = inputsp2; inputPage = false; }),
            new U("skip replay", 8),
            new U(),
            new U("back", () =>
            {
                if (inputsChanged)
                {
                    SaveManager.Save(GameData.SAVE);
                }
                _back.Remove();
                if (menuIsSettings)
                {
                    settings.Apply(false);
                }
                else
                {
                    options.Apply(false);
                }
                inputPage = false;
            }, iconSheet[2, 1]));
        U b = new U("");
        settings = new Page("settings", false,
            new U(iconSheet[0, 1]),
            new U("controls", () =>
            {
                inputsChanged = false;
                previousPage = inputs;
                inputs.Apply();
            }),
            new U(),
            new U("graphics", () => graphics.Apply()),
            new U("accessibility", () => acce.Apply()),
            new U("delete save", () =>
            {
                (b.ui1 as Text).Edit =
                "saved data is allocating: " + SaveManager.SpaceTaking.ToString() + " bytes";
                delete.Apply();
            }),
            new U(),
            new U("version " + GameData.VERSION),
            new U("back", () => { _back.Remove(); princi.Apply(false); inputPage = false; }, iconSheet[2, 1]));
        graphics = new Page("graphics", true,
            new U("full screen", GameManager.instance.FullScreen,
            () => GameManager.instance.FullScreen = true,
            () => GameManager.instance.FullScreen = false),
             new U("lock to 60 fps", GameManager.instance.FPS60,
            () =>
            {
                GameManager.instance.SetFps(60);
                Time.SetFPS(60);
            },
            () =>
            {
                GameManager.instance.SetFps(240);
                Time.SetFPS(240);
            }),
             new U("particles", GameData.particles,
            () => GameData.particles = true,
            () => GameData.particles = false),
             new U("retro filter", GameManager.instance.oldFilter,
            () =>
            {
                GameManager.instance.oFilter = GameManager.instance.Content.Load<Effect>("Shaders/oldFilter");
                GameManager.instance.oldFilter = true;
            },
            () =>
            {
                GameManager.instance.oldFilter = false;
                GameManager.instance.oFilter = null;
            }),
             new U("replay", GameData.replay,
            () =>
            {
                GameData.replay = true;
            },
            () =>
            {
                GameData.replay = false;
            }),
             new U());

        U s = new U("scores : " + GameData.sp1.ToString() + " - " + GameData.sp2.ToString());

        play1 = new Page("play", true,
             new U(iconSheet[1, 0]),
             new U("normal mode", () =>
             {
                 Ball.SetAttireMode(false);
                 play2.EditTitle("normal mode");
                 play2.Apply();
             }),
             new U(),
             new U(iconSheet[2, 0]),
             new U("target mode", () =>
             {
                 Ball.SetAttireMode(true);
                 play2.EditTitle("target mode");
                 play2.Apply();
             }),
              new U()
        );

        mappack = new Page("maps", true,
new U("soccer", () => foot.Apply()),
new U("volley", () => voll.Apply()),
new U("import a file", () =>
{
    IMPORTING = true;
    t = new Thread(OpenFile);
    t.SetApartmentState(ApartmentState.STA);
    t.Start();
}),
new U());

        play2 = new Page(Ball.AttireMode ? "target mode" : "normal mode", true,
new U("start", () =>
{
    GameData.randomMode = true;
    SetMap((byte)(new Random().Next(MapManager.numberOfMaps) + 1));
}),
new U("select a map", () =>
{
    GameData.randomMode = false;
    mappack.Apply();
}),
new U(),
new U("options", () =>
{
    menuIsSettings = false;
    options.Apply();
    }),
             new U(),
             s,
             new U("reset scores", () =>
             {
                 GameData.sp1 = 0;
                 GameData.sp2 = 0;
                 GameData.cp1 = 0;
                 GameData.cp2 = 0;
                 (s.ui1 as Text).Edit = "scores : 0 - 0";
             }),
             new U()
);

        U u = new U(GameData.sp1.ToString() + " - " + GameData.sp2.ToString());

        options = new Page("options", true,
            new U(iconSheet[0, 1]),
            new U("controls", () =>
            {
                previousPage = inputs;
                inputs.Apply();
            }),
            new U(),
            new U("own goal counter", GameData.cscCounter,
            () =>
            {
                GameData.cscCounter = true;
            },
            () =>
            {
                GameData.cscCounter = false;
            }),
            new U("score combo", GameData.bonusOnCombo,
            () =>
            {
                GameData.bonusOnCombo = true;
            },
            () =>
            {
                GameData.bonusOnCombo = false;
            }),
            new U("bigger circle", GameData.bigTapCircles,
            () =>
            {
                GameData.bigTapCircles = true;
            },
            () =>
            {
                GameData.bigTapCircles = false;
            }),
            new U()
        );

        acce = new Page("accessibility", true,
            new U("colorblind mode", GameData.dalton,
            () =>
            {
                GameData.dalton = true;
            },
            () =>
            {
                GameData.dalton = false;
            }));
        scaleY = 14;

        delete = new Page("delete save file", true,
    new U(),
    new U(),
    new U(),
    new U("!!!"),
    new U("are you sure you want to delete"),
    new U("all your saved data ?"),
    new U("this includes: saved controls, statistics."),
    new U(),
    b,
    new U(),
    new U(),
new U("yes, delete save file.", () =>
{
    SaveManager.Delete();
    GameData.SAVE = new SAVEDATA();
    settings.Apply();
}));

        foot = new Page("soccer maps", true,
            new U("map 1 - classic              ", () => SetMap(1)),
            new U("map 2 - high ground          ", () => SetMap(3)),
            new U("map 3 - small                ", () => SetMap(5)),
            new U("map 4 - core                 ", () => SetMap(6)),
            new U("map 5 - roof                 ", () => SetMap(9)),
            new U("map 6 - corridor             ", () => SetMap(10)),
            new U("map 7 - tennis               ", () => SetMap(11)),
            new U("map 8 - square               ", () => SetMap(15)),
            new U("map 9 - reversed             ", () => SetMap(17))
            );

        voll = new Page("volley maps", true,
            new U("map 10 - classic              ", () => SetMap(2)),
            new U("map 11 - edges                ", () => SetMap(4)),
            new U("map 12 - walls               ", () => SetMap(7)),
            new U("map 13 - ground              ", () => SetMap(8)),
            new U("map 14 - hole                ", () => SetMap(12)),
            new U("map 15 - beach volley        ", () => SetMap(13)),
            new U("map 16 - fire in the hole    ", () => SetMap(14)),
            new U("map 17 - trap                ", () => SetMap(16)),
            new U("map 18 - basket              ", () => SetMap(18)));

        ktc = new U("");
        takingInput = new Page("", true,
           ktc,
           new U("> press a key <"),
           new U());


        if (GameData.firstConnection)
        {
            _back = new List<Page>();
            princi.Apply(false);
        }
        else
        {
            _back = new List<Page>(princi, play1);
            play2.Apply(false);
        }

        GameData.firstConnection = false;
    }

    KeyboardState previous;

    public override void Update()
    {
        if (GameManager.instance.IsActive)
        {
            KeyboardState state = Keyboard.GetState();
            if (_back.Count > 0 && state.IsKeyDown(Keys.Escape) && !previous.IsKeyDown(Keys.Escape))
            {
                inputPage = false;
                _back.Last.Apply(false);
                _back.Remove();
            }

            if (inputPage)
            {
                if (state.GetPressedKeyCount() == 1)
                {
                    if (GameData.SAVE.i[keyToChange] != state.GetPressedKeys()[0])
                    {
                        inputsChanged = true;
                    }
                    GameData.SAVE.i[keyToChange] = state.GetPressedKeys()[0];
                    previousPage.Apply(false);
                    _back.Remove();
                    inputPage = false;
                }
            }

            previous = state;
        }
    }

    public override void AfterUpdate()
    {
        if (changeScene)
        {
            changeScene = false;
            GameManager.CurrentScene = Scenes.Game;
        }
    }

    public override void Draw()
    {
        mainPanel.Draw();
    }

    public override void Dispose()
    {
        mainPanel.Dispose();
        tileSet1.Dispose();
        tileSet2.Dispose();
        tileSet3.Dispose();
        UISheet.Dispose();
        iconSheet.Dispose();
        tex.Dispose();
        mg.Dispose();

        mainPanel = null;
        tileSet1 = null;
        tileSet2 = null;
        tileSet3 = null;
        UISheet = null;
        mg = null;

        princi.Dispose(); quit.Dispose(); credits.Dispose();
        settings.Dispose(); inputs.Dispose(); inputsp1.Dispose();
        inputsp2.Dispose(); takingInput.Dispose();
        if (stats is not null)
            stats.Dispose();
        play2.Dispose();
        mappack.Dispose();
        fileDialog.Dispose();

        princi = null; quit = null; credits = null;
        settings = null; inputs = null; inputsp1 = null;
        inputsp2 = null; takingInput = null;
        stats = null; play2 = null;
        mappack = null;
        fileDialog = null;
    }
}