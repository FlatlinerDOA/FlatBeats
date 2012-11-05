namespace Flatliner.Phone.Controls.Effects
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;

    public class FireworkParticle : Particle
    {
        private const int EllipseCount = 5;      // Number of ellipse make up of the magic dot
        private const double DotOpacity = 0.6;       // Initial opacity of the magic dot
        private const double DotOpacityIncrement = -0.15; // Opacitiy Increment for the next ellipse

        // for firwork
        public double FireworkOpacityIncrement = -0.20;

        public FireworkParticle(byte red, byte green, byte blue, double size)
        {
            double opac = DotOpacity;

            for (int i = 0; i < EllipseCount; i++)
            {
                Ellipse ellipse = new Ellipse
                    {
                        Width = size, 
                        Height = size
                    };

                if (i == 0)
                {
                    // add a white dot in the center
                    ellipse.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
                else
                {
                    ellipse.Fill = new SolidColorBrush(Color.FromArgb(255, red, green, blue));
                    ellipse.Opacity = opac;
                    opac += DotOpacityIncrement;
                    size += size;
                }

                // reposition the dots and add to the stage
                ellipse.SetValue(Canvas.LeftProperty, -ellipse.Width / 2);
                ellipse.SetValue(Canvas.TopProperty, -ellipse.Height / 2);
                this.Children.Add(ellipse);
            }
        }

        public override void Move(double currentFps)
        {
            this.Opacity += FireworkOpacityIncrement / currentFps;
            base.Move(currentFps);
            this.IsAlive = this.Opacity > 0.01;
        }
    }
}
