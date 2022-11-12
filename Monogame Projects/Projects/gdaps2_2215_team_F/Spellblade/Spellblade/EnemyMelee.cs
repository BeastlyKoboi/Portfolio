using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Spellblade
{
    /// <summary>
    /// Class to add the behavior of a melee enemy to the basic enemy
    /// </summary>
    class EnemyMelee : Enemy
    {
        bool playerSeen;
        bool canMoveLeft;
        bool canMoveRight;

        //      Animation Fields      //
        // Constants for each frame in the tile sheet
        const int spriteWidth = 48;
        const int spriteHeight = 48;

        // Constants for draw offset from position bottom middle
        const int spriteOriginX = 19;
        const int spriteOriginY = 44;

        public EnemyMelee(Player player, int range, int health, int damage,
            int moveSpeed, int attackSpeed, Texture2D texture, Texture2D spriteSheet,
            Rectangle position) : base(player, range, health, damage, moveSpeed,
            attackSpeed, texture, spriteSheet, position)
        {
            playerSeen = false;
            canMoveLeft = true;
            canMoveRight = true;

            frame = 0;
            animated = true;
            stateFrameCount = 4;
            timePerFrame = 10;
            timeCounter = 0;
        }

        public override void Attack()
        {
            // Actual damage will be dealt when the animation occurs.
            recentlyAttacked = false;
            attackCooldown = attackSpeed;
        }

        // Moves the enemy towards the player if they are within the sight range
        // (1000 pixels).
        public override void Move()
        {
            if (((player.Position.X > Position.X &&
                player.Position.X - Position.X <= 1000) ||
                (player.Position.X < Position.X &&
                Position.X - player.Position.X <= 1000)) &&
                Position.Y + Position.Height >=
                player.Position.Y + (player.Position.Height / 2) &&
                Position.Y <= player.Position.Y + (player.Position.Height / 2))
            {
                playerSeen = true;
            }
            else if ((player.Position.X > Position.X &&
                player.Position.X - Position.X > 1000) ||
                (player.Position.X < Position.X &&
                Position.X - player.Position.X > 1000))
            {
                playerSeen = false;
            }

            if (attackCooldown > 0)
            {
                enemyState = EnemyState.Attacking;
            }

            // Move right
            else if (player.Position.X > Position.X &&
                player.Position.X - (Position.X + Position.Width) <= 1000 &&
                playerSeen && canMoveRight)
            {
                facingLeft = false;
                position.X += moveSpeed;
                enemyState = EnemyState.Walking;
            }

            // Move left
            else if (player.Position.X < Position.X &&
                Position.X - (player.Position.X + Position.Width) <= 1000 &&
                playerSeen && canMoveLeft)
            {
                facingLeft = true;
                position.X -= moveSpeed;
                enemyState = EnemyState.Walking;
            }

            // If the enemy is not attacking or moving, it is set to idle.
            else if (enemyState != EnemyState.Attacking)
            {
                enemyState = EnemyState.Idle;
            }
        }

        // Stops enemies from jumping off of ledges.
        public override void EnemySuicidePrevention(List<Tile>[] tangibleTiles)
        {
            // Variable instead of hard coded in case this changes.
            int tileSize = 40;

            Rectangle left = new Rectangle(Position.X - tileSize,
                Position.Y + Position.Height, tileSize, tileSize);
            Rectangle right = new Rectangle(Position.X + Position.Width,
                Position.Y + Position.Height, tileSize, tileSize);

            canMoveLeft = false;
            canMoveRight = false;
            for (int column = 0; column < tangibleTiles.GetLength(0); column++)
            {
                for (int tileNum = 0; tileNum < tangibleTiles[column].Count; tileNum++)
                {
                    if (left.Intersects(tangibleTiles[column][tileNum].Position))
                    {
                        canMoveLeft = true;
                    }

                    if (right.Intersects(tangibleTiles[column][tileNum].Position))
                    {
                        canMoveRight = true;
                    }
                }
            }
        }

        public override void Update()
        {
            // Calls the Move method and then checks if the enemy can attack. If so,
            // it checks if the player is within range. If so, it calls its Attack
            // method and changes its state to Attacking.
            Move();

            if (AttackCooldown <= 0)
            {
                // If player is in the attack range on to the left.
                if (Position.X - player.Position.X <= range &&
                    Position.X >= player.Position.X && Position.Y + Position.Height >=
                    player.Position.Y + (player.Position.Height / 2) &&
                    Position.Y <= player.Position.Y + (player.Position.Height / 2))
                {
                    facingLeft = true;
                    enemyState = EnemyState.Attacking;
                    Attack();
                }

                // If player is in the attack range on to the right.
                else if (player.Position.X - Position.X <= range &&
                    player.Position.X > Position.X && Position.Y + Position.Height >=
                    player.Position.Y + (player.Position.Height / 2) &&
                    Position.Y <= player.Position.Y + (player.Position.Height / 2))
                {
                    facingLeft = false;
                    enemyState = EnemyState.Attacking;
                    Attack();
                }

            }

            // Hits the player if the attack conditions are met.
            if (enemyState == EnemyState.Attacking && frame == 8 &&
                !recentlyAttacked)
            {
                Rectangle attackRange;

                if (FacingLeft)
                {
                    attackRange = new Rectangle(Position.X - Range, Position.Y, Range, Position.Height);
                }
                else
                {
                    attackRange = new Rectangle(Position.X + Position.Width, Position.Y, Range, Position.Height);
                }

                if (attackRange.Intersects(Player.Position))
                {
                    Player.TakeDamage(Damage);
                }

                recentlyAttacked = true;
            }

            // Decreases attack cooldown.
            attackCooldown--;

            // Updates this enemy's animation.
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
                    stateFrameCount = 13;
                    timePerFrame = 5;
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
                            new Vector2(position.X, position.Y + position.Height),
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
                            new Vector2(position.X, position.Y + position.Height),
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
                            new Vector2(position.X, position.Y + position.Height),
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