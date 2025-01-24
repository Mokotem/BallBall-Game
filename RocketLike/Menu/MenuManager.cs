using FriteCollection.Scripting;
using FriteCollection.UI;
using FriteCollection.Tools.TileMap;
using Microsoft.Xna.Framework.Graphics;
using FriteCollection.Tools.SpriteSheet;
using System;

namespace RocketLike;

public partial class Menu : Script
{
    private static TileSet tileSet1, tileSet2, tileSet3;

    public override void BeforeStart()
    {
        tex = Open.Texture("Game/ui");
        UISheet = new SpriteSheet(tex, 24, 24);
        tileSet1 = new TileSet(UISheet[0, 0])
        {
            TileSize = new(8, 8),
            TileMargin = new(0, 0),
            TileSeparation = new(0, 0),
        };
        tileSet2 = new TileSet(UISheet[1, 0])
        {
            TileSize = new(8, 8),
            TileMargin = new(0, 0),
            TileSeparation = new(0, 0),
        };
        tileSet3 = new TileSet(UISheet[2, 0])
        {
            TileSize = new(8, 8),
            TileMargin = new(0, 0),
            TileSeparation = new(0, 0),
        };

        Screen.backGround = new(0, 0, 0);
    }

    string rogner(string t)
    {
        string r = "";
        for (int i = 0; i < t.Length; i++)
        {
            if (t[i] == ',' || t[i] == '.')
            {
                if (t.Length - i - 1 < 2)
                {
                    return t;
                }
                else
                {
                    return r + "." + t[i + 1] + t[i + 2];
                }
            }
            else
            {
                r += t[i];
            }
        }
        return t;
    }

    class U : IDisposable
    {
        public UI ui1;
        private UI ui2 = null, ui3 = null;

        public int PositionY
        {
            set
            {
                if (ui1 is not null)
                {
                    if (ui1 is Image)
                    {
                        ui1.PositionY = value - 2;
                    }
                    else
                    {
                        ui1.PositionY = value;
                    }
                }
                if (ui2 is not null)
                    ui2.PositionY = value;
                if (ui3 is not null)
                    ui3.PositionY = value;
            }
        }

        public void Dispose()
        {
            if (ui1 is not null)
                ui1.Dispose();
            if (ui2 is not null)
                ui2.Dispose();
            if (ui3 is not null)
                ui3.Dispose();
        }

        public void Activate()
        {
            if (ui1 is not null)
                ui1.Active = true;
            if (ui2 is not null)
                ui2.Active = true;
            if (ui3 is not null)
                ui3.Active = true;
        }
        public void Deactivate()
        {
            if (ui1 is not null)
                ui1.Active = false;
            if (ui2 is not null)
                ui2.Active = false;
            if (ui3 is not null)
                ui3.Active = false;
        }
        public UI[] GetValue()
        {
            if (ui2 is not null && ui3 is not null)
            {
                return new UI[3] { ui1, ui2, ui3 };
            }
            else if (ui2 is not null)
            {
                return new UI[2] { ui1, ui2 };
            }
            else if (ui1 is not null)
            {
                return new UI[1] { ui1 };
            }
            else
                return Array.Empty<UI>();
        }

        public U(string name, UI.Procedure fonction)
        {
            if (scaleY > 16)
            {
                ui1 = new Button(
                    name,
                    tileSet2,
                    new Space(Bounds.Center, Extend.None, new(120, scaleY - 2))
                , mainPanel);
            }
            else
            {
                ui1 = new Button(
                    name,
                    tileSet1,
                    new Space(Bounds.Center, Extend.None, new(140, scaleY - 2))
                , mainPanel);
            }
            (ui1 as Button).Fonction = fonction;
        }

        public U(string name, UI.Procedure fonction, Texture2D tex)
        {
            if (scaleY > 16)
            {
                ui1 = new Button(
                    name,
                    tileSet2,
                    new Space(Bounds.Center, Extend.None, new(120, scaleY - 2))
                , mainPanel);
            }
            else
            {
                ui1 = new Button(
                    name,
                    tileSet1,
                    new Space(Bounds.Center, Extend.None, new(140, scaleY - 2))
                , mainPanel);
            }
           (ui1 as Button).Fonction = fonction;
            ui2 = new Image(
                 tex,
                 new Space(Bounds.Center, Extend.None, new(22, 22), new(-73, 0))
                 , mainPanel);
        }

