using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using PhotoDateFixer.Services;
using AndroidX.ExifInterface.Media;
using PhotoDateFixer.Models;
namespace PhotoDateFixer
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges =
            ConfigChanges.ScreenSize |
            ConfigChanges.Orientation |
            ConfigChanges.UiMode |
            ConfigChanges.ScreenLayout |
            ConfigChanges.SmallestScreenSize |
            ConfigChanges.Density)]

    [IntentFilter(
        new[] { Intent.ActionSend },
        Categories = new[] { Intent.CategoryDefault },
        DataMimeType = "image/*")]

    [IntentFilter(
        new[] { Intent.ActionSendMultiple },
        Categories = new[] { Intent.CategoryDefault },
        DataMimeType = "image/*")]

    public class MainActivity : MauiAppCompatActivity
    {

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            HandleShareIntent(Intent);
        }


        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);

            if (intent != null)
            {
                HandleShareIntent(intent);
            }
        }



        private string? GetFileName(Android.Net.Uri uri)
        {
            try
            {
                using var cursor =
                    ContentResolver.Query(
                        uri,
                        null,
                        null,
                        null,
                        null);

                if (cursor != null)
                {
                    int index =
                        cursor.GetColumnIndex(
                            Android.Provider.OpenableColumns.DisplayName);

                    if (index >= 0 && cursor.MoveToFirst())
                    {
                        return cursor.GetString(index);
                    }
                }
            }
            catch
            {
            }

            return null;
        }



        private string? CopyUriToCache(Android.Net.Uri uri)
        {
            try
            {
                string fileName =
                    GetFileName(uri)
                    ?? $"shared_{Guid.NewGuid()}.jpg";


                string cachePath =
                    Path.Combine(
                        CacheDir!.AbsolutePath,
                        fileName);



                using var input =
                    ContentResolver.OpenInputStream(uri);


                if (input == null)
                    return null;


                using var output =
                    File.Create(cachePath);


                input.CopyTo(output);


                System.Diagnostics.Debug.WriteLine(
                    $"CACHE COPY: {cachePath}");


                return cachePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"CACHE ERROR: {ex}");

                return null;
            }
        }




        private void HandleShareIntent(Intent intent)
        {
            List<SharedPhoto> photos = new();



            if (intent.Action == Intent.ActionSend)
            {
                var uri =
                    intent.GetParcelableExtra(
                        Intent.ExtraStream)
                    as Android.Net.Uri;


                if (uri != null)
                {
                    string? path =
                        CopyUriToCache(uri);


                    photos.Add(new SharedPhoto
                    {
                        FileName =
                            GetFileName(uri)
                            ?? "Unknown.jpg",

                        FullPath =
                            path ?? "",

                        ContentUri =
                            uri.ToString()
                    });
                }
            }



            else if (intent.Action == Intent.ActionSendMultiple)
            {
                var uris =
                    intent.GetParcelableArrayListExtra(
                        Intent.ExtraStream);


                if (uris != null)
                {
                    foreach (var item in uris)
                    {
                        if (item is Android.Net.Uri uri)
                        {
                            string? path =
                                CopyUriToCache(uri);


                            photos.Add(new SharedPhoto
                            {
                                FileName =
                                    GetFileName(uri)
                                    ?? "Unknown.jpg",

                                FullPath =
                                    path ?? "",

                                ContentUri =
                                    uri.ToString()
                            });
                        }
                    }
                }
            }



            if (photos.Count > 0)
            {
                SharedPhotoService.AddPhotos(photos);
            }
        }
    }
}