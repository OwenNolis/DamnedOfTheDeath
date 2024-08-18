using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DamnedOfTheDeath.UI
{
    public class StartScreen
    {
        private SpriteFont _font;
        private Texture2D _buttonTexture;
        private Rectangle _startButtonRect;
        private Rectangle _level1ButtonRect;
        private Rectangle _level2ButtonRect;
        private MouseState _previousMouseState;

        public StartScreen(SpriteFont font, Texture2D buttonTexture)
        {
            _font = font;
            _buttonTexture = buttonTexture;

            // Position and size of the buttons
            _startButtonRect = new Rectangle(340, 300, 120, 50);  // Centered Start Button
            _level1ButtonRect = new Rectangle(200, 400, 120, 50);  // Level 1 Button
            _level2ButtonRect = new Rectangle(460, 400, 120, 50);  // Level 2 Button
        }

        public int Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            Point mousePosition = new Point(mouseState.X, mouseState.Y);

            // Check if Start Button is clicked
            if (_startButtonRect.Contains(mousePosition) && mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                return 1;  // Start game from level 1 by default
            }

            // Check if Level 1 Button is clicked
            if (_level1ButtonRect.Contains(mousePosition) && mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                return 2;  // Start game at level 1
            }

            // Check if Level 2 Button is clicked
            if (_level2ButtonRect.Contains(mousePosition) && mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                return 3;  // Start game at level 2
            }

            _previousMouseState = mouseState;

            return 0;  // No button clicked
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the title text
            string title = "Damned Of The Death";
            Vector2 titleSize = _font.MeasureString(title);
            Vector2 titlePosition = new Vector2((800 - titleSize.X) / 2, 200);  // Centered title
            spriteBatch.DrawString(_font, title, titlePosition, Color.White);

            // Draw the buttons
            spriteBatch.Draw(_buttonTexture, _startButtonRect, Color.White);
            spriteBatch.DrawString(_font, "Start", new Vector2(_startButtonRect.X + 20, _startButtonRect.Y + 15), Color.Black);

            spriteBatch.Draw(_buttonTexture, _level1ButtonRect, Color.White);
            spriteBatch.DrawString(_font, "Level 1", new Vector2(_level1ButtonRect.X + 10, _level1ButtonRect.Y + 15), Color.Black);

            spriteBatch.Draw(_buttonTexture, _level2ButtonRect, Color.White);
            spriteBatch.DrawString(_font, "Level 2", new Vector2(_level2ButtonRect.X + 10, _level2ButtonRect.Y + 15), Color.Black);
        }
    }
}

