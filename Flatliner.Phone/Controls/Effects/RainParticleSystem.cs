namespace Flatliner.Phone.Controls.Effects
{
    using System;
    using System.Windows;

    public class RainParticleSystem : ParticleSystem
    {
        private const int SpawnDropsPerSecond = 30;

        private readonly Random random;

        /// <summary>
        /// Initializes a new instance of the RainParticleSystem class.
        /// </summary>
        public RainParticleSystem() : base(new RainDropFactory())
        {
            int seed = (int)DateTime.Now.Ticks;
            this.random = new Random(seed);
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;
        }

        protected override void MoveParticles()
        {
            this.SpawnParticles(this.ActualWidth * this.random.NextDouble(), 0, SpawnDropsPerSecond / Fps);
            base.MoveParticles();
        }
    }
}
