namespace PhotoDateFixer.Services;

public static class SharedPhotoService
{
    public static List<string> SharedFileNames { get; set; } = new();

    public static event Action? PhotosReceived;

    public static void AddPhotos(List<string> photos)
    {
        SharedFileNames.Clear();

        SharedFileNames.AddRange(photos);

        PhotosReceived?.Invoke();
    }
}