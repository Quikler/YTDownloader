using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;
using YoutubeExplode;
using YTDownloader.CLasses.Models;
using YTDownloader.CLasses.Models.Enums;
using YTDownloader.Internal_Helpers;

namespace YTDownloader.CLasses
{
    public static class YTMediaDownloader
    {
        private static readonly YoutubeClient _client = new();

        public static async Task<DownloadedMediaInfo> DownloadMuxedAsync(string url, string destinationFolderPath, 
            Func<IEnumerable<MuxedStreamInfo>, IStreamInfo> muxedSelector)
        {
            Video video = await _client.Videos.GetAsync(url);
            StreamManifest manifest = await _client.Videos.Streams.GetManifestAsync(video.Id);

            IStreamInfo streamInfo = muxedSelector(manifest.GetMuxedStreams());
            return await DownloadAsync(video, YTType.Muxed, destinationFolderPath, streamInfo);
        }

        public static async Task<DownloadedMediaInfo> DownloadVideoOnlyAsync(string url, string destinationFolderPath,
            Func<IEnumerable<VideoOnlyStreamInfo>, IStreamInfo> videoOnlySelector)
        {
            Video video = await _client.Videos.GetAsync(url);
            StreamManifest manifest = await _client.Videos.Streams.GetManifestAsync(video.Id);

            IStreamInfo streamInfo = videoOnlySelector(manifest.GetVideoOnlyStreams());
            return await DownloadAsync(video, YTType.VideoOnly, destinationFolderPath, streamInfo);
        }

        public static async Task<DownloadedMediaInfo> DownloadAudioOnlyAsync(string url, string destinationFolderPath,
            Func<IEnumerable<AudioOnlyStreamInfo>, IStreamInfo> audioOnlySelector)
        {
            Video video = await _client.Videos.GetAsync(url);
            StreamManifest manifest = await _client.Videos.Streams.GetManifestAsync(video.Id);

            IStreamInfo streamInfo = audioOnlySelector(manifest.GetAudioOnlyStreams());
            return await DownloadAsync(video, YTType.AudioOnly, destinationFolderPath, streamInfo);
        }

        private static async Task<DownloadedMediaInfo> DownloadAsync(Video video, YTType typeToDownload,
            string destinationFolderPath, IStreamInfo mediaStreamInfo)
        {
            string destinationFilePath =
                $"{destinationFolderPath}\\{FileHelper.CreateValidFileName(video.Title)}.{mediaStreamInfo.Container.Name}";

            // Downloading youtube video to created file path
            await _client.Videos.Streams.DownloadAsync(mediaStreamInfo, destinationFilePath);

            // If audio only then convert to mp3
            if (IsAudioOnly(typeToDownload))
            {
                string mediaMp3FilePath = Path.ChangeExtension(destinationFilePath, "mp3");

                // YoutubeExplode library represents all media from youtube as mp4/webm files
                // so it's necessary to convert video to mp3 if YTType type specified as YTType.AudioOnly
                ConvertMediaHelper.ConvertMedia(destinationFilePath, mediaMp3FilePath, "mp3", true);
                destinationFilePath = mediaMp3FilePath;
            }

            return new DownloadedMediaInfo(video, new FileInfo(destinationFilePath), typeToDownload);
        }

        internal static bool IsAudioOnly(YTType type) => type == YTType.AudioOnly;
    }
}