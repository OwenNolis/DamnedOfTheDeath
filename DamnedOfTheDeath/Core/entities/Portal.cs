using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DamnedOfTheDeath.Core.interfaces;
using IUpdateable = DamnedOfTheDeath.Core.interfaces.IUpdateable;
using IDrawable = DamnedOfTheDeath.Core.interfaces.IDrawable;

namespace DamnedOfTheDeath.Core.entities
{
    public abstract class Portal : IUpdateable, IDrawable
    {
        protected Texture2D SpriteSheet;
        protected Rectangle[] Frames;
        protected int CurrentFrame;
        protected Vector2 Position;
        protected Rectangle Hitbox;
        private double TimeElapsed;
        private double TimeToUpdate;
        protected SpriteBatch SpriteBatch;
        protected Texture2D HitboxTexture;

        protected Portal(Texture2D spriteSheet, Rectangle[] frames, Vector2 position, double timeToUpdate, SpriteBatch spriteBatch, Texture2D hitboxTexture = null)
        {
            SpriteSheet = spriteSheet;
            Frames = frames;
            Position = position;
            TimeToUpdate = timeToUpdate;
            SpriteBatch = spriteBatch;
            HitboxTexture = hitboxTexture;
            Hitbox = new Rectangle((int)position.X, (int)position.Y, frames[0].Width, frames[0].Height);
        }

        public virtual void Update(GameTime gameTime)
        {
            TimeElapsed += gameTime.ElapsedGameTime.TotalSeconds;
            if (TimeElapsed >= TimeToUpdate)
            {
                TimeElapsed -= TimeToUpdate;
                CurrentFrame = (CurrentFrame + 1) % Frames.Length;
            }

            // Update hitbox position
            Hitbox.Location = Position.ToPoint();
        }

        public virtual void Draw()
        {
            SpriteBatch.Draw(SpriteSheet, Position, Frames[CurrentFrame], Color.White);

            // Draw the hitbox if texture is provided
            if (HitboxTexture != null)
            {
                //SpriteBatch.Draw(HitboxTexture, Hitbox, Color.Green * 0.5f);
            }
        }

        public Rectangle GetHitbox()
        {
            return Hitbox;
        }
    }
}
