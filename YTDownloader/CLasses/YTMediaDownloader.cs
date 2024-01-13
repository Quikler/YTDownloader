using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;
using YoutubeExplode.Common;
using YoutubeExplode;
using YTDownloader.CLasses.Models;
using YTDownloader.CLasses.Helpers;
using YTDownloader.CLasses.Models.Enums;

namespace YTDownloader.CLasses
{
    public class YTMediaDownloader : IDisposable
    {
        private HttpClient? _httpClient;
        private static readonly YoutubeClient _client = new();
        private readonly Lazy<FileStreamOptions> _getStreamOptions = new(new FileStreamOptions
        {
            Options = FileOptions.DeleteOnClose,
            Mode = FileMode.Open,
            Access = FileAccess.ReadWrite,
        });

        public async Task<DownloadedMediaStream> GetStreamAsync(string url,
            Func<StreamManifest, IStreamInfo> mediaSelector)
        {
            DownloadedMediaInfo mediaInfo = await GetMediaInfoAsync(url, null, mediaSelector);
            Stream stream = new MemoryStream();
            using (FileStream fs = mediaInfo.FileInfo.Open(_getStreamOptions.Value))
            {
                await fs.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }

            return new DownloadedMediaStream(mediaInfo.YoutubeVideo, stream, mediaInfo.MediaType, mediaInfo.Thumbnail);
        }

        public Task<DownloadedMediaInfo> DownloadAsync(string url, string destinationFolderPath,
            Func<StreamManifest, IStreamInfo> mediaSelector) => GetMediaInfoAsync(url, destinationFolderPath, mediaSelector);

        private async Task<DownloadedMediaInfo> GetMediaInfoAsync(string url,
            string? destinationFolderPath, Func<StreamManifest, IStreamInfo> mediaSelector)
        {
            Video video = await _client.Videos.GetAsync(url);
            IStreamInfo mediaStreamInfo = mediaSelector(await _client.Videos.Streams.GetManifestAsync(video.Id));

            return mediaStreamInfo is AudioOnlyStreamInfo ?
                await FileCreator.CreateAudioAsync(_client, mediaStreamInfo, _httpClient ??= new(), video, destinationFolderPath) :
                await FileCreator.CreateVideoAsync(_client, mediaStreamInfo, video, destinationFolderPath);
        }

        internal static bool IsAudioOnly(YTMediaType type) => type == YTMediaType.AudioOnly;

        ~YTMediaDownloader() => Dispose();
        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}