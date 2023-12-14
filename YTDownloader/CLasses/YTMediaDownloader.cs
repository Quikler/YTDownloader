using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;
using YoutubeExplode;
using YTDownloader.CLasses.Models;
using YTDownloader.CLasses.Models.Enums;
using YTDownloader.CLasses.Helpers.Internal_Helpers;

namespace YTDownloader.CLasses
{
    public static class YTMediaDownloader
    {
        private static readonly YoutubeClient _client = new();

        public static async ValueTask<DownloadedMediaInfo> DownloadAsync(string url, 
            string destinationFolderPath, Func<StreamManifest, IStreamInfo> mediaSelector)
        {
            Video video = await _client.Videos.GetAsync(url);
            IStreamInfo mediaStreamInfo = mediaSelector(
                await _client.Videos.Streams.GetManifestAsync(video.Id));

            string tempFolderPath = PathHelper.CreateUniqueDirectoryName(Environment.CurrentDirectory, true);

            // Creating temp file path to work with
            string tempFilePath = PathHelper.CreateValidFilePath(
                tempFolderPath, video.Title, mediaStreamInfo.Container.Name);

            // Downloading youtube video to created file path
            await _client.Videos.Streams.DownloadAsync(mediaStreamInfo, tempFilePath);
            
            YTMediaType yTType = YTMediaType.Muxed;
            if (mediaStreamInfo is AudioOnlyStreamInfo)
            {
                yTType = YTMediaType.AudioOnly;

                // If audio only then convert to mp3
                string mediaMp3FilePath = Path.ChangeExtension(tempFilePath, "mp3");
                
                // YoutubeExplode library represents all media from youtube as mp4/webm files
                // so it's necessary to convert video to mp3 if YTType type specified as YTType.AudioOnly
                ConvertMediaHelper.ConvertMedia(tempFilePath, mediaMp3FilePath, "mp3", true);
                tempFilePath = mediaMp3FilePath;
            }
            else if (mediaStreamInfo is VideoOnlyStreamInfo)
                yTType = YTMediaType.VideoOnly;

            string destinationFilePath = PathHelper.ChangeDirectory(tempFilePath, destinationFolderPath);
            File.Move(tempFilePath, destinationFilePath, true);
            Directory.Delete(tempFolderPath, true);

            return new DownloadedMediaInfo(video, new FileInfo(destinationFilePath), yTType);
        }

        internal static bool IsAudioOnly(YTMediaType type) => type == YTMediaType.AudioOnly;
    }
}