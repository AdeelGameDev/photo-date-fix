using PhotoDateFixer.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PhotoDateFixer.Services;

public class DateDetectionService
{
    private readonly string[] formats =
    {
        // Full date + time (highest confidence)
        "yyyy-MM-dd-HH-mm-ss",
        "yyyyMMddHHmmss",
        "yyyyMMddHHmmssfff",

        // Samsung / Screenshot style
        "yyyyMMdd-HHmmss",

        // Camera milliseconds
        "yyMMddHHmmssfff",

        // Scanner
        "dd-MM-yyyy HH.mm",
        "MM-dd-yyyy HH.mm",

        // Date only (lowest)
        "yyyyMMdd"
    };


    public DateDetectionResult DetectDate(string fileName)
    {
        string name = Path.GetFileNameWithoutExtension(fileName);

        foreach (var part in ExtractParts(name))
        {
            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(
                    part,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime result))
                {
                    if (IsValidDate(result))
                    {
                        return new DateDetectionResult
                        {
                            Date = result,
                            Confidence = GetConfidence(format),
                            Source = format
                        };
                    }
                }
            }
        }


        return new DateDetectionResult
        {
            Date = null,
            Confidence = 0,
            Source = "No match"
        };
    }



    private int GetConfidence(string format)
    {
        return format switch
        {
            "yyyyMMddHHmmss" => 100,
            "yyyyMMddHHmmssfff" => 100,
            "yyyy-MM-dd-HH-mm-ss" => 100,
            "yyyyMMdd_HHmmssfff" => 100,

            "yyyyMMdd-HHmmss" => 90,

            "dd-MM-yyyy HH.mm" => 95,
            "MM-dd-yyyy HH.mm" => 95,

            "yyMMddHHmmssfff" => 90,

            "yyyyMMdd" => 80,

            _ => 50
        };
    }



    private List<string> ExtractParts(string text)
    {
        HashSet<string> parts = new();


        parts.Add(text);


        AddMatches(parts, text,
            @"\d{4}-\d{2}-\d{2}-\d{2}-\d{2}-\d{2}");


        AddMatches(parts, text,
            @"\d{8}-\d{6}");


        AddMatches(parts, text,
            @"\d{17}");


        AddMatches(parts, text,
            @"\d{14}");


        AddMatches(parts, text,
            @"\d{2}-\d{2}-\d{4}\s\d{2}\.\d{2}");


        AddMatches(parts, text,
            @"\d{8}");


        return parts.ToList();
    }



    private void AddMatches(
        HashSet<string> parts,
        string text,
        string pattern)
    {
        foreach (Match match in Regex.Matches(text, pattern))
        {
            parts.Add(match.Value);
        }
    }



    private bool IsValidDate(DateTime date)
    {
        if (date.Year < 2000)
            return false;


        if (date.Year > DateTime.Now.Year + 1)
            return false;


        return true;
    }
}