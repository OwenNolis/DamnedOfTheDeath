using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DamnedOfTheDeath.UI
{
    public class ScoreOverlay
    {
        private SpriteFont _font;
        private Vector2 _scorePosition;
        private int _score;
        private int MaxScore = 10;

        public ScoreOverlay(SpriteFont font, Vector2 scorePosition)
        {
            _font = font;
            _scorePosition = scorePosition;
            _score = 0;
        }

        public void UpdateScore(int newScore)
        {
            _score = newScore;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, $"Score: {_score}/{MaxScore}", _scorePosition, Color.IndianRed);
        }
    }
}
