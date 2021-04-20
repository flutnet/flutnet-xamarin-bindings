using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Flutnet.Interop.Embedding.Android;

namespace Flutnet.Interop.Sample
{
    [
        Activity(Label = "@string/app_name", Theme = "@style/LaunchTheme", MainLauncher = true,
            // FLUTTER ACTIVITY SETUP
            HardwareAccelerated = true,
            WindowSoftInputMode = SoftInput.AdjustResize,
            ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.Keyboard |
                                   ConfigChanges.ScreenSize | ConfigChanges.Locale |
                                   ConfigChanges.LayoutDirection | ConfigChanges.FontScale | ConfigChanges.ScreenLayout |
                                   ConfigChanges.Density | ConfigChanges.UiMode
        )
    ]
    [MetaData("io.flutter.embedding.android.NormalTheme", Resource = "@style/AppTheme")]
    [MetaData("io.flutter.embedding.android.SplashScreenDrawable", Resource = "@drawable/launch_background")]
    public class MainActivity : FlutterActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            App.Init(this);
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}