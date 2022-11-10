using ParticlesTrackig;
using System.Drawing;

var files = Directory.GetFiles("../../../output");
var filesCorrect = Directory.GetFiles("../../../output - Copy");
/*
for (int i = 0; i < files.Length; i++)
{
    var file = files[i];
    var fileCorrect = filesCorrect[i];

    Bitmap bitmap = new Bitmap(file);
    Bitmap bitmapCorrect = new Bitmap(fileCorrect);
    Console.WriteLine(file);
    for (int j = 0; j < bitmap.Height; j++)
    {
        for (int k = 0; k < bitmap.Width; k++)
        {
            var pixel = bitmap.GetPixel(k, j);
            var pixelCorr = bitmapCorrect.GetPixel(k, j);

            if (pixel != pixelCorr)
            {
                Console.WriteLine($"error X: {k} Y: {j}");
                return;
            }
        }
    }
}
*/

Directory.CreateDirectory("../../../output");
var tracker = new CellTracker("../../../unet-pred-binarize", "../../../output");
//var tracker = new CellTracker("../../../Test");

tracker.TrackCells(true);

var speed = tracker.GetAverageSpeed(true);

Console.WriteLine($"\nAverage speed is \nX: {speed.X} pixels\nY: {speed.Y} pixels");
