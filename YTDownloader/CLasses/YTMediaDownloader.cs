using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;
using YoutubeExplode.Common;
using YoutubeExplode;
using YTDownloader.CLasses.Models;
using YTDownloader.CLasses.Helpers;
using YTDownloader.CLasses.Models.Enums;
using System.Net;
using AngleSharp.Dom;

namespace YTDownloader.CLasses
{
    public static class YTMediaDownloader
    {
        private static readonly YoutubeClient _client = new();
        private static readonly Lazy<FileStreamOptions> _getStreamOptions = new(new FileStreamOptions
        {
            Options = FileOptions.DeleteOnClose,
            Mode = FileMode.Open,
            Access = FileAccess.ReadWrite,
        });

        public static async Task<YTSimpleInfo> GetSimpleInfoAsync(string url)
        {
            Video video = await _client.Videos.GetAsync(url);
            StreamManifest manifest = await _client.Videos.Streams.GetManifestAsync(video.Id);

            return new(video, manifest);
        }

        public static async Task<YTSimpleInfo> GetSimpleInfoAsync(Video video)
        {
            StreamManifest manifest = await _client.Videos.Streams.GetManifestAsync(video.Id);
            return new(video, manifest);
        }

        public static async Task<DownloadedMediaStream> GetStreamAsync(YTSimpleInfo yTSimpleInfo,
            Func<StreamManifest, IStreamInfo> mediaSelector)
        {
            DownloadedMediaInfo mediaInfo = await GetMediaInfoAsync(yTSimpleInfo.YoutubeVideo, null, mediaSelector);
            Stream stream = new MemoryStream();
            using (FileStream fs = mediaInfo.FileInfo.Open(_getStreamOptions.Value))
            {
                await fs.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }

            return new(mediaInfo.YoutubeVideo, stream, mediaInfo.MediaType, mediaInfo.Thumbnail);
        }

        public static async Task<DownloadedMediaStream> GetStreamAsync(string url,
            Func<StreamManifest, IStreamInfo> mediaSelector)
        {
            DownloadedMediaInfo mediaInfo = await GetMediaInfoAsync(url, null, mediaSelector);
            Stream stream = new MemoryStream();
            using (FileStream fs = mediaInfo.FileInfo.Open(_getStreamOptions.Value))
            {
                await fs.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }

            return new(mediaInfo.YoutubeVideo, stream, mediaInfo.MediaType, mediaInfo.Thumbnail);
        }

        public static async Task<DownloadedMediaStream> GetStreamAsync(Video video,
            Func<StreamManifest, IStreamInfo> mediaSelector)
        {
            DownloadedMediaInfo mediaInfo = await GetMediaInfoAsync(video, null, mediaSelector);
            Stream stream = new MemoryStream();
            using (FileStream fs = mediaInfo.FileInfo.Open(_getStreamOptions.Value))
            {
                await fs.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }

            return new(mediaInfo.YoutubeVideo, stream, mediaInfo.MediaType, mediaInfo.Thumbnail);
        }

        public static Task<DownloadedMediaInfo> DownloadAsync(string url, string destinationFolderPath,
            Func<StreamManifest, IStreamInfo> mediaSelector) => GetMediaInfoAsync(url, destinationFolderPath, mediaSelector);

        public static Task<DownloadedMediaInfo> DownloadAsync(Video video, string destinationFolderPath,
            Func<StreamManifest, IStreamInfo> mediaSelector) => GetMediaInfoAsync(video, destinationFolderPath, mediaSelector);

        public static Task<DownloadedMediaInfo> DownloadAsync(YTSimpleInfo yTSimpleInfo, string destinationFolderPath,
            Func<StreamManifest, IStreamInfo> mediaSelector) => GetMediaInfoAsync(yTSimpleInfo.YoutubeVideo, destinationFolderPath, mediaSelector);

        private static async Task<DownloadedMediaInfo> GetMediaInfoAsync(string url,
            string? destinationFolderPath, Func<StreamManifest, IStreamInfo> mediaSelector)
        {
            Video video = await _client.Videos.GetAsync(url);
            IStreamInfo mediaStreamInfo = mediaSelector(await _client.Videos.Streams.GetManifestAsync(video.Id));

            return mediaStreamInfo is AudioOnlyStreamInfo ?
                await FileCreator.CreateAudioAsync(_client, mediaStreamInfo, video, destinationFolderPath) :
                await FileCreator.CreateVideoAsync(_client, mediaStreamInfo, video, destinationFolderPath);
        }

        private static async Task<DownloadedMediaInfo> GetMediaInfoAsync(Video video,
            string? destinationFolderPath, Func<StreamManifest, IStreamInfo> mediaSelector)
        {
            IStreamInfo mediaStreamInfo = mediaSelector(await _client.Videos.Streams.GetManifestAsync(video.Id));

            return mediaStreamInfo is AudioOnlyStreamInfo ?
                await FileCreator.CreateAudioAsync(_client, mediaStreamInfo, video, destinationFolderPath) :
                await FileCreator.CreateVideoAsync(_client, mediaStreamInfo, video, destinationFolderPath);
        }

        internal static bool IsAudioOnly(YTMediaType type) => type == YTMediaType.AudioOnly;
    }
}