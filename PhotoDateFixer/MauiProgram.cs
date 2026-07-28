using Microsoft.Extensions.Logging;
using PhotoDateFixer.Services;

#if ANDROID
using PhotoDateFixer.Platforms.Android;
#endif

namespace PhotoDateFixer
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });


            builder.Services.AddTransient<MainPage>();


#if ANDROID
            builder.Services.AddSingleton<IDateFixService, AndroidDateFixService>();
#endif


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}