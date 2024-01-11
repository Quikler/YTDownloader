using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;
using YTDownloader.CLasses.Models.Enums;
using YTDownloader.CLasses.Models;
using YoutubeExplode;
using YoutubeExplode.Common;

namespace YTDownloader.CLasses.Helpers
{
    internal static class FileCreator
    {
        /*public static async Task CreateThumbnailFileAsync(
            string thumbnailUrl, string destinationFilePath, HttpClient httpClient)
        {
            using HttpResponseMessage response = await httpClient.GetAsync(thumbnailUrl);
            using HttpContent thumbnailContent = response.Content;

            byte[] thumbnailAsByteArray = await thumbnailContent.ReadAsByteArrayAsync();

            // Thumbnail mime type. It might be webp, jpg, png etc
            string thumbnailMimeType = thumbnailContent.Headers.ContentType!.MediaType!;
            string thumbnailParsedInitialExtension = thumbnailMimeType[(thumbnailMimeType.IndexOf('/') + 1)..];

            // Converting img from unsupported front cover format (like webp) to supported
            if (thumbnailParsedInitialExtension != "jpeg" &&
                thumbnailParsedInitialExtension != "jpg" &&
                thumbnailParsedInitialExtension != "png")
            {
                string tempThumbnailFilePath = Path.GetTempFileName();
                string thumbnailJpegFilePath = Path.ChangeExtension(tempThumbnailFilePath, ".jpeg");

                try
                {
                    // Creating initial thumbnail file with initial extension
                    File.WriteAllBytes(tempThumbnailFilePath, thumbnailAsByteArray);

                    await FFmpegExecutor.ConvertImageAsync(tempThumbnailFilePath, thumbnailJpegFilePath);
                    thumbnailAsByteArray = File.ReadAllBytes(thumbnailJpegFilePath);
                }
                finally
                {
                    File.Delete(tempThumbnailFilePath);
                    File.Delete(thumbnailJpegFilePath);
                }
            }
            File.WriteAllBytes(destinationFilePath, thumbnailAsByteArray);
        }*/

        public static async Task CreateThumbnailFileAsync(
            string thumbnailUrl, string destinationFilePath, HttpClient httpClient)
        {
            using HttpResponseMessage response = await httpClient.GetAsync(thumbnailUrl);
            using HttpContent thumbnailContent = response.Content;

            byte[] thumbnailAsByteArray = await thumbnailContent.ReadAsByteArrayAsync();

            File.WriteAllBytes(destinationFilePath, thumbnailAsByteArray);
        }

        public static async Task<DownloadedMediaInfo> CreateVideoAsync(YoutubeClient ytClient,
            IStreamInfo mediaStreamInfo, Video video, string? destinationFolder)
        {
            YTMediaType yTType = YTMediaType.Muxed;

            if (mediaStreamInfo is VideoOnlyStreamInfo)
                yTType = YTMediaType.VideoOnly;

            string tempFile = Path.GetTempFileName();
            try
            {
                await ytClient.Videos.Streams.DownloadAsync(mediaStreamInfo, tempFile);

                string destinationFilePath = PathHelper.CreateValidFilePath(
                    destinationFolder ?? Path.GetTempPath(), video.Title, "mp4");

                await FFmpegExecutor.MergeVideoMetadataAsync(tempFile, destinationFilePath,
                    video.Author.ChannelTitle, video.Title, video.UploadDate.Year);

                return new DownloadedMediaInfo(video, new(destinationFilePath), yTType);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public static async Task<DownloadedMediaInfo> CreateAudioAsync(
            YoutubeClient ytClient, IStreamInfo mediaStreamInfo, 
            HttpClient httpClient, Video video, string? destinationFolder)
        {
            string tempFilePath = Path.GetTempFileName();
            string coverFilePath = Path.GetTempFileName();

            try
            {
                // download initial media to tmp
                await ytClient.Videos.Streams.DownloadAsync(mediaStreamInfo, tempFilePath);

                // create valid front cover for audio
                await CreateThumbnailFileAsync(video.Thumbnails.GetWithHighestResolution().Url, coverFilePath, httpClient);

                string destinationFilePath = PathHelper.CreateValidFilePath(
                    destinationFolder ?? Path.GetTempPath(), video.Title, "mp3");

                // convert to audio and merge metadata with cover
                await FFmpegExecutor.ConvertVideoToAudioAndMergeMetadataAsync(tempFilePath, destinationFilePath,
                    coverFilePath, video.Author.ChannelTitle, video.Title, video.UploadDate.Year);

                return new DownloadedMediaInfo(video, new FileInfo(destinationFilePath), YTMediaType.AudioOnly);
            }
            finally
            {
                File.Delete(tempFilePath);
                File.Delete(coverFilePath);
            }
        }
    }
}
