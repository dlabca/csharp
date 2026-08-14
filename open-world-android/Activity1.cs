using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;
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
        public static TextView FpsTextView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _game = new Game1();
            _view = _game.Services.GetService(typeof(View)) as View;

            // NOVÉ: místo SetContentView(_view) přímo obalíme _view do FrameLayoutu
            // spolu s nativními ovládacími prvky (joystick, tlačítka) navrch.
            // Herní View zůstává úplně stejné, jen má teď "sourozence" ve stejném layoutu.
            var overlay = new FrameLayout(this);
            overlay.AddView(_view);

            FpsTextView = new TextView(this)
            {
                Text = "FPS: 0",
                TextSize = 18f
            };
            FpsTextView.SetTextColor(Android.Graphics.Color.White);
            FpsTextView.SetShadowLayer(3, 1, 1, Android.Graphics.Color.Black); // Pro dobrou čitelnost

            var fpsParams = new FrameLayout.LayoutParams(
                FrameLayout.LayoutParams.WrapContent,
                FrameLayout.LayoutParams.WrapContent)
            {
                Gravity = GravityFlags.Top | GravityFlags.Left,
                LeftMargin = 30,
                TopMargin = 30
            };
            overlay.AddView(FpsTextView, fpsParams);

            // --- Joystick (spodní pás obrazovky, reaguje jen v levé půlce - viz JoystickView) ---
            var joystick = new JoystickView(this);
            var displayMetrics = Resources.DisplayMetrics;
            int halfWidth = (int)(displayMetrics.WidthPixels / 2);

            var joystickParams = new FrameLayout.LayoutParams(halfWidth, 400)
            { Gravity = GravityFlags.Bottom | GravityFlags.Left };
            overlay.AddView(joystick, joystickParams);

            // --- Tlačítka ---
            AddButton(overlay, "SKOK", GravityFlags.Bottom | GravityFlags.Right, 40, 40,
                () => NativeInput.JumpPressedThisFrame = true);

            AddButton(overlay, "BĚH", GravityFlags.Bottom | GravityFlags.Right, 240, 40,
                () => NativeInput.RunToggleActive = !NativeInput.RunToggleActive, isToggle: true);

            AddButton(overlay, "PAL", GravityFlags.Bottom | GravityFlags.Right, 440, 40,
                () => NativeInput.ShootPressedThisFrame = true);
            AddButton(overlay, "OBCHOD", GravityFlags.Top | GravityFlags.Left, 40, 40,
                () => ShowShopDialog());
            // V OnCreate, hned za všemi ostatními prvky (před SetContentView):
            var crosshairView = new TextView(this)
            {
                Text = "+",
                TextSize = 28f,
                Gravity = GravityFlags.Center
            };
            crosshairView.SetTextColor(Android.Graphics.Color.White); // Nebo bílá
            crosshairView.SetBackgroundColor(Android.Graphics.Color.Transparent);

            // Důležité: ať neblokuje dotyky pro hru pod ním
            crosshairView.Touch += (s, e) => { e.Handled = false; };

            var crosshairParams = new FrameLayout.LayoutParams(
                FrameLayout.LayoutParams.MatchParent,
                FrameLayout.LayoutParams.MatchParent);

            overlay.AddView(crosshairView, crosshairParams);

            SetContentView(overlay);

            _game.Run();
            ApplyImmersiveMode();
        }

        private void AddButton(FrameLayout parent, string text, GravityFlags gravity,
                                int marginRightOrLeft, int marginBottom, Action onClick, bool isToggle = false)
        {
            var button = new Android.Widget.Button(this) { Text = text };

            // Definice barev
            var defaultBgColor = new Android.Graphics.Color(255, 255, 255, 90);  // Poloprůhledná bílá
            var activeBgColor = new Android.Graphics.Color(76, 175, 80, 200);     // Zelená při aktivaci

            // Nastavení bílého textu a základního pozadí
            button.SetTextColor(Android.Graphics.Color.White);
            button.SetBackgroundColor(defaultBgColor);
            button.Clickable = true;
            button.Focusable = true;

            var lp = new FrameLayout.LayoutParams(180, 180) { Gravity = gravity };
            if (gravity.HasFlag(GravityFlags.Right)) lp.RightMargin = marginRightOrLeft;
            if (gravity.HasFlag(GravityFlags.Left)) lp.LeftMargin = marginRightOrLeft;
            if (gravity.HasFlag(GravityFlags.Bottom)) lp.BottomMargin = marginBottom;
            if (gravity.HasFlag(GravityFlags.Top)) lp.TopMargin = marginBottom;

            bool isActive = false;

            button.Click += (s, e) =>
            {
                // Spustí předaný Action (např. nastavení NativeInput)
                onClick();

                // Pokud jde o přepínací tlačítko (např. BĚH), změní barvu
                if (isToggle)
                {
                    isActive = !isActive;
                    button.SetBackgroundColor(isActive ? activeBgColor : defaultBgColor);
                }
            };

            parent.AddView(button, lp);
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
#pragma warning disable CA1422
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
        private void ShowShopDialog()
        {
            // Připravíme si pole položek pro dialog
            var upgrades = GameEconomy.Upgrades;
            string[] options = new string[upgrades.Count];

            for (int i = 0; i < upgrades.Count; i++)
            {
                var up = upgrades[i];
                string costText = up.CanBuy() ? $"[${up.NextCost()}]" : "(MAX)";
                options[i] = $"{up.Name}: {up.ValueText()}  {costText}";
            }

            var builder = new AlertDialog.Builder(this);
            builder.SetTitle($"Obchod (Peníze: ${GameEconomy.Money})");

            // Zobrazí seznam všech upgradů jako vybíratelná položková nabídka
            builder.SetItems(options, (s, e) =>
            {
                int selectedIndex = e.Which;
                if (selectedIndex >= 0 && selectedIndex < upgrades.Count)
                {
                    var chosenUpgrade = upgrades[selectedIndex];
                    if (chosenUpgrade.CanBuy())
                    {
                        chosenUpgrade.TryBuy();
                    }
                    // Dialog hned zase otevřeme aktualizovaný, aby hráč viděl nové peníze/tier
                    ShowShopDialog();
                }
            });

            builder.SetNegativeButton("Zavřít", (s, e) => { });
            builder.Show();
        }
    }
}