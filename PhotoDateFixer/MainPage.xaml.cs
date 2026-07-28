namespace PhotoDateFixer;

using PhotoDateFixer.Models;
using PhotoDateFixer.Services;
using System.Collections.ObjectModel;

public partial class MainPage : ContentPage
{
    private readonly PhotoPickerService photoPickerService;
    private readonly DateDetectionService parser = new();
    private readonly ObservableCollection<PhotoInfo> photoInfos = new();

    public MainPage()
    {
        InitializeComponent();

        photoPickerService = new PhotoPickerService();

        PhotoList.ItemsSource = photoInfos;

        SharedPhotoService.PhotosReceived += LoadSharedPhotos;

        LoadSharedPhotos();
        var exif = new ExifDateDetectionService();

        var result = exif.DetectDate(
            @"D:\Phone Backups\Phone Backup 1\All\DCIM\Camera\IMG_20260225_154951.jpg"
        );

        // Put a breakpoint here
        System.Diagnostics.Debug.WriteLine(
    $"EXIF TEST: {result.Date} | {result.Source} | {result.Confidence}"
);
    }

    private void LoadSharedPhotos()
    {
        if (SharedPhotoService.SharedFileNames.Count == 0)
            return;

        photoInfos.Clear();

        foreach (var fileName in SharedPhotoService.SharedFileNames)
        {
            var result = parser.DetectDate(fileName);

            System.Diagnostics.Debug.WriteLine(
                $"FILE: {fileName} | DATE: {result.Date} | CONFIDENCE: {result.Confidence} | SOURCE: {result.Source}"
            );

            photoInfos.Add(new PhotoInfo
            {
                FileName = fileName,
                DetectedDate = result.Date,
                Confidence = result.Confidence,
                Source = result.Source
            });
        }

        StatusLabel.Text = $"{photoInfos.Count} photos processed";

        SharedPhotoService.SharedFileNames.Clear();
    }


    private async void OnSelectPhotosClicked(object sender, EventArgs e)
    {
        var photos = await photoPickerService.PickPhotos();

        if (photos == null || photos.Count == 0)
            return;

        photoInfos.Clear();

        foreach (var photo in photos)
        {
            var result = parser.DetectDate(photo.FileName);
            System.Diagnostics.Debug.WriteLine(
    $"FILE: {photo.FileName} | DATE: {result.Date} | CONFIDENCE: {result.Confidence} | SOURCE: {result.Source}"
);
            photoInfos.Add(new PhotoInfo
            {
                FileName = photo.FileName,
                DetectedDate = result.Date,
                Confidence = result.Confidence,
                Source = result.Source
            });
        }

        StatusLabel.Text = $"{photoInfos.Count} photos processed";
    }


    private void OnFixDatesClicked(object sender, EventArgs e)
    {
        StatusLabel.Text = "Fix dates clicked";
    }
}