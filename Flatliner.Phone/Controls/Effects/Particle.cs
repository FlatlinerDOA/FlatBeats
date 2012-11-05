namespace Flatliner.Phone.Controls.Effects
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public class Particle : Canvas
    {
        public const int TerminalVelocity = 800;
        
        /// <summary>
        /// Initializes a new instance of the Particle class.
        /// </summary>
        public Particle()
        {
            this.IsAlive = true;
        }

        public virtual void Move(double currentFps)
        {
            if (this.YVelocity < TerminalVelocity)
            {
                this.YVelocity += this.Gravity / currentFps;
            }

            this.Y += this.YVelocity / currentFps;
            this.X += this.XVelocity / currentFps;
        }

        public double YVelocity
        {
            get;
            set;
        }

        public double XVelocity
        {
            get;
            set;
        }

        public double X
        {
            get { return (double)(GetValue(Canvas.LeftProperty)); }
            set { SetValue(Canvas.LeftProperty, value); }
        }

        public double Y
        {
            get { return (double)(GetValue(Canvas.TopProperty)); }
            set { SetValue(Canvas.TopProperty, value); }
        }

        /// <summary>
        /// Gets the number of pixels per second squared that a particle accelerates.
        /// </summary>
        public double Gravity
        {
            get;
            set;
        }

        public bool IsAlive
        {
            get;
            set;
        }
    }
}
