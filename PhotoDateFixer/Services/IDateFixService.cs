namespace PhotoDateFixer.Services;

public interface IDateFixService
{
    Task<bool> FixDate(
        string filePath,
        string? contentUri,
        DateTime date);
}