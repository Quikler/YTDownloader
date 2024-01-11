namespace YTDownloader.CLasses.Helpers
{
    internal static class PathHelper
    {
        public static string CreateValidFileName(string str)
        {
            ArgumentException.ThrowIfNullOrEmpty(str, nameof(str));
            char[] invalidChars = Path.GetInvalidFileNameChars();

            return new string(str.Where(c => !invalidChars.Contains(c))
                .ToArray());
        }

        public static string CreateValidFilePath(string folderPath, string fileName, string extension)
            => Path.Combine(folderPath, CreateValidFileName(fileName)) + GetExtensionWithPeriod(extension);

        public static string CreateValidFilePath(string folderPath, string fileNameWithExtension)
            => Path.Combine(folderPath, CreateValidFileName(fileNameWithExtension));

        public static string ChangeDirectory(string path, string newDirectory)
            => Path.Combine(newDirectory, Path.GetFileName(path));

        private static string GetExtensionWithPeriod(string extension)
            => extension.Contains('.') ? extension : $".{extension}";
    }
}
