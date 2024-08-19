using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamnedOfTheDeath.Core.entities
{
    public class PurplePortal : Portal
    {
        public PurplePortal(Texture2D spriteSheet, Rectangle[] frames, Vector2 position, double timeToUpdate, SpriteBatch spriteBatch, Texture2D hitboxTexture)
            : base(spriteSheet, frames, position, timeToUpdate, spriteBatch, hitboxTexture)
        {
        }
    }
}
