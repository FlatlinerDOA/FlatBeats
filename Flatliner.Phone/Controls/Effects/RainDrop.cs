namespace Flatliner.Phone.Controls.Effects
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;

    public class RainDrop : Particle
    {
        private readonly Line line;
        
        /// <summary>
        /// Initializes a new instance of the RainDrop class.
        /// </summary>
        public RainDrop()
        {
            this.Gravity = 250;
            this.YVelocity = 50;
            this.line = new Line
            { 
                X1 = 0, 
                X2 = 0,
                Y1 = -2,
                Y2 = 0,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
                StrokeThickness = 1,
            };

            this.Children.Add(this.line);
        }

        public override void Move(double currentFps)
        {
            this.line.X2 = this.XVelocity;
            this.line.Y1 = -Math.Min(this.YVelocity / 25, 10);
            base.Move(currentFps);
            if (this.Parent != null)
            {
                this.IsAlive = this.Y <= ((ParticleSystem)this.Parent).ActualHeight;
            }
        }
    }
}