        public U(string text)
        {
            ui1 = new Text(
                text,
                new Space(Bounds.Center, Extend.None, new(120, scaleY - 2))
                , mainPanel);
        }

        private readonly int k;

        public U(string name, byte keyIndex)
        {
            k = keyIndex;
            ui1 = new Text(
                name,
                new Space(Bounds.Left, Extend.None, new(120, scaleY - 2), new(20, 0))
                , mainPanel);
            ui2 = new Text(
                GameData.SAVE.i[keyIndex].ToString(),
                new Space(Bounds.Center, Extend.None, new(120, scaleY - 2))
                , mainPanel);
            ui2.Color = Pico8.Yellow;
            ui3 = new Button(
                "edit",
                tileSet3,
                new Space(Bounds.Right, Extend.None, new(60, scaleY - 2), new(-110, 20))
                , mainPanel);
            (ui3 as Button).Fonction = () =>
            {
                keyToChange = keyIndex;
                inputPage = true;
                (ktc.ui1 as Text).Edit = name;
                takingInput.Apply();
            };
        }

        public void Update()
        {
            if (ui2 is not null && ui2 is Text)
            {
                (ui2 as Text).Edit = GameData.SAVE.i[k].ToString();
            }
        }

        public U()
        {

        }

        public U(Texture2D texture)
        {
            ui1 = new Image(
                texture,
                new Space(Bounds.Center, Extend.None, new(22, 22))
                , mainPanel);
        }

        public U(string name, bool _default, UI.Procedure on, UI.Procedure off)
        {
            ui1 = new Text(
                 name,
                 new Space(Bounds.Left, Extend.None, new(180, scaleY - 2), new(75, 0))
                 , mainPanel);
            ui2 = new Toggle(
                 "",
                 tileSet3,
                 new Space(Bounds.Right, Extend.None, new(scaleY - 2, scaleY - 2), new(-180, 20))
                 , mainPanel);
            (ui2 as Toggle).Set(_default);
            (ui2 as Toggle).OnActivate = () =>
            {
                (ui3 as Text).Edit = "yes";
                on();
            };
            (ui2 as Toggle).OnDeactivate = () =>
            {
                (ui3 as Text).Edit = "no";
                off();
            };
            ui3 = new Text(
                _default ? "yes" : "no",
                new Space(Bounds.Center, Extend.None, new(120, scaleY - 2), new(20, 0))
                , mainPanel);
            ui3.Color = Pico8.Yellow;
        }
    }

    class Page : IDisposable
    {
        private static Page ActivePage;
        private readonly int tailleY;
        private readonly Text title;
        private readonly U[] buts;

        public Page(string title, bool backButton, params U[] uis)
        {
            int l = uis.Length + (backButton ? 1 : 0);
            tailleY = (uis.Length * scaleY) - (scaleY / 2);
            buts = new U[l];
            for (int i = 0; i < uis.Length; i++)
            {
                if (uis[i] is not null)
                {
                    uis[i].PositionY = (i * scaleY) - (tailleY / 2) + (scaleY / 2) + 10;
                    uis[i].Deactivate();
                    buts[i] = uis[i];
                }
            }
            if (backButton)
            {
                buts[l - 1] = new U("back", () =>
                {
                    inputPage = false;
                    _back.Last.Apply(false);
                    _back.Remove();
                }, iconSheet[2, 1]);
                buts[l - 1].PositionY = (uis.Length * scaleY) - (tailleY / 2) + (scaleY / 2) + 10;
                buts[l - 1].Deactivate();
            }
            this.title = new Text(title, new Space(Bounds.Center, Extend.Horizontal, new(0, 0), new(0, -tailleY / 2 - scaleY)));
        }

        public void EditTitle(string _title)
        {
            title.Edit = _title;
        }

        public void Apply(bool stack = true)
        {
            if (stack && ActivePage is not null)
                _back.Add(ActivePage);


            if (ActivePage is not null)
            {
                ActivePage.title.Active = false;
            }
            ActivePage = this;
            mainPanel.Clear();
            if (buts is not null)
            {
                foreach (U b in buts)
                {
                    b.Update();
                    b.Activate();
                    foreach (UI ui in b.GetValue())
                        mainPanel.Add(ui);
                }
            }
            mainPanel.Add(title);
            title.Active = true;
        }

        public void Dispose()
        {
            foreach (U b in buts)
            {
                b.Dispose();
            }
            title.Dispose();
            ActivePage = null;
        }

        public override string ToString()
        {
            return title.Edit;
        }
    }
}