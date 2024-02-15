using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YTDownloader.CLasses.Models
{
    public class YTSimpleInfo
    {
        public Video YoutubeVideo { get; }
        public StreamManifest StreamManifest { get; }

        internal YTSimpleInfo(Video video, StreamManifest manifest)
        {
            YoutubeVideo = video;
            StreamManifest = manifest;
        }
    }
}
