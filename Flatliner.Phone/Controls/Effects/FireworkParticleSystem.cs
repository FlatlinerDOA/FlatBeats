namespace Flatliner.Phone.Controls.Effects
{
    using System;

    public class FireworkParticleSystem : ParticleSystem
    {
        #region Constants and Fields

        /// <summary>
        /// Number of Dot generated each time
        /// </summary>
        private const int FireworkCount = 2;

        private double streamerX;

        private double streamerY;

        private readonly Random random;

        #endregion

        #region Constructors and Destructors

        public FireworkParticleSystem() : base(new FireworkParticleFactory())
        {
            int seed = (int)DateTime.Now.Ticks;
            this.random = new Random(seed);
        }

        #endregion

        private void ResetStreamer()
        {
            this.streamerX = this.random.Next(0, (int)this.ActualWidth);
            this.streamerY = this.ActualHeight;
        }

        protected override void MoveParticles()
        {
            this.streamerX += this.random.Next(10) - 5;
            this.streamerY -= 10;

            if (this.streamerY <= 0)
            {
                this.ResetStreamer();
            }

            this.SpawnParticles(this.streamerX, this.streamerY, FireworkCount);
            base.MoveParticles();
        }
    }
}