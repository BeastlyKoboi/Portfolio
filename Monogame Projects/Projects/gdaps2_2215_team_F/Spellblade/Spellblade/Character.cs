using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Spellblade
{
    /// <summary>
    /// Class to inherit from GameObject and add 
    /// fields and behaviors for all characters
    /// </summary>
    abstract class Character : GameObject
    {
        // Fields:
        protected int health;
        protected int maxHealth;
        protected int damage;
        protected int moveSpeed;
        protected int attackSpeed;
        protected bool facingLeft;
        protected bool isMidair;
        protected bool prevMidair;
        protected double verticalVelocity;

        const double gravity = -9.81;

        // Fields to keep track of animations
        protected int frame;
        protected bool animated;
        protected int stateFrameCount;
        protected int timePerFrame;
        protected int timeCounter;
        protected Texture2D spriteSheet;
        protected bool drawCollisionBox;

        // Properties:
        public int Health
        {
            get { return health; }
            set { health = value; }
        }
        public int MaxHealth
        {
            get { return maxHealth; }
            set { maxHealth = value; }
        }
        public int Damage
        {
            get { return damage; }
            set { damage = value; }
        }
        public int MoveSpeed
        {
            get { return moveSpeed; }
        }
        public int AttackSpeed
        {
            get { return attackSpeed; }
        }
        public bool FacingLeft
        {
            get { return facingLeft; }
            set { facingLeft = value; }
        }
        public bool IsMidair
        {
            get { return isMidair; }
        }
        public bool PrevMidair
        {
            get { return prevMidair; }
        }
        public double VerticalVelocity
        {
            get { return verticalVelocity; }
        }

        //
        public bool DrawCollisionBox
        {
            get { return drawCollisionBox; }
            set { drawCollisionBox = value; }
        }

        // Constructor:
        public Character(int health, int damage, int moveSpeed, int attackSpeed,
            Texture2D texture, Texture2D spriteSheet, Rectangle position) : base(texture, position)
        {
            this.health = health;
            this.maxHealth = health;
            this.damage = damage;
            this.moveSpeed = moveSpeed;
            this.attackSpeed = attackSpeed;
            this.facingLeft = true;
            this.isMidair = false;
            this.prevMidair = false;
            this.verticalVelocity = 0.0;
            this.spriteSheet = spriteSheet;
            this.drawCollisionBox = false;
        }

        // Called by level renderer when a Character collides with any tangible
        // tiles. Takes in a list of tiles, the ones it is intersecting.
        public virtual void Gravity(List<Tile> tangibleTiles)
        {
            // Loops through the tangible tiles and checks if the character
            // collides to either side and corrects the positioning if they do.
            isMidair = true;
            for (int i = 0; i < tangibleTiles.Count; i++)
            {
                while (CollideLeft(tangibleTiles[i]) &&
                    !CollideUp(tangibleTiles[i]) &&
                    !CollideDown(tangibleTiles[i]))
                {
                    position.X += 1;
                }

                while (CollideRight(tangibleTiles[i]) &&
                    !CollideUp(tangibleTiles[i]) &&
                    !CollideDown(tangibleTiles[i]))
                {
                    position.X -= 1;
                }

                // If character intersects a tile to the bottom, it sets midair
                // to false.
                if (tangibleTiles[i].Position.Intersects(
                    new Rectangle(Position.X + 1,
                    Position.Y + Position.Height + 1, Position.Width - 2, 1)))
                {
                    isMidair = false;
                    verticalVelocity = 0.0;

                    while (tangibleTiles[i].Position.Intersects(
                        new Rectangle(Position.X + 1,
                        Position.Y + Position.Height, Position.Width - 2, 1)) &&
                        prevMidair)
                    {
                        position.Y -= 1;
                    }
                }

                // If character intersects a tile to the top, sets
                // verticalVelocity to 0 if it was positive.
                if (tangibleTiles[i].Position.Intersects(
                    new Rectangle(Position.X + 1,
                    Position.Y - 1, Position.Width - 2, 1)) && verticalVelocity > 0)
                {
                    verticalVelocity = 0;

                    while (tangibleTiles[i].Position.Intersects(
                        new Rectangle(Position.X + 1,
                        Position.Y - 1, Position.Width - 2, 1)))
                    {
                        position.Y += 1;
                    }
                }
            }

            // If isMidair is true, increase the velocity of the character by
            // gravity.
            if (IsMidair)
            {
                verticalVelocity = verticalVelocity + (gravity * (1.0 / 60.0));

                position.Y -= (int)Math.Round(verticalVelocity);
            }

            prevMidair = isMidair;
        }

        // Takes in a tile and returns true if the character's top intersects
        // it and false otherwise.
        public bool CollideUp(Tile tangibleTile)
        {
            if (tangibleTile.Position.Intersects(new Rectangle(Position.X + 2,
                Position.Y, Position.Width - 4, 1)))
            {
                return true;
            }
            return false;
        }

        // Takes in a tile and returns true if the character's bottom intersects
        // it and false otherwise.
        public bool CollideDown(Tile tangibleTile)
        {
            if (tangibleTile.Position.Intersects(new Rectangle(Position.X + 2,
                Position.Y + Position.Height, Position.Width - 4, 1)))
            {
                return true;
            }
            return false;
        }

        // Takes in a tile and returns true if the character's left side
        // intersects it and false otherwise.
        public bool CollideLeft(Tile tangibleTile)
        {
            if (tangibleTile.Position.Intersects(new Rectangle(
                Position.X, Position.Y + 2, 1, Position.Height - 4)))
            {
                return true;
            }
            return false;
        }

        // Takes in a tile and returns true if the character's right side
        // intersects it and false otherwise.
        public bool CollideRight(Tile tangibleTile)
        {
            if (tangibleTile.Position.Intersects(
                new Rectangle(Position.X + Position.Width, Position.Y + 2, 1,
                Position.Height - 4)))
            {
                return true;
            }
            return false;
        }

        // Move:
        public abstract void Move();

        // Attack:
        public abstract void Attack();

        // Take Damage:
        public virtual void TakeDamage(int damage)
        {
            this.health -= damage;
        }
    }
}