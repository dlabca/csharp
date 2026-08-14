using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace open_world
{
    public class DesktopShopUI
    {
        private List<Button> _buttons = new List<Button>();

        // NOVÉ: relativní Y offset každého tlačítka od panelu, drženo ODDĚLENĚ
        // od btn.Bounds. btn.Bounds se každý frame přepisuje na ABSOLUTNÍ pozici,
        // takže z něj nikdy nesmíš zpětně číst "kde bylo relativně" - proto ten
        // předchozí bug (kumulativní drift každý frame).
        private List<int> _buttonRelativeY = new List<int>();

        private Button _closeButton;
        private SpriteFont _font;
        private Texture2D _pixelTexture;

        public bool CloseRequested { get; private set; }

        private const int ButtonSpacing = 60;
        private const int HeaderHeight = 70;

        public DesktopShopUI(SpriteFont font, Texture2D pixelTexture)
        {
            _font = font;
            _pixelTexture = pixelTexture;

            int y = HeaderHeight;
            foreach (var upgrade in GameEconomy.Upgrades)
            {
                var btn = new Button(new Rectangle(0, 0, 360, 50), "");
                var capturedUpgrade = upgrade;
                btn.OnClick += () => capturedUpgrade.TryBuy();
                _buttons.Add(btn);
                _buttonRelativeY.Add(y); // <- zdroj pravdy pro pozici, ne btn.Bounds
                y += ButtonSpacing;
            }

            _closeButton = new Button(new Rectangle(0, 0, 40, 40), "X");
            _closeButton.OnClick += () => CloseRequested = true;
        }

        private (int panelX, int panelY, int panelWidth, int panelHeight) GetPanelRect(int viewportWidth, int viewportHeight)
        {
            int panelWidth = viewportWidth / 2;
            int panelHeight = viewportHeight / 2;
            int panelX = (viewportWidth - panelWidth) / 2;
            int panelY = (viewportHeight - panelHeight) / 2;
            return (panelX, panelY, panelWidth, panelHeight);
        }

        public void Update(MouseState mouse, int viewportWidth, int viewportHeight)
        {
            CloseRequested = false;

            var (panelX, panelY, panelWidth, _) = GetPanelRect(viewportWidth, viewportHeight);

            for (int i = 0; i < _buttons.Count; i++)
            {
                // Vždycky počítáno z NEMĚNNÉHO _buttonRelativeY[i], ne z btn.Bounds -
                // žádná šance na kumulativní drift, ať se tohle zavolá kolikrát chce.
                _buttons[i].Bounds = new Rectangle(panelX + 20, panelY + _buttonRelativeY[i], panelWidth - 40, 50);
                _buttons[i].Update(mouse);
            }

            _closeButton.Bounds = new Rectangle(panelX + panelWidth - 50, panelY + 10, 40, 40);
            _closeButton.Update(mouse);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D panelTexture, int viewportWidth, int viewportHeight)
        {
            var (panelX, panelY, panelWidth, panelHeight) = GetPanelRect(viewportWidth, viewportHeight);

            spriteBatch.Begin();

            spriteBatch.Draw(panelTexture, new Rectangle(panelX, panelY, panelWidth, panelHeight), new Color(20, 20, 30, 230));
            spriteBatch.DrawString(_font, $"OBCHOD   $ {GameEconomy.Money}", new Vector2(panelX + 20, panelY + 15), Color.White);
            //spriteBatch.DrawString(_font, "Esc", new Vector2(panelX + panelWidth - 45, panelY + 55), Color.Gray);

            for (int i = 0; i < _buttons.Count; i++)
            {
                var upgrade = GameEconomy.Upgrades[i];

                _buttons[i].Text = upgrade.CanBuy()
                    ? $"{upgrade.Name}: {upgrade.ValueText()}  ->  ${upgrade.NextCost()}"
                    : $"{upgrade.Name}: {upgrade.ValueText()}  (MAX)";

                _buttons[i].Draw(spriteBatch, _font, _pixelTexture);
            }

            _closeButton.Draw(spriteBatch, _font, _pixelTexture);

            spriteBatch.End();
        }
    }
}