namespace PhotoDateFixer.Models;

public class DateDetectionResult
{
    public DateTime? Date { get; set; }

    public int Confidence { get; set; }

    public string Source { get; set; } = string.Empty;
}