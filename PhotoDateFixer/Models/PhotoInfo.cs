namespace PhotoDateFixer.Models;

public class PhotoInfo
{
    public required string FileName { get; set; }

    public required string FullPath { get; set; }

    public DateTime? DetectedDate { get; set; }

    public int Confidence { get; set; }
    public string? ContentUri { get; set; }
    public string Source { get; set; } = "";

    public string DisplayDate
    {
        get
        {
            if (DetectedDate.HasValue)
            {
                return $"{DetectedDate.Value:dd MMM yyyy HH:mm}\nConfidence: {Confidence}%\nSource: {Source}";
            }

            return "No date detected";
        }
    }
}