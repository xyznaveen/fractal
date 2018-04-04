using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace Fractal
{
    public partial class RootForm : Form
    {

        // Used as a global variable to hold the color palette information to use in
        // the generation of mandlebrot set.
        public static Color[] palette = new Color[6];
        public static Color[] defaultPalette = new Color[6];
        private static string ConfigFileName = "state.mdlbrt";

        // Instance variables
        private static int x1;
        private static int y1;
        private static int xs;
        private static int ys;
        private static int xe;
        private static int ye;

        private static float xy;

        private const int MAX = 256;      // max iterations
        private const double SX = -2.025; // start value real
        private const double SY = -1.125; // start value imaginary
        private const double EX = 0.6;    // end value real
        private const double EY = 1.125;  // end value imaginary

        private static double xstart;
        private static double ystart;
        private static double xende;
        private static double yende;
        private static double xzoom;
        private static double yzoom;
        
        private static bool action;
        private static bool rectangle;
        private static bool finished;

        private bool isSessionLaunched = true;
        private bool hasSavedColor = false;
        private bool colorIsCycling = false;

        private Cursor c1;
        private Cursor c2;
        private Image picture;
        private Graphics g1;

        private HSB HSBcol;

        private static RootForm context;

        private System.Windows.Forms.Timer timerColorChange;

        public RootForm()
        {
            InitializeComponent();
            HSBcol = new HSB();
        }

        private void RootForm_Load(object sender, EventArgs e)
        {
            init();
            start();
            context = this;
            timerColorChange = new System.Windows.Forms.Timer();
            
        }


        
        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.DrawImage(picture, 0, 0);
        }

        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            action = true;
            if (action)
            {
                xs = e.X;
                ys = e.Y;
                rectangle = true;
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (action)
            {
                xe = e.X;
                ye = e.Y;
                Graphics g = canvas.CreateGraphics();
                update(g);
            }
        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            int z, w;

            if (action)
            {
                xe = e.X;
                ye = e.Y;
                if (xs > xe)
                {
                    z = xs;
                    xs = xe;
                    xe = z;
                }
                if (ys > ye)
                {
                    z = ys;
                    ys = ye;
                    ye = z;
                }
                w = (xe - xs);
                z = (ye - ys);
                if ((w < 2) && (z < 2)) initvalues();
                else
                {
                    if (((float)w > (float)z * xy)) ye = (int)((float)ys + (float)w / xy);
                    else xe = (int)((float)xs + (float)z * xy);
                    xende = xstart + xzoom * (double)xe;
                    yende = ystart + yzoom * (double)ye;
                    xstart += xzoom * (double)xs;
                    ystart += yzoom * (double)ys;
                }
                xzoom = (xende - xstart) / (double)x1;
                yzoom = (yende - ystart) / (double)y1;
                rectangle = false;
                mandelbrot();
            }
        }

        /// <summary>
        /// Below lies the equivalent working C# code from Java source
        /// </summary>

        // Getting the instances ready, must be called once when the form loads, or
        // in the constructor
        public void init() 
        {
            finished = false;
            c1 = Cursors.WaitCursor;
            c2 = Cursors.Cross;
            x1 = canvas.Size.Width;
            y1 = canvas.Size.Height;
            xy = (float)x1 / (float)y1;

            picture = new Bitmap(x1, y1);

            g1 = Graphics.FromImage(picture);

            finished = true;
        }

        private void initvalues() // reset start values
        {
            
            string[] state = readValues(ConfigFileName);

            if (isSessionLaunched && state.Length > 0)
            {
                isSessionLaunched = false;
                xstart = float.Parse(state[0]);
                ystart = float.Parse(state[1]);
                xende = float.Parse(state[2]);
                yende = float.Parse(state[3]);
            }
            else
            {
                xstart = SX;
                ystart = SY;
                xende = EX;
                yende = EY;
                if ((float)((xende - xstart) / (yende - ystart)) != xy)
                    xstart = xende - (yende - ystart) * (double)xy;
            }
        }
        
        public void start()
        {
            action = false;
            rectangle = false;
            initvalues();
            xzoom = (xende - xstart) / (double)x1;
            yzoom = (yende - ystart) / (double)y1;
            initColors();
            mandelbrot();
        }

        /// <summary>
        /// The mandelbrot set generation algorithm.
        /// Calculates all the points.
        /// </summary>
        private void mandelbrot()
        {
            int x, y;
            float h, b, alt = 0.0f;

            action = false;
            this.Cursor = c1;

            Pen pen = null;

            for (x = 0; x < x1; x += 2)
                for (y = 0; y < y1; y++)
                {
                    h = pointcolour(xstart + xzoom * (double)x, ystart + yzoom * (double)y); // color value
                    if (h != alt)
                    {
                        b = 1.0f - h * h; // brightnes
                        Color col = HSB.HSBtoRGB(h, 0.8f, b);
                        pen = new Pen(col);
                        alt = h;
                    }
                    g1.DrawLine(pen, x, y, x + 1, y);
                }
            this.Cursor = c2;
            action = true;
        }

        public void update(Graphics g)
        {

            Pen pen = new Pen(Color.White, 1);

            g.DrawImage(picture, 0, 0);

            if (rectangle)
            {
                if (xs < xe)
                {
                    if (ys < ye) g.DrawRectangle(pen, xs, ys, (xe - xs), (ye - ys));
                    else g.DrawRectangle(pen, xs, ye, (xe - xs), (ys - ye));
                }
                else
                {
                    if (ys < ye) g.DrawRectangle(pen, xe, ys, (xs - xe), (ye - ys));
                    else g.DrawRectangle(pen, xe, ye, (xs - xe), (ys - ye));
                }
            }
        }

        /// <summary>
        /// Used by the main algorithm for coloring the points.
        /// </summary>
        /// <param name="xwert"></param>
        /// <param name="ywert"></param>
        /// <returns></returns>
        private float pointcolour(double xwert, double ywert) // color value from 0.0 to 1.0 by iterations
        {
            double r = 0.0, i = 0.0, m = 0.0;
            int j = 0;

            while ((j < MAX) && (m < 4.0))
            {
                j++;
                m = r * r - i * i;
                i = 2.0 * r * i + ywert;
                r = m + xwert;
            }
            return (float)j / (float)MAX;
        }

        /// <summary>
        /// End of migrated source code
        /// </summary>

        /// <summary>
        /// Additional source code will be added below
        /// </summary>

        private string[] readValues(string fileName)
        {
            string[] list = { };
            bool theFileExists = File.Exists(fileName);

            if (theFileExists)
            {
                string line = " ";
                string temp = "";
                try
                {
                    System.IO.StreamReader file = new System.IO.StreamReader(fileName);
                    while ((line = file.ReadLine()) != null)
                    {
                        temp += (line + ",");
                    }
                    list = temp.Split(',');
                    file.Close();
                }
                catch (Exception) { }
            }

            return list;
        }
        
        private void initColors()
        {
            
            if(!hasSavedColor) // If there are no color saved
            {
                for (var i = 0; i < 6; ++i)
                {
                    palette[i] = Color.FromArgb(255, 255, 255);
                }
                return;
            }

            randomizeColors();
        }

        private void randomizeColors()
        {
            Random rn = new Random();
            palette = new Color[6];
            for (var i = 0; i < 6; ++i)
            {
                palette[i] = Color.FromArgb(rn.Next(0,255), rn.Next(0,255), rn.Next(0,255));
            }
        }

        private void colorGroup()
        {
            Color temp = palette[palette.Length - 1];
            for (int i = palette.Length - 2; i > -1; i--)
            {
                palette[i + 1] = palette[i];
            }
            palette[0] = temp;

            mandelbrot();
        }

        private bool deleteConfigurationFile(string filePath)
        {
            bool fileWasFound = File.Exists(filePath);
            if (fileWasFound)
                File.Delete(filePath);

            return fileWasFound;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JPEG Image| *.jpg | Bitmap Image | *.bmp | Gif Image | *.gif | Png Image | *.png";
            saveFileDialog.Title = "Save file as Image.";
            saveFileDialog.ShowDialog();


            if (saveFileDialog.FileName != "")
            {
                System.IO.FileStream fileStream = (System.IO.FileStream)saveFileDialog.OpenFile();
                switch (saveFileDialog.FilterIndex)
                {
                    case 1:
                        {
                            // JPEG
                            picture.Save(fileStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                            break;
                        }
                    case 2:
                        {
                            // BMP
                            picture.Save(fileStream, System.Drawing.Imaging.ImageFormat.Bmp);
                            break;
                        }
                    case 3:
                        {
                            // GIF
                            picture.Save(fileStream, System.Drawing.Imaging.ImageFormat.Gif);
                            break;
                        }
                    case 4:
                        {
                            // PNG
                            picture.Save(fileStream, System.Drawing.Imaging.ImageFormat.Png);
                            break;
                        }
                }
                fileStream.Close();
            }
        }

        private void saveCurrentScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Mandelbrot Save State | *.mdlbrt";

            if(saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                saveCurrentState(saveFileDialog.FileName);
            }

        }

        private void saveCurrentState(string filePath)
        {
            StreamWriter file = new StreamWriter(filePath);
            file.WriteLine(xstart);
            file.WriteLine(ystart);
            file.WriteLine(xende);
            file.WriteLine(yende);
            file.Close();
        }

        private void resetSavedStateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool fileWasDeleted = deleteConfigurationFile(ConfigFileName);

            if (fileWasDeleted)
                MessageBox.Show("Configuration was deleted.");

        }

        private void changeColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            randomizeColors();
            mandelbrot();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            colorGroup();
        }

        private void startStopColorCyclingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(colorIsCycling)
            {
                timer1.Stop();
            } else
            {
                timer1.Start();
            }
            colorIsCycling = !colorIsCycling;
        }

        private void loadStateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Mandlebrot Saved State | *.mdlbrt";
            bool fileOpened = openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;

            if (fileOpened)
            {
                string fileToOpen = openFileDialog.FileName;
                string[] state = readValues(fileToOpen);
                xstart = float.Parse(state[0]);
                ystart = float.Parse(state[1]);
                xende = float.Parse(state[2]);
                yende = float.Parse(state[3]);
                xzoom = (xende - xstart) / (double)x1;
                yzoom = (yende - ystart) / (double)y1;
                mandelbrot();
            }

        }

        private void RootForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveCurrentState(ConfigFileName);
        }

        private void resetColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for(int i = 0; i < 6; ++i)
            {
                palette[i] = Color.FromArgb(255,255,255);
            }
            mandelbrot();
        }

        /// <summary>
        /// End of additional source code
        /// </summary>


    }
}
