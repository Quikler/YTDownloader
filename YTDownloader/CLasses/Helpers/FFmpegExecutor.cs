using System.Diagnostics;
using YTDownloader.CLasses.Exceptions;

namespace YTDownloader.CLasses.Helpers
{
    public static class FFmpegExecutor
    {
        private const string FFMPEG_EXE_FILE = "ffmpeg.exe";
        private const string IMAGE_CONVERT_FORMAT = "-y -i \"{0}\" \"{1}\"";
        private const string VIDEO_TO_AUDIO_CONVERT_FORMAT = "-hide_banner -y -i \"{0}\" -q:a 0 -map a \"{1}\"";

        private const string DEFAULT_METADATA_FORMAT =
            "-metadata artist=\"{0}\"" +
            " -metadata title=\"{1}\"" +
            " -metadata date=\"{2}\"" +
            " -metadata album=\"YTDownloader\"" +
            " -metadata album_artist=\"Quikler\"" +
            " -metadata comment=\"Merged by © YTDownloader | Quikler\"" +
            " -metadata copyright=\"Merged by © YTDownloader | Quikler\"";

        private const string VIDEO_METADATA_FORMAT = "-hide_banner -y -i \"{0}\" -c copy {1} \"{2}\"";

        private const string VIDEO_TO_AUDIO_METADATA_FORMAT =
            "-hide_banner -y -i \"{0}\" -i \"{1}\"" +
            " -map 0:a -c:a libmp3lame -id3v2_version 3 {2}" +
            " -map 1 -metadata:s:v comment=\"Cover (front)\" \"{3}\"";

        private static string _ffmpegExeFilePath = $"{AppContext.BaseDirectory}{FFMPEG_EXE_FILE}";
        public static string FFmpegExeFilePath
        {
            get => _ffmpegExeFilePath;
            set
            {
                _ffmpegExeFilePath = value;
                ffmpegProcessStartInfo.FileName = _ffmpegExeFilePath;
            }
        }

        private static readonly ProcessStartInfo ffmpegProcessStartInfo = new()
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = _ffmpegExeFilePath,
        };

        public static Task ConvertVideoToAudioAndMergeMetadataAsync(
            string inputFilePath, string outputFilePath, string coverFilePath, params object[] metadatas)
            => Execute(string.Format(VIDEO_TO_AUDIO_METADATA_FORMAT, inputFilePath, coverFilePath,
                string.Format(DEFAULT_METADATA_FORMAT, metadatas), outputFilePath));

        public static Task MergeVideoMetadataAsync(
            string inputFilePath, string outputFilePath, params object[] metadatas)
            => Execute(string.Format(VIDEO_METADATA_FORMAT, inputFilePath,
                string.Format(DEFAULT_METADATA_FORMAT, metadatas), outputFilePath));

        public static Task ConvertImageAsync(string inputFilePath, string outputFilePath)
            => Execute(string.Format(IMAGE_CONVERT_FORMAT, inputFilePath, outputFilePath));

        public static Task ConvertVideoToAudioAsync(string inputFilePath, string outputFilePath)
            => Execute(string.Format(VIDEO_TO_AUDIO_CONVERT_FORMAT, inputFilePath, outputFilePath));

        private static async Task Execute(string arguments)
        {
            ffmpegProcessStartInfo.Arguments = arguments;

            using Process process = new();
            process.StartInfo = ffmpegProcessStartInfo;

            string? lastErrorLine = null;
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data is not null)
                    lastErrorLine += e.Data;
            };

            process.Start();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();
            FFmpegException.ThrowIf(process.ExitCode != 0, process.ExitCode, lastErrorLine);
        }
    }
}
