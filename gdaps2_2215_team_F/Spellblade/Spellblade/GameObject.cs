using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Text;

namespace Spellblade
{
    /// <summary>
    /// Abstract to hold the asset and position/size of a game object,
    /// some basic behaviors and be extended through child classes. 
    /// </summary>
    abstract class GameObject
    {
        // Fields to hold the game object's asset and position
        protected Texture2D asset;
        protected Rectangle position;

        /// <summary>
        /// Protected parameterized constructor to initialize game object fields, 
        /// but only when called by a derived class.
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="position"></param>
        protected GameObject(Texture2D asset, Rectangle position)
        {
            this.asset = asset;
            this.position = position;
        }

        /// <summary>
        /// Property to make the position rectangle read-only from outside the derived object.
        /// </summary>
        public Rectangle Position
        {
            get { return position; }
        }

        /// <summary>
        /// Virtual method to draw the state of the GameObject, or be overridden. 
        /// </summary>
        /// <param name="sb"></param>
        public virtual void Draw(SpriteBatch sb)
        {
            sb.Draw(asset, position, Color.White);
        }

        /// <summary>
        /// Abstract method to be overidden in each child class, so they can update themselves.
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Update();

    }
}
