using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Spellblade
{
    /// <summary>
    /// Class to inherit from game object and add the fields and behaviors of a projectile. 
    /// </summary>
    class Projectile : GameObject
    {
        // Fields:
        protected int damage;
        protected int windowWidth;
        protected bool active;
        protected bool playerProjectile;
        protected bool left;

        protected const int speed = 8;

        // Fields to keep track of animations
        protected int frame;
        protected bool animated;
        protected int stateFrameCount;
        protected int timePerFrame;
        protected int timeCounter;
        protected Texture2D spriteSheet;
        protected bool drawCollisionBox;

        //      Animation Fields      //
        // Constants for each frame in the tile sheet
        const int projectileSpriteWidth = 80;
        const int projectileSpriteHeight = 29;

        // Constants for draw offset from position bottom middle
        const int projectileSpriteOriginX = 0;
        const int projectileSpriteOriginY = 27;

        // Properties:
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }
        public int Damage
        {
            get { return damage; }
        }
        public bool PlayerProjectile
        {
            get { return playerProjectile; }
        }
        public int Frame
        {
            get { return frame; }
        }

        // Constructor:
        public Projectile(int damage, bool left, int windowWidth,
            bool playerProjectile, Texture2D texture, Rectangle position) :
            base(texture, new Rectangle(position.X,
                position.Y - (position.Height / 2), position.Width, position.Height))
        {
            this.damage = damage;
            this.left = left;
            this.windowWidth = windowWidth;
            this.playerProjectile = playerProjectile;
            this.active = true;
            frame = 0;
            stateFrameCount = 8;
            timePerFrame = 5;
            timeCounter = 0;
            spriteSheet = texture;
        }

        // Checks if the projectile has collided with a given GameObject. Returns true if it has
		// (and deactivates the projectile), and returns false if it hasn't.
        public bool CheckCollision(GameObject check)
        {
            if (active && Position.Intersects(check.Position))
            {
                active = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        // Swaps the direction the projectile is facing.
        public void SwapDirection()
        {
            left = !left;
        }

        // Changes the control of the projectile (from the player's control to
        // the enemies' or visa versa).
        public void SwapControl()
        {
            playerProjectile = !playerProjectile;
        }

        // Moves the projectile along the x-axis based on the given speed, so it
        // stays in the same position as the background moves.ß
        public void LevelMove(int speed)
        {
            position = new Rectangle(Position.X + speed, Position.Y, Position.Width, Position.Height);
        }

        // Moves the projectile and deactivates it when it hits a side.
        public override void Update()
        {
            if (left)
            {
                position = new Rectangle(Position.X - speed, Position.Y, Position.Width,
                    Position.Height);
                if (Position.X < 0)
                {
                    active = false;
                }
            }
            else
            {
                position = new Rectangle(Position.X + speed, Position.Y, Position.Width,
                    Position.Height);
                if (Position.X > windowWidth)
                {
                    active = false;
                }
            }

            UpdateAnimation();
        }

        // Update the the current animation frame based on state and time spent.
        private void UpdateAnimation()
        {

            if (active)
            {
                timeCounter++;

                if (timeCounter >= timePerFrame)
                {
                    frame++;

                    if (frame > stateFrameCount - 1)
                    {
                        frame = 0;
                    }

                    timeCounter -= timePerFrame;
                }
            }

        }

        // Draws the projectile if its active.
        public override void Draw(SpriteBatch sb)
        {
            if (active && left)
            {
                //base.Draw(sb);
                sb.Draw(
                            spriteSheet,
                            new Vector2(position.X + 8, position.Y + position.Height),
                            new Rectangle(
                                frame * projectileSpriteWidth,
                                0,
                                projectileSpriteWidth,
                                projectileSpriteHeight),
                            Color.White,
                            0,
                            new Vector2(projectileSpriteOriginX, projectileSpriteOriginY),
                            1f,
                            SpriteEffects.FlipHorizontally,
                            1);
            }
            else if (active && !left)
            {
                //base.Draw(sb);
                sb.Draw(
                            spriteSheet,
                            new Vector2(position.X - position.Width - 8, position.Y + position.Height),
                            new Rectangle(
                                frame * projectileSpriteWidth,
                                0,
                                projectileSpriteWidth,
                                projectileSpriteHeight),
                            Color.White,
                            0,
                            new Vector2(projectileSpriteOriginX, projectileSpriteOriginY),
                            1f,
                            SpriteEffects.None,
                            1);
            }
        }
    }
}
