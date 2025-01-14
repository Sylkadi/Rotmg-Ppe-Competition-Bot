using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace DiscordBot.Bot.RealmBot.Game
{
    public class ProcessedImages
    {
        public static void Process(string fromPath, string toPath, int width, int height)
        {
            if (width > 1024) width = 1024;
            if (height > 1024) height = 1024;
            string[] imagePaths = Directory.GetFiles(fromPath);

            foreach(string imagePath in imagePaths)
            {
                if (!imagePath.EndsWith(".png")) continue;

                Image image = Image.FromFile(imagePath);
                if (image == null) continue;

                Bitmap rezizedBitmap = RezizeImage(image, width, height);
                rezizedBitmap.Save($@"{toPath}\{Path.GetFileName(imagePath)}", ImageFormat.Png);
            }
        }

        public static Bitmap RezizeImage(Image image, int toWidth, int toHeight)
        {
            Bitmap output = new Bitmap(toWidth, toHeight);

            using (Graphics g = Graphics.FromImage(output))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                g.DrawImage(image, new Rectangle(0, 0, toWidth, toHeight), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
            }

            return output;
        }
    }
}
