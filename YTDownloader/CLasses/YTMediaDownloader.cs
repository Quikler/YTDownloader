using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;
using YoutubeExplode;
using YTDownloader.CLasses.Models;

namespace YTDownloader.CLasses
{
    public enum YTType { Video, Audio }
    public static class YTMediaDownloader
    {
        private static readonly YoutubeClient _client = new();

        public static async Task<DownloadedMediaInfo> DownloadAsync(
            string url, YTType typeToDownload, string destinationFolderPath)
        {
            Video video = await _client.Videos.GetAsync(url);
            IStreamInfo streamInfo = await GetMediaStreamAsync(typeToDownload, video.Id);

            string destinationFilePath =
                $"{destinationFolderPath}\\{FileHelper.CreateValidFileName(video.Title)}.{streamInfo.Container.Name}";

            // Downloading youtube video to specified directory
            await _client.Videos.Streams.DownloadAsync(streamInfo, destinationFilePath);

            // If not video then convert to mp3
            if (!IsVideo(typeToDownload))
            {
                string mediaMp3FilePath = Path.ChangeExtension(destinationFilePath, "mp3");

                // YoutubeExplode library represents all media from youtube as mp4/webm files
                // so it's necessary to convert video to mp3 if YTType type specified as YTType.Audio
                ConvertMediaHelper.ConvertMedia(destinationFilePath, mediaMp3FilePath, "mp3", true);
                destinationFilePath = mediaMp3FilePath;
            }

            return new DownloadedMediaInfo(video, new FileInfo(destinationFilePath), typeToDownload);
        }

        private static async Task<IStreamInfo> GetMediaStreamAsync(YTType type, VideoId videoId)
        {
            StreamManifest streamInfoSet = await _client.Videos.Streams.GetManifestAsync(videoId);

            IStreamInfo streamInfo = IsVideo(type) ?
                streamInfoSet.GetMuxedStreams().GetWithHighestVideoQuality() :
                streamInfoSet.GetAudioOnlyStreams().GetWithHighestBitrate();

            return streamInfo;
        }

        internal static bool IsVideo(YTType type) => type == YTType.Video;
    }
}