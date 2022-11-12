using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AMAZEingGameV2
{
    /// <summary>
    /// Class to hold teh basic information of a game object
    /// </summary>
    public abstract class GameObject
    {
        // Fields to hold the texture and position of this GameObject
        protected Texture2D Asset { get; set; }
        protected Rectangle position;

        /// <summary>
        /// Constructor to give GameObject its starting asset and position
        /// </summary>
        /// <param name="Asset"></param>
        /// <param name="Position"></param>
        protected GameObject(Texture2D Asset, Rectangle Position)
        {
            this.Asset = Asset;
            this.position = Position;
        }

        /// <summary>
        /// Property to make the position read-only
        /// </summary>
        public Rectangle Position
        {
            get { return position; }
        }

        /// <summary>
        /// Method to draw the GameObject, or to be overidden later
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Asset, Position, Color.White);
        }

        /// <summary>
        /// Method to be overidden to update the child version of this object
        /// </summary>
        public abstract void Update();

    }
}
