using YoutubeExplode.Videos;

namespace YTDownloader.CLasses.Models
{
    public class DownloadedMediaInfo
    {
        public Video YoutubeVideoInfo { get; }
        public FileInfo FileInfo { get; }
        public YTType MediaType { get; }

        internal DownloadedMediaInfo(Video video, FileInfo fileInfo, YTType mediaType)
        {
            YoutubeVideoInfo = video;
            FileInfo = fileInfo;
            MediaType = mediaType;
        }
    }
}
