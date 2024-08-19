using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamnedOfTheDeath.Core.entities
{
    public class GreenPortal : Portal
    {
        public GreenPortal(Texture2D spriteSheet, Rectangle[] frames, Vector2 position, double timeToUpdate, SpriteBatch spriteBatch, Texture2D hitboxTexture)
            : base(spriteSheet, frames, position, timeToUpdate, spriteBatch, hitboxTexture)
        {
        }

        // No additional code needed if behavior is identical to other portals
    }
}
