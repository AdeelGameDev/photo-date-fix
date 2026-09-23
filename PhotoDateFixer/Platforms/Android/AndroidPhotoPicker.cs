#if ANDROID

using Android.App;
using Android.Content;
using Android.Provider;
using AndroidUri = Android.Net.Uri;
using PhotoDateFixer.Models;

namespace PhotoDateFixer.Platforms.Android;

/// <summary>
/// Uses the Storage Access Framework so selected images retain their original
/// content URI and can be written back to the gallery, rather than to a cache copy.
/// </summary>
public static class AndroidPhotoPicker
{
    private const int PickPhotosRequestCode = 4817;
    private static TaskCompletionSource<List<SharedPhoto>>? pendingPick;

    public static Task<List<SharedPhoto>> PickMultipleAsync()
    {
        if (pendingPick is not null)
            throw new InvalidOperationException("A photo selection is already in progress.");

        var activity = Platform.CurrentActivity
            ?? throw new InvalidOperationException("No Android activity is available for photo selection.");

        var intent = new Intent(Intent.ActionOpenDocument);
        intent.AddCategory(Intent.CategoryOpenable);
        intent.SetType("image/*");
        intent.PutExtra(Intent.ExtraAllowMultiple, true);
        intent.AddFlags(
            ActivityFlags.GrantReadUriPermission |
            ActivityFlags.GrantWriteUriPermission |
            ActivityFlags.GrantPersistableUriPermission);

        pendingPick = new TaskCompletionSource<List<SharedPhoto>>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        activity.StartActivityForResult(intent, PickPhotosRequestCode);
        return pendingPick.Task;
    }

    public static bool HandleActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        if (requestCode != PickPhotosRequestCode)
            return false;

        var completion = pendingPick;
        pendingPick = null;

        if (completion is null)
            return true;

        if (resultCode != Result.Ok || data is null)
        {
            completion.TrySetResult([]);
            return true;
        }

        try
        {
            var photos = new List<SharedPhoto>();

            foreach (var uri in GetUris(data))
            {
                PersistGrantedPermissions(uri, data.Flags);
                var photo = CreatePhoto(uri);

                if (photo is not null)
                    photos.Add(photo);
            }

            completion.TrySetResult(photos);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PHOTO PICKER ERROR: {ex}");
            completion.TrySetException(ex);
        }

        return true;
    }

    private static IEnumerable<AndroidUri> GetUris(Intent data)
    {
        if (data.Data is not null)
            yield return data.Data;

        if (data.ClipData is null)
            yield break;

        for (var index = 0; index < data.ClipData.ItemCount; index++)
        {
            var uri = data.ClipData.GetItemAt(index)?.Uri;

            if (uri is not null)
                yield return uri;
        }
    }

    private static void PersistGrantedPermissions(AndroidUri uri, ActivityFlags resultFlags)
    {
        var grantedFlags = resultFlags &
            (ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);

        try
        {
            global::Android.App.Application.Context.ContentResolver
                .TakePersistableUriPermission(uri, grantedFlags);
        }
        catch (Exception ex)
        {
            // Some providers do not support persisted permissions. The activity
            // result grant is still valid for the current editing session.
            System.Diagnostics.Debug.WriteLine($"URI PERMISSION NOT PERSISTED: {ex.Message}");
        }
    }

    private static SharedPhoto? CreatePhoto(AndroidUri uri)
    {
        var context = global::Android.App.Application.Context;
        var fileName = GetFileName(context, uri) ?? $"photo_{Guid.NewGuid():N}.jpg";
        var cachePath = Path.Combine(context.CacheDir!.AbsolutePath, $"{Guid.NewGuid():N}_{fileName}");

        using var input = context.ContentResolver.OpenInputStream(uri);
        if (input is null)
            return null;

        using var output = File.Create(cachePath);
        input.CopyTo(output);

        return new SharedPhoto
        {
            FileName = fileName,
            FullPath = cachePath,
            ContentUri = uri.ToString()
        };
    }

    private static string? GetFileName(Context context, AndroidUri uri)
    {
        using var cursor = context.ContentResolver.Query(uri, null, null, null, null);

        if (cursor is null)
            return null;

        var index = cursor.GetColumnIndex(OpenableColumns.DisplayName);
        return index >= 0 && cursor.MoveToFirst() ? cursor.GetString(index) : null;
    }
}

#endif
