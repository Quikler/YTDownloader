using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YTDownloader.CLasses.Models.Enums;

namespace YTDownloader.CLasses.Models
{
    public class DownloadedMediaInfo
    {
        public Video YoutubeVideo { get; }
        public FileInfo FileInfo { get; }
        public YTType MediaType { get; }
        public Thumbnail? Thumbnail { get; set; }

        internal DownloadedMediaInfo(Video video, FileInfo fileInfo, YTType mediaType)
        {
            YoutubeVideo = video;
            FileInfo = fileInfo;
            MediaType = mediaType;
        }
    }
}
