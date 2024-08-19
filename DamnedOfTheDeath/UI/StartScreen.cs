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
        private SpriteFont _titleFont;
        private Texture2D _buttonTexture;
        private Texture2D _backgroundTexture;
        private Rectangle _startButtonRect;
        private Rectangle _level1ButtonRect;
        private Rectangle _level2ButtonRect;
        private MouseState _previousMouseState;

        public StartScreen(SpriteFont font, SpriteFont titleFont, Texture2D buttonTexture, Texture2D backgroundTexture)
        {
            _font = font;
            _titleFont = titleFont;
            _buttonTexture = buttonTexture;
            _backgroundTexture = backgroundTexture;

            // Position and size of the buttons
            _startButtonRect = new Rectangle(800, 240, 120, 50);  // Centered Start Button
            _level1ButtonRect = new Rectangle(700, 350, 120, 50);  // Level 1 Button
            _level2ButtonRect = new Rectangle(900, 350, 120, 50);  // Level 2 Button
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
            // Draw the background
            spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, 1600, 480), Color.White);

            // Draw the title text
            string title = "Damned Of The Death";
            Vector2 titleSize = _titleFont.MeasureString(title);
            Vector2 titlePosition = new Vector2((1700 - titleSize.X) / 2, 100);  // Centered title
            spriteBatch.DrawString(_titleFont, title, titlePosition, Color.Black);

            // Draw the buttons
            spriteBatch.Draw(_buttonTexture, _startButtonRect, Color.IndianRed);
            spriteBatch.DrawString(_font, "Start", new Vector2(_startButtonRect.X + 30, _startButtonRect.Y + 10), Color.Black);

            spriteBatch.Draw(_buttonTexture, _level1ButtonRect, Color.IndianRed);
            spriteBatch.DrawString(_font, "Level 1", new Vector2(_level1ButtonRect.X + 30, _level1ButtonRect.Y + 10), Color.Black);

            spriteBatch.Draw(_buttonTexture, _level2ButtonRect, Color.IndianRed);
            spriteBatch.DrawString(_font, "Level 2", new Vector2(_level2ButtonRect.X + 30, _level2ButtonRect.Y + 10), Color.Black);
        }
    }
}

