/****************************************************************************

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

-- Copyright 2009 Terence Tsang
-- admin@shinedraw.com
-- http://www.shinedraw.com
-- Your Flash vs Silverlight Repositry

****************************************************************************/

/*
*	A Colourful Firework Demonstratoin in C#
*   from shinedraw.com
*/

namespace Flatliner.Phone.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Threading;

    public partial class ColorfulFireworks : Canvas
    {
        #region Constants and Fields

        /// <summary>
        /// Number of Dot generated each time
        /// </summary>
        private const double FireworkCount = 2;

        /// <summary>
        /// fps of the on enter frame event
        /// </summary>
        private const int Fps = 15;

        /// <summary>
        /// Gravity
        /// </summary>
        private const double Gravity = 0.5;

        /// <summary>
        /// Maximum Size
        /// </summary>
        private const int SizeMax = 3;

        /// <summary>
        /// Minimum Size
        /// </summary>
        private const int SizeMin = 1;

        /// <summary>
        /// Maximum X Velocity
        /// </summary>
        private const double XVelocity = 5;

        /// <summary>
        /// Maximum Y Velocity
        /// </summary>
        private const double YVelocity = 5;

        private readonly List<MagicDot> fireworks = new List<MagicDot>();

        private double streamerX;

        private double streamerY;

        /// <summary>
        /// on enter frame simulator
        /// </summary>
        private DispatcherTimer timer;

        private readonly Random random;
        #endregion

        #region Constructors and Destructors

        public ColorfulFireworks()
        {
            this.InitializeComponent();
            int seed = (int)DateTime.Now.Ticks;
            this.random = new Random(seed);
        }

        #endregion

        public void Start()
        {
            this.timer = new DispatcherTimer();
            this.timer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / Fps);
            this.timer.Tick += this.TimerTick;
            this.timer.Start();
        }

        private void AddFirework(double x, double y)
        {
            for (int i = 0; i < FireworkCount; i++)
            {
                double size = SizeMin + (SizeMax - SizeMin) * random.NextDouble();
                byte red = (byte)(128 + (128 * random.NextDouble()));
                byte green = (byte)(128 + (128 * random.NextDouble()));
                byte blue = (byte)(128 + (128 * random.NextDouble()));

                double xVelocity = XVelocity - 2 * XVelocity * random.NextDouble();
                double yVelocity = -YVelocity * random.NextDouble();

                MagicDot dot = new MagicDot(red, green, blue, size);
                dot.X = x;
                dot.Y = y;
                dot.XVelocity = xVelocity;
                dot.YVelocity = yVelocity;
                dot.Gravity = Gravity;
                dot.RunFirework();
                this.fireworks.Add(dot);

                this.LayoutRoot.Children.Add(dot);
            }
        }

        private void MoveFirework()
        {
            for (int i = this.fireworks.Count - 1; i >= 0; i--)
            {
                MagicDot dot = this.fireworks[i];
                dot.RunFirework();
                if (dot.Opacity <= 0.1)
                {
                    this.LayoutRoot.Children.Remove(dot);
                    this.fireworks.Remove(dot);
                }
            }
        }

        private void ResetStreamer()
        {
            this.streamerX = this.random.Next(0, (int)this.ActualWidth);
            this.streamerY = this.ActualHeight;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            this.streamerX += this.random.Next(10) - 5;
            this.streamerY -= 10;

            if (this.streamerY <= 0)
            {
                this.ResetStreamer();
            }

            this.AddFirework(this.streamerX, this.streamerY);
            this.MoveFirework();
        }
    }
}