using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace open_world
{
    // Prochází GameEconomy.Upgrades a vygeneruje pro každý jedno tlačítko -
    // přidáš nový Upgrade do GameEconomy, tady se objeví automaticky, žádná
    // změna v tomhle souboru není potřeba.
    public class DesktopShopUI
    {
        private List<Button> _buttons = new List<Button>();
        private SpriteFont _font;
        private Texture2D _pixelTexture;

        public DesktopShopUI(SpriteFont font, Texture2D pixelTexture)
        {
            _font = font;
            _pixelTexture = pixelTexture;

            int y = 110;
            foreach (var upgrade in GameEconomy.Upgrades)
            {
                var btn = new Button(new Rectangle(40, y, 360, 50), "");
                var capturedUpgrade = upgrade; // closure - důležité, ať každé tlačítko koupí SVŮJ upgrade
                btn.OnClick += () => capturedUpgrade.TryBuy();
                _buttons.Add(btn);
                y += 60;
            }
        }

        public void Update(MouseState mouse)
        {
            foreach (var btn in _buttons) btn.Update(mouse);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D panelTexture)
        {
            spriteBatch.Begin();

            int panelHeight = _buttons.Count * 60 + 70;
            spriteBatch.Draw(panelTexture, new Rectangle(20, 60, 400, panelHeight), new Color(20, 20, 30, 220));
            spriteBatch.DrawString(_font, $"OBCHOD   $ {GameEconomy.Money}", new Vector2(40, 75), Color.White);
            spriteBatch.DrawString(_font, "(B = zpět do hry)", new Vector2(40, 95), Color.Gray);

            for (int i = 0; i < _buttons.Count; i++)
            {
                var upgrade = GameEconomy.Upgrades[i];

                _buttons[i].Text = upgrade.CanBuy()
                    ? $"{upgrade.Name}: {upgrade.ValueText()}  ->  ${upgrade.NextCost()}"
                    : $"{upgrade.Name}: {upgrade.ValueText()}  (MAX)";

                _buttons[i].Draw(spriteBatch, _font, _pixelTexture);
            }

            spriteBatch.End();
        }
    }
}
