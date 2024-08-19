using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamnedOfTheDeath.Core.enemies
{
    public class Alien
    {
        private Texture2D spritesheet;
        private int frameWidth = 32;  // Width of a single frame (224 / 4 columns)
        private int frameHeight = 32; // Height of a single frame (128 / 4 rows)
        private int currentFrame = 0;
        private float timer = 0f;
        private float interval = 0.1f; // Frame interval
        private Vector2 position;
        private Vector2[] path;
        private int currentPathIndex = 0;
        private float speed; // Speed in pixels per second
        private bool moving = true;

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

        // Constructor with the correct parameters
        public Alien(Texture2D spritesheet, Vector2[] path, float speed)
        {
            this.spritesheet = spritesheet;
            this.path = path;
            this.speed = speed;
            this.position = path[0]; // Start position
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
                if (currentFrame >= 4) // 4 frames in the movement row
                    currentFrame = 0;
            }

            // Update position along the path
            if (path.Length < 2) return; // Ensure there are at least 2 points to move between

            Vector2 startPoint = path[currentPathIndex];
            Vector2 endPoint = path[(currentPathIndex + 1) % path.Length];

            // Calculate distance to travel this frame
            float distanceToTravel = speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            float distanceToEnd = Vector2.Distance(position, endPoint);

            if (distanceToTravel >= distanceToEnd)
            {
                position = endPoint;
                currentPathIndex = (currentPathIndex + 1) % path.Length;
            }
            else
            {
                // Move towards the endPoint
                Vector2 direction = endPoint - position;
                direction.Normalize();
                position += direction * distanceToTravel;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int row = 0; // Movement stage (first row)
            Rectangle sourceRectangle = new Rectangle(currentFrame * frameWidth, row * frameHeight, frameWidth, frameHeight);
            spriteBatch.Draw(spritesheet, position, sourceRectangle, Color.White);
        }
    }
}
