using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pixel_Sorter_3 {
    public partial class PixelSorter3Main : Form {
        public enum SortMode {
            None,
            Alpha,
            Red,
            Green,
            Blue,
            Hue,
            SaturationHSL,
            SaturationHSV,
            Lightness,
            Value,
        }

        public static float GetSaturationHSL(Color c) {
            float Xmax = Math.Max(c.R, Math.Max(c.G, c.B))/255.0f;
            float Xmin = Math.Min(c.R, Math.Min(c.G, c.B));
            float C = Xmax - Xmin;
            float L = (Xmax + Xmin) / 2;
            return L == 0 || L == 1 ? 0 : C/(1-Math.Abs(2*Xmax-C-1));
        }

        public static float GetSaturationHSV(Color c) {
            float Xmax = Math.Max(c.R, Math.Max(c.G, c.B))/255.0f;
            float Xmin = Math.Min(c.R, Math.Min(c.G, c.B));
            float C = Xmax - Xmin;
            return Xmax == 0 ? 0 : C / Xmax;
        }

        public static float GetLightness(Color c) {
            float Xmax = Math.Max(c.R, Math.Max(c.G, c.B)) / 255.0f;
            float Xmin = Math.Min(c.R, Math.Min(c.G, c.B));
            return (Xmax + Xmin) / 2;
        }

        public static float GetValue(Color c) {
            return Math.Max(c.R, Math.Max(c.G, c.B)) / 255.0f;
        }

        public class PixelComparer : Comparer<Color> {
            SortMode sortMode;
            public PixelComparer(SortMode mode) {
                sortMode = mode;
            }

            public override int Compare(Color x, Color y) {
                int ret = 0;
                switch (sortMode) {
                case SortMode.Alpha:
                    ret = x.A.CompareTo(y.A);
                    break;
                case SortMode.Red:
                    ret = x.R.CompareTo(y.R);
                    break;
                case SortMode.Green:
                    ret = x.G.CompareTo(y.G);
                    break;
                case SortMode.Blue:
                    ret = x.B.CompareTo(y.B);
                    break;
                case SortMode.Hue:
                    ret = x.GetHue().CompareTo(y.GetHue());
                    break;
                case SortMode.SaturationHSL:
                    ret = GetSaturationHSL(x).CompareTo(GetSaturationHSL(y));
                    break;
                case SortMode.SaturationHSV:
                    ret = GetSaturationHSV(x).CompareTo(GetSaturationHSV(y));
                    break;
                case SortMode.Lightness:
                    ret = GetLightness(x).CompareTo(GetLightness(y));
                    break;
                case SortMode.Value:
                    ret = GetValue(x).CompareTo(GetValue(y));
                    break;
                case SortMode.None:
                default:
                    return 0;
                }
                return ret;
            }
        }

        private static void Shuffle<T>(IList<T> list) {
            Random rand = new Random();
            int n = list.Count;

            for (int i = list.Count - 1; i > 1; i--) {
                int rnd = rand.Next(i + 1);

                T value = list[rnd];
                list[rnd] = list[i];
                list[i] = value;
            }
        }

        OpenFileDialog ofd;
        SaveFileDialog sfd;
        Bitmap img;

        private void sort(SortMode sortMode) {
            List<Color> pixels = new List<Color>(img.Width * img.Height);

            for(int y = 0; y < img.Height; y++)
                for(int x = 0; x < img.Width; x++)
                    pixels.Add(img.GetPixel(x, y));

            if (shuffleToolStripMenuItem.Checked)
                Shuffle(pixels);

            pixels.Sort(new PixelComparer(sortMode));

            for(int y = 0; y < img.Height; y++)
                for(int x = 0; x < img.Width; x++)
                    img.SetPixel(x, y, pixels[y * img.Width + x]);

            pictureBox1.Refresh();
        }

        private void reverse() {
            List<Color> pixels = new List<Color>(img.Width * img.Height);

            for(int y = 0; y < img.Height; y++)
                for(int x = 0; x < img.Width; x++)
                    pixels.Add(img.GetPixel(x, y));

            pixels.Reverse();

            for(int y = 0; y < img.Height; y++)
                for(int x = 0; x < img.Width; x++)
                    img.SetPixel(x, y, pixels[y * img.Width + x]);

            pictureBox1.Refresh();
        }

        public PixelSorter3Main() {
            ofd = new OpenFileDialog {
                AddToRecent = true,
                CheckFileExists = true,
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.gif;*.webp)|*.png;*.jpg;*.jpeg;*.gif;*.webp|All files (*.*)|*.*",
                FilterIndex = 0,
                RestoreDirectory = true,
                Title = "Open Image",
            };

            sfd = new SaveFileDialog {
                AddToRecent = true,
                CheckWriteAccess = true,
                DereferenceLinks = true,
                Filter = "All files (*.*)|*.*",
                OverwritePrompt = true,
                RestoreDirectory = true,
                Title = "Save Image",
            };

            img = new Bitmap(1, 1);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            InitializeComponent();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e) {
            e.Graphics.Clear(TransparencyKey);
            e.Graphics.DrawImage(img, 0, 0, pictureBox1.Width, pictureBox1.Height);
        }

        private void pictureBox1_Resize(object sender, EventArgs e) {
            pictureBox1.Refresh();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            string filePath = string.Empty;

            if (ofd.ShowDialog() == DialogResult.OK) {
                filePath = ofd.FileName;
                img = new Bitmap(filePath);
                //pictureBox1.Image = img;
                pictureBox1.Refresh();
                sortToolStripMenuItem.Enabled = true;
                saveAsToolStripMenuItem.Enabled = true;
                alphaToolStripMenuItem.Enabled = Image.IsAlphaPixelFormat(img.PixelFormat);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) {
            if (sfd.ShowDialog() == DialogResult.OK) {
                img.Save(sfd.FileName);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void reverseToolStripMenuItem_Click(object sender, EventArgs e) {
            reverse();
        }

        private void alphaToolStripMenuItem_Click(object sender, EventArgs e) {
            sort(SortMode.Alpha);
        }

        private void redToolStripMenuItem_Click(object sender, EventArgs e) {
            sort(SortMode.Red);
        }

        private void greenToolStripMenuItem_Click(object sender, EventArgs e) {
            sort(SortMode.Green);
        }

        private void blueToolStripMenuItem_Click(object sender, EventArgs e) {
            sort(SortMode.Blue);
        }

        private void hueToolStripMenuItem_Click(object sender, EventArgs e) {
            sort(SortMode.Hue);
        }

        private void saturationToolStripMenuItem_Click(object sender, EventArgs e) {
            sort(SortMode.SaturationHSL);
        }

        private void saturationHSVToolStripMenuItem_Click(object sender, EventArgs e) {
            sort(SortMode.SaturationHSV);
        }

        private void luminosityToolStripMenuItem_Click(object sender, EventArgs e) {
            sort(SortMode.Lightness);
        }

        private void valueToolStripMenuItem_Click(object sender, EventArgs e) {
            sort(SortMode.Value);
        }
    }
}
