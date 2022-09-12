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
    class EnemyGrim : Enemy
    {
        private EnemyManager enemyManager;
        private ProjectileManager projectileManager;

        private enum GrimState
        {
            Idle,
            Attack,
            TeleportOut,
            TeleportIn,
            SummonMinions,
            SpellAttack,
        }
        private GrimState state;
        private int teleportCooldown;
        private int summonCooldown;
        private int spellCooldown;
        private int windowWidth;
        private int currentLevel;
        private bool halfHealthTrigger;
        private int spellAttackIndex;

        //      Animation Fields      //
        // Constants for each frame in the tile sheet
        const int spriteWidth = 252;
        const int spriteHeight = 205;

        // Constants for draw offset from position bottom middle
        const int spriteOriginX = 115;
        const int spriteOriginY = 205;

        public EnemyGrim(EnemyManager enemyManager, ProjectileManager projectileManager,
            int windowWidth, int currentLevel, Player player, int range, int health, int damage,
            int moveSpeed, int attackSpeed, Texture2D texture, Texture2D spriteSheet,
            Rectangle position) : base(player, range, health, damage, moveSpeed,
            attackSpeed, texture, spriteSheet, position)
        {
            this.enemyManager = enemyManager;
            this.projectileManager = projectileManager;
            this.windowWidth = windowWidth;
            this.currentLevel = currentLevel;
            this.halfHealthTrigger = false;
            this.spellAttackIndex = 0;

            state = GrimState.Idle;
            teleportCooldown = 60 * 5;
            summonCooldown = 30;
            spellCooldown = 60 * 15;
        }

        // Moves towards the player on both the X and Y direction. Once spawned,
        // it will always track after the player.
        public override void Move()
        {
            // If the player is to the right and not within range, move right.
            if (player.Position.X + player.Position.Width >
                Position.X + Position.Width + Range)
            {
                facingLeft = false;
                position.X += moveSpeed;
            }

            // If the player is to the left and not within range, move left.
            else if (player.Position.X < Position.X - Range)
            {
                facingLeft = true;
                position.X -= moveSpeed;
            }

            // Move left if the player is within range.
            else if (player.FacingLeft &&
                player.Position.X - (player.Range * 1.2) <
                Position.X + Position.Width)
            {
                position.X -= moveSpeed;
            }

            // Move right if the player is within range.
            else if (!player.FacingLeft &&
                player.Position.X + player.Position.Width +
                (player.Range * 1.2) > Position.X)
            {
                position.X += moveSpeed;
            }

            // Reposition if it would go offscreen. The boss does not despawn.
            if (Position.X + Position.Width > windowWidth)
            {
                position.X = windowWidth - Position.Width;
            }
            else if (Position.X < 0)
            {
                position.X = 0;
            }

            // If the player is below the Reaper, move down.
            if (player.Position.Y + player.Position.Height >
                Position.Y + Position.Height)
            {
                position.Y += moveSpeed;
            }

            // If the player is above the Reaper, move up.
            else if (player.Position.Y < Position.Y)
            {
                position.Y -= moveSpeed;
            }
        }
        

        // Basic Attack: normal melee attack. Will deal a lot of damage and can
        // be used once per second.
        public override void Attack()
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
        }

        // Disappears for two seconds.
        public void TeleportAttack()
        {
            teleportCooldown = 60 * 2;
        }

        // Summons a skeleton warrior to the left and right of the player.
        public void SummonMinions()
        {
            enemyManager.AddEnemy(enemyManager.CreateEnemy(currentLevel,
                EnemyManager.enemyType.skeletonWarrior, player,
                player.Position.X - 51, Player.Position.Y));
            enemyManager.AddEnemy(enemyManager.CreateEnemy(currentLevel,
                EnemyManager.enemyType.skeletonWarrior, player,
                player.Position.X + player.Position.Width + 20, Player.Position.Y));

            summonCooldown = 60 * 6;
        }

        // Summons eight tracking projectiles, one at a time.
        public void SpellAttack()
        {
            switch (spellAttackIndex)
            {
                case 0:
                    // Top
                    projectileManager.CreateTrackingProjectile(enemyManager, false, 0,
                        damage / 2, Position.X + (Position.Width / 2), Position.Y - 10);
                    break;

                case 1:
                    // Top left
                    projectileManager.CreateTrackingProjectile(enemyManager, false, 4,
                        damage / 2, Position.X, Position.Y);
                    break;

                case 2:
                    // Left
                    projectileManager.CreateTrackingProjectile(enemyManager, false, 1,
                        damage / 2, Position.X - 10, Position.Y + (Position.Height / 2));
                    break;

                case 3:
                    // Bottom left
                    projectileManager.CreateTrackingProjectile(enemyManager, false, 6,
                        damage / 2, Position.X, Position.Y + Position.Height);
                    break;

                case 4:
                    // Bottom
                    projectileManager.CreateTrackingProjectile(enemyManager, false, 3,
                        damage / 2, Position.X + (Position.Width / 2), Position.Y + 10);
                    break;

                case 5:
                    // Bottom right
                    projectileManager.CreateTrackingProjectile(enemyManager, false, 7,
                        damage / 2, Position.X + Position.Width,
                        Position.Y + Position.Height);
                    break;

                case 6:
                    // Right
                    projectileManager.CreateTrackingProjectile(enemyManager, false, 4,
                        damage / 2, Position.X + Position.Width + 10,
                        Position.Y + (Position.Height / 2));
                    break;

                case 7:
                    // Top right
                    projectileManager.CreateTrackingProjectile(enemyManager, false, 4,
                        damage / 2, Position.X + Position.Width, Position.Y);
                    break;
            }

            recentlyAttacked = true;
        }

        public override void Gravity(List<Tile> tangibleTiles)
        {
            // The Reaper has no collision, it can enter into tangible tiles as
            // if they were intangible.
        }

        public override void Update()
        {
            // If the current state is SpellAttack, call the relevant method.
            if (state == GrimState.SpellAttack)
            {
                if (!recentlyAttacked)
                {
                    SpellAttack();
                }
            }

            // Checks if it is time for the Reaper to reappear and if it is, the
            // Reaper reappears usually behind the player, though in front of
            // them if there isn't room behind them.
            else if (state == GrimState.TeleportOut)
            {
                if (teleportCooldown <= 0)
                {
                    position.Y = player.Position.Y - Position.Height + player.Position.Height;

                    if (player.Position.X < Position.Width &&
                        !player.FacingLeft)
                    {
                        position.X = player.Position.X + player.Position.Width;
                        facingLeft = true;
                    }
                    else if (player.Position.X + player.Position.Width + Position.Width > windowWidth &&
                        player.FacingLeft)
                    {
                        position.X = player.Position.X - Position.Width;
                        facingLeft = false;
                    }
                    else
                    {
                        if (player.FacingLeft)
                        {
                            position.X = player.Position.X + player.Position.Width;
                            facingLeft = true;
                        }
                        else
                        {
                            position.X = player.Position.X - Position.Width;
                            facingLeft = false;
                        }
                    }
                    
                    teleportCooldown = 60 * 5;

                    state = GrimState.TeleportIn;
                }
            }

            // After reappearing, checks if it is the time in the animation for
            // the Reaper to attack.
            else if (state == GrimState.TeleportIn)
            {
                if (frame == 6 && !recentlyAttacked)
                {
                    Attack();
                    Attack();
                    recentlyAttacked = true;
                }
            }

            // If the Reaper is in the SummonMinions state, checks if it is the
            // time in the animation for the new enemies to be created.
            else if (state == GrimState.SummonMinions)
            {
                if (frame == 6 && !recentlyAttacked)
                {
                    SummonMinions();
                    recentlyAttacked = true;
                }
            }

            // When in the Attack state, checks if it is the time in the
            // animation for the Reaper to attack.
            else if (state == GrimState.Attack)
            {
                if (frame == 3 && !recentlyAttacked)
                {
                    Attack();
                    recentlyAttacked = true;
                }
            }

            // If the Reaper isn't currently doing anything and the attack
            // cooldown has ended, it checks what ability the Reaper should do
            // next.
            else if (AttackCooldown <= 0 && state == GrimState.Idle)
            {
                recentlyAttacked = false;

                // When equal to or below half health, sets the cooldown of the
                // boss's big attack to 0 so it can be used immediately.
                if (health <= maxHealth / 2 && !halfHealthTrigger)
                {
                    spellCooldown = 0;
                    halfHealthTrigger = true;
                }

                // Highest priority, checks if the Spell Attack is off cooldown.
                if (spellCooldown <= 0)
                {
                    state = GrimState.SpellAttack;

                    position.X = (windowWidth / 2) + (Position.Width / 2);
                    position.Y = 360;

                    recentlyAttacked = false;
                    SpellAttack();

                    spellCooldown = 60 * 15;
                }

                // Next highest priority, checks if the Summon Minions ability
                // is off cooldown.
                else if (summonCooldown <= 0)
                {
                    state = GrimState.SummonMinions;
                    
                }

                // Checks if the Teleport Attack is off cooldown.
                else if (teleportCooldown <= 0)
                {
                    state = GrimState.TeleportOut;
                    TeleportAttack();
                }

                // If none of the other triggered, checks if the player is in
                // range for a normal attack. If so, it does the strike,
                // otherwise it just moves.
                else
                {
                    // If the player is in the attack range to the left.
                    if (Position.X - range < player.Position.X + player.Position.Width &&
                        Position.X > player.Position.X)
                    {
                        facingLeft = true;
                        state = GrimState.Attack;
                    }

                    // If player is in the attack range to the right.
                    else if (Position.X + Position.Width + range > player.Position.X &&
                        player.Position.X > Position.X)
                    {
                        facingLeft = false;
                        state = GrimState.Attack;
                    }

                    // Moves if the player isn't in range.
                    else
                    {
                        state = GrimState.Idle;
                        Move();
                    }
                }

                attackCooldown = attackSpeed;
            }

            // While there are no attacks to do, move.
            else
            {
                state = GrimState.Idle;
                Move();
            }

            UpdateAnimation();

            // Decreases the various attack cooldowns.
            if (attackCooldown > 0)
            {
                attackCooldown--;
            }
            if (teleportCooldown > 0)
            {
                teleportCooldown--;
            }
            if (summonCooldown > 0)
            {
                summonCooldown--;
            }
            if(spellCooldown > 0)
            {
                spellCooldown--;
            }
        }

        // Update the the current animation frame based on state and time spent.
        private void UpdateAnimation()
        {
            switch (state)
            {
                case GrimState.Idle:
                    animated = true;
                    stateFrameCount = 8;
                    timePerFrame = 5;
                    break;
                case GrimState.Attack:
                    animated = true;
                    stateFrameCount = 7;
                    timePerFrame = 5;
                    break;
                case GrimState.TeleportOut:
                    animated = true;
                    stateFrameCount = 6;
                    timePerFrame = 5;
                    break;
                case GrimState.TeleportIn:
                    animated = true;
                    stateFrameCount = 10;
                    timePerFrame = 5;
                    break;
                case GrimState.SummonMinions:
                    animated = true;
                    stateFrameCount = 8;
                    timePerFrame = 5;
                    break;
                case GrimState.SpellAttack:
                    animated = true;
                    stateFrameCount = 6;
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

                        // If the Reaper has teleported away, makes sure it
                        // stays away.
                        if (state == GrimState.TeleportOut)
                        {
                            position.Y -= 10000;
                        }

                        // If the Reaper is doing its Spell Attack, increment
                        // the Spell Attack Index and set recentlyAttacked to
                        // false so the program knows that the point has been
                        // reached to summon another projectile for the Spell
                        // Attack.
                        else if (state == GrimState.SpellAttack)
                        {
                            spellAttackIndex++;
                            recentlyAttacked = false;
                        }

                        // If neither of the others triggered, sets the Reaper's
                        // state to Idle because the current animation is
                        // complete.
                        else
                        {
                            state = GrimState.Idle;
                        }

                        // After doing one last runthrough of the animation, the
                        // spellAttackIndex is reset and the Reaper's state is
                        // set to Idle.
                        if (spellAttackIndex >= 8)
                        {
                            spellAttackIndex = 0;
                            state = GrimState.Idle;
                        }
                    }

                    timeCounter -= timePerFrame;
                }
            }

        }

        // Draws the Reaper's collision box and the Reaper's animations.
        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (state)
            {
                case GrimState.Idle:
                    if (!facingLeft)
                    {
                        spriteBatch.Draw(spriteSheet,
                            new Vector2(position.X + (position.Width / 2) - 10,
                            position.Y + position.Height + 50),
                            new Rectangle(frame * spriteWidth, 5 * spriteHeight,
                            spriteWidth, spriteHeight), Color.White, 0,
                            new Vector2(spriteOriginX, spriteOriginY), 1.8f,
                            SpriteEffects.None, 1);
                    }
                    else
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2) - 35, position.Y + position.Height + 50),
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
                            1);
                    }
                    break;
                case GrimState.Attack:
                    if (!facingLeft)
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2) - 10, position.Y + position.Height + 50),
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
                            0);
                    }
                    else
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2) - 35, position.Y + position.Height + 50),
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
                            0);
                    }
                    break;
                case GrimState.TeleportOut:
                    if (!facingLeft)
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2) - 10, position.Y + position.Height + 50),
                            new Rectangle(
                                frame * spriteWidth,
                                6 * spriteHeight,
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
                            new Vector2(position.X + (position.Width / 2) - 35, position.Y + position.Height + 50),
                            new Rectangle(
                                frame * spriteWidth,
                                6 * spriteHeight,
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
                case GrimState.TeleportIn:
                    if (!facingLeft)
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2) - 10, position.Y + position.Height + 50),
                            new Rectangle(
                                frame * spriteWidth,
                                spriteHeight,
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
                            new Vector2(position.X + (position.Width / 2) - 35, position.Y + position.Height + 50),
                            new Rectangle(
                                frame * spriteWidth,
                                spriteHeight,
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
                case GrimState.SummonMinions:
                    if (!facingLeft)
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2) - 10, position.Y + position.Height + 50),
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
                            new Vector2(position.X + (position.Width / 2) - 35, position.Y + position.Height + 50),
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

                case GrimState.SpellAttack:
                    if (!facingLeft)
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2) - 10, position.Y + position.Height + 50),
                            new Rectangle(
                                frame * spriteWidth,
                                3 * spriteHeight,
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
                            new Vector2(position.X + (position.Width / 2) - 35, position.Y + position.Height + 50),
                            new Rectangle(
                                frame * spriteWidth,
                                3 * spriteHeight,
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