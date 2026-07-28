using System.ComponentModel;
using System.Runtime.CompilerServices;

public class PhotoInfo : INotifyPropertyChanged
{
    public required string FileName { get; set; }

    public required string FullPath { get; set; }

    public string? ContentUri { get; set; }

    public ImageSource? Thumbnail { get; set; }


    private bool isSelected;

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            OnPropertyChanged();
        }
    }


    private DateTime? detectedDate;

    public DateTime? DetectedDate
    {
        get => detectedDate;
        set
        {
            detectedDate = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayDate));
        }
    }


    public int Confidence { get; set; }

    public string Source { get; set; } = "";


    public string DisplayDate
    {
        get
        {
            if (DetectedDate.HasValue)
            {
                return $"{DetectedDate.Value:dd MMM yyyy HH:mm}\nConfidence: {Confidence}%\nSource: {Source}";
            }

            return "No date detected";
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;


    private void OnPropertyChanged(
        [CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(name));
    }
}