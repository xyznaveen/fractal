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
                Update(g);
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
                Mandelbrot();
            }
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
                SaveCurrentState(saveFileDialog.FileName);
            }
        }

        private void resetSavedStateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool fileWasDeleted = DeleteConfigurationFile(ConfigFileName);

            if (fileWasDeleted)
                MessageBox.Show("State file has been successfully reset.","Success");
        }

        /// <summary>
        /// This method generates a set of random colors and then redraws the mandelbrot set.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void changeColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RandomizeColors();
            Mandelbrot();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CycleExistingColors();
        }

        private void startStopColorCyclingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(colorIsCycling)  // Stop color cycling if already running
            {
                timer1.Stop();
            } else              // Start color cycling
            {
                timer1.Start();
            }

            // Toggle color cycling condition
            colorIsCycling = !colorIsCycling;
        }

        private void loadStateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            
            // custom file extension
            openFileDialog.Filter = "Mandlebrot Saved State | *.mdlbrt";
            bool fileOpened = openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;

            if (fileOpened)
            {
                string fileToOpen = openFileDialog.FileName;

                string[] state = ReadValues(fileToOpen);

                xstart = float.Parse(state[0]);
                ystart = float.Parse(state[1]);
                xende = float.Parse(state[2]);
                yende = float.Parse(state[3]);
                
                // Sets the zoom level
                xzoom = (xende - xstart) / (double)x1;
                yzoom = (yende - ystart) / (double)y1;

                LoadSavedColor(state);

                Mandelbrot();
            }
        }

        private void RootForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save state to the default config file
            // When the close buton is pressed on the Window
            SaveCurrentState(ConfigFileName);
        }

        private void resetColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Resets to default color
            // When the menu item is clicked
            for (int i = 0; i < 6; ++i)
            {
                palette[i] = Color.FromArgb(255,255,255);
            }
            Mandelbrot();
        }

        /// <summary>
        /// Lines below are converted code from Java source
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
            string[] state = ReadValues(ConfigFileName);
            if (isSessionLaunched && state.Length > 0)
            {
                isSessionLaunched = false;
                xstart = float.Parse(state[0]);
                ystart = float.Parse(state[1]);
                xende = float.Parse(state[2]);
                yende = float.Parse(state[3]);

                LoadSavedColor(state);
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
            InitColors();
            initvalues();
            xzoom = (xende - xstart) / (double)x1;
            yzoom = (yende - ystart) / (double)y1;
            Mandelbrot();
        }

        private void Mandelbrot() // The mandelbrot set generation algorithm
        {
            int x, y;
            float h, b, alt = 0.0f;
            Pen pen = null;

            action = false;
            this.Cursor = c1;

            for (x = 0; x < x1; x += 2)
                for (y = 0; y < y1; y++)
                {
                    h = Pointcolour(xstart + xzoom * (double)x, ystart + yzoom * (double)y); // color value
                    if (h != alt)
                    {
                        b = 1.0f - h * h; // brightnes
                        Color col = HSB.HSBtoRGB(h, 0.8f, b); // Prepare RGB based on HSB
                        pen = new Pen(col);
                        alt = h;
                    }
                    g1.DrawLine(pen, x, y, x + 1, y);
                }
            this.Cursor = c2;
            action = true;
        }

        public void Update(Graphics g)  // Update the graphics on the screen
        {
            Pen pen = new Pen(Color.White);
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

        private float Pointcolour(double xwert, double ywert) // color value from 0.0 to 1.0 by iterations
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

        private string[] ReadValues(string fileName)
        {
            string[] list = { }; // To hold the state values from file
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
                    list = temp.Split(','); // Split the comma separated values to array
                    file.Close();
                }
                catch (Exception) { }
            }

            return list;
        }
        
        private void InitColors()
        {
            
            if(!hasSavedColor) // If there are no color saved
            {
                for (var i = 0; i < 6; ++i)
                {
                    // Default color 255, 255, 255 
                    palette[i] = Color.FromArgb(255, 255, 255);
                }
                return;
            }

            RandomizeColors();
        }

        /// <summary>
        /// The method generates an array of 6 random color and assigns it to the default color palette.
        /// </summary>
        private void RandomizeColors()
        {
            Random rn = new Random();
            palette = new Color[6];
            for (var i = 0; i < 6; ++i)
            {
                palette[i] = Color.FromArgb(rn.Next(0,255), rn.Next(0,255), rn.Next(0,255));
            }
        }

        private void CycleExistingColors()
        {
            // Hold the last color
            Color temp = palette[palette.Length - 1];

            // Shift the colors by 1 to the right
            for (int i = palette.Length - 2; i > -1; i--)
            {
                palette[i + 1] = palette[i];
            }

            // Copy the last color to the first
            palette[0] = temp;

            // Show the changes on screen
            Mandelbrot();
            canvas.Refresh();
        }

        private bool DeleteConfigurationFile(string filePath)
        {
            bool fileWasFound = File.Exists(filePath);
            if (fileWasFound)
                File.Delete(filePath);

            return fileWasFound;
        }

        /// <summary>
        /// Saves the state information to the configuration file
        /// </summary>
        /// <param name="filePath"></param>
        private void SaveCurrentState(string filePath)
        {
            StreamWriter file = new StreamWriter(filePath);

            file.WriteLine(xstart);file.WriteLine(ystart);
            file.WriteLine(xende);file.WriteLine(yende);

            for(int i =0; i < palette.Length; ++i) // Color palette
            {
                file.WriteLine(palette[i].R + "," + palette[i].R + "," + palette[i].R);
            }

            file.Close();
        }

        /// <summary>
        /// Extracts the color from the saved state file and assigns it as the current working palette
        /// </summary>
        /// <param name="state"></param>
        private void LoadSavedColor(string[] state)
        {
            // populate color
            int counter = 0;

            // Index of color starts at 4th line in the saved file
            for (int i = 4; i < state.Length; i += 3)
            {
                try
                {
                    if ((i + 3) < state.Length)
                    {
                        Color c = Color.FromArgb(int.Parse(state[i]), int.Parse(state[i + 1]), int.Parse(state[i + 2]));
                        palette[counter] = c;
                        counter++;
                    }
                }
                catch (Exception ex) { }
            }
        }
        
        /// <summary>
        /// End of additional source code
        /// </summary>


    }
}
