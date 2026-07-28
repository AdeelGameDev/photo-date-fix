namespace PhotoDateFixer.Models;

public class DateDetectionResult
{
    public DateTime? Date { get; init; }

    public int Confidence { get; init; }

    public string Source { get; init; } = "";
}