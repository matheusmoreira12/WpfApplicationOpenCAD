using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenCAD.Drawing
{

    static class AdvancedMethods
    {
        static public void SetPixelSmooth(this System.Drawing.Bitmap bitmap, System.Drawing.PointF where, System.Drawing.Color color)
        {
            int nearestX = (int)Math.Round(where.X),
                nearestY = (int)Math.Round(where.Y);

            float value = Math.Abs(where.X - nearestX) / 2 + Math.Abs(where.Y - nearestY) / 2;

            bitmap.SetPixel(nearestX, nearestY, color.CombineColors(bitmap.GetPixel(nearestX, nearestY), 1 - value));
        }

        static public System.Drawing.Color CombineColors(this System.Drawing.Color color1, System.Drawing.Color color2, float amount)
        {
            float colorA = color1.A * (1 - amount) + color2.A * amount,
                colorR = color1.R * (1 - amount) + color2.R * amount,
                colorG = color1.G * (1 - amount) + color2.G * amount,
                colorB = color1.B * (1 - amount) + color2.B * amount;

            return System.Drawing.Color.FromArgb((int)colorA, (int)colorR, (int)colorG, (int)colorB);
        }

        static public System.Drawing.Color InterpolateColors(System.Drawing.Color color1, float position1, System.Drawing.Color color2,
            float position2, float value)
        {
            if (position1 > position2)
                throw new InvalidOperationException("Cannot interpolate colors. Parameter \"position2\" must be greater than \"position1\".");

            if (value < position1)
            {
                return color1;
            }
            else if (value > position2)
            {
                return color2;
            }
            else
            {
                float amount = (value - position1) / (position2 - position1);
                return color1.CombineColors(color2, amount);
            }
        }
    }

    struct GradientStop
    {
        public System.Drawing.Color StopColor;
        public float Position;

        public GradientStop(System.Drawing.Color stopColor, float position)
        {
            StopColor = stopColor;
            Position = position;
        }
    }

    delegate EventHandler GradientStopCollectionChangedEvent(object sender, EventArgs args);

    class GradientStopCollection : List<GradientStop>
    {
        GradientStopCollectionChangedEvent GradientCollectionChanged;

        public void SelectSegment(float value, out GradientStop stop1, out GradientStop stop2)
        {
            stop1 = new GradientStop(System.Drawing.Color.Transparent, value);
            stop2 = new GradientStop(System.Drawing.Color.Transparent, value);

            for (int i = 1; i < Count; i++)
            {
                stop1 = this[i - 1];
                stop2 = this[i];

                if (stop1.Position <= value && value <= stop2.Position)
                    return;
            }
        }

        protected void OnGradientStopCollectionChanged(object sender, EventArgs args)
        {

        }

        public void OnGradientStopCollectionChanged(EventArgs args)
        {
            GradientCollectionChanged?.Invoke(this, args);
        }

        public new void Add(GradientStop item)
        {
            base.Add(item);
            OnGradientStopCollectionChanged(new EventArgs());
        }

		public new void AddRange(IEnumerable<GradientStop> items)
        {
            base.AddRange(items);
			OnGradientStopCollectionChanged(new EventArgs());
        }

        public GradientStopCollection() { }

        public GradientStopCollection(IEnumerable<GradientStop> items)
        {
            foreach (var item in items)
                base.Add(item);

            OnGradientStopCollectionChanged(new EventArgs());
        }
    }

    /// <summary>
    /// Wraps a conic gradient structure.
    /// </summary>
    class ConicGradientBrush
    {
        static public implicit operator System.Drawing.Brush(ConicGradientBrush gradient)
        {
            return gradient.brush;
        }

        public float RadiusX;
        public float RadiusY;

        private System.Drawing.Brush brush;

        public GradientStopCollection Stops { get; private set; }

        protected void Stops_OnGradientStopCollectionChanged(object sender, EventArgs args)
        {

        }

        private void updateBrush()
        {
            int width = (int)(RadiusX * 2),
                height = (int)(RadiusY * 2),
                size = Math.Max(width, height);

            System.Drawing.Bitmap brushBitmap = new System.Drawing.Bitmap(width, height);

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    float x = j - RadiusX,
                        y = i - RadiusY;

                    float value = (float)(Math.Atan2(y, x) / (2 * Math.PI));

                    if (value < 0)
                        value += 1;

                    GradientStop stop1, stop2;
                    Stops.SelectSegment(value, out stop1, out stop2);

                    System.Drawing.Color color = AdvancedMethods.InterpolateColors(stop1.StopColor, stop1.Position, stop2.StopColor,
                        stop2.Position, value);

                    brushBitmap.SetPixel(j, i, color);
                }

            brush = new System.Drawing.TextureBrush(brushBitmap);
        }

        public ConicGradientBrush()
        {
            Stops = new GradientStopCollection();
        }

        public ConicGradientBrush(params GradientStop[] items)
        {
            Stops = new GradientStopCollection(items.AsEnumerable());
        }

        public ConicGradientBrush(GradientStopCollection items)
        {
            Stops = new GradientStopCollection(items);
        }
    }
}
