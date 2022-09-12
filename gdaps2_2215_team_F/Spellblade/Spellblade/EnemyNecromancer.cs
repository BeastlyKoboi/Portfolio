using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Spellblade
{
    class EnemyNecromancer : Enemy
    {
        //      Animation Fields      //
        // Constants for each frame in the tile sheet
        const int spriteWidth = 160;
        const int spriteHeight = 128;

        // Constants for draw offset from position bottom middle
        const int spriteOriginX = 80;
        const int spriteOriginY = 114;

        // Fields to hold managers to call when needed
        private EnemyManager enemyManager;
        private ProjectileManager projectileManager;

        private enum NecromancerState
        {
            Idle,
            Attack,
            Teleport,
            Summon,
            Death,
        }
        private NecromancerState state;
        private int summonCooldown;
        private bool recentlyTeleported;
        private int currentLevel;

        public EnemyNecromancer(EnemyManager enemyManager, ProjectileManager projectileManager, int currentLevel, Player player,
            int range, int health, int damage, int moveSpeed, int attackSpeed,
            Texture2D texture, Texture2D spriteSheet, Rectangle position) : base(player, range, health,
            damage, moveSpeed, attackSpeed, texture, spriteSheet, position)
        {
            this.enemyManager = enemyManager;
            this.projectileManager = projectileManager;
            this.currentLevel = currentLevel;

            state = NecromancerState.Idle;
            this.summonCooldown = 60 * 10;
            recentlyTeleported = false;
            drawCollisionBox = true;
        }

        // Creates a projectile.
        public override void Attack()
        {
            projectileManager.CreateTrackingProjectile(enemyManager, true, 1,
                damage, Position.X, Position.Y + (Position.Height / 2));
            attackCooldown = attackSpeed;
        }

        // Calls the EnemyManager's DuplicateEnemies method. This creates a
        // melee enemy above each enemy besides this one.
        public void SummonEnemies()
        {
            enemyManager.DuplicateEnemies(this, player, currentLevel);
            summonCooldown = 60 * 10;
            attackCooldown = attackSpeed;
        }

        // Takes damage like a normal Character, but then sets the current state
        // to Teleport.
        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);

            // Teleport away when hit.
            state = NecromancerState.Teleport;
            frame = 0;
        }

        // Teleports to a random enemy besides this one.
        public override void Move()
        {
            recentlyTeleported = true;
            Enemy teleportTarget = enemyManager.GetRandomEnemy(this);

            position.X = teleportTarget.Position.X +
                (teleportTarget.Position.Width / 2) - (Position.Width / 2);
            position.Y = teleportTarget.Position.Y +
                teleportTarget.Position.Height - Position.Height;
        }

        public override void Update()
        {
            // Checks all the different possible states. If it is currently in
            // a non-Idle state, the frame of the animation will be checked to
            // determine if it is time for the ability to fully trigger.
            if (state == NecromancerState.Teleport)
            {
                UpdateAnimation();
                if (frame == 2 && !recentlyTeleported)
                {
                    Move();
                }
                else if (frame > 2)
                {
                    recentlyTeleported = false;
                }
            }
            else if (state == NecromancerState.Summon)
            {
                UpdateAnimation();
                if (frame > 15 && !recentlyAttacked)
                {
                    SummonEnemies();
                    recentlyAttacked = true;
                }
            }
            else if (state == NecromancerState.Attack)
            {
                UpdateAnimation();
                if (frame > 10 && !recentlyAttacked)
                {
                    Attack();
                    recentlyAttacked = true;
                }
            }

            // If the state is Idle but it is time for the Necromancer to use an
            // ability, this checks which ability it should use.
            else if (AttackCooldown <= 0 && state == NecromancerState.Idle)
            {
                recentlyAttacked = false;

                if (summonCooldown <= 0 && enemyManager.Count > 1)
                {
                    state = NecromancerState.Summon;
                    UpdateAnimation();
                    
                }

                // If player is in the attack range to the left.
                else if (Position.X - player.Position.X <= range &&
                    Position.X >= player.Position.X)
                {
                    facingLeft = true;
                    state = NecromancerState.Attack;
                    UpdateAnimation();
                }

                // If player is in the attack range to the right.
                else if (player.Position.X - Position.X <= range &&
                    player.Position.X > Position.X)
                {
                    facingLeft = false;
                    state = NecromancerState.Attack;
                    UpdateAnimation();
                }
                else
                {
                    state = NecromancerState.Idle;
                    UpdateAnimation();
                }
            }
            
            else
            {
                UpdateAnimation();

            }

            // Decreases the ability cooldowns.
            if (attackCooldown > 0)
            {
                attackCooldown--;
            }
            if (summonCooldown > 0)
            {
                summonCooldown--;
            }


        }

        // Update the the current animation frame based on state and time spent.
        private void UpdateAnimation()
        {
            switch (state)
            {
                case NecromancerState.Idle:
                    animated = true;
                    stateFrameCount = 8;
                    timePerFrame = 10;
                    break;
                case NecromancerState.Attack:
                    animated = true;
                    stateFrameCount = 13;
                    timePerFrame = 5;
                    break;
                case NecromancerState.Teleport:
                    animated = true;
                    stateFrameCount = 5;
                    timePerFrame = 10;
                    break;
                case NecromancerState.Summon:
                    animated = true;
                    stateFrameCount = 17;
                    timePerFrame = 5;
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

                        // Once each animation finishes, the Necromancer's state
                        // is set to Idle.
                        state = NecromancerState.Idle;
                    }

                    timeCounter -= timePerFrame;
                }
            }

        }

        // Draws the Necromancer's collision box and its animation.
        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (state)
            {
                case NecromancerState.Idle:
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
                            SpriteEffects.FlipHorizontally,
                            1);
                    }
                    break;
                case NecromancerState.Attack:
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
                            SpriteEffects.FlipHorizontally,
                            0);
                    }
                    break;
                case NecromancerState.Teleport:
                    if (!facingLeft)
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                            new Rectangle(
                                frame * spriteWidth,
                                5 * spriteHeight,
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
                            new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                            new Rectangle(
                                frame * spriteWidth,
                                5 * spriteHeight,
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
                case NecromancerState.Summon:
                    if (!facingLeft)
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                            new Rectangle(
                                frame * spriteWidth,
                                4 * spriteHeight,
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
                            new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                            new Rectangle(
                                frame * spriteWidth,
                                4 * spriteHeight,
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
