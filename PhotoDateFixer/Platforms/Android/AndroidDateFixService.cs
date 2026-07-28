#if ANDROID

using AndroidX.ExifInterface.Media;
using PhotoDateFixer.Services;
using AndroidUri = Android.Net.Uri;

namespace PhotoDateFixer.Platforms.Android;

public class AndroidDateFixService : IDateFixService
{
    public Task<bool> FixDate(
        string filePath,
        string? contentUri,
        DateTime date)
    {
        try
        {
            ExifInterface exif;

            var context = global::Android.App.Application.Context;


            if (!string.IsNullOrEmpty(contentUri))
            {
                var uri = AndroidUri.Parse(contentUri);

                using var descriptor =
                    context.ContentResolver
                    .OpenFileDescriptor(uri, "rw");

                if (descriptor == null)
                    return Task.FromResult(false);

                exif = new ExifInterface(
                    descriptor.FileDescriptor);
            }
            else
            {
                exif = new ExifInterface(filePath);
            }


            string formattedDate =
                date.ToString("yyyy:MM:dd HH:mm:ss");


            // Update EXIF dates
            exif.SetAttribute(
                ExifInterface.TagDatetimeOriginal,
                formattedDate);

            exif.SetAttribute(
                ExifInterface.TagDatetimeDigitized,
                formattedDate);

            exif.SetAttribute(
                ExifInterface.TagDatetime,
                formattedDate);


            exif.SaveAttributes();


            // Refresh Android media database
            if (!string.IsNullOrEmpty(contentUri))
            {
                var uri = AndroidUri.Parse(contentUri);

                context.ContentResolver.NotifyChange(
                    uri,
                    null);

                System.Diagnostics.Debug.WriteLine(
                    "MEDIASTORE REFRESH REQUESTED");
            }


            System.Diagnostics.Debug.WriteLine(
                $"EXIF UPDATED: {formattedDate}");

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"EXIF ERROR: {ex}");

            return Task.FromResult(false);
        }
    }
}

#endif