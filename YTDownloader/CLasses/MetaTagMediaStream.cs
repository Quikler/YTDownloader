﻿using YoutubeExplode.Videos;
using YTDownloader.CLasses.Helpers.Internal_Helpers;
using YTDownloader.CLasses.Models;
using YTDownloader.CLasses.Models.Enums;

namespace YTDownloader.CLasses
{
    public class MetaTagMediaStream : IDisposable
    {
        private readonly string _mediafilePath;
        private readonly TagLib.File _mediaFile;

        private readonly DownloadedMediaInfo _downloadedMediaInfo;
        private readonly YTMediaType _mediaType;

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

        public async Task MergeMetadataAsync()
        {
            MergeMediaMatadata(_downloadedMediaInfo.YoutubeVideo);
            string? thumbnailUrl = _downloadedMediaInfo.Thumbnail?.Url;

            if (thumbnailUrl is not null)
                await MergeThumbnailAsync(thumbnailUrl);
        }

        private void MergeMediaMatadata(Video video)
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
            if (!YTMediaDownloader.IsAudioOnly(_mediaType))
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
                string tempDirectory = PathHelper.CreateUniqueDirectoryName(Environment.CurrentDirectory, true);

                string tempThumbnailFilePath = PathHelper.CreateValidFilePath(
                    tempDirectory, _downloadedMediaInfo.YoutubeVideo.Title, thumbnailParsedInitialExtension);

                string thumbnailJpegFilePath = Path.ChangeExtension(tempThumbnailFilePath, ".jpeg");

                try
                {
                    // Creating initial thumbnail file with initial extension
                    File.WriteAllBytes(tempThumbnailFilePath, thumbnailAsByteArray);

                    await ConvertMediaHelper.ConvertImageAsync(
                        tempThumbnailFilePath, thumbnailJpegFilePath, "jpeg", true);
                    thumbnailAsByteArray = File.ReadAllBytes(thumbnailJpegFilePath);
                }
                finally
                {
                    Directory.Delete(tempDirectory, true);
                }
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