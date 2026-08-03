using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace open_world
{
    // Funguje na obou platformách zároveň:
    // - Desktop: klávesnice + myš (beze změny, pro rychlé testování v PC buildu)
    // - Android: virtuální joystick (levá půlka obrazovky = pohyb),
    //            drag (pravá půlka = rozhlížení), tlačítka (skok / běh / pauza)
    public static class Input
    {
        // ================= DESKTOP (beze změny) =================
        public static KeyboardState CurrentKeyboardState { get; private set; }
        public static KeyboardState PreviousKeyboardState { get; private set; }

        public static MouseState CurrentMouseState { get; private set; }
        public static MouseState PreviousMouseState { get; private set; }

        // ================= SPOLEČNÉ VÝSTUPY (obě platformy do nich píšou) =================

        // Delta pro rozhlížení - na desktopu z myši, na mobilu z touch dragu.
        public static Vector2 MouseDelta { get; private set; }

        // ================= TOUCH STAV =================

        private const float JoystickMaxRadius = 90f; // px, poloměr "virtuálního joysticku"
        private const float ButtonSize = 110f;       // px, čtvercová tlačítka

        private static int? _moveTouchId;
        private static Vector2 _moveStartPos;
        private static Vector2 _moveCurrentPos;
        private static Vector2 _movementVector; // výsledek pro GetMovementVector()

        private static int? _lookTouchId;
        private static Vector2 _lookLastPos;

        private static bool _jumpPressedThisFrame;
        private static bool _pausePressedThisFrame;
        private static bool _runToggleActive;

        private static Rectangle _jumpButtonBounds;
        private static Rectangle _runButtonBounds;
        private static Rectangle _pauseButtonBounds;

        // Pro vykreslení virtuálních tlačítek/joysticku v Game1.Draw() (viz níže).
        public static bool IsTouchPlatform => OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();
        public static Rectangle JumpButtonBounds => _jumpButtonBounds;
        public static Rectangle RunButtonBounds => _runButtonBounds;
        public static Rectangle PauseButtonBounds => _pauseButtonBounds;
        public static bool RunToggleActive => _runToggleActive;
        public static bool IsJoystickActive => _moveTouchId.HasValue;
        public static Vector2 JoystickCenter => _moveStartPos;
        public static Vector2 JoystickCurrentPos => _moveCurrentPos;

        /// <summary>
        /// Aktualizace stavů - volej jako první řádek v Update() v Game1.cs
        /// </summary>
        public static void Update(GameWindow window)
        {
            UpdateLayout(window);

            if (IsTouchPlatform)
            {
                UpdateTouch(window);
            }
            else
            {
                UpdateDesktop(window);
            }
        }

        // ================= DESKTOP VĚTEV (původní kód, beze změny) =================

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

            // Na desktopu se joystick/tlačítka nepoužívají - vynulovat, ať se nic
            // z předchozího touch stavu netahá do desktop běhu (kdyby se přepínalo).
            _movementVector = Vector2.Zero;
            _jumpPressedThisFrame = false;
            _pausePressedThisFrame = false;
        }

        // ================= TOUCH VĚTEV (nové) =================

        private static void UpdateLayout(GameWindow window)
        {
            int w = window.ClientBounds.Width;
            int h = window.ClientBounds.Height;

            // Tlačítka vpravo dole, nad sebou (skok, běh) + pauza vpravo nahoře.
            _jumpButtonBounds = new Rectangle((int)(w - ButtonSize * 1.4f), (int)(h - ButtonSize * 1.4f), (int)ButtonSize, (int)ButtonSize);
            _runButtonBounds = new Rectangle((int)(w - ButtonSize * 2.8f), (int)(h - ButtonSize * 1.4f), (int)ButtonSize, (int)ButtonSize);
            _pauseButtonBounds = new Rectangle((int)(w - ButtonSize * 1.1f), (int)(ButtonSize * 0.3f), (int)ButtonSize, (int)ButtonSize);
        }

        private static void UpdateTouch(GameWindow window)
        {
            _jumpPressedThisFrame = false;
            _pausePressedThisFrame = false;

            TouchCollection touches = TouchPanel.GetState();
            int screenWidth = window.ClientBounds.Width;

            bool moveTouchStillPresent = false;
            bool lookTouchStillPresent = false;

            foreach (TouchLocation touch in touches)
            {
                Vector2 pos = touch.Position;

                switch (touch.State)
                {
                    case TouchLocationState.Pressed:
                        // Tlačítka mají přednost před joystickem/lookem.
                        if (_jumpButtonBounds.Contains(pos))
                        {
                            _jumpPressedThisFrame = true;
                        }
                        else if (_runButtonBounds.Contains(pos))
                        {
                            _runToggleActive = !_runToggleActive; // přepínací tlačítko
                        }
                        else if (_pauseButtonBounds.Contains(pos))
                        {
                            _pausePressedThisFrame = true;
                        }
                        else if (pos.X < screenWidth * 0.5f && !_moveTouchId.HasValue)
                        {
                            // Levá půlka obrazovky = virtuální joystick pro pohyb
                            _moveTouchId = touch.Id;
                            _moveStartPos = pos;
                            _moveCurrentPos = pos;
                        }
                        else if (pos.X >= screenWidth * 0.5f && !_lookTouchId.HasValue)
                        {
                            // Pravá půlka = drag pro rozhlížení (nahrazuje MouseDelta)
                            _lookTouchId = touch.Id;
                            _lookLastPos = pos;
                        }
                        break;

                    case TouchLocationState.Moved:
                        if (_moveTouchId.HasValue && touch.Id == _moveTouchId.Value)
                        {
                            moveTouchStillPresent = true;
                            _moveCurrentPos = pos;
                        }
                        else if (_lookTouchId.HasValue && touch.Id == _lookTouchId.Value)
                        {
                            lookTouchStillPresent = true;
                            Vector2 delta = pos - _lookLastPos;
                            MouseDelta = delta; // stejné jméno property jako na desktopu -> Game1.cs se nemusí měnit
                            _lookLastPos = pos;
                        }
                        break;

                    case TouchLocationState.Released:
                    case TouchLocationState.Invalid:
                        if (_moveTouchId.HasValue && touch.Id == _moveTouchId.Value)
                        {
                            _moveTouchId = null;
                        }
                        if (_lookTouchId.HasValue && touch.Id == _lookTouchId.Value)
                        {
                            _lookTouchId = null;
                        }
                        break;
                }
            }

            // Pokud se prst na joysticku/looku "ztratil" (např. TouchPanel ho mezi
            // framy přeskočil rovnou na Released bez Moved), pojistka na vynulování:
            if (_moveTouchId.HasValue && !moveTouchStillPresent)
            {
                bool stillInList = false;
                foreach (var t in touches) if (t.Id == _moveTouchId.Value) stillInList = true;
                if (!stillInList) _moveTouchId = null;
            }
            if (_lookTouchId.HasValue && !lookTouchStillPresent)
            {
                bool stillInList = false;
                foreach (var t in touches) if (t.Id == _lookTouchId.Value) stillInList = true;
                if (!stillInList) _lookTouchId = null;
            }

            // Look delta se resetuje, pokud tenhle frame nebylo Moved pro look touch
            // (jinak by poslední hodnota "trčela" navěky a kamera by se točila sama).
            if (!lookTouchStillPresent) MouseDelta = Vector2.Zero;

            // Přepočet výsledného pohybového vektoru z joysticku (kruh, ne čtverec).
            if (_moveTouchId.HasValue)
            {
                Vector2 raw = _moveCurrentPos - _moveStartPos;
                float dist = raw.Length();
                if (dist > JoystickMaxRadius)
                    raw = raw / dist * JoystickMaxRadius;

                Vector2 normalized = raw / JoystickMaxRadius; // -1..1 na obou osách
                // Obrazovkové Y roste dolů -> tažení prstu NAHORU (záporné Y) = dopředu (kladné Y).
                _movementVector = new Vector2(normalized.X, -normalized.Y);
                if (_movementVector.LengthSquared() > 1f)
                    _movementVector.Normalize();
            }
            else
            {
                _movementVector = Vector2.Zero;
            }
        }

        /// <summary>
        /// Vycentruje kurzor myši (na touch platformách je to no-op).
        /// </summary>
        public static void CenterMouse(GameWindow window)
        {
            if (IsTouchPlatform) return; // Mouse.SetPosition na mobilu nedává smysl / může spadnout

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
            if (IsTouchPlatform) return _pausePressedThisFrame;
            return IsKeyPressed(Keys.Escape);
        }

        public static bool IsMenuConfirmPressed()
        {
            if (IsTouchPlatform) return _pausePressedThisFrame; // tap na pauza-tlačítko = potvrzení i v menu
            return IsKeyPressed(Keys.Enter) ||
                   IsKeyPressed(Keys.Space) ||
                   IsKeyPressed(Keys.Escape);
        }

        // ==========================================
        //  HERNÍ AKCE (POHYB A AKCE HRÁČE)
        // ==========================================

        public static bool IsJumpPressed()
        {
            if (IsTouchPlatform) return _jumpPressedThisFrame;
            return IsKeyPressed(Keys.Space);
        }

        public static bool IsRunning()
        {
            if (IsTouchPlatform) return _runToggleActive;
            return IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift);
        }

        /// <summary>
        /// Vrací směr pohybu na osách X a Y - z WASD na desktopu, z virtuálního
        /// joysticku na touch platformách. Rozhraní (návratový typ i význam os)
        /// je stejné, takže Game1.cs se nemusí vůbec měnit.
        /// </summary>
        public static Vector2 GetMovementVector()
        {
            if (IsTouchPlatform) return _movementVector;

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