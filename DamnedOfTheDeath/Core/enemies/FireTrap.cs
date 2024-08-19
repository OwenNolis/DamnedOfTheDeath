using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DamnedOfTheDeath.Core.enemies
{
    public class FireTrap
    {
        private readonly Texture2D _texture;
        private readonly int _frameWidth;
        private readonly int _frameHeight;
        private readonly int _totalFrames;
        private readonly float _frameTime;
        private float _elapsedTime;
        private int _currentFrame;

        public Vector2 Position { get; set; }

        public FireTrap(Texture2D texture, int frameWidth, int frameHeight, int totalFrames, float frameTime)
        {
            _texture = texture;
            _frameWidth = frameWidth;
            _frameHeight = frameHeight;
            _totalFrames = totalFrames;
            _frameTime = frameTime;
            _elapsedTime = 0f;
            _currentFrame = 0;
        }

        public void Update(GameTime gameTime)
        {
            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_elapsedTime >= _frameTime)
            {
                _elapsedTime -= _frameTime;
                _currentFrame = (_currentFrame + 1) % _totalFrames;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRectangle = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);
            spriteBatch.Draw(_texture, Position, sourceRectangle, Color.White);
        }

        public Rectangle GetHitbox()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, _frameWidth, _frameHeight);
        }
    }
}
