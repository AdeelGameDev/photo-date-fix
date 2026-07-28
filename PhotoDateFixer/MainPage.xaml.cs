namespace PhotoDateFixer;

using PhotoDateFixer.Models;
using PhotoDateFixer.Services;
using System.Collections.ObjectModel;

public partial class MainPage : ContentPage
{
    private readonly PhotoPickerService photoPickerService;
    private readonly DateDetectionService parser = new();
    private readonly IDateFixService dateFixService;
    private readonly ObservableCollection<PhotoInfo> photoInfos = new();

    public MainPage(IDateFixService dateFixService)
    {
        InitializeComponent();
        this.dateFixService = dateFixService;

        photoPickerService = new PhotoPickerService();

        PhotoList.ItemsSource = photoInfos;

        SharedPhotoService.PhotosReceived += LoadSharedPhotos;

        LoadSharedPhotos();
    }

    private void LoadSharedPhotos()
    {
        if (SharedPhotoService.SharedPhotos.Count == 0)
            return;

        photoInfos.Clear();

        foreach (var sharedPhoto in SharedPhotoService.SharedPhotos)
        {
            System.Diagnostics.Debug.WriteLine($"PATH: {sharedPhoto.FullPath}");

            var result = parser.DetectDate(
                sharedPhoto.FullPath,
                sharedPhoto.FileName);

            System.Diagnostics.Debug.WriteLine(
                $"FILE: {sharedPhoto.FileName} | DATE: {result.Date} | CONFIDENCE: {result.Confidence} | SOURCE: {result.Source}"
            );

            photoInfos.Add(new PhotoInfo
            {
                FileName = sharedPhoto.FileName,
                FullPath = sharedPhoto.FullPath,
                ContentUri = sharedPhoto.ContentUri,
                DetectedDate = result.Date,
                Confidence = result.Confidence,
                Source = result.Source
            });
        }

        StatusLabel.Text = $"{photoInfos.Count} photos processed";

        SharedPhotoService.SharedPhotos.Clear();
    }

    private async void OnSelectPhotosClicked(object sender, EventArgs e)
    {
        var photos = await photoPickerService.PickPhotos();

        if (photos == null || photos.Count == 0)
            return;

        photoInfos.Clear();

        foreach (var photo in photos)
        {
            System.Diagnostics.Debug.WriteLine($"FullPath: {photo.FullPath}");
            System.Diagnostics.Debug.WriteLine($"FileName: {photo.FileName}");
            var result = parser.DetectDate(photo.FullPath, photo.FileName);

            System.Diagnostics.Debug.WriteLine(
                $"FILE: {photo.FileName} | DATE: {result.Date} | CONFIDENCE: {result.Confidence} | SOURCE: {result.Source}"
            );

            photoInfos.Add(new PhotoInfo
            {
                FileName = photo.FileName,
                FullPath = photo.FullPath,
                DetectedDate = result.Date,
                Confidence = result.Confidence,
                Source = result.Source
            });
        }

        StatusLabel.Text = $"{photoInfos.Count} photos processed";
    }

    private async void OnFixDatesClicked(object sender, EventArgs e)
    {
        try
        {
            StatusLabel.Text = "Fixing dates...";

            if (dateFixService == null)
            {
                System.Diagnostics.Debug.WriteLine("DATE FIX SERVICE IS NULL");
                StatusLabel.Text = "Service is null";
                return;
            }

            foreach (var photo in photoInfos)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Processing: {photo.FileName}");

                if (photo.DetectedDate == null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "No date detected, skipping");
                    continue;
                }

                bool success = await dateFixService.FixDate(
     photo.FullPath,
     photo.ContentUri,
     photo.DetectedDate.Value);

                System.Diagnostics.Debug.WriteLine(
                    $"{photo.FileName} -> {success}");
            }

            StatusLabel.Text = "Finished";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"FIX ERROR: {ex}");

            StatusLabel.Text = "Error";
        }
    }
}