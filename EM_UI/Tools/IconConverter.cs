using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace EM_UI.Tools
{
    internal class IconConverter
    {
        //converts an image into an icon
        //image: the image that shall become an icon
        //size: the width and height of the icon, standard sizes are 16x16, 32x32, 48x48, 64x64
        //keepAspectRatio: whether the image should be squashed into a square or whether whitespace should be put around it
        internal static Icon MakeIcon(Image image, int size = 16, bool keepAspectRatio = true)
        {
            if (image == null)
                return null;

            Bitmap square = new Bitmap(size, size); // create new bitmap
            Graphics graphics = Graphics.FromImage(square); // allow drawing to it

            int x, y, w, h; // dimensions for new image

            if (!keepAspectRatio || image.Height == image.Width)
            {
                // just fill the square
                x = y = 0; // set x and y to 0
                w = h = size; // set width and height to size
            }
            else
            {
                // work out the aspect ratio
                float r = (float)image.Width / (float)image.Height;

                // set dimensions accordingly to fit inside size^2 square
                if (r > 1)
                { // w is bigger, so divide h by r
                    w = size;
                    h = (int)((float)size / r);
                    x = 0; y = (size - h) / 2; // center the image
                }
                else
                { // h is bigger, so multiply w by r
                    w = (int)((float)size * r);
                    h = size;
                    y = 0; x = (size - w) / 2; // center the image
                }
            }

            // make the image shrink nicely by using HighQualityBicubic mode
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(image, x, y, w, h); // draw image with specified dimensions
            graphics.Flush(); // make sure all drawing operations complete before we get the icon

            // following line would work directly on any image, but then
            // it wouldn't look as nice.
            return Icon.FromHandle(square.GetHicon());
        }
    }
}
