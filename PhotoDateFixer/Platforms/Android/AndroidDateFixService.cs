#if ANDROID

using AndroidX.ExifInterface.Media;
using Android.Content;
using Android.Provider;
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

                RefreshGalleryDate(context, uri, date);
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

    private static void RefreshGalleryDate(
        global::Android.Content.Context context,
        AndroidUri uri,
        DateTime date)
    {
        try
        {
            context.ContentResolver.NotifyChange(uri, null);

            // Gallery apps commonly read this indexed MediaStore field rather
            // than re-parsing EXIF immediately, so keep it in sync as well.
            var values = new ContentValues();
            values.Put(
                MediaStore.Images.ImageColumns.DateTaken,
                new DateTimeOffset(date).ToUnixTimeMilliseconds());
            values.Put(
                MediaStore.IMediaColumns.DateModified,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            context.ContentResolver.Update(uri, values, null, null);
            System.Diagnostics.Debug.WriteLine("EXIF AND MEDIASTORE DATE UPDATED");
        }
        catch (Exception ex)
        {
            // An external document provider may allow EXIF writes but not expose
            // MediaStore columns. The EXIF change has already succeeded in that case.
            System.Diagnostics.Debug.WriteLine($"MEDIASTORE DATE REFRESH FAILED: {ex}");
        }
    }
}

#endif
