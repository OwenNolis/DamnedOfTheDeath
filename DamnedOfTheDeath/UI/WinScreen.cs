using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamnedOfTheDeath.UI
{
    public class WinScreen
    {
        private SpriteFont _font;
        private SpriteFont _titleFont;
        private Texture2D _buttonTexture;
        private Texture2D _backgroundTexture;
        private Vector2 _victoryPosition;
        private Rectangle _buttonRectangle;
        private Vector2 _buttonTextPosition;
        private bool _isButtonHovered;

        public WinScreen(SpriteFont font, SpriteFont titleFont, Texture2D buttonTexture, GraphicsDevice graphicsDevice, Texture2D backgroundTexture)
        {
            _font = font;
            _titleFont = titleFont;
            _buttonTexture = buttonTexture;
            _backgroundTexture = backgroundTexture;
            int screenWidth = graphicsDevice.Viewport.Width;
            int screenHeight = graphicsDevice.Viewport.Height;

            // Position "Victory" in the center of the screen
            _victoryPosition = new Vector2(screenWidth / 2 - font.MeasureString("Victory").X / 2, screenHeight / 2 - 100);

            // Create a rectangle for the button
            _buttonRectangle = new Rectangle(screenWidth / 2 - 100, screenHeight / 2, 200, 50);

            // Position the button text in the center of the button
            _buttonTextPosition = new Vector2(
                _buttonRectangle.X + (_buttonRectangle.Width / 2) - (font.MeasureString("Play Again").X / 2),
                _buttonRectangle.Y + (_buttonRectangle.Height / 2) - (font.MeasureString("Play Again").Y / 2)
            );
        }

        public bool Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();

            _isButtonHovered = _buttonRectangle.Contains(mouseState.Position);

            if (_isButtonHovered && mouseState.LeftButton == ButtonState.Pressed)
            {
                return true; // Indicate that the button was clicked
            }

            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the background
            spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, 1600, 480), Color.White);

            spriteBatch.DrawString(_titleFont, "Victory", _victoryPosition, Color.Black);

            // Draw the button
            Color buttonColor = _isButtonHovered ? Color.DarkGreen : Color.White;
            spriteBatch.Draw(CreateRectangleTexture(spriteBatch.GraphicsDevice, _buttonRectangle.Width, _buttonRectangle.Height, buttonColor), _buttonRectangle, Color.IndianRed);

            // Draw the button text
            spriteBatch.DrawString(_font, "Play Again", _buttonTextPosition, Color.Black);
        }

        private Texture2D CreateRectangleTexture(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; ++i) data[i] = color;
            texture.SetData(data);
            return texture;
        }
    }
}
