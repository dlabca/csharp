using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace open_world
{
    public class Button
    {
        public Rectangle Bounds { get; set; }
        public string Text { get; set; }
        
        // Událost, která se zavolá při kliknutí
        public event Action OnClick;

        private bool _isHovered;
        private bool _isPressed;

        public Button(Rectangle bounds, string text)
        {
            Bounds = bounds;
            Text = text;
        }

        public void Update(MouseState currentMouseState)
        {
            // Na touch platformách ignorujeme MouseState úplně a řešíme to přes
            // Input.IsButtonTapped() - viz níže. Parametr necháváme kvůli zpětné
            // kompatibilitě volání z Game1.cs (na desktopu funguje jako dřív).
            if (Input.IsTouchPlatform)
            {
                UpdateTouch();
                return;
            }

            Point mousePos = new Point(currentMouseState.X, currentMouseState.Y);

            // Kontrola, zda je kurzor myši uvnitř tlačítka
            _isHovered = Bounds.Contains(mousePos);

            if (_isHovered)
            {
                // Zjistíme, zda uživatel stiskl a pustil levé tlačítko myši (tzv. Click)
                if (currentMouseState.LeftButton == ButtonState.Pressed)
                {
                    _isPressed = true;
                }
                else if (_isPressed && currentMouseState.LeftButton == ButtonState.Released)
                {
                    _isPressed = false;
                    OnClick?.Invoke(); // Vyvolání akce tlačítka
                }
            }
            else
            {
                _isPressed = false;
            }
        }

        // Touch verze - tap kdekoliv uvnitř Bounds = klik (na dotykovém displeji
        // nedává smysl rozlišovat "hover", takže se to zjednoduší na press+release uvnitř).
        private void UpdateTouch()
        {
            _isHovered = false;
            _isPressed = false;

            foreach (var touch in Microsoft.Xna.Framework.Input.Touch.TouchPanel.GetState())
            {
                Point touchPos = new Point((int)touch.Position.X, (int)touch.Position.Y);

                if (!Bounds.Contains(touchPos)) continue;

                if (touch.State == Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Pressed ||
                    touch.State == Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Moved)
                {
                    _isPressed = true;
                    _isHovered = true;
                }
                else if (touch.State == Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Released)
                {
                    OnClick?.Invoke();
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
        {
            // Změna barvy podle stavu (vymezení: normální / najetí myší / stisknutí)
            Color backgroundColor = Color.DarkSlateGray;
            if (_isPressed)
                backgroundColor = Color.Black;
            else if (_isHovered)
                backgroundColor = Color.DimGray;

            // 1. Nakreslení pozadí tlačítka
            spriteBatch.Draw(pixelTexture, Bounds, backgroundColor);

            // 2. Nakreslení rámečku tlačítka (volitelné)
            Color borderColor = _isHovered ? Color.Yellow : Color.White;
            DrawBorder(spriteBatch, pixelTexture, Bounds, 2, borderColor);

            // 3. Vycentrování textu uvnitř tlačítka
            if (font != null && !string.IsNullOrEmpty(Text))
            {
                Vector2 textSize = font.MeasureString(Text);
                Vector2 textPosition = new Vector2(
                    MathF.Round(Bounds.X + (Bounds.Width - textSize.X) / 2),
                    MathF.Round(Bounds.Y + (Bounds.Height - textSize.Y) / 2)
                );

                spriteBatch.DrawString(font, Text, textPosition, Color.White);
            }
        }

        private void DrawBorder(SpriteBatch spriteBatch, Texture2D pixel, Rectangle rect, int thickness, Color color)
        {
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color); // Horní
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color); // Dolní
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color); // Levá
            spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color); // Pravá
        }
    }
}