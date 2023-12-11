namespace YTDownloader.Internal_Helpers
{
    internal static class ConvertMediaHelper
    {
        public static void ConvertMedia(string inputFilePath,
            string outputFilePath, string outputFormat, bool deleteInputFile = false)
        {
            NReco.VideoConverter.FFMpegConverter converter = new();
            converter.ConvertMedia(inputFilePath, outputFilePath, outputFormat);

            if (deleteInputFile)
                File.Delete(inputFilePath);
        }

        public static void ConvertImage(string inputFilePath,
            string outputFilePath, string outputFormat, bool deleteInputFile = false)
        {
            using ImageMagick.MagickImage image = new(inputFilePath);
            image.Write(outputFilePath, Enum.Parse<ImageMagick.MagickFormat>(outputFormat, true));

            if (deleteInputFile)
                File.Delete(inputFilePath);
        }
    }
}
