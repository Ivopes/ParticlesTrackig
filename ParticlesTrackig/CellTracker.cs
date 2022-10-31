using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ParticlesTrackig
{
    public class CellTracker
    {
        private string outputDir;

        private static readonly string Black = "ff000000";
        private static readonly string White = "ffffffff";
        private static readonly Vector2 Up = new Vector2(0, -1);
        private static readonly Vector2 Right = new Vector2(1, 0);
        private static readonly Vector2 Down = new Vector2(0, 1);
        private static readonly Vector2 Left = new Vector2(-1, 0);

        private const float MaxCentroidLength = 14f * 14f; // squared, pro rychlejsi vypocet
        private bool[,] _checkedMask;
        private int _time = -1;

        private List<List<Particle>> _particles = new();
        public CellTracker(string folder, string outputFolder)
        {
            outputDir = outputFolder; ;

            var files = Directory.GetFiles(folder);

            foreach (var file in files)
            {
                ++_time;
                Console.WriteLine(file);

                List<Particle> par = FindParticlesInPicture(file);
                var news = par.Where(p => p.TimeOfCreation == _time).ToList();
                _particles.Add(par);
                /*
                if (_particles.Count > 1)
                {
                    List<Particle> yes = new();
                    List<Particle> no = new();
                    for (int i = 0; i < par.Count; i++)
                    {
                        if (TryFindLastParent(par[i].GetCenInT(0), _time-1, out var result)) // cas 0 ptotoze jsou ted vytvorene
                        {
                            //result.Positions.Add(par[i].GetPosInT(0)); // pridat aktualni pozice a centr do rodice
                            //result.Centroids.Add(par[i].GetCenInT(0));
                            //par[i] = result; // priradit rodice na tento prvek (jsou vlastne stejni)
                            yes.Add(result);
                        }
                        else
                        {
                            no.Add(par[i]);
                        }
                    }

                    ChangeBitmapColor(file, no);
                }
                */
                //ChangeBitmapColor(file, news);
                DrawDebugLines(file, par);
            }


        }

        private void DrawDebugLines(string filename, List<Particle> par)
        {
            var bmp = new Bitmap(filename);

            Pen redPen = new Pen(Color.Red, 1);

            foreach (Particle particle in par)
            {
                for (int i = 1; i < particle.Centroids.Count; i++)
                {
                    if (i < 1) continue;
                    Vector2 cen = particle.Centroids[i];
                    Vector2 cenLast = particle.Centroids[i-1];

                    using (var graphics = Graphics.FromImage(bmp))
                    {
                        graphics.DrawLine(redPen, cen.X, cen.Y, cenLast.X, cenLast.Y);
                    }
                }
            }

            SaveBmp(filename.Substring(filename.LastIndexOf("\\")), bmp);
        }
        private List<Particle> FindParticlesInPicture(string file)
        {
            Bitmap bitmap = new Bitmap(file);

            _checkedMask = new bool[bitmap.Height, bitmap.Width];

            var par = new List<Particle>();

            for (int i = 0; i < bitmap.Height; i++)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    var pixel = bitmap.GetPixel(j, i);

                    if (IsWhite(pixel))
                    {
                        if (TryFindParticle(bitmap, _checkedMask, new Vector2(j, i), out var particle))
                        {
                            if (_time > 0)
                            {
                                if (TryFindLastParent(particle.GetCenInT(0), _time-1, out var parentPar))
                                {
                                    if (parentPar.Centroids.Count <= _time)
                                    {
                                        //parentPar.Positions.Add(particle.GetPosInT(0));
                                        //parentPar.Centroids.Add(particle.GetCenInT(0));
                                        //particle = parentPar;
                                    }
                                    else // vytvor novy, pravdepodobne se rozpulila, nebo byly na sobe
                                    {
                                        /*var dividedP = new Particle(parentPar);
                                        dividedP.Positions.Add(particle.GetPosInT(0));
                                        dividedP.Centroids.Add(particle.GetCenInT(0));
                                        particle = dividedP*/
                                    }
                                    particle.TimeOfCreation = parentPar.TimeOfCreation;
                                    particle.Centroids.InsertRange(0, parentPar.Centroids);
                                    particle.Positions.InsertRange(0, parentPar.Positions);
                                }
                            }
                            par.Add(particle);
                        }
                    }
                }
            }

            return par;
        }
        private bool IsWhite(Color pixel)
        {
            return pixel.Name == White;
        }
        private bool IsBlack(Color pixel)
        {
            return !IsWhite(pixel);
        }
        private bool TryFindLastParent(Vector2 centroid, int timeToFind, out Particle par)
        {
            var particles = _particles[timeToFind];

            float minDist = float.MaxValue;
            Particle p = null;
            foreach (var particle in particles)
            {
                var l = (centroid - particle.GetCenInT(timeToFind - particle.TimeOfCreation)).LengthSquared(); // time takovy, protoze byly vytvoreny mozna pozdeji.
                if (l < minDist && l < MaxCentroidLength)
                {
                    minDist = l;
                    p = particle;
                }
            }
            par = p; 
            if (p == null)
            {
                return false;
            }
            return true;
        }
        private bool TryFindParticle(Bitmap bitmap, bool[,] checkMask, Vector2 point, out Particle particle) 
        {
            if (IsBlack(bitmap.GetPixel((int)point.X, (int)point.Y)) || checkMask[(int)point.Y, (int)point.X])
            {
                particle = default;
                return false;
            }

            Stack<Vector2> searchStack = new Stack<Vector2>();
            searchStack.Push(point);

            var resultPositions = new List<Vector2>();
            
            do
            {
                var p = searchStack.Pop();
                checkMask[(int)p.Y, (int)p.X] = true;
                resultPositions.Add(p);

                Vector2 newP = p.Add(Up);

                if (IsInArea(bitmap, newP) && !checkMask[(int)newP.Y, (int)newP.X])
                {
                    var c = bitmap.GetPixel((int)newP.X, (int)newP.Y);

                    if (IsWhite(c) && !searchStack.Contains(newP))
                        searchStack.Push(newP);
                }

                newP = p.Add(Right);

                if (IsInArea(bitmap, newP) && !checkMask[(int)newP.Y, (int)newP.X])
                {
                    var c = bitmap.GetPixel((int)newP.X, (int)newP.Y);

                    if (IsWhite(c) && !searchStack.Contains(newP))
                        searchStack.Push(newP);
                }
                newP = p.Add(Down);

                if (IsInArea(bitmap, newP) && !checkMask[(int)newP.Y, (int)newP.X])
                {
                    var c = bitmap.GetPixel((int)newP.X, (int)newP.Y);

                    if (IsWhite(c) && !searchStack.Contains(newP))
                        searchStack.Push(newP);
                }
                newP = p.Add(Left);

                if (IsInArea(bitmap, newP) && !checkMask[(int)newP.Y, (int)newP.X])
                {
                    var c = bitmap.GetPixel((int)newP.X, (int)newP.Y);

                    if (IsWhite(c) && !searchStack.Contains(newP))
                        searchStack.Push(newP);
                }

            } while (searchStack.Count > 0);

            var par = new Particle(resultPositions, _time);
            particle = par;
            return true;
        }
        private bool IsInArea(Bitmap b, Vector2 p)
        {
            return b.Width > p.X && p.X >= 0 && b.Height > p.Y && p.Y >= 0;
        }
        public static void PrintArray<T>(T[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write(matrix[i, j] + "\t");
                }
                Console.WriteLine();
            }
        }
        private void SaveBmp(string name, Bitmap bmp)
        {
            bmp.Save(outputDir + $"/{name}.png", ImageFormat.Png);
        }
        private void ChangeBitmapColor(string filename, List<Particle> particles)
        {
            var bmp = new Bitmap(filename);

            foreach (Particle particle in particles)
            {
                foreach (var v in particle.Positions[0])
                {
                    bmp.SetPixel((int)v.X ,(int)v.Y, Color.Red); 
                }
            }

            SaveBmp(filename.Substring(filename.LastIndexOf("\\")), bmp);
        }
        
    }
    static class PointExtension
    {
        public static Vector2 Add(this Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
    }
}
