namespace YTDownloader.CLasses.Helpers.Internal_Helpers
{
    internal static class ConvertMediaHelper
    {
        private static NReco.VideoConverter.FFMpegConverter? _converter;

        public static async Task ConvertMediaAsync(string inputFilePath,
            string outputFilePath, string outputFormat, bool deleteInputFile = false)
        {
            _converter ??= new();
            await Task.Run(() => _converter.ConvertMedia(inputFilePath, outputFilePath, outputFormat));

            if (deleteInputFile)
                File.Delete(inputFilePath);
        }

        public static async Task ConvertImageAsync(string inputFilePath,
            string outputFilePath, string outputFormat, bool deleteInputFile = false)
        {
            using ImageMagick.MagickImage image = new(inputFilePath);
            await image.WriteAsync(outputFilePath, Enum.Parse<ImageMagick.MagickFormat>(outputFormat, true));

            if (deleteInputFile)
                File.Delete(inputFilePath);
        }
    }
}
