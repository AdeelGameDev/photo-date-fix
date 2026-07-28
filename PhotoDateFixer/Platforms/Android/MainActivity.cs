using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using PhotoDateFixer.Services;
namespace PhotoDateFixer
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ConfigChanges.ScreenSize
        | ConfigChanges.Orientation
        | ConfigChanges.UiMode
        | ConfigChanges.ScreenLayout
        | ConfigChanges.SmallestScreenSize
        | ConfigChanges.Density)]

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
            string? fileName = null;

            using (var cursor = ContentResolver.Query(uri, null, null, null, null))
            {
                if (cursor != null)
                {
                    int nameIndex = cursor.GetColumnIndex(
                        Android.Provider.OpenableColumns.DisplayName);

                    if (nameIndex >= 0 && cursor.MoveToFirst())
                    {
                        fileName = cursor.GetString(nameIndex);
                    }
                }
            }

            return fileName;
        }


        private void HandleShareIntent(Intent intent)
        {
            List<string> photos = new();

            if (intent.Action == Intent.ActionSend)
            {
                var uri = intent.GetParcelableExtra(Intent.ExtraStream)
                    as Android.Net.Uri;

                if (uri != null)
                {
                    var fileName = GetFileName(uri);

                    if (fileName != null)
                    {
                        photos.Add(fileName);
                    }
                }
            }


            else if (intent.Action == Intent.ActionSendMultiple)
            {
                var uris = intent.GetParcelableArrayListExtra(Intent.ExtraStream);

                if (uris != null)
                {
                    foreach (var item in uris)
                    {
                        if (item is Android.Net.Uri uri)
                        {
                            var fileName = GetFileName(uri);

                            if (fileName != null)
                            {
                                photos.Add(fileName);
                            }
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