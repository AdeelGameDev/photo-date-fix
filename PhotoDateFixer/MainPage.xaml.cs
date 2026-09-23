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
    private bool hasPlayedEntranceAnimation;

    public MainPage(IDateFixService dateFixService)
    {
        InitializeComponent();

        this.dateFixService = dateFixService;
        photoPickerService = new PhotoPickerService();
        PhotoList.ItemsSource = photoInfos;

        SharedPhotoService.PhotosReceived += LoadSharedPhotos;
        ManualDatePicker.Date = DateTime.Today;
        ManualTimePicker.Time = DateTime.Now.TimeOfDay;
        LoadSharedPhotos();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (hasPlayedEntranceAnimation)
            return;

        hasPlayedEntranceAnimation = true;
        await PageContent.FadeTo(1, 220, Easing.CubicOut);
    }

    private void LoadSharedPhotos()
    {
        if (SharedPhotoService.SharedPhotos.Count == 0)
            return;

        photoInfos.Clear();

        foreach (var sharedPhoto in SharedPhotoService.SharedPhotos)
        {
            var result = parser.DetectDate(sharedPhoto.FullPath, sharedPhoto.FileName);

            photoInfos.Add(new PhotoInfo
            {
                FileName = sharedPhoto.FileName,
                FullPath = sharedPhoto.FullPath,
                ContentUri = sharedPhoto.ContentUri,
                Thumbnail = ImageSource.FromFile(sharedPhoto.FullPath),
                DetectedDate = result.Date,
                Confidence = result.Confidence,
                Source = result.Source
            });
        }

        StatusLabel.Text = $"{photoInfos.Count} photo(s) ready to review";
        SharedPhotoService.SharedPhotos.Clear();
    }

    private async void OnSetDateClicked(object sender, EventArgs e)
    {
        var selectedPhotos = photoInfos.Where(photo => photo.IsSelected).ToList();

        if (selectedPhotos.Count == 0)
        {
            await DisplayAlert("Select photos", "Check one or more photos before applying a date.", "OK");
            return;
        }

        var selectedDate = ManualDatePicker.Date.Date + new TimeSpan(
            ManualTimePicker.Time.Hours,
            ManualTimePicker.Time.Minutes,
            0);

        foreach (var photo in selectedPhotos)
        {
            photo.DetectedDate = selectedDate;
            photo.Confidence = 100;
            photo.Source = "Manual";
        }

        StatusLabel.Text = $"Date applied to {selectedPhotos.Count} selected photo(s)";
    }

    private async void OnSelectPhotosClicked(object sender, EventArgs e)
    {
        SetBusy(true, "Opening your photo library…");

        try
        {
            var photos = await photoPickerService.PickPhotos();

            if (photos == null || photos.Count == 0)
            {
                StatusLabel.Text = "No photos selected";
                return;
            }

            StatusLabel.Text = "Looking for photo dates…";
            photoInfos.Clear();

            foreach (var photo in photos)
            {
                var result = parser.DetectDate(photo.FullPath, photo.FileName);

                photoInfos.Add(new PhotoInfo
                {
                    FileName = photo.FileName,
                    FullPath = photo.FullPath,
                    ContentUri = photo.ContentUri,
                    Thumbnail = ImageSource.FromFile(photo.FullPath),
                    DetectedDate = result.Date,
                    Confidence = result.Confidence,
                    Source = result.Source
                });
            }

            StatusLabel.Text = $"{photoInfos.Count} photo(s) ready to review";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PHOTO PICKER ERROR: {ex}");
            StatusLabel.Text = "Couldn’t load those photos";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void OnPhotoTapped(object sender, TappedEventArgs e)
    {
        if (sender is BindableObject view && view.BindingContext is PhotoInfo photo)
            photo.IsSelected = !photo.IsSelected;
    }

    private async void OnToggleManualDateEditor(object sender, EventArgs e)
    {
        var isOpening = !ManualDateEditor.IsVisible;
        ManualDateEditor.IsVisible = isOpening;
        ManualDateToggleButton.Text = isOpening
            ? "Hide custom date controls  ▴"
            : "Set a custom date for selected photos  ▾";

        if (isOpening)
        {
            ManualDateEditor.Opacity = 0;
            await ManualDateEditor.FadeTo(1, 160, Easing.CubicOut);
        }
    }

    private async void OnFixDatesClicked(object sender, EventArgs e)
    {
        if (photoInfos.Count == 0)
        {
            await DisplayAlert("No photos", "Select photos before updating their EXIF dates.", "OK");
            return;
        }

        var shouldUpdate = await DisplayAlert(
            "Update EXIF dates?",
            $"This will write dates into {photoInfos.Count} loaded photo(s).",
            "Update",
            "Cancel");

        if (!shouldUpdate)
            return;

        try
        {
            FixDatesButton.IsEnabled = false;
            SetBusy(true, "Updating EXIF dates…");

            var updatedCount = 0;
            var failedCount = 0;

            foreach (var photo in photoInfos.Where(photo => photo.DetectedDate.HasValue))
            {
                var success = await dateFixService.FixDate(
                    photo.FullPath,
                    photo.ContentUri,
                    photo.DetectedDate!.Value);

                if (success)
                    updatedCount++;
                else
                    failedCount++;
            }

            StatusLabel.Text = failedCount == 0
                ? $"Updated {updatedCount} photo(s)"
                : $"Updated {updatedCount}; {failedCount} could not be updated";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FIX ERROR: {ex}");
            StatusLabel.Text = "Something went wrong while updating dates";
        }
        finally
        {
            FixDatesButton.IsEnabled = true;
            SetBusy(false);
        }
    }

    private void SetBusy(bool isBusy, string? status = null)
    {
        LoadingIndicator.IsVisible = isBusy;
        LoadingIndicator.IsRunning = isBusy;

        if (status is not null)
            StatusLabel.Text = status;
    }
}
