namespace Flatliner.Phone.Controls.Effects
{
    using System;
    using Flatliner.Portable;

    public class FireworkParticleFactory : IFactory<Particle>
    {
        /// <summary>
        /// Maximum X Velocity
        /// </summary>
        private const double XVelocity = 50;

        /// <summary>
        /// Maximum Y Velocity
        /// </summary>
        private const double YVelocity = 50;

        /// <summary>
        /// Maximum Size
        /// </summary>
        private const int SizeMax = 3;

        /// <summary>
        /// Minimum Size
        /// </summary>
        private const int SizeMin = 1;

        /// <summary>
        /// Gravity
        /// </summary>
        private const double Gravity = 9.8;

        /// <summary>
        /// Initializes a new instance of the FireworkParticleFactory class.
        /// </summary>
        public FireworkParticleFactory()
        {
            int seed = (int)DateTime.Now.Ticks;
            this.random = new Random(seed);
        }

        private readonly Random random;

        public Particle Create()
        {
            double size = SizeMin + (SizeMax - SizeMin) * random.NextDouble();
            byte red = (byte)(128 + (128 * random.NextDouble()));
            byte green = (byte)(128 + (128 * random.NextDouble()));
            byte blue = (byte)(128 + (128 * random.NextDouble()));

            double xVelocity = XVelocity - 2 * XVelocity * random.NextDouble();
            double yVelocity = -YVelocity * random.NextDouble();
            var dot = new FireworkParticle(red, green, blue, size)
                {
                    XVelocity = xVelocity, 
                    YVelocity = yVelocity, 
                    Gravity = Gravity
                };
            return dot;
        }
    }
}
