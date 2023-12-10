using YoutubeExplode.Videos;

namespace YTDownloader.CLasses.Helpers
{
    public class MetaTagMediaHelper : IDisposable
    {
        private readonly string _mediafilePath;
        private readonly TagLib.File _mediaFile;
        private readonly HttpClient _httpClient = new();

        private const string LibraryName = "YTDownloader", LibraryCreator = "Quikler";

        public MetaTagMediaHelper(string mediaFilePath)
        {
            _mediafilePath = mediaFilePath;
            _mediaFile = TagLib.File.Create(_mediafilePath);
        }

        public void MergeVideoMatadata(Video video)
        {
            TagLib.Tag tag = _mediaFile.Tag;

            tag.Publisher = video.Author.ChannelTitle;
            tag.Comment = $"Merged by © {LibraryName} | {LibraryCreator}";
            tag.Title = video.Title;
            tag.DateTagged = DateTime.Now;
            tag.Album = LibraryName;

            _mediaFile.Save();
        }

        public async Task MergeThumbnailAsync(string thumbnailUrl)
        {
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

                // Creating initial thumbnail file with initial extension
                File.WriteAllBytes(thumbnailInitialFilePath, thumbnailAsByteArray);

                string thumbnailJpegFilePath = Path.ChangeExtension(_mediafilePath, ".jpeg");

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
