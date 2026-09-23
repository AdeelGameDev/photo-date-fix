using Microsoft.Maui.Storage;
using PhotoDateFixer.Models;

#if ANDROID
using PhotoDateFixer.Platforms.Android;
#endif

namespace PhotoDateFixer.Services;

public class PhotoPickerService
{
    public async Task<List<SharedPhoto>> PickPhotos()
    {
#if ANDROID
        return await AndroidPhotoPicker.PickMultipleAsync();
#else
        var results = await FilePicker.Default.PickMultipleAsync(
            new PickOptions
            {
                PickerTitle = "Select photos",
                FileTypes = FilePickerFileType.Images
            });

        return results.Select(photo => new SharedPhoto
        {
            FileName = photo.FileName,
            FullPath = photo.FullPath,
            ContentUri = null
        }).ToList();
#endif
    }
}
