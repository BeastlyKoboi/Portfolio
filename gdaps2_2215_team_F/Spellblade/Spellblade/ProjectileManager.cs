using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Spellblade
{
    /// <summary>
    /// Class to manage all existing projectiles and update their positions
    /// </summary>
    class ProjectileManager
    {
        // Fields:
        private List<Projectile> projectiles;
        private List<TrackingProjectile> explodingProjectiles;
        private Texture2D projectileTexture;
        private Texture2D necromancerProjectileTexture;
        private Texture2D reaperProjectileTexture;
        private Player player;
        private int windowWidth;

        public ProjectileManager(Texture2D projectileTexture,
            Texture2D necromancerProjectileTexture,
            Texture2D reaperProjectileTexture, Player player, int windowWidth)
        {
            projectiles = new List<Projectile>();
            explodingProjectiles = new List<TrackingProjectile>();
            this.projectileTexture = projectileTexture;
            this.necromancerProjectileTexture = necromancerProjectileTexture;
            this.reaperProjectileTexture = reaperProjectileTexture;
            this.player = player;
            this.windowWidth = windowWidth;
        }

        // Properties:
        public List<Projectile> Projectiles
        {
            get { return projectiles; }
        }

        // Creates a projectile and adds it to the list of projectiles.
        public void CreateProjectile(int damage, bool direction, bool playerProjectile, int X, int Y)
        {
            projectiles.Add(new Projectile(damage, direction, windowWidth,
                playerProjectile, projectileTexture, new Rectangle(X, Y, 40, 23)));
        }

        // Creates a projectile and adds it to the list of projectiles.
        public void CreateTrackingProjectile(EnemyManager enemyManager,
            bool red, int direction, int damage, int X, int Y)
        {
            if (red)
            {
                projectiles.Add(new TrackingProjectile(enemyManager, player,
                    direction, damage, true, windowWidth, false,
                    necromancerProjectileTexture, new Rectangle(X, Y, 40, 40)));
            }
            else
            {
                projectiles.Add(new TrackingProjectile(enemyManager, player,
                    direction, damage, true, windowWidth, false,
                    reaperProjectileTexture, new Rectangle(X, Y, 40, 40)));
            }
        }

        // Removes a projectile from the list of projectiles. If it is a
        // tracking projectile, it sends it to the list of exploding projectiles
        // so the projectile is harmless while its explosion animation can be
        // drawn.
        public void RemoveProjectile(int index)
        {
            if (projectiles[index] is TrackingProjectile)
            {
                explodingProjectiles.Add((TrackingProjectile)projectiles[index]);
            }
            projectiles[index].Active = false;
            projectiles.RemoveAt(index);
        }

        // Moves the projectiles based on the movement of the level.
        public void LevelBasedMovement(int speed)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].LevelMove(speed);
            }

            for (int i = 0; i < explodingProjectiles.Count; i++)
            {
                explodingProjectiles[i].LevelMove(speed);
            }
        }

        // Removes projectiles if they hit one of the tiles in the given array
        // of lists of Tiles.
        public void ProjectileIntersectsTiles(List<Tile>[] tangibleTiles)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                // The current enemy for this iteration of the loop.
                Projectile projectile = projectiles[i];

                // Checks every tangible tile to see if any intersect with the
                // enemy, and, if so, add tile to list.
                for (int column = 0; column < tangibleTiles.GetLength(0); column++)
                {
                    for (int tileNum = 0; tileNum < tangibleTiles[column].Count; tileNum++)
                    {
                        if (tangibleTiles[column][tileNum].Position.Intersects(projectile.Position))
                        {
                            projectiles[i].CheckCollision(tangibleTiles[column][tileNum]);
                            RemoveProjectile(i);
                            return;
                        }
                    }
                }
            }
        }

        // Loops through the list of projectiles and updates them.
        public void Update(EnemyManager enemyManager)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].Update();

                // Loops through the projectiles to see if they are colliding
                // with each other, and removes them if they are.
                bool removed = false;
                for (int j = 0; j < projectiles.Count; j++)
                {
                    if (i != j && projectiles[i].CheckCollision(projectiles[j]))
                    {
                        if (i < j)
                        {
                            RemoveProjectile(j);
                            RemoveProjectile(i);
                        }
                        else
                        {
                            RemoveProjectile(i);
                            RemoveProjectile(j);
                        }

                        removed = true;
                        i--;
                        break;
                    }
                }

                if (!removed)
                {
                    // If the projectile is under the player's control, check if
                    // it has collided with the player. If so, have the player
                    // take damage and remove the projectile.
                    if (!projectiles[i].PlayerProjectile)
                    {
                        if (projectiles[i].CheckCollision(player))
                        {
                            player.TakeDamage(projectiles[i].Damage);
                            RemoveProjectile(i);
                            i--;
                        }
                    }

                    // If the projectile is not under the player's control,
                    // check if it collides with any of the enemies. If so, have
                    // the enemy take damage and remove the projectile.
                    else
                    {
                        Enemy enemy = enemyManager.EnemyCollision(projectiles[i]);
                        if (enemy != null)
                        {
                            enemy.TakeDamage(projectiles[i].Damage);
                            RemoveProjectile(i);
                            i--;
                        }
                    }
                }
            }

            // Loops through currently exploding projectiles to update them.
            for (int i = 0; i < explodingProjectiles.Count; i++)
            {
                explodingProjectiles[i].Update();
                
                if (explodingProjectiles[i].SummonedSkeleton)
                {
                    explodingProjectiles.RemoveAt(i);
                    i--;
                }
            }
        }

        // Loops through the projectiles and draws them.
        public void Draw(SpriteBatch sb)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].Draw(sb);
            }

            for (int i = 0; i < explodingProjectiles.Count; i++)
            {
                explodingProjectiles[i].Draw(sb);
            }
        }
    }
}
