using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Spellblade
{
    /// <summary>
    /// Class to manage all enemies, update their positions, and draw them.
    /// </summary>
    class EnemyManager
    {
        private List<Enemy> enemies;
        private List<Enemy> despawnedEnemies;
        public enum enemyType
        {
            skeletonWizard,
            skeletonWarrior,
            boss,
        }

        private Texture2D skeletonWizardTexture;
        private Texture2D skeletonWarriorTexture;
        private Texture2D enemyNecromancerTexture;
        private Texture2D enemyGrimTexture;
        private Texture2D collisionBoxTexture;
        private enemyType type;
        private ProjectileManager projectileManager;
        private bool toggleHitBox;
        private bool shouldDrawHUD;
        public Enemy CurrentBoss { get; private set; }

        private int windowWidth;
        private int windowHeight;

        public EnemyManager(ProjectileManager projectileManager,
            int windowWidth, int windowHeight, Texture2D skeletonWizardTexture,
            Texture2D skeletonWarriorTexture, Texture2D enemyNecromancerTexture,
            Texture2D enemyGrimTexture, Texture2D collisionBoxTexture)
        {
            this.enemies = new List<Enemy>();
            this.despawnedEnemies = new List<Enemy>();
            this.skeletonWizardTexture = skeletonWizardTexture;
            this.skeletonWarriorTexture = skeletonWarriorTexture;
            this.enemyNecromancerTexture = enemyNecromancerTexture;
            this.enemyGrimTexture = enemyGrimTexture;
            this.collisionBoxTexture = collisionBoxTexture;
            this.projectileManager = projectileManager;
            this.toggleHitBox = false;
            this.shouldDrawHUD = false;

            this.windowWidth = windowWidth;
            this.windowHeight = windowHeight;
        }

        public enemyType Type
        {
            get { return type; }
            set { type = value; }
        }

        public int Count
        {
            get { return enemies.Count; }
        }

        public bool ShouldDrawHUD
        {
            get { return shouldDrawHUD; }
        }

        // Adds the given enemy to the list of enemies.
        public void AddEnemy(Enemy enemy)
        {
            enemies.Add(enemy);
            enemy.DrawCollisionBox = toggleHitBox;
        }

        // Creates an enemy based on the given type and at the given location.
        // Then it returns that enemy.
        public Enemy CreateEnemy(int currentLevel, enemyType type, Player player, int X, int Y)
        {
            switch (type)
            {
                case enemyType.skeletonWizard:
                    return new EnemyRanged(projectileManager, player, 1500, 20,
                        10, 0, 120, collisionBoxTexture, skeletonWizardTexture,
                        new Rectangle(X, Y, 40, 53));

                case enemyType.skeletonWarrior:
                    return new EnemyMelee(player, 36, 30, 10, 2, 60, collisionBoxTexture,
                        skeletonWarriorTexture, new Rectangle(X, Y, 31, 53));

                case enemyType.boss:
                    switch (currentLevel)
                    {
                        case 0:
                            CurrentBoss = new EnemyNecromancer(this,
                                projectileManager, currentLevel, player, 2000,
                                100, 7, 0, 120, collisionBoxTexture,
                                enemyNecromancerTexture,
                                new Rectangle(X, Y, 80, 106));
                            return CurrentBoss;

                        case 3:
                            CurrentBoss = new EnemyGrim(this, projectileManager,
                                windowWidth, currentLevel, player, 50, 200, 10, 5, 60,
                                collisionBoxTexture, enemyGrimTexture,
                                new Rectangle(X, Y, 200, 174));
                            return CurrentBoss;

                        // If an invalid level is recieved, it creates a Reaper
                        // boss and returns it.
                        default:
                            CurrentBoss = new EnemyGrim(this, projectileManager,
                                windowWidth, currentLevel, player, 50, 200, 10, 5, 60,
                                collisionBoxTexture, enemyGrimTexture,
                                new Rectangle(X, Y, 200, 174));
                            return CurrentBoss;
                    }

            }

            // If invalid type is recieved, it creates a Skeleton Warrior
            return new EnemyMelee(player, 36, 30, 10, 2, 60, collisionBoxTexture,
                skeletonWarriorTexture, new Rectangle(X, Y, 31, 53));
        }

        // Takes in an enemy and returns a random enemy. The given enemy won't
        // be returned unless there are no other options.
        public Enemy GetRandomEnemy(Enemy enemy)
        {
            Random rng = new Random();
            bool enemyFound = false;

            if (enemies.Count > 1)
            {
                while (!enemyFound)
                {
                    int index = rng.Next(0, enemies.Count);

                    if (enemies[index] != enemy)
                    {
                        return enemies[index];
                    }
                }
            }
            return enemy;
        }

        // Creates a skeleton warrior above each enemy's current position with
        // the exception of the given enemy.
        public void DuplicateEnemies(Enemy enemy, Player player, int currentLevel)
        {
            int originalEnemiesLength = enemies.Count;
            for (int i = 0; i < originalEnemiesLength; i++)
            {
                if (enemies[i] != enemy)
                {
                    AddEnemy(CreateEnemy(currentLevel, enemyType.skeletonWarrior, player,
                        enemies[i].Position.X, enemies[i].Position.Y -
                        (int)(enemies[i].Position.Height * 2)));
                }
            }
        }

        // Moves the enemies when the level moves. Takes in a speed based on how
        // far the tiles of the level are moving. If enemies are out of bounds
        // to the screen it removes them. Then it moves despawned enemies and
        // puts them back if they are back on the screen.
        public void LevelBasedMovement(int speed)
        {
            // Moves despawned enemies.
            for (int i = 0; i < despawnedEnemies.Count; i++)
            {
                despawnedEnemies[i].LevelMove(speed);
            }

            // Moves enemies on the screen
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].LevelMove(speed);

                // Kills enemies that have fallen off of the screen.
                if (enemies[i].Position.Y > windowHeight)
                {
                    enemies[i].TakeDamage(int.MaxValue);
                }

                // Despawns enemies that have been moved off of the screen.
                else if (enemies[i].Position.X > windowWidth ||
                    enemies[i].Position.X < -40)
                {
                    despawnedEnemies.Add(enemies[i]);
                    enemies.RemoveAt(i);
                    i--;
                }
            }

            // Checks if any despawned enemies should be respawned.
            RespawnEnemies();
        }

        // Checks if any despawned enemies should be respawned based on their
        // position.
        public void RespawnEnemies()
        {
            for (int i = 0; i < despawnedEnemies.Count; i++)
            {
                if (despawnedEnemies[i].Position.X > -40 &&
                    despawnedEnemies[i].Position.X < windowWidth)
                {
                    enemies.Add(despawnedEnemies[i]);
                    despawnedEnemies.RemoveAt(i);
                    i--;
                }
            }
        }

        // Returns the first Enemy found to collide with the given GameObject.
        public Enemy EnemyCollision(GameObject gameObject)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] != gameObject &&
                    enemies[i].Position.Intersects(gameObject.Position))
                {
                    return enemies[i];
                }
            }

            return null;
        }

        // Takes in a Rectangle and returns a list of enemies that intersect
        // with that Rectangle.
        public List<Enemy> SwordCollision(Rectangle playerSword)
        {
            List<Enemy> hitEnemies = new List<Enemy>();
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].Position.Intersects(playerSword))
                {
                    hitEnemies.Add(enemies[i]);
                }
            }

            return hitEnemies;
        }

        // Takes an array of lists of Tiles and calls the various collision
        // methods on all the enemies using that array of lists of Tiles.
        public void EnemyIntersectsTiles(List<Tile>[] tangibleTiles)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                // The current enemy for this iteration of the loop.
                Enemy enemy = enemies[i];

                // List of intersecting tangible tiles to call Colide for
                // enemies later.
                List<Tile> intersectingTiles = new List<Tile>();

                // Checks every tangible tile to see if any intersect with the
                // enemy, and, if so, add tile to list.
                for (int column = 0; column < tangibleTiles.GetLength(0); column++)
                {
                    for (int tileNum = 0; tileNum < tangibleTiles[column].Count; tileNum++)
                    {
                        if (tangibleTiles[column][tileNum].Position.Intersects(enemy.Position))
                        {
                            intersectingTiles.Add(tangibleTiles[column][tileNum]);
                        }
                    }
                }

                enemy.EnemySuicidePrevention(tangibleTiles);

                // Sends all the tiles that intersect with the enemy to the
                // Gravity method in Character.
                enemy.Gravity(intersectingTiles);
            }
        }

        // Loops through all the enemies and calls their Update method. Then it
        // kills those enemies if they have 0 or less health.
        public void Update()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Update();

                // Kills enemies if they are dead.
                if (enemies[i].Health <= 0)
                {
                    enemies.RemoveAt(i);
                    i--;
                }
            }
        }

        // Loops through all enemies and draws them. If any of the enemies are
        // the CurrentBoss, shouldDrawHUD is set to true, otherwise it's false.
        public void Draw(SpriteBatch sb)
        {
            shouldDrawHUD = false;
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Draw(sb);

                if (enemies[i] == CurrentBoss)
                {
                    shouldDrawHUD = true;
                }
            }
        }

        // Draw and update boss health bar.
        public void DrawHUD(Texture2D healthBar, Texture2D healthBarExtension, Texture2D square, SpriteBatch sb)
        {
            int missingHP = CurrentBoss.MaxHealth - CurrentBoss.Health;
            int healthOverHundred = (CurrentBoss.MaxHealth - 100) / 10;

            // Health bar logic
            sb.Draw(healthBar, new Rectangle(1650 - (healthOverHundred * 21),
                10, 260, 50), Color.White);
            for (int i = 0; i < healthOverHundred; i++)
            {
                sb.Draw(healthBarExtension, new Rectangle(1901 + (i * 21) -
                    (healthOverHundred * 21), 10, 31, 50), Color.White);
            }
            // First black square for health.
            //sb.Draw(square, new Rectangle(57, 32, 17, 17), Color.Black);
            // Second square.
            //sb.Draw(square, new Rectangle(79, 32, 17, 17), Color.Black);
            for (int i = 1; i <= CurrentBoss.MaxHealth; i++)
            {
                if (missingHP >= i * 10)
                {
                    sb.Draw(square, new Rectangle(1882 +
                        (healthOverHundred * 21) - (21 * (i - 1)) -
                        (healthOverHundred * 21), 32, 16, 17), Color.Black);
                }
            }
        }

        // Loops through the enemies and tells them to swap their current
        // toggleHitBox setting. If they are currently drawing their hitbox,
        // they stop, otherwise they start drawing their hitbox. 
        public void ToggleHitBox()
        {
            toggleHitBox = !toggleHitBox;
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].DrawCollisionBox = toggleHitBox;
            }
        }
    }
}
