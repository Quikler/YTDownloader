using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YTDownloader.CLasses.Models.Enums;

namespace YTDownloader.CLasses.Models
{
    public class DownloadedMediaInfo
    {
        public Video YoutubeVideo { get; }
        public YTMediaType MediaType { get; }
        public FileInfo FileInfo { get; }
        public YTThumbnail? Thumbnail { get; }

        internal DownloadedMediaInfo(Video video, FileInfo fileInfo, YTMediaType mediaType, YTThumbnail? thumbnail)
        {
            YoutubeVideo = video;
            MediaType = mediaType;
            FileInfo = fileInfo;
            Thumbnail = thumbnail;
        }
    }
}
