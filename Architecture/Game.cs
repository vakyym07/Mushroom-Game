using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;

namespace MyGame
{
    public class Game
    {
        public BackGround BackGround1;
        public BackGround BackGround2;
        public Player player;
        public List<Tuple<IFlyingObject, IFlyingObject>> Bombs;
        public int SpeedBG;
        public int SpeedBGMin;
        public int SpeedBGMax;
        public int Distance;
        public bool GameOver;
        public bool Menu;
        public int TimeGameOver;
        public int Record;
        public bool NewRecord;
        public bool PrintRecord;

        public MediaPlayer BackGroundMusic;

        public Game()
        {
            SpeedBGMin = 4;
            SpeedBGMax = 8;
            SpeedBG = 4;
            Distance = 1000;
            GameOver = false;
            Menu = true;
            TimeGameOver = -1;
            BackGround1 = new BackGround(0, 0, "backGround.jpg");
            BackGround2 = new BackGround(BackGround1.Width, 0, "backGround.jpg");
            player = new Player();
            Bombs = GenerateBombs();
            BackGroundMusic = new MediaPlayer();
            Record = int.Parse(File.ReadAllText(new DirectoryInfo("Records").FullName + @"\records.txt"));
            NewRecord = false;
            PrintRecord = false;
        }

        public void NewGame()
        {
            player = new Player();
            GameOver = false;
            player.Scores = 0;
            player.Bonuses = 0;
            SpeedBGMin = 4;
            SpeedBGMax = 8;
            SpeedBG = SpeedBGMax;
            Distance = 1000;
            NewRecord = false;
            TimeGameOver = -1;
            BackGroundMusic.Volume = 0.3;
            BackGroundMusic.MediaEnded += MediaEnded;
            BackGroundMusic.Play();
        }

        public void Act(Game game)
        {
            BackGround1.Act(game);
            BackGround2.Act(game);
            for (var i = 0; i < Bombs.Count(); i++)
            {
                Bombs[i].Item1.Act(game);
                Bombs[i].Item1.Conflict(game);
                Bombs[i].Item2.Act(game);
            }
            if (!GameOver && !Menu)
            {
                player.Act();
                player.Scores += (int)(SpeedBG / 4);
                if (player.Scores > Distance)
                {
                    Distance += 1500;
                    AddBomb();
                    if (SpeedBGMax < 16)
                    {
                        SpeedBGMax = (int)(SpeedBGMax * 1.2);
                        SpeedBG = SpeedBGMax;
                        SpeedBGMin = (int)(SpeedBGMax / 2);
                    }
                }
            }
            if (MyForm.tickCount == TimeGameOver)
            {
                Menu = true;
                UpdatesBombs();
            }
            if (Menu) SpeedBG = 4;
        }

        public List<Tuple<IFlyingObject, IFlyingObject>> GenerateBombs()
        {
            var list = new List<Tuple<IFlyingObject, IFlyingObject>>();
            var rnd = new Random();
            IFlyingObject bomb = new Bomb(rnd.Next(450, 2300), rnd.Next(-500, -84), "bomb.png", 0);
            IFlyingObject shadow = new Shadow(bomb.Location.X, 1001 - 6 - bomb.Size.Item2, "bombShadow.png", 0); ;
            list.Add(Tuple.Create(bomb, shadow));
            for (var i = 1; i < 3; i++)
            {
                bomb = new Bomb(list[i - 1].Item1.Location.X + 300, rnd.Next(-500, -bomb.Size.Item2), "bomb.png", i);
                shadow = new Shadow(bomb.Location.X, 1001 - 6 - bomb.Size.Item2, "bombShadow.png", i);
                list.Add(Tuple.Create(bomb, shadow));
            }
            return list;
        }

        public void NewBomb(int index)
        {
            if (index < Bombs.Count())
            {
                var rnd = new Random();
                IFlyingObject flyingObject;
                IFlyingObject shadow;

                if (rnd.Next(0, 40) == 1)
                {
                    flyingObject = new Present(rnd.Next(1100, 2300), rnd.Next(-500, -84), "present.png", index);
                    shadow = new Shadow(flyingObject.Location.X, 1001 - 6 - flyingObject.Size.Item2, "presentShadow.png", index);
                }
                else
                {
                    flyingObject = new Bomb(rnd.Next(700, 2300), rnd.Next(-500, -84), "bomb.png", index);
                    shadow = new Shadow(flyingObject.Location.X, 1001 - 6 - flyingObject.Size.Item2, "bombShadow.png", index);
                }
                Bombs[index] = Tuple.Create(flyingObject, shadow);
            }
        }

