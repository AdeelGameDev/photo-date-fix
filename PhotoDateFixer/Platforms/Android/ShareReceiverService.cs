using Android.Content;

namespace PhotoDateFixer.Services;

public class ShareReceiverService
{
    public List<Android.Net.Uri> GetSharedImages(Intent intent)
    {
        var images = new List<Android.Net.Uri>();

        if (intent.Action == Intent.ActionSend)
        {
            var image = intent.GetParcelableExtra(Intent.ExtraStream);

            if (image is Android.Net.Uri uri)
            {
                images.Add(uri);
            }
        }

        else if (intent.Action == Intent.ActionSendMultiple)
        {
            var imageList = intent.GetParcelableArrayListExtra(Intent.ExtraStream);

            if (imageList != null)
            {
                foreach (var item in imageList)
                {
                    if (item is Android.Net.Uri uri)
                    {
                        images.Add(uri);
                    }
                }
            }
        }

        return images;
    }
}