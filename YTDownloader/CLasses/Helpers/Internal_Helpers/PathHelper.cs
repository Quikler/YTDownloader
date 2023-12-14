namespace YTDownloader.CLasses.Helpers.Internal_Helpers
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
        
        public static string CreateUniqueDirectoryName(string parentPath, bool createDirectory)
        {
            string folderPath;
            do
            {
                folderPath = $"{parentPath}\\{Guid.NewGuid()}";
            } while (Directory.Exists(folderPath));

            if (createDirectory)
                Directory.CreateDirectory(folderPath);

            return folderPath;
        }

        private static string GetExtensionWithPeriod(string extension)
            => extension.Contains('.') ? extension : $".{extension}";
    }
}
