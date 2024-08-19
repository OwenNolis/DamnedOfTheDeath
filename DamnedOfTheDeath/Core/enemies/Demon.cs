using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamnedOfTheDeath.Core.enemies
{
    public class Demon
    {
        private Texture2D spritesheet;
        private int frameWidth = 64;  // Width of a single frame (512 / 6 columns)
        private int frameHeight = 64; // Height of a single frame (256 / 4 rows)
        private int currentFrame = 0;
        private float timer = 0f;
        private float interval = 0.1f; // Frame interval
        private Vector2 position;
        private Vector2[] path;
        private int currentPathIndex = 0;
        private float speed; // Speed in pixels per second
        private bool moving = true;

        // Hitbox of the demon
        public Rectangle Hitbox
        {
            get
            {
                return new Rectangle(
                    (int)position.X,
                    (int)position.Y,
                    frameWidth,
                    frameHeight
                );
            }
        }

        public Demon(Texture2D spritesheet, Vector2 startPosition, Vector2[] path, float speed)
        {
            this.spritesheet = spritesheet;
            this.position = startPosition;
            this.path = path;
            this.speed = speed;
        }

        public void Update(GameTime gameTime)
        {
            if (!moving) return;

            // Update the frame animation
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer >= interval)
            {
                timer = 0f;
                currentFrame++;
                if (currentFrame >= 6) // 6 frames in the movement row
                    currentFrame = 0;
            }

            // Update position along the path
            Vector2 target = path[currentPathIndex];
            Vector2 direction = target - position;
            float distance = direction.Length();

            if (distance < speed * (float)gameTime.ElapsedGameTime.TotalSeconds)
            {
                position = target;
                currentPathIndex = (currentPathIndex + 1) % path.Length;
                target = path[currentPathIndex];
            }
            else
            {
                direction.Normalize();
                position += direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int row = 1; // Movement stage (second row)
            Rectangle sourceRectangle = new Rectangle(currentFrame * frameWidth, row * frameHeight, frameWidth, frameHeight);
            spriteBatch.Draw(spritesheet, position, sourceRectangle, Color.White);
        }
    }
}