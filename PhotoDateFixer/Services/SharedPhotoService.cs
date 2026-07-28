using PhotoDateFixer.Models;

namespace PhotoDateFixer.Services;

public static class SharedPhotoService
{
    public static List<SharedPhoto> SharedPhotos { get; } = new();

    public static event Action? PhotosReceived;

    public static void AddPhotos(List<SharedPhoto> photos)
    {
        SharedPhotos.Clear();

        SharedPhotos.AddRange(photos);

        PhotosReceived?.Invoke();
    }
}