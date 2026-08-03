using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace open_world
{
    // Funguje na obou platformách zároveň:
    // - Desktop: klávesnice + myš (beze změny, pro rychlé testování v PC buildu)
    // - Android: pohyb/skok/běh/pauza čtou z NativeInput (nativní Android View
    //            tlačítka + JoystickView), rozhlížení (drag na pravé půlce) jede
    //            přes MonoGame TouchPanel, protože tam žádný nativní View nepřekáží.
    public static class Input
    {
        // ================= DESKTOP (beze změny) =================
        public static KeyboardState CurrentKeyboardState { get; private set; }
        public static KeyboardState PreviousKeyboardState { get; private set; }

        public static MouseState CurrentMouseState { get; private set; }
        public static MouseState PreviousMouseState { get; private set; }

        // ================= SPOLEČNÉ VÝSTUPY (obě platformy do nich píšou) =================

        public static Vector2 MouseDelta { get; private set; }

        // ================= TOUCH STAV (jen look-drag, zbytek jde přes NativeInput) =================

        private static int? _lookTouchId;
        private static Vector2 _lookLastPos;

        public static bool IsTouchPlatform => OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();

        public static void Update(GameWindow window)
        {
            if (IsTouchPlatform)
            {
                UpdateTouch(window);
            }
            else
            {
                UpdateDesktop(window);
            }
        }

        private static void UpdateDesktop(GameWindow window)
        {
            PreviousKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();

            PreviousMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();

            int centerX = window.ClientBounds.Width / 2;
            int centerY = window.ClientBounds.Height / 2;

            MouseDelta = new Vector2(
                CurrentMouseState.X - centerX,
                CurrentMouseState.Y - centerY
            );
        }

        // Řeší JEN drag pro rozhlížení (pravá půlka obrazovky) - žádný nativní
        // View tam nic nezakrývá, takže dotyky normálně propadnou do TouchPanelu.
        private static void UpdateTouch(GameWindow window)
        {
            TouchCollection touches = TouchPanel.GetState();
            int screenWidth = window.ClientBounds.Width;

            bool lookTouchStillPresent = false;

            foreach (TouchLocation touch in touches)
            {
                Vector2 pos = touch.Position;

                switch (touch.State)
                {
                    case TouchLocationState.Pressed:
                        if (pos.X >= screenWidth * 0.5f && !_lookTouchId.HasValue)
                        {
                            _lookTouchId = touch.Id;
                            _lookLastPos = pos;
                        }
                        break;

                    case TouchLocationState.Moved:
                        if (_lookTouchId.HasValue && touch.Id == _lookTouchId.Value)
                        {
                            lookTouchStillPresent = true;
                            Vector2 delta = pos - _lookLastPos;
                            MouseDelta = delta;
                            _lookLastPos = pos;
                        }
                        break;

                    case TouchLocationState.Released:
                    case TouchLocationState.Invalid:
                        if (_lookTouchId.HasValue && touch.Id == _lookTouchId.Value)
                        {
                            _lookTouchId = null;
                        }
                        break;
                }
            }

            if (_lookTouchId.HasValue && !lookTouchStillPresent)
            {
                bool stillInList = false;
                foreach (var t in touches) if (t.Id == _lookTouchId.Value) stillInList = true;
                if (!stillInList) _lookTouchId = null;
            }

            if (!lookTouchStillPresent) MouseDelta = Vector2.Zero;
        }

        public static void CenterMouse(GameWindow window)
        {
            if (IsTouchPlatform) return;

            int centerX = window.ClientBounds.Width / 2;
            int centerY = window.ClientBounds.Height / 2;
            Mouse.SetPosition(centerX, centerY);
        }

        // ==========================================
        //  SYSTÉMOVÉ A UI AKCE
        // ==========================================

        public static bool IsToggleFullscreenPressed() => !IsTouchPlatform && IsKeyPressed(Keys.F11);

        public static bool IsPausePressed()
        {
            if (IsTouchPlatform)
            {
#if ANDROID
                bool pressed = NativeInput.PausePressedThisFrame;
                NativeInput.PausePressedThisFrame = false;
                return pressed;
#else
                return false; // sem se na Desktopu nikdy nedostaneme (IsTouchPlatform == false)
#endif
            }
            return IsKeyPressed(Keys.Escape);
        }

        public static bool IsMenuConfirmPressed()
        {
            if (IsTouchPlatform)
            {
#if ANDROID
                bool pressed = NativeInput.PausePressedThisFrame;
                NativeInput.PausePressedThisFrame = false;
                return pressed;
#else
                return false;
#endif
            }
            return IsKeyPressed(Keys.Enter) ||
                   IsKeyPressed(Keys.Space) ||
                   IsKeyPressed(Keys.Escape);
        }

        // ==========================================
        //  HERNÍ AKCE (POHYB A AKCE HRÁČE) - na touch platformě z NativeInput
        // ==========================================

        public static bool IsJumpPressed()
        {
            if (IsTouchPlatform)
            {
#if ANDROID
                bool pressed = NativeInput.JumpPressedThisFrame;
                NativeInput.JumpPressedThisFrame = false;
                return pressed;
#else
                return false;
#endif
            }
            return IsKeyPressed(Keys.Space);
        }

        public static bool IsShootPressed()
        {
            if (IsTouchPlatform)
            {
#if ANDROID
                bool pressed = NativeInput.ShootPressedThisFrame;
                NativeInput.ShootPressedThisFrame = false;
                return pressed;
#else
                return false;
#endif
            }
            return CurrentMouseState.LeftButton == ButtonState.Pressed
                && PreviousMouseState.LeftButton == ButtonState.Released;
        }

        public static bool IsRunning()
        {
            if (IsTouchPlatform)
            {
#if ANDROID
                return NativeInput.RunToggleActive;
#else
                return false;
#endif
            }
            return IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift);
        }

        public static Vector2 GetMovementVector()
        {
            if (IsTouchPlatform)
            {
#if ANDROID
                return NativeInput.MovementVector;
#else
                return Vector2.Zero;
#endif
            }

            Vector2 movement = Vector2.Zero;

            if (IsKeyDown(Keys.W)) movement.Y += 1f;
            if (IsKeyDown(Keys.S)) movement.Y -= 1f;
            if (IsKeyDown(Keys.A)) movement.X -= 1f;
            if (IsKeyDown(Keys.D)) movement.X += 1f;

            if (movement != Vector2.Zero)
                movement.Normalize();

            return movement;
        }

        // ==========================================
        //  POMOCNÉ METODY PRO KLÁVESNICI A MYŠ (jen desktop, beze změny)
        // ==========================================

        public static bool IsKeyDown(Keys key) => CurrentKeyboardState.IsKeyDown(key);
        public static bool IsKeyPressed(Keys key) => CurrentKeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyUp(key);
    }
}