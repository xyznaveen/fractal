using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fractal
{
    public partial class RootForm : Form
    {

        // Used as a global variable to hold the color palette information to use in
        // the generation of mandlebrot set.
        public static Color[] palette = new Color[6];

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

        private Cursor c1;
        private Cursor c2;
        private Image picture;
        private Image tempPicture;
        private Graphics g1;

        private HSB HSBcol;

        public RootForm()
        {
            InitializeComponent();
            HSBcol = new HSB();
        }

        private void RootForm_Load(object sender, EventArgs e)
        {
            init();
            start();
        }
        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.DrawImage(picture, 0, 0);
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

            string[] state = readValues();

            if (isSessionLaunched && state.Length > 0)
            {
                isSessionLaunched = false;
                xstart = float.Parse(state[6]);
                ystart = float.Parse(state[7]);
                xende = float.Parse(state[8]);
                yende = float.Parse(state[9]);
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
            randomPalette();
            initvalues();
            xzoom = (xende - xstart) / (double)x1;
            yzoom = (yende - ystart) / (double)y1;
            mandelbrot();
        }

        /// <summary>
        /// The mandelbrot set generation algorithm.
        /// Calculates all the points.
        /// </summary>
        private void mandelbrot()
        {
            Console.WriteLine("Is being called");
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

        private string[] readValues()
        {
            string line = " ";
            string temp = "";
            string[] list = { };
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader("config.txt");
                while ((line = file.ReadLine()) != null)
                {
                    temp += (line + ",");
                }
                list = temp.Split(',');
                file.Close();
            }
            catch (Exception) { }

            return list;
        }

        private void colorCycling()
        {

            try
            {

                System.Drawing.Imaging.ColorPalette palette = tempPicture.Palette;
                //palette.Entries[0] = Color.Black;
                Random rn = new Random();

                for (int i = 1; i < 256; i++)
                {
                    palette.Entries[i] = Color.FromArgb(rn.Next(255), rn.Next(255), rn.Next(255));
                }

                tempPicture.Palette = palette;
            }
            catch (Exception)
            {
            }

        }

        private void randomPalette()
        {
            // TO generate random set of numbers for color
            Random rn = new Random();

            // Based on the HSB algorithm there are a total of 6 conditions
            // The clor will be random as the <code>rn</code> will be used to generate set of colors
            palette[0] = Color.FromArgb(rn.Next(255), rn.Next(255), rn.Next(255), rn.Next(255));
            palette[1] = Color.FromArgb(rn.Next(255), rn.Next(255), rn.Next(255), rn.Next(255));
            palette[2] = Color.FromArgb(rn.Next(255), rn.Next(255), rn.Next(255), rn.Next(255));
            palette[3] = Color.FromArgb(rn.Next(255), rn.Next(255), rn.Next(255), rn.Next(255));
            palette[4] = Color.FromArgb(rn.Next(255), rn.Next(255), rn.Next(255), rn.Next(255));
            palette[5] = Color.FromArgb(rn.Next(255), rn.Next(255), rn.Next(255), rn.Next(255));
        }

        /// <summary>
        /// End of additional source code
        /// </summary>


    }
}
