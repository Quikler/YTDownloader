namespace YTDownloader.CLasses.Models
{
    public class YTThumbnail
    {
        public byte[] Bytes { get; }
        public string Uri { get; }
        internal YTThumbnail(byte[] bytes, string uri)
        {
            Bytes = bytes;
            Uri = uri;
        }
    }
}
