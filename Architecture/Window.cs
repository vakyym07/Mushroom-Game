using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MyGame
{
	public class MyForm : Form
    {
        public static Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();
        Game game = new Game();
        public static int ScreenWidth = 1920;
        public static int ScreenHeight = 1001;


        public MyForm()
        {
            ClientSize = new Size(1920, 1000);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Text = "Mushroom";
            DoubleBuffered = true;

            KeyDown += game.KeyPressed;
            KeyUp += game.KeyUnPressed;
            
            game.BackGroundMusic.Open(new Uri(new DirectoryInfo("Music").FullName + @"\backGround.mp3"));
            var imagesDirectory = new DirectoryInfo("Images");
            foreach(var e in imagesDirectory.GetFiles("*"))
                bitmaps[e.Name]=(Bitmap)Bitmap.FromFile(e.FullName);
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 1;
            timer.Tick += TimerTick;
            timer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graph = e.Graphics;
            graph.DrawImage(bitmaps[game.BackGround1.GetFileName()], game.BackGround1.Location());
            graph.DrawImage(bitmaps[game.BackGround2.GetFileName()], game.BackGround2.Location());


            //var image = bitmaps[game.BackGround1.GetFileName()];
            //var x1 = game.BackGround1.X;
            //var y1 = game.BackGround1.Y;
            //graph.DrawImage(image, new Rectangle(0, 0, ScreenWidth, ScreenHeight), new Rectangle(-x1, 0, image.Width, image.Height), GraphicsUnit.Pixel);

            //var x2 = game.BackGround2.X;
            //var y2 = game.BackGround2.Y;
            //graph.DrawImage(image, new Rectangle(0, 0, ScreenWidth, ScreenHeight), new Rectangle(-x2+(ScreenWidth - image.Width), 0, image.Width, image.Height), GraphicsUnit.Pixel);


            for (var i = 0; i < game.Bombs.Count(); i++)
            {
                graph.DrawImage(bitmaps[game.Bombs[i].Item1.GetFileName], game.Bombs[i].Item1.Location);
                if (game.Bombs[i].Item2.GetFileName != null)
                    graph.DrawImage(bitmaps[game.Bombs[i].Item2.GetFileName], game.Bombs[i].Item2.Location);
            }
            if (!game.GameOver && !game.Menu)
            {
                graph.DrawImage(bitmaps[game.player.GetFileName()], game.player.Location());
                e.Graphics.DrawString("" + (game.player.Scores+ game.player.Bonuses), new Font("Arial", 35), Brushes.White, 20, 20);
            }
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            //stringFormat.LineAlignment = StringAlignment.Center;

            var height = bitmaps[game.BackGround1.GetFileName()].Height.ToString();
            var width = bitmaps[game.BackGround1.GetFileName()].Width.ToString();

            if (game.PrintRecord) e.Graphics.DrawString("Record " + game.Record, new Font("Arial", 30), Brushes.White,
                                                    game.BackGround1.Width / 2, game.BackGround1.Height / 2+150, stringFormat);
            if (game.GameOver && !game.Menu)
            {
                e.Graphics.DrawString("GAME OVER", new Font("Arial", 50), Brushes.White, 
                                      game.BackGround1.Width / 2, game.BackGround1.Height / 2 - 100, stringFormat);
                if (game.NewRecord)
                {
                    e.Graphics.DrawString("New record " + (game.player.Scores + game.player.Bonuses), new Font("Arial", 30), Brushes.White,
                                      game.BackGround1.Width / 2, game.BackGround1.Height / 2 + 50, stringFormat);
                }
                else
                {
                    e.Graphics.DrawString("Scores " + (game.player.Scores + game.player.Bonuses), new Font("Arial", 30), Brushes.White,
                                          game.BackGround1.Width / 2, game.BackGround1.Height / 2 + 50, stringFormat);
                }
                game.BackGroundMusic.Pause();
            }
            if (game.Menu)
                e.Graphics.DrawString("Mushroom", new Font("Arial", 60), Brushes.White, 
                                      game.BackGround1.Width / 2, game.BackGround1.Height / 2 - 50, stringFormat);
        }

        public static int tickCount = 0;

        //public object DrawUnit { get; private set; }

        void TimerTick(object sender, EventArgs args)
        {
            game.Act(game);
            tickCount++;
            Invalidate();
        }
    }
}
