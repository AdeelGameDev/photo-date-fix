using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using PhotoDateFixer.Models;

namespace PhotoDateFixer.Services;

public class ExifDateDetectionService
{
    private bool IsValidDate(DateTime date)
    {
        int currentYear = DateTime.Now.Year;

        // Reject impossible years
        if (date.Year < 1990)
            return false;

        if (date.Year > currentYear)
            return false;

        // Reject invalid month/day
        if (date.Month < 1 || date.Month > 12)
            return false;

        if (date.Day < 1 || date.Day > DateTime.DaysInMonth(date.Year, date.Month))
            return false;

        return true;
    }
    public DateDetectionResult DetectDate(string filePath)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine(
                $"Reading EXIF: {filePath}"
            );

            var directories = ImageMetadataReader.ReadMetadata(filePath);

            System.Diagnostics.Debug.WriteLine(
                $"Directories found: {directories.Count()}"
            );

            var exifDirectory = directories
                .OfType<ExifSubIfdDirectory>()
                .FirstOrDefault();

            if (exifDirectory == null)
            {
                System.Diagnostics.Debug.WriteLine(
                    "No ExifSubIfdDirectory"
                );
            }
            else
            {
                if (exifDirectory.TryGetDateTime(
     ExifDirectoryBase.TagDateTimeOriginal,
     out DateTime result))
                {
                    if (IsValidDate(result))
                    {
                        return new DateDetectionResult
                        {
                            Date = result,
                            Confidence = 100,
                            Source = "EXIF DateTimeOriginal"
                        };
                    }

                    System.Diagnostics.Debug.WriteLine(
                        $"Invalid EXIF date ignored: {result}"
                    );
                }

                System.Diagnostics.Debug.WriteLine(
                    "DateTimeOriginal tag not found"
                );
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"EXIF ERROR: {ex}"
            );
        }

        return new DateDetectionResult
        {
            Date = null,
            Confidence = 0,
            Source = "No EXIF"
        };
    }
}