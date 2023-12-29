using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YTDownloader.CLasses.Models.Enums;

namespace YTDownloader.CLasses.Models
{
    public class DownloadedMediaInfo
    {
        public Video YoutubeVideo { get; }
        public YTMediaType MediaType { get; }
        public Thumbnail? Thumbnail { get; set; }
        public FileInfo FileInfo { get; }

        internal DownloadedMediaInfo(Video video, FileInfo fileInfo, YTMediaType mediaType)
        {
            YoutubeVideo = video;
            MediaType = mediaType;
            FileInfo = fileInfo;
        }
    }
}
