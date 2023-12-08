using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;
using YoutubeExplode;

namespace YTDownloader
{
    public enum YTType { Video, Audio }
    public static class YTMediaDownloader
    {
        private static readonly YoutubeClient _client = new();

        public static async Task<bool> DownloadAsync(string url, YTType typeToDownload,
            string destinationFolderPath, bool mergeThumbnailWithMedia = false)
        {
            if (!IsUrlValid(url))
                return false;

            Video video = await _client.Videos.GetAsync(url);

            if (video is null)
                return false;

            IStreamInfo streamInfo = await GetMediaStreamAsync(typeToDownload, video.Id);

            string destinationFilePath = 
                $"{destinationFolderPath}\\{CreateValidFileName(video.Title)}.{streamInfo.Container.Name}";

            // Downloading youtube video to specified directory
            await _client.Videos.Streams.DownloadAsync(streamInfo, destinationFilePath);

            if (!mergeThumbnailWithMedia)
                return true;

            bool ok = await MediaHelper.MergeThumbnailWithMediaAsync(
                video.Thumbnails.GetWithHighestResolution(), typeToDownload, destinationFilePath);
            if (ok) return true;

            return false;
        }

        private static async Task<IStreamInfo> GetMediaStreamAsync(YTType type, VideoId videoId)
        {
            StreamManifest streamInfoSet = await _client.Videos.Streams.GetManifestAsync(videoId);

            // Finding stream with .mp4 extension
            IStreamInfo streamInfo = IsVideo(type) ?
                streamInfoSet.GetMuxedStreams().GetWithHighestVideoQuality() :
                streamInfoSet.GetAudioOnlyStreams().GetWithHighestBitrate();

            return streamInfo;
        }

        internal static bool IsVideo(YTType type) => type == YTType.Video;

        private static bool IsUrlValid(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out _))
                return true;
            return false;
        }

        private static string CreateValidFileName(string str)
        {
            ArgumentException.ThrowIfNullOrEmpty(str, nameof(str));
            char[] invalidChars = Path.GetInvalidFileNameChars();

            return new string(str.Where(c => !invalidChars.Contains(c))
                .ToArray());
        }
    }
}