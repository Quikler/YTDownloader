using YoutubeExplode.Videos;
using YTDownloader.CLasses.Models.Enums;

namespace YTDownloader.CLasses.Models
{
    public class DownloadedMediaStream : IDisposable
    {
        public Video YoutubeVideo { get; }
        public YTMediaType MediaType { get; }
        public Stream Stream { get; }
        public YTThumbnail Thumbnail { get; }

        internal DownloadedMediaStream(Video video, Stream stream, YTMediaType mediaType, YTThumbnail thumbnail)
        {
            YoutubeVideo = video;
            MediaType = mediaType;
            Stream = stream;
            Thumbnail = thumbnail;
        }

        ~DownloadedMediaStream() => Dispose();
        public void Dispose()
        {
            Stream?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
