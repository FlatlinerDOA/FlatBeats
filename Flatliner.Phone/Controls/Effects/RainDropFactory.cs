namespace Flatliner.Phone.Controls.Effects
{
    using Flatliner.Portable;

    public class RainDropFactory : IFactory<Particle>
    {
        public Particle Create()
        {
            return new RainDrop();
        }
    }
}