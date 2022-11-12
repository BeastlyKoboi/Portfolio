using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Spellblade
{
    // Defines states for basic enemies
    enum EnemyState { Idle, Attacking, Walking, TakeDamage, Dying }
    
    /// <summary>
    /// Class to inherit from Character and add fields
    /// and behaviors to representa a basic enemy
    /// </summary>
    abstract class Enemy : Character
    {
        protected Player player;
        protected int range;
        protected int attackCooldown;
        protected EnemyState enemyState;
        protected bool recentlyAttacked;

        public Enemy(Player player, int range, int health, int damage, int moveSpeed, int attackSpeed,
            Texture2D texture, Texture2D spriteSheet, Rectangle position) : base(health, damage,
            moveSpeed, attackSpeed, texture, spriteSheet, position)
        {
            this.player = player;
            this.range = range;
            this.attackCooldown = 0;
            this.recentlyAttacked = false;
        }

        public Player Player
        {
            get { return player; }
        }

        public int Range
        {
            get { return range; }
        }

        public int AttackCooldown
        {
            get { return attackCooldown; }
        }

        // Moves the enemy to the side based on a given speed so as to keep them
        // lined up when the LevelRenderer moves the background.
        public void LevelMove(int speed)
        {
            position = new Rectangle(Position.X + speed, Position.Y, Position.Width, Position.Height);
        }

        // Public method to move 1 to the horizontal in the opposite direction
        // this is facing. Used for collision with other enemies.
        public void HorizontalMove()
        {
            if (FacingLeft)
            {
                position.X += 1;
            }
            else
            {
                position.X -= 1;
            }
        }

        public virtual void EnemySuicidePrevention(List<Tile>[] tangibleTiles)
        {
            // Most enemies do not need this method, and so it will not be
            // implemented for them.
        }
    }
}
