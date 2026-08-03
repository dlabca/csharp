using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using open_world;
using System;

namespace open_world_android
{
    [Activity(
        Label = "@string/app_name",
        MainLauncher = true,
        Icon = "@drawable/icon",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize |
                     ConfigChanges.ScreenLayout | ConfigChanges.KeyboardHidden |
                     ConfigChanges.UiMode,
        ScreenOrientation = ScreenOrientation.Landscape

    )]
    public class Activity1 : AndroidGameActivity
    {
        private Game1 _game;
        private View _view;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _game = new Game1();
            _view = _game.Services.GetService(typeof(View)) as View;

            SetContentView(_view);
            _game.Run();
            ApplyImmersiveMode();
        }
        protected override void OnResume()
        {
            base.OnResume();
            ApplyImmersiveMode();
        }

        private void ApplyImmersiveMode()
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(30))
            {
                // Moderní cesta (Android 11 / API 30+) - staré SystemUiFlags jsou tady obsolete.
                Window.SetDecorFitsSystemWindows(false);

                var controller = Window.InsetsController;
                if (controller != null)
                {
                    controller.Hide(WindowInsets.Type.SystemBars());
                    controller.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
                }
            }
            else
            {
                // Fallback pro starší telefony (API 23-29), kde WindowInsetsController neexistuje.
#pragma warning disable CA1422 // víme, že je to obsolete - je to tu jen pro starší API
                Window.DecorView.SystemUiFlags =
                    SystemUiFlags.LayoutStable
                    | SystemUiFlags.LayoutHideNavigation
                    | SystemUiFlags.LayoutFullscreen
                    | SystemUiFlags.HideNavigation
                    | SystemUiFlags.Fullscreen
                    | SystemUiFlags.ImmersiveSticky;
#pragma warning restore CA1422
            }
        }


    }
}
