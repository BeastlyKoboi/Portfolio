using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Minesweeper
{
    /// <summary>
    /// Enum to have all the states of the game
    /// </summary>
    public enum GameState { Menu, Gameplay, Result }

    //
    public enum Difficulty { Easy, Medium, Hard }

    /// <summary>
    /// TODO: Make the button positions also change when window size changes
    /// TODO: Make Lose condition and loss screen
    /// </summary>
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Saves the current window dimensions
        private int windowWidth;
        private int windowHeight;

        //
        private Random rng;

        // Holds the current state of the game
        private GameState gameState;

        //
        private Difficulty difficulty;

        // 
        private MouseState prevMouseState;

        // Button info
        private List<Button>[] buttons;

        // Holds the grid, and the tile info
        private Texture2D tileTextures;
        private Texture2D bckTexture;
        private Rectangle bckPos;
        private Tile[,] grid;
        private Point tileSize;

        // fonts
        SpriteFont defaultFont;
        SpriteFont smallTextFont;
        SpriteFont titleFont;

        //
        bool hasWon;

        //
        private List<Tile> mineTiles;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            // Set window size
            windowHeight = 700;
            windowWidth = 960;
            _graphics.PreferredBackBufferHeight = windowHeight;
            _graphics.PreferredBackBufferWidth = windowWidth;
            _graphics.ApplyChanges();

            // Sets beginning game state
            gameState = GameState.Menu;

            rng = new Random();

            tileSize = new Point(32, 32);


            // Set grid background color
            int[] colorData = { (int)Color.Beige.PackedValue };
            bckTexture = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            bckTexture.SetData<Int32>(colorData, 0, 1);
            bckPos = new Rectangle(0, 0, windowWidth, windowHeight);

            // Initialize the three lists of buttons
            buttons = new List<Button>[3];
            buttons[0] = new List<Button>();
            buttons[1] = new List<Button>();
            buttons[2] = new List<Button>();

            hasWon = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            // Load Textures
            tileTextures = Content.Load<Texture2D>("TileTextures");


            // Load fonts
            defaultFont = Content.Load<SpriteFont>("defaultFont");
            smallTextFont = Content.Load<SpriteFont>("smallTextFont");
            titleFont = Content.Load<SpriteFont>("titleFont");

            // Initialize button size 
            Point largeButtonSize = new Point(200, 50);
            Point mediumButtonSize = new Point(160, 40);
            Point smallButtonSize = new Point(100, 30);

            // Initialize each state's buttons and add methods to their events
            // Add Prompt screen buttons and left click behavior
            buttons[0].Add(new Button(GraphicsDevice, null,
                new Rectangle(windowWidth / 4 - smallButtonSize.X / 2, 2 * windowHeight / 5, smallButtonSize.X, smallButtonSize.Y),
                Color.LimeGreen, "Easy", Color.White, smallTextFont));
            buttons[0].Add(new Button(GraphicsDevice, null,
                new Rectangle(windowWidth / 2 - smallButtonSize.X / 2, 2 * windowHeight / 5, smallButtonSize.X, smallButtonSize.Y),
                Color.LimeGreen, "Medium", Color.White, smallTextFont));
            buttons[0].Add(new Button(GraphicsDevice, null,
                new Rectangle(3 * windowWidth / 4 - smallButtonSize.X / 2, 2 * windowHeight / 5, smallButtonSize.X, smallButtonSize.Y),
                Color.LimeGreen, "Hard", Color.White, smallTextFont));
            buttons[0].Add(new Button(GraphicsDevice, null,
                new Rectangle(windowWidth / 2 - largeButtonSize.X / 2, 3 * windowHeight / 5, largeButtonSize.X, largeButtonSize.Y),
                Color.Green, "New Game", Color.White, defaultFont));
            buttons[0].Add(new Button(GraphicsDevice, null,
                new Rectangle(windowWidth / 2 - largeButtonSize.X / 2, 4 * windowHeight / 5, largeButtonSize.X, largeButtonSize.Y),
                Color.Red, "Quit", Color.White, defaultFont));

            buttons[0][0].OnLeftClick += SetDifficultyEasy;
            buttons[0][1].OnLeftClick += SetDifficultyMedium;
            buttons[0][2].OnLeftClick += SetDifficultyHard;
            buttons[0][3].OnLeftClick += NewGrid;
            buttons[0][4].OnLeftClick += QuitGame;


        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            // 
            MouseState mouseState = Mouse.GetState();

            //
            switch (gameState)
            {
                case GameState.Menu:

                    break;

                case GameState.Gameplay:
                    if (mouseState.LeftButton == ButtonState.Pressed ||
                        mouseState.RightButton == ButtonState.Pressed ||
                        LeftClicked(mouseState) || RightClicked(mouseState))
                    {
                        // Updates every tile
                        for (int row = 0; row < grid.GetLength(0); row++)
                        {
                            for (int col = 0; col < grid.GetLength(1); col++)
                            {
                                grid[row, col].Update();
                            }
                        }

                        // Check if hasn't lost
                        if (hasWon)
                        {
                            bool allMinesFlagged = true;

                            // check if all mines are covered and flagged
                            foreach (Tile mine in mineTiles)
                            {
                                if (!mine.IsCovered || !mine.IsFlagged)
                                {
                                    allMinesFlagged = false;
                                    break;
                                }
                            }

                            if (allMinesFlagged)
                                gameState = GameState.Result;
                        }
                    }
                    break;

                case GameState.Result:

                    break;
            }


            // Checks for all button clicks
            foreach (Button button in buttons[(int)gameState])
            {
                button.Update();
            }


            prevMouseState = mouseState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            // Stat spritebatch and inputs PointClamp
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            Vector2 stringSize;


            // 
            switch (gameState)
            {
                case GameState.Menu:
                    stringSize = titleFont.MeasureString("MINESWEEPER!");

                    _spriteBatch.DrawString(titleFont, "MINESWEEPER!",
                        new Vector2(windowWidth / 2 - stringSize.X / 2, windowHeight / 6), Color.LightGreen);

                    stringSize = defaultFont.MeasureString("To play select a difficulty level.");
                    _spriteBatch.DrawString(defaultFont, "To play select a difficulty level.",
                        new Vector2(windowWidth / 2 - stringSize.X / 2, windowHeight / 3), Color.White);

                    stringSize = defaultFont.MeasureString("Left-Click to uncover, Right-Click to Flag");
                    _spriteBatch.DrawString(defaultFont, "Left-Click to uncover, Right-Click to Flag",
                        new Vector2(windowWidth / 2 - stringSize.X / 2, windowHeight / 2), Color.White);

                    break;

                case GameState.Gameplay:

                    _spriteBatch.Draw(bckTexture, bckPos, Color.White);

                    // Draws every tile
                    foreach (Tile tile in grid)
                    {
                        tile.Draw(_spriteBatch);
                    }

                    break;

                case GameState.Result:

                    _spriteBatch.Draw(bckTexture, bckPos, Color.White);

                    // Draws every tile
                    foreach (Tile tile in grid)
                    {
                        tile.Draw(_spriteBatch);
                    }

                    if (hasWon)
                    {
                        stringSize = titleFont.MeasureString("WIN!");
                        _spriteBatch.DrawString(titleFont,"WIN!", new Vector2(windowWidth / 2 - stringSize.X / 2, windowHeight / 4 - stringSize.Y / 2), Color.Blue);
                    }
                    else
                    {
                        stringSize = titleFont.MeasureString("LOSE!");
                        _spriteBatch.DrawString(titleFont, "LOSE!", new Vector2(windowWidth / 2 - stringSize.X / 2, windowHeight / 4 - stringSize.Y / 2), Color.Red);
                    }

                    break;
            }

            // Checks for all button clicks
            foreach (Button button in buttons[(int)gameState])
            {
                button.Draw(_spriteBatch);
            }


            // Finishes drawing
            _spriteBatch.End();

            base.Draw(gameTime);
        }


        /// <summary>
        /// Method to check whether tile has been left clicked
        /// </summary>
        /// <param name="mouseState"></param>
        /// <returns></returns>
        private bool LeftClicked(MouseState mouseState)
        {
            return (prevMouseState.LeftButton == ButtonState.Pressed &&
                mouseState.LeftButton != ButtonState.Pressed);
        }

        /// <summary>
        /// Method to check whether tile has been right clicked
        /// </summary>
        /// <param name="mouseState"></param>
        /// <returns></returns>
        private bool RightClicked(MouseState mouseState)
        {
            return (prevMouseState.RightButton == ButtonState.Pressed &&
                mouseState.RightButton != ButtonState.Pressed);
        }

        //
        private void NewGrid()
        {
            // Initialize button size 
            Point largeButtonSize = new Point(200, 50);
            Point mediumButtonSize = new Point(160, 40);
            Point smallButtonSize = new Point(100, 30);

            //
            int numMines = 0;
            Point chosenBtnSize = mediumButtonSize;
            Point randTile;
            mineTiles = new List<Tile>();

            //
            switch (difficulty)
            {
                case Difficulty.Easy:
                    // initialize grid 
                    grid = new Tile[10, 10];
                    numMines = 10;
                    chosenBtnSize = smallButtonSize;
                    ChangeWindowSize(320, 400);
                    bckPos.Width = 10 * tileSize.X;
                    bckPos.Height = 10 * tileSize.Y;
                    break;

                case Difficulty.Medium:
                    // initialize grid 
                    grid = new Tile[16, 16];
                    numMines = 40;
                    chosenBtnSize = mediumButtonSize;
                    ChangeWindowSize(512, 600);
                    bckPos.Width = 16 * tileSize.X;
                    bckPos.Height = 16 * tileSize.Y;
                    break;

                case Difficulty.Hard:
                    // initialize grid 
                    grid = new Tile[30, 16];
                    numMines = 99;
                    chosenBtnSize = largeButtonSize;
                    ChangeWindowSize(960, 600);
                    bckPos.Width = 30 * tileSize.X;
                    bckPos.Height = 16 * tileSize.Y;
                    break;
            }

            buttons[1] = new List<Button>();

            // Add Gameplay screen buttons
            buttons[1].Add(new Button(GraphicsDevice, null,
                new Rectangle(windowWidth / 6 - chosenBtnSize.X / 2, windowHeight - chosenBtnSize.Y - 15, chosenBtnSize.X, chosenBtnSize.Y),
                Color.Green, "Reset Game", Color.White, defaultFont));
            buttons[1].Add(new Button(GraphicsDevice, null,
                new Rectangle(windowWidth / 2 - chosenBtnSize.X / 2, windowHeight - chosenBtnSize.Y - 15, chosenBtnSize.X, chosenBtnSize.Y),
                Color.Green, "New Game", Color.White, defaultFont));
            buttons[1].Add(new Button(GraphicsDevice, null,
                new Rectangle(5 * windowWidth / 6 - chosenBtnSize.X / 2, windowHeight - chosenBtnSize.Y - 15, chosenBtnSize.X, chosenBtnSize.Y),
                Color.Green, "Main Menu", Color.White, defaultFont));
            
            buttons[1][0].OnLeftClick += ResetGrid;
            buttons[1][1].OnLeftClick += NewGrid;
            buttons[1][2].OnLeftClick += MainMenu;

            // Add Result screen buttons
            buttons[2] = buttons[1];


            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int col = 0; col < grid.GetLength(1); col++)
                {
                    grid[row, col] = new Tile(tileTextures,
                        new Rectangle(row * tileSize.X, col * tileSize.Y, tileSize.X, tileSize.Y),
                        new Point(row, col), defaultFont);
                }
            }

            for (int count = 0; count < numMines; count++)
            {
                do
                {
                    randTile = new Point(rng.Next(grid.GetLength(0)), rng.Next(grid.GetLength(1)));
                } while (grid[randTile.X,randTile.Y].Type == TileType.Mine);
                grid[randTile.X, randTile.Y].Type = TileType.Mine;
                mineTiles.Add(grid[randTile.X, randTile.Y]);
            }

            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int col = 0; col < grid.GetLength(1); col++)
                {
                    grid[row, col].NumMinesNear = CountSurroundingMines(new Point(row, col));
                }
            }

            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int col = 0; col < grid.GetLength(1); col++)
                {
                    grid[row, col].OnLeftClick += UncoverTile;
                    grid[row, col].OnLeftClick += UncoverMines;
                }
            }

            /*
            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int col = 0; col < grid.GetLength(1); col++)
                {
                    grid[row, col].OnRightClick += ;
                }
            }*/

            gameState = GameState.Gameplay; 
        }

        /// <summary>
        /// Method to reset the grid to beginning 
        /// state and move to gameplay again. 
        /// </summary>
        public void ResetGrid()
        {
            // Goes back to gameplay and resets hasWon
            gameState = GameState.Gameplay;
            hasWon = true;

            // Re-covers every tile and removes flags
            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int col = 0; col < grid.GetLength(1); col++)
                {
                    grid[row, col].IsCovered = true;
                    grid[row, col].IsFlagged = false;
                }
            }
        }

        /// <summary>
        /// Method to check whether a point is a valid point within the grid
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool IsValidPos(Point point)
        {
            return (point.X >= 0 && point.Y >= 0 &&
                point.X < grid.GetLength(0) && point.Y < grid.GetLength(1));
        }

        /// <summary>
        /// Counts the number of mines that surround the given tile. 
        /// </summary>
        /// <param name="tileLoc"></param>
        /// <returns></returns>
        private int CountSurroundingMines(Point tileLoc)
        {
            int numMines = 0;

            // Top left
            if (IsValidPos(new Point(tileLoc.X - 1, tileLoc.Y - 1)) &&
                grid[tileLoc.X - 1, tileLoc.Y - 1].Type == TileType.Mine)
            {
                numMines++;
            }

            // Top middle
            if (IsValidPos(new Point(tileLoc.X, tileLoc.Y - 1)) &&
                grid[tileLoc.X, tileLoc.Y - 1].Type == TileType.Mine)
            {
                numMines++;
            }

            // Top right
            if (IsValidPos(new Point(tileLoc.X + 1, tileLoc.Y - 1)) &&
                grid[tileLoc.X + 1, tileLoc.Y - 1].Type == TileType.Mine)
            {
                numMines++;
            }

            // Middle Left
            if (IsValidPos(new Point(tileLoc.X - 1, tileLoc.Y)) &&
                grid[tileLoc.X - 1, tileLoc.Y].Type == TileType.Mine)
            {
                numMines++;
            }

            // Middle Right
            if (IsValidPos(new Point(tileLoc.X + 1, tileLoc.Y)) &&
                grid[tileLoc.X + 1, tileLoc.Y].Type == TileType.Mine)
            {
                numMines++;
            }

            // Bottom Left
            if (IsValidPos(new Point(tileLoc.X - 1, tileLoc.Y + 1)) &&
                grid[tileLoc.X - 1, tileLoc.Y + 1].Type == TileType.Mine)
            {
                numMines++;
            }

            // Bottom Middle 
            if (IsValidPos(new Point(tileLoc.X, tileLoc.Y + 1)) &&
                grid[tileLoc.X, tileLoc.Y + 1].Type == TileType.Mine)
            {
                numMines++;
            }

            //Bottom Right
            if (IsValidPos(new Point(tileLoc.X + 1, tileLoc.Y + 1)) &&
                grid[tileLoc.X + 1, tileLoc.Y + 1].Type == TileType.Mine)
            {
                numMines++;
            }

            return numMines;
        }

        /// <summary>
        /// Method to uncover connected empty tiles in the grid
        /// Base Case: Tile's num is 1 or more, or is a mine
        /// Recursive Case: Tile is empty
        /// State Change: The x, y of tile
        /// </summary>
        /// <param name="point"></param>
        private void UncoverTile(Point point)
        {

            if (grid[point.X, point.Y].Type == TileType.Grass && grid[point.X, point.Y].IsCovered)
            {
                grid[point.X, point.Y].IsCovered = false;

                if (grid[point.X, point.Y].NumMinesNear == 0)
                {
                    // Top left
                    if (IsValidPos(new Point(point.X - 1, point.Y - 1)))
                    {
                        UncoverTile(new Point(point.X - 1, point.Y - 1));
                    }

                    // Top middle
                    if (IsValidPos(new Point(point.X, point.Y - 1)))
                    {
                        UncoverTile(new Point(point.X, point.Y - 1));
                    }

                    // Top right
                    if (IsValidPos(new Point(point.X + 1, point.Y - 1)))
                    {
                        UncoverTile(new Point(point.X + 1, point.Y - 1));
                    }

                    // Middle Left
                    if (IsValidPos(new Point(point.X - 1, point.Y)))
                    {
                        UncoverTile(new Point(point.X - 1, point.Y));
                    }

                    // Middle Right
                    if (IsValidPos(new Point(point.X + 1, point.Y)))
                    {
                        UncoverTile(new Point(point.X + 1, point.Y));
                    }

                    // Bottom left
                    if (IsValidPos(new Point(point.X - 1, point.Y + 1)))
                    {
                        UncoverTile(new Point(point.X - 1, point.Y + 1));
                    }

                    // Bottom Middle 
                    if (IsValidPos(new Point(point.X, point.Y + 1)))
                    {
                        UncoverTile(new Point(point.X, point.Y + 1));
                    }

                    // Bottom right
                    if (IsValidPos(new Point(point.X + 1, point.Y + 1)))
                    {
                        UncoverTile(new Point(point.X + 1, point.Y + 1));
                    }
                }
            }
        }

        /// <summary>
        /// Method to uncover every mine in the grid, 
        /// set the game state to result page, and 
        /// change hasWon to false.
        /// </summary>
        /// <param name="point"></param>
        private void UncoverMines(Point point)
        {
            if (grid[point.X, point.Y].Type == TileType.Mine)
            {
                foreach (Tile tile in mineTiles)
                {
                    tile.IsCovered = false;
                }

                gameState = GameState.Result;
                hasWon = false;
            }

        }

        /// <summary>
        /// Sets difficulty mode to easy
        /// </summary>
        private void SetDifficultyEasy()
        {
            difficulty = Difficulty.Easy;
            buttons[0][1].IsSelected = false;
            buttons[0][2].IsSelected = false;
            buttons[0][0].IsSelected = true;
        }

        /// <summary>
        /// Sets difficulty mode to medium
        /// </summary>
        private void SetDifficultyMedium()
        {
            difficulty = Difficulty.Medium;
            buttons[0][0].IsSelected = false;
            buttons[0][2].IsSelected = false;
            buttons[0][1].IsSelected = true;
        }

        /// <summary>
        /// Sets difficulty mode to hard
        /// </summary>
        private void SetDifficultyHard()
        {
            difficulty = Difficulty.Hard;
            buttons[0][0].IsSelected = false;
            buttons[0][1].IsSelected = false;
            buttons[0][2].IsSelected = true;
        }

        /// <summary>
        /// Changes state to prompt screen
        /// </summary>
        private void MainMenu()
        {
            gameState = GameState.Menu;
            buttons[0][0].IsSelected = false;
            buttons[0][1].IsSelected = false;
            buttons[0][2].IsSelected = false;
            ChangeWindowSize(960, 700);
        }

        /// <summary>
        /// Quits the program
        /// </summary>
        private void QuitGame()
        {
            this.Exit();
        }

        /// <summary>
        /// Change the window size to match given width and height. 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void ChangeWindowSize(int width, int height)
        {
            // Set window size
            windowHeight = height;
            windowWidth = width;
            _graphics.PreferredBackBufferHeight = windowHeight;
            _graphics.PreferredBackBufferWidth = windowWidth;
            _graphics.ApplyChanges();
        }
    }

}
