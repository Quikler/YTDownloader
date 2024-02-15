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
        private static async Task<byte[]> GetThumbnailBytesAsync(string thumbnailUrl)
        {
            using HttpClient hc = new();
            using HttpResponseMessage response = await hc.GetAsync(thumbnailUrl);
            using HttpContent thumbnailContent = response.Content;

            return await thumbnailContent.ReadAsByteArrayAsync();
        }

        public static async Task<byte[]> CreateThumbnailFileAsync(
            string thumbnailUrl, string destinationFilePath)
        {
            byte[] thumbnailAsByteArray = await GetThumbnailBytesAsync(thumbnailUrl);

            File.WriteAllBytes(destinationFilePath, thumbnailAsByteArray);
            return thumbnailAsByteArray;
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

                string thumbnailUrl = video.Thumbnails.GetWithHighestResolution().Url;
                byte[] thumbnailBytes = await GetThumbnailBytesAsync(thumbnailUrl);

                return new DownloadedMediaInfo(video, new(destinationFilePath), yTType, new(thumbnailBytes, thumbnailUrl));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public static async Task<DownloadedMediaInfo> CreateAudioAsync(YoutubeClient ytClient, 
            IStreamInfo mediaStreamInfo, Video video, string? destinationFolder)
        {
            string tempFilePath = Path.GetTempFileName();
            string coverFilePath = Path.GetTempFileName();

            try
            {
                // download initial media to tmp
                await ytClient.Videos.Streams.DownloadAsync(mediaStreamInfo, tempFilePath);

                string thumbnailUrl = video.Thumbnails.GetWithHighestResolution().Url;

                // create valid front cover for audio
                byte[] thumbnailBytes = await CreateThumbnailFileAsync(
                    video.Thumbnails.GetWithHighestResolution().Url, coverFilePath);

                string destinationFilePath = PathHelper.CreateValidFilePath(
                    destinationFolder ?? Path.GetTempPath(), video.Title, "mp3");

                // convert to audio and merge metadata with cover
                await FFmpegExecutor.ConvertVideoToAudioAndMergeMetadataAsync(tempFilePath, destinationFilePath,
                    coverFilePath, video.Author.ChannelTitle, video.Title, video.UploadDate.Year);

                return new DownloadedMediaInfo(video, new FileInfo(destinationFilePath),
                    YTMediaType.AudioOnly, new(thumbnailBytes, thumbnailUrl));
            }
            finally
            {
                File.Delete(tempFilePath);
                File.Delete(coverFilePath);
            }
        }
    }
}
