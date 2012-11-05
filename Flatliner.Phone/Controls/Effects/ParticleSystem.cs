namespace Flatliner.Phone.Controls.Effects
{
    using System;
    using System.Windows.Controls;
    using System.Collections.Generic;
    using System.Windows.Threading;
    using Flatliner.Portable;

    public class ParticleSystem : Canvas
    {
        /// <summary>
        /// Frames per second of the update timer
        /// </summary>
        public const int Fps = 25;

        private readonly List<Particle> particles = new List<Particle>();

        /// <summary>
        /// Initializes a new instance of the ParticleSystem class.
        /// </summary>
        public ParticleSystem(IFactory<Particle> particleFactory)
        {
            this.MaximumParticleCount = 500;
            this.ParticleFactory = particleFactory;
        }

        private DispatcherTimer timer;

        public IFactory<Particle> ParticleFactory
        {
            get;
            private set;
        }

        public IList<Particle> Particles
        {
            get
            {
                return this.particles;
            }
        }

        public int MaximumParticleCount
        {
            get;
            set;
        }

        public void Start()
        {
            this.timer = new DispatcherTimer 
            { 
                Interval = TimeSpan.FromMilliseconds(1000 / Fps)
            };

            this.timer.Tick += this.TimerTick;
            this.timer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            this.MoveParticles();
        }

        protected virtual void SpawnParticles(double spawnX, double spawnY, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Particle particle;
                bool wasRecycled = false;
                if (this.recycled.Count != 0)
                {
                    wasRecycled = true;
                    particle = this.recycled.Dequeue();
                }
                else
                {
                    particle = this.ParticleFactory.Create();
                }

                particle.X = spawnX;
                particle.Y = spawnY;
                particle.Move(Fps);
                if (!wasRecycled)
                {
                    this.Children.Add(particle);
                }
                else
                {
                    particle.Opacity = 1;
                }

                this.Particles.Add(particle);
            }   
        }

        protected virtual void MoveParticles()
        {
            for (int i = this.Particles.Count - 1; i >= 0; i--)
            {
                var particle = this.Particles[i];
                particle.Move(Fps);
                if (!particle.IsAlive)
                {
                    if (this.Particles.Count > MaximumParticleCount)
                    {
                        this.Children.Remove(particle);
                        this.Particles.Remove(particle);
                    }
                    else
                    {
                        this.RecycleParticle(particle);
                    }
                }
            }
        }

        private Queue<Particle> recycled = new Queue<Particle>();

        protected virtual void RecycleParticle(Particle particle)
        {
            particle.Opacity = 0;
            this.Particles.Remove(particle);
            this.recycled.Enqueue(particle);
        }
    }
}
