using PhotoDateFixer.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PhotoDateFixer.Services;

public class DateDetectionService
{
    private readonly ExifDateDetectionService exifDetector = new();

    private readonly string[] formats =
    {
        // Full date time
        "yyyy-MM-dd-HH-mm-ss",
        "yyyy_MM_dd_HH_mm_ss",
        "yyyy-MM-dd_HH-mm-ss",

        // Camera formats
        "yyyyMMddHHmmssfff",
        "yyyyMMddHHmmss",

        // Oppo / OnePlus Android
        "yyMMddHHmmssfff",

        // Samsung
        "yyyyMMdd-HHmmss",

        // Scanner
        "dd-MM-yyyy HH.mm",
        "MM-dd-yyyy HH.mm",

        // Date only
        "yyyy-MM-dd",
        "yyyy_MM_dd",
        "yyyyMMdd"
    };


    public DateDetectionResult DetectDate(
        string filePath,
        string fileName)
    {
        // 1. EXIF FIRST
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            var exifResult = exifDetector.DetectDate(filePath);

            if (exifResult.Date.HasValue)
            {
                return exifResult;
            }
        }


        // 2. Filename detection

        string name =
            Path.GetFileNameWithoutExtension(fileName);


        foreach (var part in ExtractDateCandidates(name))
        {
            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(
                    part,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime date))
                {
                    if (!IsValidDate(date))
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"Invalid filename date ignored: {part}"
                        );

                        continue;
                    }

                    return new DateDetectionResult
                    {
                        Date = date,
                        Confidence = GetConfidence(format),
                        Source = format
                    };
                }
            }
        }


        // 3. Unix timestamp detection
        var unixDate = FindUnixTimestamp(name);

        if (unixDate.HasValue && IsValidDate(unixDate.Value))
        {
            return new DateDetectionResult
            {
                Date = unixDate,
                Confidence = 70,
                Source = "Unix timestamp"
            };
        }
        // 3. Generic regex fallback

        var regexDate = FindLooseDate(name);

        if (regexDate.HasValue)
        {
            return new DateDetectionResult
            {
                Date = regexDate,
                Confidence = 60,
                Source = "Regex date"
            };
        }



        return new DateDetectionResult
        {
            Date = null,
            Confidence = 0,
            Source = "No match"
        };
    }

    private DateTime? FindUnixTimestamp(string text)
    {
        var match = Regex.Match(text, @"\b\d{13}\b");

        if (!match.Success)
            return null;

        if (long.TryParse(match.Value, out long milliseconds))
        {
            try
            {
                return DateTimeOffset
                    .FromUnixTimeMilliseconds(milliseconds)
                    .LocalDateTime;
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private IEnumerable<string> ExtractDateCandidates(string text)
    {
        HashSet<string> results = new();

        results.Add(text);


        string[] patterns =
        {
            // 2025-01-19-13-54-00
            @"\d{4}-\d{2}-\d{2}-\d{2}-\d{2}-\d{2}",

            // 2025-01-19_13-54-00
            @"\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}",

            // 2025_01_19
            @"\d{4}_\d{2}_\d{2}",

            // yyyyMMddHHmmssfff
            @"\d{17}",

            // Oppo / OnePlus: 241215093845937
            @"\d{15}",

            // yyyyMMddHHmmss
            @"\d{14}",

            // yyyyMMdd
            @"\d{8}",

            // 19-01-2025 13.54
            @"\d{2}-\d{2}-\d{4}\s\d{2}\.\d{2}",

            // 2025-01-19
            @"\d{4}-\d{2}-\d{2}"
        };


        foreach (var pattern in patterns)
        {
            foreach (Match match in Regex.Matches(text, pattern))
            {
                results.Add(match.Value);
            }
        }


        return results;
    }



    private DateTime? FindLooseDate(string text)
    {
        var match = Regex.Match(
            text,
            @"(20\d{2})[-_](\d{1,2})[-_](\d{1,2})");


        if (match.Success)
        {
            if (DateTime.TryParse(
                $"{match.Groups[1].Value}-{match.Groups[2].Value}-{match.Groups[3].Value}",
                out DateTime date))
            {
                if (IsValidDate(date))
                    return date;
            }
        }


        return null;
    }




    private int GetConfidence(string format)
    {
        return format switch
        {
            "yyyyMMddHHmmss" => 100,
            "yyyyMMddHHmmssfff" => 100,

            "yyyy-MM-dd-HH-mm-ss" => 100,
            "yyyy-MM-dd_HH-mm-ss" => 100,

            "yyMMddHHmmssfff" => 95,

            "dd-MM-yyyy HH.mm" => 95,
            "MM-dd-yyyy HH.mm" => 95,

            "yyyyMMdd-HHmmss" => 90,

            "yyyy-MM-dd" => 85,
            "yyyy_MM_dd" => 85,

            "yyyyMMdd" => 80,

            _ => 50
        };
    }



    private bool IsValidDate(DateTime date)
    {
        if (date.Year < 1990)
            return false;


        if (date.Year > DateTime.Now.Year + 1)
            return false;


        return true;
    }
}