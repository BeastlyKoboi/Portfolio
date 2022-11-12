using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Spellblade
{
    class TrackingProjectile : Projectile
    {
        enum ProjectileState { Moving, Exploding }
        enum Directions { Up, Left, Right, Down, UpLeft, UpRight, DownLeft, DownRight }

        private ProjectileState state;
        private Directions direction;

        private EnemyManager enemyManager;
        private Player player;
        private bool summonedSkeleton;
        private int currentLevel;

        private new const int speed = 6;

        //      Animation Fields      //
        // Constants for each frame in the tile sheet
        const int projectileSpriteWidth = 66;
        const int projectileSpriteHeight = 68;

        // Constants for draw offset from position bottom middle
        const int projectileSpriteOriginX = 34;
        const int projectileSpriteOriginY = 56;

        public TrackingProjectile(EnemyManager enemyManager, Player player, int direction, int damage,
            bool left, int windowWidth,  bool playerProjectile,
            Texture2D texture, Rectangle position) : base(damage, left,
            windowWidth, playerProjectile, texture, position)
        {
            this.state = ProjectileState.Moving;
            this.direction = (Directions)direction;

            this.enemyManager = enemyManager;
            this.player = player;
            summonedSkeleton = false;
            currentLevel = 0;
        }

        public bool SummonedSkeleton
        {
            get { return summonedSkeleton; }
        }

        public override void Update()
        {
            // Middle of this projectile.
            int thisPosX = Position.X + (Position.Width / 2);
            int thisPosY = Position.Y + (Position.Height / 2);

            // Middle of the target player.
            int targetPosX = player.Position.X + (player.Position.Width / 2);
            int targetPosY = player.Position.Y + (player.Position.Height / 2);

            // If the projectile has collided with something, it explodes.
            if (!active)
            {
                state = ProjectileState.Exploding;
            }

            switch (state)
            {
                case ProjectileState.Moving:
                    // If the target is more than twice the distance upwards
                    // than they are on the x-axis, move upward.
                    if (thisPosY > targetPosY && thisPosY - targetPosY >
                        Math.Abs(thisPosX - targetPosX) * 2 &&
                        (thisPosY - targetPosY > 100 ||
                        Math.Abs(thisPosX - targetPosX) == 0))
                    {
                        direction = Directions.Up;
                    }

                    // If the target is more than twice the distance to the left
                    // than they are on the y-axis, move to the left.
                    else if (thisPosX > targetPosX &&
                        thisPosX - targetPosX >
                        Math.Abs(thisPosY - targetPosY) * 2 &&
                        (thisPosX - targetPosX > 100 ||
                        Math.Abs(thisPosY - targetPosY) == 0))
                    {
                        direction = Directions.Left;
                    }

                    // If the target is more than twice the distance to the
                    // right than they are on the y-axis, move to the right.
                    else if (targetPosX > thisPosX &&
                        targetPosX - thisPosX >
                        Math.Abs(thisPosY - targetPosY) * 2 &&
                        (targetPosX - thisPosX > 100 ||
                        Math.Abs(thisPosY - targetPosY) == 0))
                    {
                        direction = Directions.Right;
                    }

                    // If the target is more than twice the distance downwards
                    // than they are on the x-axis, move downwards.
                    else if (targetPosY > thisPosY &&
                        targetPosY - thisPosY >
                        Math.Abs(thisPosX - targetPosX) * 2 &&
                        (targetPosY - thisPosY > 1002 ||
                        Math.Abs(thisPosX - targetPosX) == 0))
                    {
                        direction = Directions.Down;
                    }

                    // If the target is above and to the left of this
                    // projectile.
                    else if (thisPosY > targetPosY && thisPosX > targetPosX)
                    {
                        direction = Directions.UpLeft;
                    }

                    // If the target is above and to the right of this
                    // projectile.
                    else if (thisPosY > targetPosY && targetPosX > thisPosX)
                    {
                        direction = Directions.UpRight;
                    }

                    // If the target is below and to the left of this
                    // projectile.
                    else if (targetPosY > thisPosY && thisPosX > targetPosX)
                    {
                        direction = Directions.DownLeft;
                    }

                    // If the target is below and to the right of this
                    // projectile.
                    else if (targetPosY > thisPosY && targetPosX > thisPosX)
                    {
                        direction = Directions.DownRight;
                    }

                    // Moves the projectile based on the current direction it is
                    // moving.
                    switch (direction)
                    {
                        case Directions.Up:
                            position.Y -= speed;
                            break;

                        case Directions.Left:
                            position.X -= speed;
                            break;

                        case Directions.Right:
                            position.X += speed;
                            break;

                        case Directions.Down:
                            position.Y += speed;
                            break;

                        case Directions.UpLeft:
                            position.X -= speed / 2;
                            position.Y -= speed / 2;
                            break;

                        case Directions.UpRight:
                            position.X += speed / 2;
                            position.Y -= speed / 2;
                            break;

                        case Directions.DownLeft:
                            position.X -= speed / 2;
                            position.Y += speed / 2;
                            break;

                        case Directions.DownRight:
                            position.X += speed / 2;
                            position.Y += speed / 2;
                            break;
                    }

                    // If the projectile is off the screen, remove it and don't
                    // summon a skeleton.
                    if (Position.X < 0 || Position.X > windowWidth)
                    {
                        active = false;
                        summonedSkeleton = true;
                    }
                    break;

                case ProjectileState.Exploding:
                    active = false;
                    break;
            }
            UpdateAnimation();
        }

        // Update the the current animation frame based on state and time spent.
        private void UpdateAnimation()
        {
            switch (state)
            {
                case ProjectileState.Moving:
                    animated = true;
                    stateFrameCount = 6;
                    timePerFrame = 5;
                    break;
                case ProjectileState.Exploding:
                    animated = true;
                    stateFrameCount = 5;
                    timePerFrame = 10;
                    break;
            }

            timeCounter++;

            if (timeCounter >= timePerFrame)
            {
                frame++;

                if (frame > stateFrameCount - 1)
                {
                    // If it is the correct state and frame and a skeleton has
                    // not yet been summoned, create a new enemy.
                    if (state == ProjectileState.Exploding && frame == 5)
                    {
                        if (!summonedSkeleton)
                        {
                            enemyManager.AddEnemy(enemyManager.CreateEnemy(currentLevel,
                                EnemyManager.enemyType.skeletonWizard, player,
                                Position.X, Position.Y - 40));
                            summonedSkeleton = true;
                        }
                    }

                    frame = 0;
                }

                timeCounter -= timePerFrame;
            }

        }

        // Draws the tracking projectile if its active.
        public override void Draw(SpriteBatch sb)
        {
            if (state == ProjectileState.Moving)
            {
                switch (direction)
                {
                    case Directions.Up:
                        sb.Draw(spriteSheet, new Vector2(position.X,
                            position.Y + position.Height),
                            new Rectangle(frame * projectileSpriteWidth,
                            5 * projectileSpriteHeight, projectileSpriteWidth,
                            projectileSpriteHeight), Color.White, 0,
                            new Vector2(projectileSpriteOriginX,
                            projectileSpriteOriginY), 1.8f, SpriteEffects.None,
                            1);
                        break;

                    case Directions.Left:
                        sb.Draw(spriteSheet, new Vector2(position.X,
                            position.Y + position.Height),
                            new Rectangle(frame * projectileSpriteWidth,
                            4 * projectileSpriteHeight, projectileSpriteWidth,
                            projectileSpriteHeight), Color.White, 0,
                            new Vector2(projectileSpriteOriginX,
                            projectileSpriteOriginY), 1.8f, SpriteEffects.None,
                            1);
                        break;

                    case Directions.Right:
                        sb.Draw(spriteSheet, new Vector2(position.X,
                            position.Y + position.Height),
                            new Rectangle(frame * projectileSpriteWidth,
                            4 * projectileSpriteHeight, projectileSpriteWidth,
                            projectileSpriteHeight), Color.White, 0,
                            new Vector2(projectileSpriteOriginX,
                            projectileSpriteOriginY), 1.8f,
                            SpriteEffects.FlipHorizontally, 1);
                        break;

                    case Directions.Down:
                        sb.Draw(spriteSheet, new Vector2(position.X,
                            position.Y + position.Height),
                            new Rectangle(frame * projectileSpriteWidth,
                            3 * projectileSpriteHeight, projectileSpriteWidth,
                            projectileSpriteHeight), Color.White, 0,
                            new Vector2(projectileSpriteOriginX,
                            projectileSpriteOriginY), 1.8f, SpriteEffects.None,
                            1);
                        break;

                    case Directions.UpLeft:
                        sb.Draw(spriteSheet, new Vector2(position.X,
                            position.Y + position.Height),
                            new Rectangle(frame * projectileSpriteWidth,
                            2 * projectileSpriteHeight, projectileSpriteWidth,
                            projectileSpriteHeight), Color.White, 0,
                            new Vector2(projectileSpriteOriginX,
                            projectileSpriteOriginY), 1.8f, SpriteEffects.None,
                            1);
                        break;

                    case Directions.UpRight:
                        sb.Draw(spriteSheet, new Vector2(position.X,
                            position.Y + position.Height),
                            new Rectangle(frame * projectileSpriteWidth,
                            2 * projectileSpriteHeight, projectileSpriteWidth,
                            projectileSpriteHeight), Color.White, 0,
                            new Vector2(projectileSpriteOriginX,
                            projectileSpriteOriginY), 1.8f,
                            SpriteEffects.FlipHorizontally, 1);
                        break;

                    case Directions.DownLeft:
                        sb.Draw(spriteSheet, new Vector2(position.X,
                            position.Y + position.Height),
                            new Rectangle(frame * projectileSpriteWidth,
                            projectileSpriteHeight, projectileSpriteWidth,
                            projectileSpriteHeight), Color.White, 0,
                            new Vector2(projectileSpriteOriginX,
                            projectileSpriteOriginY), 1.8f, SpriteEffects.None,
                            1);
                        break;

                    case Directions.DownRight:
                        sb.Draw(spriteSheet, new Vector2(position.X,
                            position.Y + position.Height),
                            new Rectangle(frame * projectileSpriteWidth,
                            projectileSpriteHeight, projectileSpriteWidth,
                            projectileSpriteHeight), Color.White, 0,
                            new Vector2(projectileSpriteOriginX,
                            projectileSpriteOriginY), 1.8f,
                            SpriteEffects.FlipHorizontally, 1);
                        break;
                }
            }
            else
            {
                sb.Draw(spriteSheet,
                    new Vector2(position.X, position.Y + position.Height),
                    new Rectangle(frame * projectileSpriteWidth, 0,
                    projectileSpriteWidth, projectileSpriteHeight), Color.White,
                    0, new Vector2(projectileSpriteOriginX,
                    projectileSpriteOriginY), 1.8f, SpriteEffects.None, 1);
            }
        }
    }
}
