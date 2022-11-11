using System.Numerics;

namespace ParticlesTrackig
{
    internal class Particle
    {
        public List<List<Vector2>> Positions = new();
        public List<Vector2> Centroids = new();
        public List<Vector2> GetPosInT(int t) => Positions[t - TimeOfCreation];
        public Vector2 GetCenInT(int t) => Centroids[t - TimeOfCreation];
        public int TimeOfCreation;
        public Particle()
        {
        }
        public Particle(List<Vector2> points, int timeOfCreation)
        {
            Positions.Add(points);

            Centroids.Add(GetCentroid(points));
            TimeOfCreation = timeOfCreation;
        }
        public Particle(Particle b)
        {
            //Positions.AddRange(b.Positions);
            for (int i = 0; i < b.Positions.Count; i++)
            {
                Positions.Add(b.Positions[i].ToList());
            }
            Centroids.AddRange(b.Centroids);

            TimeOfCreation = b.TimeOfCreation;
        }
        public static Vector2 GetCentroid(List<Vector2> points)
        {
            float x = 0;
            float y = 0;

            for (int i = 0; i < points.Count; i++)
            {
                x += points[i].X;
                y += points[i].Y;
            }
            x /= points.Count;
            y /= points.Count;

            return new Vector2(Convert.ToInt32(x), Convert.ToInt32(y));
        }
    }
}