        public void AddBomb()
        {
            var rnd = new Random();
            IFlyingObject bomb = new Bomb(rnd.Next(700, 2200), rnd.Next(-500, -84), "bomb.png", Bombs.Count());
            IFlyingObject shadow = new Shadow(bomb.Location.X, 1001 - 6 - bomb.Size.Item2, "bombShadow.png", Bombs.Count());
            Bombs.Add(Tuple.Create(bomb, shadow));
        }

        public void RemoveBomb()
        {
            if (Bombs.Count() > 3)
                if (Bombs[Bombs.Count() - 1].Item1 is Bomb)
                    Bombs.Remove(Bombs[Bombs.Count()-1]);
        }

        public void UpdatesBombs()
        {
            for(var i = Bombs.Count()-1; i > 3; i--)
                Bombs.Remove(Bombs[i]);
        }

        public void KeyPressed(object sender, KeyEventArgs key)
        {
            if (key.KeyCode == Keys.ControlKey)
            {
                if (player.X < 0) player.X = 0;
                player.Speed = 4;
            }
            if (key.KeyCode == Keys.Escape)
            {
                if (GameOver) MyForm.ActiveForm.Close();
                else
                {
                    GameOver = true;
                    Menu = true;
                    UpdatesBombs();
                    SpeedBG = 4;
                    BackGroundMusic.Pause();
                }
            } 
            if (key.KeyCode == Keys.Space)
            {
                if (Menu)
                {
                    Menu = false;
                    BackGroundMusic.Play();
                    NewGame();
                }
                else if (!GameOver)
                    {
                        player.ImageName = "playerDirty.png";
                        player.Speed = -1;
                        SpeedBG = SpeedBGMin;
                    }
                    else if (MyForm.tickCount > TimeGameOver - 50)
                        {
                            Menu = false;
                            BackGroundMusic.Play();
                            NewGame();
                            UpdatesBombs();
                        }   
            }
            if (key.KeyCode == Keys.R && (Menu || GameOver)) PrintRecord = true;
        }

        public void KeyUnPressed(object sender, KeyEventArgs key)
        {
            
            if (key.KeyCode == Keys.Space)
            {
                if (!GameOver && !Menu)
                {
                    player.ImageName = "player.png";
                    player.Speed = 0;
                    SpeedBG = SpeedBGMax;
                }
            }
            if (key.KeyCode == Keys.ControlKey) player.Speed = 0;
            if (key.KeyCode == Keys.R && (Menu || GameOver)) PrintRecord = false;
        }

        public void MediaEnded(object sender, EventArgs e)
        {
            BackGroundMusic.Position = TimeSpan.Zero;
            BackGroundMusic.Play();
        }

    }

    public class Player
    {
        public int X;
        public int Y;
        public int Speed = 0;
        public int Height = 140;
        public int Width = 138;
        public string ImageName;
        public int Scores;
        public int Bonuses;

        public Player()
        {
            X = 200;
            Y = 850;
            ImageName = "player.png";
        }

        public string GetFileName()
        {
            return ImageName;
        }

        public void Act()
        {
            if (X + Width > 1500 || X < -2)
                Speed = 0;
            X += Speed;
        }

        public Point Location()
        {
            var rnd = new Random();
            var dY = rnd.Next(4);
            var a = rnd.Next(4);
            if (a != 1) dY = 0;
            return new Point(X, Y+dY);
        }
    }

    public class BackGround
    {
        public int X;
        public int Y;
        public string Name;
        public int Width;
        public int Height;
        
        public BackGround(int x, int y, string name)
        {
            Y = y;
            X = x;
            Width = 1920;
            Height = 1001;
            Name = name;
        }

        public string GetFileName()
        {
            return Name;
        }

        public Point Location()
        {
            return new Point(X, Y);
        }

        public void Act(Game game)
        {
            if (X + Width < 0)
                X += 2 * Width;
            //if (X + MyForm.ScreenWidth < 0)
            //    X += 2*MyForm.ScreenWidth;
            X -= game.SpeedBG;
        }
    }

