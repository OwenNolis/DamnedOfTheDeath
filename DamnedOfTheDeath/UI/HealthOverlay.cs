using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DamnedOfTheDeath.UI
{
    public class HealthOverlay
    {
        private SpriteFont _font;
        private Vector2 _healthPosition;
        private int _health;

        public HealthOverlay(SpriteFont font, Vector2 healthPosition)
        {
            _font = font;
            _healthPosition = healthPosition;
            _health = 3;
        }

        public void UpdateHealth(int newHealth)
        {
            _health = newHealth;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, $"Health: {_health}", _healthPosition, Color.IndianRed);
        }
    }
}
