using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamnedOfTheDeath.Core.Collectibles
{
    public class Coin
    {
        private Texture2D _hitboxCoinTexture;
        private int _frameWidth = 20; // 180px / 9 columns
        private int _frameHeight = 20; // 20px / 1 rows
        private int _currentFrame;
        private int _totalFrames;
        private float _frameTime;
        private float _elapsedTime;

        public Vector2 Position { get; set; }
        public bool IsVisible { get; set; } = true; // Property to manage visibility

        public Coin(Texture2D texture, int frameWidth, int frameHeight, int totalFrames, float frameTime)
        {
            _hitboxCoinTexture = texture;
            _frameWidth = frameWidth;
            _frameHeight = frameHeight;
            _totalFrames = totalFrames;
            _frameTime = frameTime;
            _elapsedTime = 0f;
            _currentFrame = 0;
        }

        public void Update(GameTime gameTime)
        {
            if (!IsVisible) return;

            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_elapsedTime >= _frameTime)
            {
                _elapsedTime -= _frameTime;
                _currentFrame = (_currentFrame + 1) % _totalFrames;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            int row = 0; // For a single row spritesheet
            int column = _currentFrame;
            Rectangle sourceRectangle = new Rectangle(column * _frameWidth, row * _frameHeight, _frameWidth, _frameHeight);
            spriteBatch.Draw(_hitboxCoinTexture, Position, sourceRectangle, Color.White);
        }

        public Rectangle GetHitbox()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, _frameWidth, _frameHeight);
        }
    }
}
