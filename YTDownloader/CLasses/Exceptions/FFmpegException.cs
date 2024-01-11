namespace YTDownloader.CLasses.Exceptions;

internal class FFmpegException : Exception
{
    public int ErrorCode { get; }
    public FFmpegException(int errorCode, string? message)
        : base($"Error message: \"{message?.Trim() ?? "None"}\" exit code: {errorCode}") => ErrorCode = errorCode;

    internal static void ThrowIf(bool condition, int errorCode, string? message)
    {
        if (condition)
            throw new FFmpegException(errorCode, message?.Trim());
    }
}