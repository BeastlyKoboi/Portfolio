using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace Spellblade
{
    /// <summary>
    /// Enum to hold the current level
    /// </summary>
    enum Levels { Forest, Village, Castle, Astral }

    /// <summary>
    /// Class to load, update, and draw the current level.
    /// </summary>
    class LevelRenderer
    {
        // Field to hold the filenames and tilesheets for the levels
        private string[] fileNames;
        private Texture2D[] levelTileSheets;
        private Rectangle[] tileTextureLocs;

        // Fields to hold the Tile Sheet, the level map, the current tiles, and the tiles exposed to air
        private Texture2D currentTileSheet;
        private string[] map;
        private Tile[,] tiles;
        private List<Tile>[] tangibleTiles;

        // Fields to hold references to hold the EnemyManager, ProjectileManager, and Player, 
        // to use in later update
        private EnemyManager enemyManager;
        private ProjectileManager projectileManager;
        private Player player;

        // Fields to hold the window position on the map
        private Rectangle windowPosition;

        // Field to hold the size of each tile
        private int tileXSize;
        private int tileYSize;

        // Field to hold the window dimensions
        private int windowWidth;
        private int windowHeight;

        // Saves tiles that later create more specific effects
        private Tile progressionTile;
        private Tile spawnBossTile;
        private bool bossSpawned;

        /// <summary>
        /// Property to make the progression tile read-only
        /// </summary>
        public Tile ProgressionTile
        {
            get
            {
                if (progressionTile == null)
                {
                    return null;
                }
                else
                {
                    return progressionTile;
                }
            }
        }

        // Property to hold the current level to render and make it read only
        public Levels currentLevel { get; }

        /// <summary>
        /// Property to make the tangible tiles read only
        /// </summary>
        public List<Tile>[] TangibleTiles
        {
            get { return tangibleTiles; }
        }

        /// <summary>
        /// Parameterized constructor to initialize the starting level,
        /// save the window size, save tile info, and initialize the
        /// 2d tile array based on the window size.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="levelTileSheets"></param>
        /// <param name="windowWidth"></param>
        /// <param name="windowHeight"></param>
        /// <param name="enemyManager"></param>
        /// <param name="projectileManager"></param>
        /// <param name="player"></param>
        public LevelRenderer(Levels level, Texture2D[] levelTileSheets, int windowWidth, int windowHeight,
            // Additional fields for various managers and game objects.
            EnemyManager enemyManager, ProjectileManager projectileManager, Player player)
        {
            // Save current level
            currentLevel = level;

            // Saves the file paths of all the level maps 
            fileNames = new string[] { "../../../../../data_files/Forest.txt", "", "../../../../../data_files/Castle.txt", "../../../../../data_files/Space.txt" };

            // Saves tile textures and drawn sizes
            this.levelTileSheets = levelTileSheets;
            tileXSize = 40;
            tileYSize = 40;

            // Initializes the 2d tile array with d-values based on window size
            tiles = new Tile[(int)(windowHeight / tileYSize), (int)(windowWidth / tileXSize) + 1];

            // Initializes the list of tiles that can be touched by a character
            tangibleTiles = new List<Tile>[tiles.GetLength(1)];

            // Initialize each list in the tangibleTiles array
            for (int index = 0; index < tangibleTiles.GetLength(0); index++)
            {
                tangibleTiles[index] = new List<Tile>();
            }

            // Saves the window dimensions
            this.windowWidth = windowWidth;
            this.windowHeight = windowHeight;

            // Saves the starting position of the window on the tile map
            windowPosition.X = 0;
            windowPosition.Y = 0;
            windowPosition.Width = tiles.GetLength(1);
            windowPosition.Height = tiles.GetLength(0) - 1;

            // Saves references to other objects needed
            this.enemyManager = enemyManager;
            this.projectileManager = projectileManager;
            this.player = player;

            //
            progressionTile = null;
            spawnBossTile = null;
            bossSpawned = false;

        }


        /// <summary>
        /// Method to Load the current level from  txt file. 
        /// </summary>
        public void LoadLevel()
        {
            // Save the tilesheet that is currently being used
            currentTileSheet = levelTileSheets[(int)currentLevel];

            // Sets stream to null first to check for later
            StreamReader input = null;

            // Tries to read the level map and saves it to the map
            try
            {
                // Variables to temp hold file info
                int numRectangles;
                int numMapRows;
                string data;
                string[] recData;

                // opens the stream to the file 
                input = new StreamReader(fileNames[(int)currentLevel]);

                // Read the first line to get the amount of rectangle locations needed
                data = input.ReadLine();
                numRectangles = int.Parse(data);

                tileTextureLocs = new Rectangle[numRectangles];

                // Read the values for the rectangles 
                for (int index = 0; index < numRectangles; index++)
                {
                    data = input.ReadLine();
                    recData = data.Split(',');

                    tileTextureLocs[index] = new Rectangle(int.Parse(recData[0]),
                        int.Parse(recData[1]),
                        int.Parse(recData[2]),
                        int.Parse(recData[3]));
                }

                // Read next line to get map dimensions
                data = input.ReadLine();
                numMapRows = int.Parse(data);

                // Initialize map
                map = new string[numMapRows];

                // Read and save each row of the tile map
                for (int index = 0; index < numMapRows; index++)
                {
                    map[index] = input.ReadLine();
                }

            }
            // If unsuccessful, prints error message
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

            // If input was open, close it
            if (input != null)
            {
                input.Close();
            }

            // Declares rectangle for later use in loop
            Rectangle texturePosition;

            StringBuilder sb;

            // Creates a tile with a different texture depending
            // on the character used and adds it to the array
            // until the window is filled
            for (int row = 0; row < tiles.GetLength(0); row++)
            {
                for (int column = 0; column < tiles.GetLength(1); column++)
                {
                    // Interprets the character to its corresponding rectangle on the tile sheet
                    switch (map[row][column])
                    {
                        // Checks for Air tiles to create
                        case '-':
                            texturePosition = tileTextureLocs[0];
                            break;
                        // Checks for Earth tiles, tangible
                        case 'm':
                            texturePosition = tileTextureLocs[1]; 
                            break;
                        // Checks for Dirt tiles, tangible
                        case 'I':
                            texturePosition = tileTextureLocs[2]; 
                            break;
                        // Checks for Dirt tiles, non-tangible
                        case 'x':
                            texturePosition = tileTextureLocs[3]; 
                            break;
                        // Checks for Enemies to place and air tiles as well
                        case '*':
                            texturePosition = tileTextureLocs[4];
                            enemyManager.AddEnemy(enemyManager.
                                CreateEnemy((int)currentLevel, EnemyManager.enemyType.skeletonWizard, player,
                                column * tileXSize, (row * tileYSize) - 40));
                            sb = new StringBuilder(map[row]);
                            sb[column] = '-';
                            map[row] = sb.ToString();
                            break;
                        case '!':
                            texturePosition = tileTextureLocs[5]; 
                            enemyManager.AddEnemy(enemyManager.
                                CreateEnemy((int)currentLevel, EnemyManager.enemyType.skeletonWarrior, player,
                                column * tileXSize, (row * tileYSize) - 40));
                            sb = new StringBuilder(map[row]);
                            sb[column] = '-';
                            map[row] = sb.ToString();
                            break;
                        case '=':
                            texturePosition = tileTextureLocs[6]; 
                            enemyManager.AddEnemy(enemyManager.
                                CreateEnemy((int)currentLevel, EnemyManager.enemyType.boss, player,
                                column * tileXSize, (row * tileYSize) - 40));
                            sb = new StringBuilder(map[row]);
                            sb[column] = '-';
                            map[row] = sb.ToString();
                            break;
                        case '+':
                            texturePosition = tileTextureLocs[7]; 
                            break;
                        case '_':
                            texturePosition = tileTextureLocs[8];
                            break;
                        // Checks for other Dirt tiles, non-tangible
                        case 'v':
                            texturePosition = tileTextureLocs[9]; 
                            break;
                        // Checks for other Dirt tiles, non-tangible
                        case 'l':
                            texturePosition = tileTextureLocs[10]; 
                            break;
                        default:
                            texturePosition = tileTextureLocs[11]; 
                            break;
                    }

                    // Creates tile with texture chosen from data file in current row and column in map
                    tiles[row, column] = new Tile(currentTileSheet,
                        new Rectangle(column * tileXSize, row * tileYSize, tileXSize, tileYSize), texturePosition);

                    // If the tile borders the air, add it to tangible list
                    if (map[row][column] == 'm' || map[row][column] == 'I' || map[row][column] == '_')
                    {
                        tangibleTiles[column].Add(tiles[row, column]);
                    }

                    // 
                    if (map[row][windowPosition.Right] == '+')
                    {
                        progressionTile = tiles[row, column];

                        if (currentLevel == Levels.Astral)
                        {
                            spawnBossTile = tiles[row, column];
                        }
                    }

                }
            }

        }

        /// <summary>
        /// Method to update the player offset,
        /// then updates all the tile locations.
        /// </summary>
        /// <param name="playerXPos"></param>
        public int Update(int playerXMovement)
        {
            // Saves the list of tiles that intersect with the player 
            List<Tile> intersectingTiles = CharacterIntersectsTiles(player);

            // Checks whether the player intersects with any of the tangible tiles
            for (int i = 0; i < intersectingTiles.Count; i++)
            {
                // Then checks if they collide on left or right side, impeding x movement
                if (player.CollideLeft(intersectingTiles[i]) && playerXMovement < 0)
                {
                    playerXMovement = 0;
                }
                if (player.CollideRight(intersectingTiles[i]) && playerXMovement > 0)
                {
                    playerXMovement = 0;
                }
            }

            if (spawnBossTile != null && !bossSpawned &&
                player.Position.Intersects(spawnBossTile.Position))
            {
                enemyManager.AddEnemy(enemyManager.CreateEnemy(
                    (int)currentLevel, EnemyManager.enemyType.boss, player,
                    spawnBossTile.Position.X - 80,
                    spawnBossTile.Position.Y + 120));

                bossSpawned = true;
            }

            // return the given player's X movement,
            // or the amount of player movement possible if it is lower.
            int possibleTileMovement = 0;

            // If not centered, move player
            if (player.Position.X >= windowWidth / 2 + 10 || player.Position.X <= windowWidth / 2 - 10)
            {
                // if on left of center, moving over center to right,
                // Do possible X movement, then remaining movement is given to tile
                if (player.Position.X < windowWidth / 2 &&
                    player.Position.X + playerXMovement >= windowWidth / 2)
                {

                    possibleTileMovement = (player.Position.X + playerXMovement) - (windowWidth / 2);
                    playerXMovement = (windowWidth / 2) - player.Position.X;
                }
                // If on right of center, moving over center to left,
                // Do possible X movement, then remaining movement is given to tile
                else if (player.Position.X > windowWidth / 2 &&
                    player.Position.X + playerXMovement <= windowWidth / 2)
                {
                    possibleTileMovement = (windowWidth / 2) - (player.Position.X + playerXMovement);
                    playerXMovement = (windowWidth / 2) - player.Position.X;
                }
            }
            // If centered, move tiling
            else
            {
                // Set the possible tile movement to -playerXMovement
                // Because tiles move in opposite direction
                possibleTileMovement = -playerXMovement;

                // When centered player does not move
                playerXMovement = 0;

                // If left column tiles become invisible,
                // Delete left column tiles and create right column tiles
                if (tiles[0, 0].Position.X + possibleTileMovement <= -tileXSize)
                {
                    // If not at map border, create and destroy tiles
                    if (windowPosition.Right != map[0].Length)
                    {
                        MoveTilingRight();
                    }
                    // If at map border, player moves instead, tiles do not
                    else
                    {
                        playerXMovement = -possibleTileMovement;
                        possibleTileMovement = 0;
                    }
                }

                // If right column tiles become invisible,
                // Delete right column tiles and create left column tiles
                else if (tiles[0, 0].Position.X + possibleTileMovement >= 0)
                {
                    // If not at map border, create and destroy tiles
                    if (windowPosition.X != 0)
                    {
                        MoveTilingLeft();
                    }
                    // If at map border, player moves instead, tiles do not
                    else
                    {
                        playerXMovement = -possibleTileMovement;
                        possibleTileMovement = 0;
                    }
                }
            }

            // Updates the position of every tile in the tilemap
            for (int row = 0; row < tiles.GetLength(0); row++)
            {
                for (int column = 0; column < tiles.GetLength(1); column++)
                {
                    tiles[row, column].Update(possibleTileMovement);
                }
            }

            // Makes orb invisible if collected
            if (progressionTile != null && player.IsOrbCollected[(int)currentLevel])
            {
                progressionTile.TextureRect = tileTextureLocs[0];
            }

            // Gives the enemy manager the movement of the level
            enemyManager.LevelBasedMovement(possibleTileMovement);

            // Gives the projectile manager the movement of the level
            projectileManager.LevelBasedMovement(possibleTileMovement);

            // Calls the players gravity and sends the tiles it intersects with
            player.Gravity(intersectingTiles);

            // Sends the list of tangible tiles to the EnemyManager
            enemyManager.EnemyIntersectsTiles(tangibleTiles);

            // Sends the list of tangible tiles to the ProjectileManager
            projectileManager.ProjectileIntersectsTiles(tangibleTiles);

            // If player X movement would take them outside the left side of the window, 
            // make movement equal to distance from left edge of window
            if (player.Position.X + playerXMovement <= 0)
            {
                playerXMovement = playerXMovement - (player.Position.X + playerXMovement);
            }
            // If player X movement would take them outside the right side of the window, 
            // make movement equal to distance from right edge of window
            else if (player.Position.X + player.Position.Width + playerXMovement >= windowWidth)
            {
                playerXMovement = windowWidth - (player.Position.X + player.Position.Width);
            }

            return playerXMovement;
        }

        /// <summary>
        /// Method to loop the entire 2d tile array and call their draw method
        /// </summary>
        /// <param name="sb"></param>
        public void Draw(SpriteBatch sb)
        {
            // Calls Draw() for every tile
            for (int row = 0; row < tiles.GetLength(0); row++)
            {
                for (int column = 0; column < tiles.GetLength(1); column++)
                {
                    tiles[row, column].Draw(sb);
                }
            }
        }

        /// <summary>
        /// Method to move the tiling, by deleting the first column,
        /// creating the last column, and updating the position on
        /// the map.
        /// </summary>
        private void MoveTilingRight()
        {
            // Moves all columns in tile array left once
            // Overwriting first column
            for (int row = 0; row < tiles.GetLength(0); row++)
            {
                for (int column = 0; column < tiles.GetLength(1) - 1; column++)
                {
                    tiles[row, column] = tiles[row, column + 1];
                }
            }

            // Moves all columns in the tangibleTiles array left once
            // Overwriting first column
            for (int column = 0; column < tiles.GetLength(1) - 1; column++)
            {
                tangibleTiles[column] = tangibleTiles[column + 1];
            }

            // Declares rectangle for later use in loop
            Rectangle texturePosition;
            int finalColumnIndex = tiles.GetLength(1) - 1;

            // Initializes the tile list in the last column of the tangibleTiles array
            tangibleTiles[finalColumnIndex] = new List<Tile>();

            StringBuilder sb;

            // Creates new tiles for each row in the last column 
            for (int row = 0; row < tiles.GetLength(0); row++)
            {
                // Interprets the character to its corresponding rectangle on the tile sheet
                switch (map[row][windowPosition.Right])
                {
                    // Checks for Air tiles to create
                    case '-':
                        texturePosition = tileTextureLocs[0]; 
                        break;
                    // Checks for Earth tiles, tangible
                    case 'm':
                        texturePosition = tileTextureLocs[1];
                        break;
                    // Checks for Dirt tiles, tangible
                    case 'I':
                        texturePosition = tileTextureLocs[2]; 
                        break;
                    // Checks for Dirt tiles, non-tangible
                    case 'x':
                        texturePosition = tileTextureLocs[3];
                        break;
                    // Checks for Enemies to place and air tiles as well
                    case '*':
                        texturePosition = tileTextureLocs[4]; 
                        // Creates enemy in location of tile
                        enemyManager.AddEnemy(enemyManager.
                            CreateEnemy((int)currentLevel, EnemyManager.enemyType.skeletonWizard, player,
                            tiles[tiles.GetLength(0) - 1,
                            finalColumnIndex].Position.X + tileXSize,
                            (row * tileYSize) - 40));

                        // Updates the map to air tile instead of enemy
                        sb = new StringBuilder(map[row]);
                        sb[windowPosition.Right] = '-';
                        map[row] = sb.ToString();
                        break;
                    case '!':
                        texturePosition = tileTextureLocs[5]; 
                        // Creates enemy in location of tile
                        enemyManager.AddEnemy(enemyManager.
                            CreateEnemy((int)currentLevel, EnemyManager.enemyType.skeletonWarrior, player,
                            tiles[tiles.GetLength(0) - 1,
                            finalColumnIndex].Position.X + tileXSize,
                            (row * tileYSize) - 40));

                        // Updates the map to air tile instead of enemy
                        sb = new StringBuilder(map[row]);
                        sb[windowPosition.Right] = '-';
                        map[row] = sb.ToString();
                        break;
                    case '=':
                        texturePosition = tileTextureLocs[6];
                        // Creates enemy in location of tile
                        enemyManager.AddEnemy(enemyManager.
                            CreateEnemy((int)currentLevel, EnemyManager.enemyType.boss, player,
                            tiles[tiles.GetLength(0) - 1,
                            finalColumnIndex].Position.X + tileXSize,
                            (row * tileYSize) - 40));

                        // Updates the map to air tile instead of enemy
                        sb = new StringBuilder(map[row]);
                        sb[windowPosition.Right] = '-';
                        map[row] = sb.ToString();
                        break;
                    case '+':
                        texturePosition = tileTextureLocs[7]; 

                        break;
                    case '_':
                        texturePosition = tileTextureLocs[8]; 
                        break;
                    // Checks for other Dirt tiles, non-tangible
                    case 'v':
                        texturePosition = tileTextureLocs[9]; 
                        break;
                    // Checks for other Dirt tiles, non-tangible
                    case 'l':
                        texturePosition = tileTextureLocs[10]; 
                        break;
                    default:
                        texturePosition = tileTextureLocs[11]; 
                        break;
                }

                // Creates tile with texture chosen from data file in current row and column in map
                tiles[row, finalColumnIndex] = new Tile(currentTileSheet,
                    new Rectangle(tiles[tiles.GetLength(0) - 1, finalColumnIndex].Position.X + tileXSize,
                    row * tileYSize, tileXSize, tileYSize), texturePosition);

                // If the tile borders the air, add it to tangible list
                if (map[row][windowPosition.Right] == 'm' || map[row][windowPosition.Right] == 'I' || map[row][windowPosition.Right] == '_')
                {
                    tangibleTiles[finalColumnIndex].Add(tiles[row, finalColumnIndex]);
                }

                // 
                if (map[row][windowPosition.Right] == '+')
                {
                    progressionTile = tiles[row, finalColumnIndex];

                    if (currentLevel == Levels.Astral)
                    {
                        spawnBossTile = tiles[row, finalColumnIndex];
                    }
                }
            }

            // Updates the window position on the map
            windowPosition.X++;
        }

        /// <summary>
        /// Method to move the tiling, by deleting the last column,
        /// creating the first column, and updating the position on
        /// the map.
        /// </summary>
        private void MoveTilingLeft()
        {
            // Moves all columns in tile array right once
            // Overwriting last column
            for (int row = 0; row < tiles.GetLength(0); row++)
            {
                for (int column = tiles.GetLength(1) - 1; column > 0; column--)
                {
                    tiles[row, column] = tiles[row, column - 1];
                }
            }

            // Moves all columns in the tangibleTiles array right once
            // Overwriting last column
            for (int column = tiles.GetLength(1) - 1; column > 0; column--)
            {
                tangibleTiles[column] = tangibleTiles[column - 1];
            }

            // Declares rectangle for later use in loop
            Rectangle texturePosition;

            // Initializes the tile list in the first column of the tangibleTiles array
            tangibleTiles[0] = new List<Tile>();

            StringBuilder sb;

            // Creates new tiles for each row in the last column 
            for (int row = 0; row < tiles.GetLength(0); row++)
            {
                // Interprets the character to its corresponding rectangle on the tile sheet
                switch (map[row][windowPosition.X - 1])
                {
                    // Checks for Air tiles to create
                    case '-':
                        texturePosition = tileTextureLocs[0]; 
                        break;
                    // Checks for Earth tiles, tangible
                    case 'm':
                        texturePosition = tileTextureLocs[1]; 
                        break;
                    // Checks for Dirt tiles, tangible
                    case 'I':
                        texturePosition = tileTextureLocs[2]; 
                        break;
                    // Checks for Dirt tile, non-tangible
                    case 'x':
                        texturePosition = tileTextureLocs[3];
                        break;
                    // Checks for Enemies to place and air tiles as well
                    case '*':
                        texturePosition = tileTextureLocs[4]; 
                        // Creates enemy in location of tile
                        enemyManager.AddEnemy(enemyManager.
                            CreateEnemy((int)currentLevel, EnemyManager.enemyType.skeletonWizard, player,
                            tiles[tiles.GetLength(0) - 1, 0].Position.X -
                            tileXSize, (row * tileYSize) - 40));
                        sb = new StringBuilder(map[row]);
                        sb[windowPosition.X - 1] = '-';
                        map[row] = sb.ToString();
                        break;
                    case '!':
                        texturePosition = tileTextureLocs[5]; 
                        // Creates enemy in location of tile
                        enemyManager.AddEnemy(enemyManager.
                            CreateEnemy((int)currentLevel, EnemyManager.enemyType.skeletonWarrior, player,
                            tiles[tiles.GetLength(0) - 1, 0].Position.X -
                            tileXSize, (row * tileYSize) - 40));
                        sb = new StringBuilder(map[row]);
                        sb[windowPosition.X - 1] = '-';
                        map[row] = sb.ToString();
                        break;
                    case '=':
                        texturePosition = tileTextureLocs[6]; 
                        // Creates enemy in location of tile
                        enemyManager.AddEnemy(enemyManager.
                            CreateEnemy((int)currentLevel, EnemyManager.enemyType.boss, player,
                            tiles[tiles.GetLength(0) - 1, 0].Position.X -
                            tileXSize, (row * tileYSize) - 40));
                        sb = new StringBuilder(map[row]);
                        sb[windowPosition.X - 1] = '-';
                        map[row] = sb.ToString();
                        break;
                    case '+':
                        texturePosition = tileTextureLocs[7]; 
                        break;
                    case '_':
                        texturePosition = tileTextureLocs[8];
                        break;
                    // Checks for other Dirt tiles, non-tangible
                    case 'v':
                        texturePosition = tileTextureLocs[9]; 
                        break;
                    // Checks for other Dirt tiles, non-tangible
                    case 'l':
                        texturePosition = tileTextureLocs[10];
                        break;
                    default:
                        texturePosition = tileTextureLocs[11]; 
                        break;
                }

                // Creates tile with texture chosen from data file in current row and column in map
                tiles[row, 0] = new Tile(currentTileSheet,
                    new Rectangle(tiles[tiles.GetLength(0) - 1, 0].Position.X - tileXSize,
                    row * tileYSize, tileXSize, tileYSize), texturePosition);

                // If the tile borders the air, add it to tangible list
                if (map[row][windowPosition.X - 1] == 'm' || map[row][windowPosition.X - 1] == 'I' || map[row][windowPosition.X - 1] == '_')
                {
                    tangibleTiles[0].Add(tiles[row, 0]);
                }

                //
                if (map[row][windowPosition.X - 1] == '+')
                {
                    progressionTile = tiles[row, 0];

                    if (currentLevel == Levels.Astral)
                    {
                        spawnBossTile = tiles[row, 0];
                    }
                }

            }

            // Updates the window position on the map
            windowPosition.X--;
        }

        /// <summary>
        /// Method to check if the inputted character intersects with any of the tangible tiles,
        /// if so returns a list of them, if not return null. 
        /// </summary>
        /// <returns></returns>
        private List<Tile> CharacterIntersectsTiles(Character character)
        {
            // List of intersecting tangible tiles to later return
            List<Tile> intersectingTiles = new List<Tile>();

            // Checks every tangible tile to see if any intersect with character,
            // If so, add tile to list to later return
            for (int column = 0; column < tangibleTiles.GetLength(0); column++)
            {
                for (int tileNum = 0; tileNum < tangibleTiles[column].Count; tileNum++)
                {
                    if (tangibleTiles[column][tileNum].Position.Intersects(character.Position))
                    {
                        intersectingTiles.Add(tangibleTiles[column][tileNum]);
                    }
                }
            }

            // Returns all the tiles that intersect with the character
            return intersectingTiles;
        }

    }
}