namespace YTDownloader
{
    internal static class FileHelper
    {
        public static string CreateValidFileName(string str)
        {
            ArgumentException.ThrowIfNullOrEmpty(str, nameof(str));
            char[] invalidChars = Path.GetInvalidFileNameChars();

            return new string(str.Where(c => !invalidChars.Contains(c))
                .ToArray());
        }
    }
}
