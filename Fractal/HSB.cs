using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fractal
{
    class HSB
    {
        public static Color HSBtoRGB(float hue, float saturation, float brightness)
        {
            int r = 0, g = 0, b = 0;
            if (saturation == 0)
            {
                r = g = b = (int)(brightness * 255.0f + 0.5f);
            }
            else
            {
                float h = (hue - (float)Math.Floor(hue)) * 6.0f;
                float f = h - (float)Math.Floor(h);
                float p = brightness * (1.0f - saturation);
                float q = brightness * (1.0f - saturation * f);
                float t = brightness * (1.0f - (saturation * (1.0f - f)));
                switch ((int)h)
                {
                    case 0:
                        Color tempColor0 = RootForm.palette[0];
                        r = (int)(brightness * (tempColor0.R * 1.0f) + 0.5f);
                        g = (int)(t * (tempColor0.G * 1.0f) + 0.5f);
                        b = (int)(p * (tempColor0.B * 1.0f) + 0.5f);
                        break;
                    case 1:
                        Color tempColor1 = RootForm.palette[1];
                        r = (int)(q * (tempColor1.R * 1.0F) + 0.5f);
                        g = (int)(brightness * (tempColor1.G * 1.0F) + 0.5f);
                        b = (int)(p * (tempColor1.B * 1.0F) + 0.5f);
                        break;
                    case 2:
                        Color tempColor2 = RootForm.palette[2];
                        r = (int)(p * (tempColor2.R * 1.0f) + 0.5f);
                        g = (int)(brightness * (tempColor2.G * 1.0f) + 0.5f);
                        b = (int)(t * (tempColor2.B * 1.0f) + 0.5f);
                        break;
                    case 3:
                        Color tempColor3 = RootForm.palette[3];
                        r = (int)(p * (tempColor3.R * 1.0f) + 0.5f);
                        g = (int)(q * (tempColor3.G * 1.0f) + 0.5f);
                        b = (int)(brightness * (tempColor3.B * 1.0f) + 0.5f);
                        break;
                    case 4:
                        Color tempColor4 = RootForm.palette[4];
                        r = (int)(t * (tempColor4.R * 1.0f) + 0.5f);
                        g = (int)(p * (tempColor4.G * 1.0f) + 0.5f);
                        b = (int)(brightness * (tempColor4.B * 1.0f) + 0.5f);
                        break;
                    case 5:
                        Color tempColor5 = RootForm.palette[5];
                        r = (int)(brightness * (tempColor5.R * 1.0f) + 0.5f);
                        g = (int)(p * (tempColor5.G * 1.0f) + 0.5f);
                        b = (int)(q * (tempColor5.B * 1.0f) + 0.5f);
                        break;
                }
            }
            return Color.FromArgb(Convert.ToByte(255), Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
        }
    }
}
