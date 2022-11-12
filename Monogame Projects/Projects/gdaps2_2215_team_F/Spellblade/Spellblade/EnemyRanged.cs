using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Spellblade
{
    /// <summary>
    /// Class to add the behavior of a ranged enemy to the basic enemy
    /// </summary>
    class EnemyRanged : Enemy
    {
        private ProjectileManager projectileManager;

        //      Animation Fields      //
        // Constants for each frame in the tile sheet
        const int spriteWidth = 48;
        const int spriteHeight = 48;

        // Constants for draw offset from position bottom middle
        const int spriteOriginX = 19;
        const int spriteOriginY = 44;

        public EnemyRanged(ProjectileManager projectileManager, Player player,
            int range, int health, int damage, int moveSpeed, int attackSpeed,
            Texture2D texture, Texture2D spriteSheet, Rectangle position) : base(player, range, health,
            damage, moveSpeed, attackSpeed, texture, spriteSheet, position)
        {
            this.projectileManager = projectileManager;
            frame = 0;
            animated = true;
            stateFrameCount = 4;
            timePerFrame = 10;
            timeCounter = 0;
        }

        public override void Attack()
        {
            // Actual attack is done later when it's frame occurs.
            recentlyAttacked = false;
            attackCooldown = attackSpeed;
        }

        // This ranged enemy does not move.
        public override void Move()
        {
            // Not currently implemented
        }

        public override void Update()
        {
            // If the enemy's attack cooldown is less than or equal to 0, it
            // checks if the player is within range on the x-axis and within
            // sight on the y-axis. If so, it goes to the Attacking state and
            // begins the animation to shoot a projectile at the player.
            if (AttackCooldown <= 0)
            {
                // If player is in the attack range to the left.
                if (Position.X - player.Position.X <= range &&
                    Position.X >= player.Position.X && Position.Y + Position.Height >=
                    player.Position.Y + (player.Position.Height / 2) &&
                    Position.Y <= player.Position.Y + (player.Position.Height / 2))
                {
                    facingLeft = true;
                    enemyState = EnemyState.Attacking;
                    Attack();
                }

                // If player is in the attack range to the right.
                else if (player.Position.X - Position.X <= range &&
                    player.Position.X > Position.X && Position.Y + Position.Height >=
                    player.Position.Y + (player.Position.Height / 2) &&
                    Position.Y <= player.Position.Y + (player.Position.Height / 2))
                {
                    facingLeft = false;
                    enemyState = EnemyState.Attacking;
                    Attack();
                }
                else
                {
                    enemyState = EnemyState.Idle;
                }
            }

            // If this enemy is currently in the Attacking state and is on the
            // frame of it's animation where it shoots a projectile, the Attack
            // method is called to create a projectile.
            if (enemyState == EnemyState.Attacking && frame == 11 &&
                !recentlyAttacked)
            {
                projectileManager.CreateProjectile(damage, facingLeft, false,
                    Position.X, Position.Y + (Position.Height / 2));
                recentlyAttacked = true;
            }

            attackCooldown--;

            UpdateAnimation();
        }

        // Update the the current animation frame based on state and time spent.
        private void UpdateAnimation()
        {
            switch (enemyState)
            {
                case EnemyState.Idle:
                    animated = true;
                    stateFrameCount = 8;
                    timePerFrame = 10;
                    break;
                case EnemyState.Attacking:
                    animated = true;
                    stateFrameCount = 21;
                    timePerFrame = 6;
                    break;
                case EnemyState.Walking:
                    animated = true;
                    stateFrameCount = 6;
                    timePerFrame = 8;
                    break;

            }

            if (animated)
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

        // Draws the enemy's collision box and the enemy's animation.
        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (enemyState)
            {
                // The player will be able to perform all actions while idle or walking
                case EnemyState.Idle:
                    if (!facingLeft)
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                            new Rectangle(
                                frame * spriteWidth,
                                0,
                                spriteWidth,
                                spriteHeight),
                            Color.White,
                            0,
                            new Vector2(spriteOriginX, spriteOriginY),
                            1.8f,
                            SpriteEffects.None,
                            1);
                    }
                    else
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + 5, position.Y + position.Height),
                            new Rectangle(
                                frame * spriteWidth,
                                0,
                                spriteWidth,
                                spriteHeight),
                            Color.White,
                            0,
                            new Vector2(spriteOriginX, spriteOriginY),
                            1.8f,
                            SpriteEffects.FlipHorizontally,
                            1);
                    }
                    break;
                case EnemyState.Attacking:
                    if (!facingLeft)
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                            new Rectangle(
                                frame * spriteWidth,
                                1 * spriteHeight,
                                spriteWidth,
                                spriteHeight),
                            Color.White,
                            0,
                            new Vector2(spriteOriginX, spriteOriginY),
                            1.8f,
                            SpriteEffects.None,
                            0);
                    }
                    else
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + 5, position.Y + position.Height),
                            new Rectangle(
                                frame * spriteWidth,
                                1 * spriteHeight,
                                spriteWidth,
                                spriteHeight),
                            Color.White,
                            0,
                            new Vector2(spriteOriginX, spriteOriginY),
                            1.8f,
                            SpriteEffects.FlipHorizontally,
                            0);
                    }
                    break;
                case EnemyState.Walking:
                    if (!facingLeft)
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                            new Rectangle(
                                frame * spriteWidth,
                                2 * spriteHeight,
                                spriteWidth,
                                spriteHeight),
                            Color.White,
                            0,
                            new Vector2(spriteOriginX, spriteOriginY),
                            1.8f,
                            SpriteEffects.None,
                            0);
                    }
                    else
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + 5, position.Y + position.Height),
                            new Rectangle(
                                frame * spriteWidth,
                                2 * spriteHeight,
                                spriteWidth,
                                spriteHeight),
                            Color.White,
                            0,
                            new Vector2(spriteOriginX, spriteOriginY),
                            1.8f,
                            SpriteEffects.FlipHorizontally,
                            0);
                    }
                    break;

            }
            if (drawCollisionBox)
            {
                base.Draw(spriteBatch);
            }
        }
    }
}
