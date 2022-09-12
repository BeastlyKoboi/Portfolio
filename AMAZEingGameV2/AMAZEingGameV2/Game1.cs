using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AMAZEingGameV2
{
    public enum GameState { Prompt, Gameplay, Result }

    //
    public enum Difficulty { Easy, Medium, Hard }

    /// <summary>
    /// TODO: Create finite state machine for maze game
    ///     TODO: Create menu for maze generation
    ///     TODO: Create result screen
    /// TODO: Create second maze generation method
    /// TODO: Make win condition
    /// TODO: Make Reset and New maze keys
    /// 
    /// </summary>
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Saves the current window dimensions
        private int windowWidth;
        private int windowHeight;

        // Field to hold the maze manager
        private MazeManager mazeManager;

        // Field for saving keyboard input
        private KeyboardState prevKbState;

        // Field to hold the game state
        private GameState gameState;
        private Difficulty difficulty;

        // Fonts to use
        private SpriteFont defaultFont;

        // Saves the menus in the game
        private Menu[] menus;



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
            // Set window size
            windowHeight = 700;
            windowWidth = 1200;
            _graphics.PreferredBackBufferHeight = windowHeight;
            _graphics.PreferredBackBufferWidth = windowWidth;
            _graphics.ApplyChanges();

            // Create the maze
            mazeManager = new MazeManager(GraphicsDevice);

            // Set Default game state
            gameState = GameState.Prompt;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            // Load fonts
            defaultFont = Content.Load<SpriteFont>("defaultFont");

            // Create the menus
            menus = CreateMenus();

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            // Gets the keyboard state
            KeyboardState kbState = Keyboard.GetState();

            //
            switch (gameState)
            {
                case GameState.Prompt:
                    if (KeyPressed(kbState, Keys.N))
                    {
                        mazeManager.CreateDFSMaze(45, 45, 150, 0,
                            GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                        gameState = GameState.Gameplay;
                    }
                    break;

                case GameState.Gameplay:
                    if (KeyPressed(kbState, Keys.M))
                    {
                        gameState = GameState.Prompt;
                    }
                    if (KeyPressed(kbState, Keys.N))
                    {
                        mazeManager.CreateDFSMaze(45, 45, 0, 0,
                            GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                    }
                    if (KeyPressed(kbState, Keys.R))
                    {
                        mazeManager.ResetAllVertices();
                    }

                    // Read movement input 

                    mazeManager.Update();



                    break;

                case GameState.Result:
                    if (KeyPressed(kbState, Keys.M))
                    {
                        gameState = GameState.Prompt;
                    }
                    if (KeyPressed(kbState, Keys.N))
                    {
                        mazeManager.CreateDFSMaze(45, 45, 0, 0,
                            GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                    }
                    if (KeyPressed(kbState, Keys.R))
                    {
                        mazeManager.ResetAllVertices();
                        gameState = GameState.Gameplay;
                    }
                    break;
            }

            

            // Save the keyboard state for next update
            prevKbState = kbState;


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            // Draw the puzzle
            _spriteBatch.Begin();

            switch (gameState)
            {
                case GameState.Prompt:
                    
                    break;
                
                case GameState.Gameplay:
                    mazeManager.DrawMaze(_spriteBatch);
                    break;

                case GameState.Result:
                    mazeManager.DrawMaze(_spriteBatch);
                    break;
            }

            
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Method to find out if a key has been pressed
        /// </summary>
        /// <returns></returns>
        private bool KeyPressed(KeyboardState kbState, Keys key)
        {
            return (prevKbState.IsKeyUp(key) && kbState.IsKeyDown(key));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Menu[] CreateMenus()
        {
            //Initialize the array of menus
            Menu[] menus = new Menu[3];
            Rectangle mainMenuRect = new Rectangle(0, 0, windowWidth, windowHeight);
            Rectangle sideMenuRect = new Rectangle(0, 0, windowWidth/3, windowHeight);

            // Initialize the three lists of buttons
            List<Button>[] menuButtons = new List<Button>[3];
            menuButtons[0] = new List<Button>();
            menuButtons[1] = new List<Button>();
            menuButtons[2] = new List<Button>();

            // Initialize button size 
            Point largeButtonSize = new Point(200, 50);
            Point mediumButtonSize = new Point(160, 40);
            Point smallButtonSize = new Point(100, 30);

            // Initialize first menu buttons
            menuButtons[0].Add(new Button(GraphicsDevice, null,
                new Rectangle(windowWidth / 4 - smallButtonSize.X / 2, 2 * windowHeight / 5, smallButtonSize.X, smallButtonSize.Y),
                Color.LimeGreen, "Easy", Color.White, defaultFont));
            menuButtons[0].Add(new Button(GraphicsDevice, null,
                new Rectangle(windowWidth / 2 - smallButtonSize.X / 2, 2 * windowHeight / 5, smallButtonSize.X, smallButtonSize.Y),
                Color.LimeGreen, "Medium", Color.White, defaultFont));
            menuButtons[0].Add(new Button(GraphicsDevice, null,
                new Rectangle(3 * windowWidth / 4 - smallButtonSize.X / 2, 2 * windowHeight / 5, smallButtonSize.X, smallButtonSize.Y),
                Color.LimeGreen, "Hard", Color.White, defaultFont));
            menuButtons[0].Add(new Button(GraphicsDevice, null,
                new Rectangle(windowWidth / 2 - largeButtonSize.X / 2, 3 * windowHeight / 5, largeButtonSize.X, largeButtonSize.Y),
                Color.Green, "New Game", Color.White, defaultFont));
            menuButtons[0].Add(new Button(GraphicsDevice, null,
                new Rectangle(windowWidth / 2 - largeButtonSize.X / 2, 4 * windowHeight / 5, largeButtonSize.X, largeButtonSize.Y),
                Color.Red, "Quit", Color.White, defaultFont));

            // Add delegates to first menu buttons
            menuButtons[0][0].OnLeftClick += SetDifficultyEasy;
            menuButtons[0][1].OnLeftClick += SetDifficultyMedium;
            menuButtons[0][2].OnLeftClick += SetDifficultyHard;
            menuButtons[0][3].OnLeftClick += NewGrid;
            menuButtons[0][4].OnLeftClick += QuitGame;


            // Initialize second menu buttons


            // Add delegates to second menu buttons



            // Initialize third menu buttons


            // Add delegates to third menu buttons




            // Initialize each menu


            return menus;
        }

        /// <summary>
        /// Sets difficulty mode to easy
        /// </summary>
        private void SetDifficultyEasy()
        {
            difficulty = Difficulty.Easy;
            menus[0].menuButtons[1].IsSelected = false;
            menus[0].menuButtons[2].IsSelected = false;
            menus[0].menuButtons[0].IsSelected = true;
        }

        /// <summary>
        /// Sets difficulty mode to medium
        /// </summary>
        private void SetDifficultyMedium()
        {
            difficulty = Difficulty.Medium;
            menus[0].menuButtons[0].IsSelected = false;
            menus[0].menuButtons[2].IsSelected = false;
            menus[0].menuButtons[1].IsSelected = true;
        }

        /// <summary>
        /// Sets difficulty mode to hard
        /// </summary>
        private void SetDifficultyHard()
        {
            difficulty = Difficulty.Hard;
            menus[0].menuButtons[0].IsSelected = false;
            menus[0].menuButtons[1].IsSelected = false;
            menus[0].menuButtons[2].IsSelected = true;
        }

        /// <summary>
        /// Changes state to prompt screen
        /// </summary>
        private void MainMenu()
        {
            gameState = GameState.Prompt;
            menus[0].menuButtons[0].IsSelected = false;
            menus[0].menuButtons[1].IsSelected = false;
            menus[0].menuButtons[2].IsSelected = false;
        }

        //
        private void NewGrid()
        {
            //
            switch (difficulty)
            {
                case Difficulty.Easy:
                    mazeManager.CreateDFSMaze(25, 25, 150, 0,
                            GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

                    break;

                case Difficulty.Medium:
                    mazeManager.CreateDFSMaze(35, 35, 150, 0,
                            GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

                    break;

                case Difficulty.Hard:
                    mazeManager.CreateDFSMaze(65, 65, 150, 0,
                            GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

                    break;
            }




            gameState = GameState.Gameplay;
        }


        /// <summary>
        /// Quits the program
        /// </summary>
        private void QuitGame()
        {
            this.Exit();
        }
    }

    
}