    public class Bomb : IFlyingObject
    {
        public int X;
        public int Y;
        public int SpeedDown;
        public string ImageName;
        public int INN;
        public int TimeDead;
        public int Width = 84;
        public int Height = 84;
        public int BangWidth = 110;
        public int BangHeight = 134;
        public int TimeBang = MyForm.tickCount + 1000;
        MediaPlayer music = new MediaPlayer();

        public Bomb(int x, int y, string name, int inn)
        {
            X = x;
            Y = y;
            ImageName = name;
            SpeedDown = 12;
            INN = inn;
            music.Open(new Uri(new DirectoryInfo("Music").FullName + @"\bang.mp3"));
        }

        public string GetFileName
        {
            get { return ImageName; }
        }

        public Point Location
        {
            get { return new Point(X, Y); }
        }

        public Tuple<int, int> Size
        {
            get { return Tuple.Create(Width, Height); }
        }

        public void Act(Game game)
        {
            if (Y + Height > 1001 - 20)
            {
                music.Volume = 0.7;
                music.Play();
                ImageName = "bang.png";
                SpeedDown = 0;
                Y = Y + Height - BangHeight;
                TimeBang = MyForm.tickCount + 10;
            }
            Y += SpeedDown;
            if (MyForm.tickCount > TimeBang) game.NewBomb(INN);
            X -= game.SpeedBG;
        }

        public void Conflict(Game game)
        {
            if (!game.Menu && !game.GameOver && Y + Height > 1001 - 6 - game.player.Height + 30)
                if (X < game.player.X + game.player.Width - 40 && X + Width > game.player.X + 40
                    && ImageName == "bomb.png")
                {
                    game.GameOver = true;
                    game.TimeGameOver = MyForm.tickCount + 100;
                    game.SpeedBG = 4;
                    if (game.player.Scores + game.player.Bonuses > game.Record)
                    {
                        game.Record = game.player.Scores + game.player.Bonuses;
                        game.NewRecord = true;
                        File.WriteAllText(new DirectoryInfo("Records").FullName + @"\records.txt", game.Record.ToString());
                    }
                }
        }
    }

    public class Shadow : IFlyingObject
    {
        public int X;
        public int Y;
        public string ImageName;
        public int INN;
        public int Width;
        public int Height;

        public Shadow(int x, int y, string name, int inn)
        {
            Y = y;
            X = x;
            Width = 50;
            Height = 50;
            ImageName = name;
            INN = inn;
        }

        public string GetFileName
        {
            get { return ImageName; }
        }

        public Point Location
        {
            get { return new Point(X, Y); }
        }

        public Tuple<int, int> Size
        {
            get { return Tuple.Create(Width, Height); }
        }

        public void Act(Game game)
        {
            if (INN < game.Bombs.Count())
            {
                X = game.Bombs[INN].Item1.Location.X;
                if (game.Bombs[INN].Item1.GetFileName == "bang.png")
                    ImageName = null;
            }
        }

        public void Conflict(Game game)
        {

        }
    }

    public class Present : IFlyingObject
    {
        public int X;
        public int Y;
        public int SpeedDown;
        public string ImageName;
        public int INN;
        public int Width = 84;
        public int Height = 84;

        public Present(int x, int y, string name, int inn)
        {
            X = x;
            Y = y;
            ImageName = name;
            SpeedDown = 7;
            INN = inn;
        }

        public string GetFileName
        {
            get { return ImageName; }
        }

        public Point Location
        {
            get { return new Point(X, Y); }
        }

        public Tuple<int, int> Size
        {
            get { return Tuple.Create(Width, Height); }
        }

        public void Act(Game game)
        {
            if (Y + Height > 1001 - 20) game.NewBomb(INN);
            Y += SpeedDown;
            X -= game.SpeedBG;
        }

        public void Conflict(Game game)
        {
            if (!game.Menu && Y + Height > 1001 - 6 - game.player.Height + 30)
                if (X < game.player.X + game.player.Width - 40 && X + Width > game.player.X + 40)
                {
                    var rnd = new Random();
                    if (rnd.Next(0, 2) == 0) game.RemoveBomb();
                    else game.player.Bonuses += 1000;
                    game.NewBomb(INN);
                }
        }
    }
}
