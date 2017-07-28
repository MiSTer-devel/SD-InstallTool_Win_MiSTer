using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace SDInstallTool.UI
{
    public class ColorProgressBar : ProgressBar
    {
        private Brush color = Brushes.Green;

        public ColorProgressBar()
        {
            this.Style = ProgressBarStyle.Continuous;
            //this.SetStyle(ControlStyles.UserPaint, true);
        }

        public ColorProgressBar(Color color): this()
        {
            //this.ForeColor = color;
            this.color = new SolidBrush(color);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rec = e.ClipRectangle;

            rec.Width = (int)(rec.Width * ((double)Value / Maximum)) - 4;
            if (ProgressBarRenderer.IsSupported)
                ProgressBarRenderer.DrawHorizontalBar(e.Graphics, e.ClipRectangle);
            rec.Height = rec.Height - 4;
            e.Graphics.FillRectangle(this.color, 2, 2, rec.Width, rec.Height);
        }
    }
}
