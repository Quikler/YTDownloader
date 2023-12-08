using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode.Common;

namespace YTDownloader
{
    internal static class MediaHelper
    {
        public static async Task<bool> MergeThumbnailWithMediaAsync(
            Thumbnail thumbnail, YTType type, string mediaFilePath)
        {
            using HttpContent? thumbnailContent = await GetThumbnailContentAsync(thumbnail);
            if (thumbnailContent is null)
                return false;

            byte[]? thumbnailAsByteArray = await thumbnailContent.ReadAsByteArrayAsync();
            if (thumbnailAsByteArray is null)
                return false;

            // Thumbnail mime type. It might be webp, jpg, png etc
            string thumbnailMimeType = thumbnailContent.Headers.ContentType!.MediaType!;
            string thumbnailParsedInitialExtension = thumbnailMimeType[(thumbnailMimeType.IndexOf('/') + 1)..];
            
            if (thumbnailParsedInitialExtension != "jpeg" &&
                thumbnailParsedInitialExtension != "jpg" &&
                thumbnailParsedInitialExtension != "png")
            {
                // Creating in current directory input and output files
                string thumbnailInitialFilePath = CreateRelativeFilePath(
                    mediaFilePath, $".{thumbnailParsedInitialExtension}");
                string thumbnailJpegFilePath = CreateRelativeFilePath(mediaFilePath, ".jpeg");

                // Creating initial thumbnail file with initial extension
                File.WriteAllBytes(thumbnailInitialFilePath, thumbnailAsByteArray);

                ConvertImage(thumbnailInitialFilePath, thumbnailJpegFilePath, "jpeg", true);
                thumbnailAsByteArray = File.ReadAllBytes(thumbnailJpegFilePath);
            }

            if (YTMediaDownloader.IsVideo(type)) return true;
            //AddThumbnailToMedia(mediaFilePath, thumbnailAsByteArray);
            else
            {
                string mediaMp3FilePath = CreateRelativeFilePath(mediaFilePath, ".mp3");

                // YoutubeExplode library represents all media from youtube as mp4/webm files
                // so it's necessary to convert video to mp3 if YTType type specified as YTType.Audio
                ConvertMedia(mediaFilePath, mediaMp3FilePath, "mp3", true);
                AddThumbnailToMedia(mediaMp3FilePath, thumbnailAsByteArray);
            }

            return true;
        }

        private static string CreateRelativeFilePath(string filePath, string newExtensionWithPeriod, string? newFileName = null)
        {
            string directoryName = Path.GetDirectoryName(filePath)!;
            if (newFileName is not null)
                return directoryName + "\\" + newFileName + newExtensionWithPeriod;

            string fileName = Path.GetFileNameWithoutExtension(filePath);
            return directoryName + "\\" + fileName + newExtensionWithPeriod;
        }

        private static async Task<HttpContent?> GetThumbnailContentAsync(Thumbnail thumbnail)
        {
            using HttpClient hc = new();
            HttpResponseMessage response = await hc.GetAsync(thumbnail.Url);

            if (!response.IsSuccessStatusCode)
                return null;

            // Returning a content of thumbnail
            return response.Content;
        }

        private static void ConvertMedia(string inputFilePath,
            string outputFilePath, string outputFormat, bool deleteInputFile = false)
        {
            NReco.VideoConverter.FFMpegConverter converter = new();
            converter.ConvertMedia(inputFilePath, outputFilePath, outputFormat);

            if (deleteInputFile)
                File.Delete(inputFilePath);
        }

        private static void ConvertImage(string inputFilePath,
            string outputFilePath, string outputFormat, bool deleteInputFile = false)
        {
            using ImageMagick.MagickImage image = new(inputFilePath);
            image.Write(outputFilePath, Enum.Parse<ImageMagick.MagickFormat>(outputFormat, true));

            if (deleteInputFile)
                File.Delete(inputFilePath);
        }

        private static void AddThumbnailToMedia(string mediaFilePath, byte[] byteArrayThumbnail)
        {
            // mediaFile represents specified media file as TagLib.File object
            using TagLib.File mediaFile = TagLib.File.Create(mediaFilePath);

            // adding to media file front cover picture
            mediaFile.Tag.Pictures = new[]
            {
                new TagLib.Id3v2.AttachmentFrame(new TagLib.Picture(byteArrayThumbnail))
                {
                    TextEncoding = TagLib.StringType.UTF16,
                    Type = TagLib.PictureType.FrontCover,
                }
            };

            mediaFile.Save();
        }
    }
}
