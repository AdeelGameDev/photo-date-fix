namespace PhotoDateFixer;
using PhotoDateFixer.Services;

public partial class MainPage : ContentPage
{
    private readonly PhotoPickerService photoPickerService;

    public MainPage()
    {
        InitializeComponent();

        photoPickerService = new PhotoPickerService();
    }

    private async void OnSelectPhotosClicked(object sender, EventArgs e)
    {
        var photos = await photoPickerService.PickPhotos();

        StatusLabel.Text = $"{photos.Count} photos selected";
    }

    private void OnFixDatesClicked(object sender, EventArgs e)
    {
        StatusLabel.Text = "Fix dates clicked";
    }
}