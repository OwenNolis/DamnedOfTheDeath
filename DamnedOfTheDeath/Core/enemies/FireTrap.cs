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
        private Texture2D _hitboxFireTrapTexture;
        private int _frameWidth = 32; // 448px / 14 columns
        private int _frameHeight = 41; // 41px / 1 rows
        private int _currentFireTrapFrame;
        private int _totalFireTrapFrames;
        private float _frameTimeFireTrap;
        private float _elapsedFireTrapTime;

        public Vector2 _fireTrapPosition { get; set; }

        public FireTrap(Texture2D texture, int frameWidth, int frameHeight, int totalFrames, float frameTime)
        {
            _hitboxFireTrapTexture = texture;
            _frameWidth = frameWidth;
            _frameHeight = frameHeight;
            _totalFireTrapFrames = totalFrames;
            _frameTimeFireTrap = frameTime;
            _elapsedFireTrapTime = 0f;
            _currentFireTrapFrame = 0;
        }
        public void Update(GameTime gameTime)
        {
            _elapsedFireTrapTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_elapsedFireTrapTime >= _frameTimeFireTrap)
            {
                _elapsedFireTrapTime -= _frameTimeFireTrap;
                _currentFireTrapFrame = (_currentFireTrapFrame + 1) % _totalFireTrapFrames;
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            int row = 0; // For a single row spritesheet
            int column = _currentFireTrapFrame;
            Rectangle sourceRectangle = new Rectangle(column * _frameWidth, row * _frameHeight, _frameWidth, _frameHeight);
            spriteBatch.Draw(_hitboxFireTrapTexture, _fireTrapPosition, sourceRectangle, Color.White);
        }
        public Rectangle GetHitbox()
        {
            return new Rectangle((int)_fireTrapPosition.X, (int)_fireTrapPosition.Y, _frameWidth, _frameHeight);
        }
    }
}
