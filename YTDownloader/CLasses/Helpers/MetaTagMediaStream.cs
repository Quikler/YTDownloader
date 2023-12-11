using YoutubeExplode.Videos;
using YTDownloader.CLasses.Models;
using YTDownloader.CLasses.Models.Enums;
using YTDownloader.Internal_Helpers;

namespace YTDownloader.CLasses.Helpers
{
    public class MetaTagMediaStream : IDisposable
    {
        private readonly string _mediafilePath;
        private readonly TagLib.File _mediaFile;

        private readonly DownloadedMediaInfo _downloadedMediaInfo;
        private readonly YTType _mediaType;

        private readonly HttpClient _httpClient = new();

        private const string
            LibraryName = "YTDownloader",
            LibraryCreator = "Quikler",
            Copyright = $"Merged by © {LibraryName} | {LibraryCreator}";

        public MetaTagMediaStream(DownloadedMediaInfo downloadedMediaInfo)
        {
            _downloadedMediaInfo = downloadedMediaInfo;
            _mediaType = _downloadedMediaInfo.MediaType;
            _mediafilePath = _downloadedMediaInfo.FileInfo.FullName;
            _mediaFile = TagLib.File.Create(_mediafilePath);
        }

        public void MergeMetadata()
        {
            MergeVideoMatadata(_downloadedMediaInfo.YoutubeVideo);
            string? thumbnailUrl = _downloadedMediaInfo.Thumbnail?.Url;

            if (thumbnailUrl is not null)
                MergeThumbnailAsync(thumbnailUrl).Wait();
        }

        private void MergeVideoMatadata(Video video)
        {
            TagLib.Tag tag = _mediaFile.Tag;

            tag.Year = (uint)video.UploadDate.Year;
            tag.Copyright = Copyright;
            tag.Comment = Copyright;
            tag.Title = video.Title;
            tag.Performers = new[] { video.Author.ChannelTitle };
            tag.AlbumArtists = new[] { LibraryCreator };
            tag.DateTagged = DateTime.Now;
            tag.Album = LibraryName;

            _mediaFile.Save();
        }

        private async Task MergeThumbnailAsync(string thumbnailUrl)
        {
            // If media type is video then return cuz video files already have thumbnail in this case
            if (_mediaType != YTType.AudioOnly)
                return;

            using HttpResponseMessage response = await _httpClient.GetAsync(thumbnailUrl);
            using HttpContent thumbnailContent = response.Content;

            byte[] thumbnailAsByteArray = await thumbnailContent.ReadAsByteArrayAsync();

            // Thumbnail mime type. It might be webp, jpg, png etc
            string thumbnailMimeType = thumbnailContent.Headers.ContentType!.MediaType!;
            string thumbnailParsedInitialExtension = thumbnailMimeType[(thumbnailMimeType.IndexOf('/') + 1)..];

            // Converting img from unsupported front cover format (like webp) to supported
            if (thumbnailParsedInitialExtension != "jpeg" &&
                thumbnailParsedInitialExtension != "jpg" &&
                thumbnailParsedInitialExtension != "png")
            {
                // Creating in current directory input and output files
                string thumbnailInitialFilePath = Path.ChangeExtension(
                    _mediafilePath, $".{thumbnailParsedInitialExtension}");
                string thumbnailJpegFilePath = Path.ChangeExtension(_mediafilePath, ".jpeg");

                // Creating initial thumbnail file with initial extension
                File.WriteAllBytes(thumbnailInitialFilePath, thumbnailAsByteArray);

                ConvertMediaHelper.ConvertImage(thumbnailInitialFilePath, thumbnailJpegFilePath, "jpeg", true);
                thumbnailAsByteArray = File.ReadAllBytes(thumbnailJpegFilePath);
            }

            AddThumbnailToMedia(thumbnailAsByteArray);
        }

        private void AddThumbnailToMedia(byte[] byteArrayThumbnail)
        {
            // Adding to media file front cover picture
            _mediaFile.Tag.Pictures = new[]
            {
                new TagLib.Id3v2.AttachmentFrame(new TagLib.Picture(byteArrayThumbnail))
                {
                    TextEncoding = TagLib.StringType.UTF16,
                    Type = TagLib.PictureType.FrontCover,
                }
            };

            // Saving changes
            _mediaFile.Save();
        }

        public void Dispose()
        {
            _mediaFile?.Dispose();
            _httpClient?.Dispose();
        }
    }
}
