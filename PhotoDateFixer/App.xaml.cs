using PhotoDateFixer.Services;

namespace PhotoDateFixer;

public partial class App : Application
{
    public App(IDateFixService dateFixService)
    {
        InitializeComponent();

        MainPage = new AppShell(
            new MainPage(dateFixService)
        );
    }
}