using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using PhotoDateFixer.Models;

namespace PhotoDateFixer.Services;

public class ExifDateDetectionService
{
    public DateDetectionResult DetectDate(string filePath)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(filePath);

            var exifDirectory = directories
                .OfType<ExifSubIfdDirectory>()
                .FirstOrDefault();

            if (exifDirectory != null)
            {
                if (exifDirectory.TryGetDateTime(
    ExifDirectoryBase.TagDateTimeOriginal,
    out DateTime result))
                {
                    return new DateDetectionResult
                    {
                        Date = result,
                        Confidence = 100,
                        Source = "EXIF DateTimeOriginal"
                    };
                }
                {
                    return new DateDetectionResult
                    {
                        Date = result,
                        Confidence = 100,
                        Source = "EXIF DateTimeOriginal"
                    };
                }
            }
        }
        catch
        {
        }

        return new DateDetectionResult
        {
            Date = null,
            Confidence = 0,
            Source = "No EXIF"
        };
    }
}