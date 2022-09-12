using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Text;

namespace Spellblade
{
    /// <summary>
    /// Class to represent a tile on the window
    /// </summary>
    class Tile : GameObject
    {
        // Field to hold which part of the Tile Sheet to draw
        private Rectangle textureRect;

        //
        public Rectangle TextureRect
        {
            get { return textureRect; }
            set { textureRect = value; }
        }

        /// <summary>
        /// Parameterized constructor to call base constructor and
        /// save the location of the texture in the asset
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="position"></param>
        /// <param name="textureRect"></param>
        public Tile(Texture2D asset, Rectangle position, Rectangle textureRect)
            : base(asset, position)
        {
            this.textureRect = textureRect;
        }

        //
        public override void Update()
        {
            
        }

        /// <summary>
        /// Method to update the the x position of the tile
        /// </summary>
        /// <param name="xChange"></param>
        public void Update(int offset)
        {
            position.X += offset;
        }

        /// <summary>
        /// Method to override Draw to draw the current tile to the window 
        /// </summary>
        /// <param name="sb"></param>
        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(asset, position, textureRect, Color.White);
        }

    }
}
