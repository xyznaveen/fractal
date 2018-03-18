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

        }

        /// <summary>
        /// Below lies the equivalent working C# code from Java source
        /// </summary>

        // Load when the form loads
        public void init() // all instances will be prepared
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

        /// <summary>
        /// End of migrated source code
        /// </summary>

        /// <summary>
        /// Additional source code will be added below
        /// </summary>


        /// <summary>
        /// End of additional source code
        /// </summary>


    }
}
