using Microsoft.Xna.Framework;

namespace open_world
{
    // Sem zapisují nativní Android View prvky (tlačítka, joystick) z Activity1.cs.
    // Input.cs si to jen čte - stejné jméno metod/vlastností jako předtím,
    // takže Game1.cs se nemusí měnit vůbec.
    public static class NativeInput
    {
        public static bool JumpPressedThisFrame; // nastaví OnClick, Input.cs po přečtení vynuluje
        public static bool ShootPressedThisFrame;
        public static bool PausePressedThisFrame;
        public static bool RunToggleActive;
        public static Vector2 MovementVector; // -1..1 na obou osách, píše JoystickView
    }
}