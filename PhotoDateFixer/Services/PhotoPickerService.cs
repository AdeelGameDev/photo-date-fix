using Microsoft.Maui.Storage;

namespace PhotoDateFixer.Services;

public class PhotoPickerService
{
    public async Task<List<FileResult>> PickPhotos()
    {
        var results = await FilePicker.Default.PickMultipleAsync(
            new PickOptions
            {
                PickerTitle = "Select photos",
                FileTypes = FilePickerFileType.Images
            });

        return results.ToList();
    }
}