using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace Challenge_TheFountainOfObjects
{
    /// <summary>
    /// Enumeration to say the state of the program
    /// </summary>
    public enum GameState { Prompt, Gameplay, Result }

    /// <summary>
    /// Enumeration to say the diffuculty of the level.
    /// </summary>
    public enum DifficultyLevels { Unchosen, Easy, Medium, Hard }

    /// <summary>
    /// TODO: Add maelstrom behavior
    /// TODO: Do work on color scheme
    /// TODO: Add tutorial screen
    /// </summary>
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Saves window dimensions
        private int windowWidth;
        private int windowHeight;

        // Field to hold state of game and previous state of keyboard
        private GameState state;
        private KeyboardState prevKBState;

        // 2D array to hold the rooms in the grid
        private Room[,] rooms;
        private DifficultyLevels difficulty;

        // Fields to hold the start and fountain room
        private Room startRoom;
        private Room fountainRoom;
        private Room playerRoom;

        // Random object to provide rng for game
        private Random rng = new Random();

        // Assets for rooms
        private Texture2D roomsAsset;

        // Fonts 
        private SpriteFont defaultFont;
        private SpriteFont smallTextFont; 

        // Button info
        private List<Button>[] buttons;

        // Fields to hold whther the player has moved, the level result,
        // and whether the maelstrom has been hit
        private bool hasMoved;
        private bool hasWon;
        private bool hitMaelstrom;

        // Field to hold the text that describes the players surroundings
        private List<String> textDescriptions;

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
            windowWidth = 512;
            _graphics.PreferredBackBufferHeight = windowHeight;
            _graphics.PreferredBackBufferWidth = windowWidth;
            _graphics.ApplyChanges();

            // Start game in prompt state
            state = GameState.Prompt;
            difficulty = DifficultyLevels.Unchosen;

            // Initialize the three lists of buttons
            buttons = new List<Button>[3];
            buttons[0] = new List<Button>();
            buttons[1] = new List<Button>();
            buttons[2] = new List<Button>();

            // Initialize lis of lines to be filled later
            textDescriptions = new List<string>();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            // Load Rooms asset
            roomsAsset = Content.Load<Texture2D>("Rooms");

            // Load fonts
            defaultFont = Content.Load<SpriteFont>("defaultFont");
            smallTextFont = Content.Load<SpriteFont>("textSmallFont");

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

            // Add Gameplay screen buttons
            buttons[1].Add(new Button(GraphicsDevice, null,
                new Rectangle(windowWidth / 6 - mediumButtonSize.X / 2, windowHeight - 45, mediumButtonSize.X, mediumButtonSize.Y),
                Color.Green, "Reset Game", Color.White, defaultFont));
            buttons[1].Add(new Button(GraphicsDevice, null,
                new Rectangle(windowWidth / 2 - mediumButtonSize.X / 2, windowHeight - 45, mediumButtonSize.X, mediumButtonSize.Y),
                Color.Green, "New Game", Color.White, defaultFont));
            buttons[1].Add(new Button(GraphicsDevice, null,
                new Rectangle(5 * windowWidth / 6 - mediumButtonSize.X / 2, windowHeight - 45, mediumButtonSize.X, mediumButtonSize.Y),
                Color.Green, "Main Menu", Color.White, defaultFont));

            buttons[1][0].OnLeftClick += ResetGrid;
            buttons[1][1].OnLeftClick += NewGrid;
            buttons[1][2].OnLeftClick += MainMenu;


            // Add Result screen buttons
            buttons[2] = buttons[1];
            

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            // Get keyboard state
            KeyboardState kbState = Keyboard.GetState();

            // Checks state for what screen to monitor
            switch (state)
            {
                // Accepts difficulty input
                case GameState.Prompt:
                    break;

                // Checks for movements and results
                case GameState.Gameplay:

                    // Update the position based on input keys
                    // and saves when it has been moved
                    hasMoved = CheckWASD(kbState);

                    // If player moved, checks the new room for state changes
                    if (hasMoved)
                    {
                        if (playerRoom.State == RoomState.Maelstrom)
                        {
                            hitMaelstrom = true;
                            playerRoom.Visited = true;

                            if (IsValidPos(new Point(playerRoom.GridPos.X + 2, playerRoom.GridPos.Y + 1)))
                            {
                                playerRoom = rooms[playerRoom.GridPos.X + 2, playerRoom.GridPos.Y + 1];
                            }
                            else
                            {
                                playerRoom = rooms[rooms.GetLength(0) - 1, rooms.GetLength(1) - 1];
                            }
                            
                        }

                        // Check if fountain room reached, if so, revive it
                        if (fountainRoom.State == RoomState.DrippingFountain && playerRoom == fountainRoom)
                        {
                            fountainRoom.State = RoomState.FountainOfObjects;
                        }
                        // Check win condition
                        else if (startRoom == playerRoom && fountainRoom.State == RoomState.FountainOfObjects)
                        {
                            state = GameState.Result;
                            hasWon = true;
                        }
                        // Check lose condition
                        else if (playerRoom.State == RoomState.Amarok || playerRoom.State == RoomState.Pit)
                        {
                            state = GameState.Result;
                            ToggleRoomVisibility(true);
                            hasWon = false;
                        }

                        playerRoom.Visited = true;

                        UpdateTextDescription();
                        
                    }
                    break;

                // Checks for restart input
                case GameState.Result:
                    break;
            }

            // Checks for all button clicks
            foreach (Button button in buttons[(int)state])
            {
                button.Update();
            }

            // Makes the current keyboard state the previous one
            prevKBState = kbState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here

            // Stat spritebatch and inputs PointClamp
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            Vector2 stringSize;

            switch (state)
            {
                // Prompt for difficulty
                case GameState.Prompt:

                    stringSize = defaultFont.MeasureString("The Fountain of Objects");

                    _spriteBatch.DrawString(defaultFont, "The Fountain of Objects!",
                        new Vector2(windowWidth / 2 - stringSize.X / 2, windowHeight / 5), Color.White);

                    stringSize = defaultFont.MeasureString("To play select a difficulty level.");
                    _spriteBatch.DrawString(defaultFont, "To play select a difficulty level.",
                        new Vector2(windowWidth / 2 - stringSize.X / 2, windowHeight / 4), Color.White);

                    // Draws the difficulty, new game, and quit buttons
                    foreach (Button button in buttons[(int)state])
                    {
                        button.Draw(_spriteBatch);
                    }

                    break;

                // Display all the tiles
                case GameState.Gameplay:
                    // Draws the grid
                    foreach (Room room in rooms)
                    {
                        room.Draw(_spriteBatch);
                    }

                    // Overwrites the square with the player
                    playerRoom.DrawPlayer(_spriteBatch);

                    // Draws the reset, new game, quit, and menu buttons
                    foreach (Button button in buttons[(int)state])
                    {
                        button.Draw(_spriteBatch);
                    }

                    for (int lineNum = 0; lineNum < textDescriptions.Count; lineNum++)
                    {
                        _spriteBatch.DrawString(smallTextFont, textDescriptions[lineNum],
                            new Vector2(0, windowWidth + smallTextFont.LineSpacing * lineNum), Color.White);
                    }

                    break;

                // Display the results of the game
                case GameState.Result:
                    // Draws the grid
                    foreach (Room room in rooms)
                    {
                        room.Draw(_spriteBatch);
                    }
                    playerRoom.DrawPlayer(_spriteBatch);

                    // Checks for win and displays results
                    if (hasWon)
                    {
                        stringSize = defaultFont.MeasureString("The Fountain of Objects has been Restored!");

                        _spriteBatch.DrawString(defaultFont, "The Fountain of Objects has been Restored!",
                            new Vector2(windowWidth / 2 - stringSize.X / 2, 4 * windowHeight / 5), Color.White);
                    }
                    else
                    {
                        stringSize = defaultFont.MeasureString("You have Died!");
                        _spriteBatch.DrawString(defaultFont, "You have Died!",
                            new Vector2(windowWidth / 2 - stringSize.X / 2, 4 * windowHeight / 5), Color.White);
                    }

                    // Draws the reset, new game, quit, and menu buttons
                    foreach (Button button in buttons[(int)state])
                    {
                        button.Draw(_spriteBatch);
                    }

                    break;
            }

            // Finishes drawing
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Method to make a new grid and give it its starting values based on size
        /// </summary>
        private void NewGrid()
        {
            // Variable to hold sidelength
            int gridLength = 0;
            int roomLength;
            bool makeGrid = true;
            int numPits = 0;
            int numMaelstroms = 0;
            int numAmaroks = 0;

            // Switch to save sidelength, or not if no difficulty chosen
            switch (difficulty)
            {
                case DifficultyLevels.Easy:
                    gridLength = 4;
                    numPits = 1;
                    numMaelstroms = 1;
                    numAmaroks = 1;
                    break;
                case DifficultyLevels.Medium:
                    gridLength = 6;
                    numPits = 2;
                    numMaelstroms = 1;
                    numAmaroks = 2;
                    break;
                case DifficultyLevels.Hard:
                    gridLength = 8;
                    numPits = 4;
                    numMaelstroms = 2;
                    numAmaroks = 3;
                    break;

                case DifficultyLevels.Unchosen:
                    makeGrid = false;
                    break;
            }

            // Check if grid should be made
            if (makeGrid)
            {
                // Point to use later
                Point randRoom;

                // Initialize grid
                rooms = new Room[gridLength, gridLength];

                // Save room pixel size
                roomLength = windowWidth / gridLength;

                // Populate grid
                for (int row = 0; row < gridLength; row++)
                {
                    for (int col = 0; col < gridLength; col++)
                    {
                        rooms[row, col] = new Room(roomsAsset,
                            new Rectangle(col * roomLength, row * roomLength, roomLength, roomLength),
                            RoomState.Empty, new Point(row, col));
                    }
                }

                // Set start room
                // startRoom = rooms[rng.Next(0, sideLength), rng.Next(0, sideLength)];
                startRoom = rooms[0, 0];
                startRoom.State = RoomState.Start;
                playerRoom = startRoom;

                // Set fountain room 
                
                do
                {
                    fountainRoom = rooms[rng.Next(0, gridLength), rng.Next(0, gridLength)];
                } while (fountainRoom.State != RoomState.Empty);
                
                // fountainRoom = rooms[0, 2];
                fountainRoom.State = RoomState.DrippingFountain;

                // Randomly make a given number of rooms pits
                for (int count = 0; count < numPits; count++)
                {
                    do
                    {
                        randRoom = new Point(rng.Next(0,gridLength), rng.Next(0, gridLength));
                    } while (rooms[randRoom.X,randRoom.Y].State != RoomState.Empty ||
                    randRoom.Equals(new Point(0, 1)) ||
                    randRoom.Equals(new Point(1, 0)));
                    rooms[randRoom.X, randRoom.Y].State = RoomState.Pit;
                }

                // Randomly make a given number of rooms maelstroms
                for (int count = 0; count < numMaelstroms; count++)
                {
                    do
                    {
                        randRoom = new Point(rng.Next(0, gridLength), rng.Next(0, gridLength));
                    } while (rooms[randRoom.X, randRoom.Y].State != RoomState.Empty ||
                    randRoom.Equals(new Point(0, 1)) ||
                    randRoom.Equals(new Point(1, 0)));
                    rooms[randRoom.X, randRoom.Y].State = RoomState.Maelstrom;
                }

                // Randomly make a given number of rooms Amaroks
                for (int count = 0; count < numAmaroks; count++)
                {
                    do
                    {
                        randRoom = new Point(rng.Next(0, gridLength), rng.Next(0, gridLength));
                    } while (rooms[randRoom.X, randRoom.Y].State != RoomState.Empty ||
                    randRoom.Equals(new Point(0, 1)) ||
                    randRoom.Equals(new Point(1, 0)));
                    rooms[randRoom.X, randRoom.Y].State = RoomState.Amarok;
                }

                // Start Gameplay state
                state = GameState.Gameplay;

                // Changes text for new surroundings
                UpdateTextDescription();
            }
        }

        /// <summary>
        /// Method to reset the grid without chanigng the beginning state
        /// </summary>
        private void ResetGrid()
        {
            playerRoom = rooms[0, 0];
            fountainRoom.State = RoomState.DrippingFountain;
            state = GameState.Gameplay;
            ToggleRoomVisibility(false);
        }

        /// <summary>
        /// Method to find out if a key has been pressed
        /// </summary>
        /// <returns></returns>
        private bool KeyPressed(KeyboardState kbState, Keys key)
        {
            return (prevKBState.IsKeyUp(key) && kbState.IsKeyDown(key));
        }

        /// <summary>
        /// Method to check whether a point is a valid point within the grid
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool IsValidPos(Point point)
        {
            return (point.X >= 0 && point.Y >= 0 &&
                point.X < rooms.GetLength(0) && point.Y < rooms.GetLength(1));
        }

        /// <summary>
        /// Checks the WASD and arrows for movement and updates position
        /// </summary>
        /// <param name="kbState"></param>
        /// <returns></returns>
        private bool CheckWASD(KeyboardState kbState)
        {
            bool madeValidMove = false;

            // Check up keys
            if ((KeyPressed(kbState, Keys.W) || KeyPressed(kbState, Keys.Up)) &&
                IsValidPos(new Point(playerRoom.GridPos.X - 1, playerRoom.GridPos.Y)))
            {
                playerRoom = rooms[playerRoom.GridPos.X - 1, playerRoom.GridPos.Y];
                madeValidMove = true;
            }

            // Check down keys
            else if ((KeyPressed(kbState, Keys.S) || KeyPressed(kbState, Keys.Down)) &&
                IsValidPos(new Point(playerRoom.GridPos.X + 1, playerRoom.GridPos.Y)))
            {
                playerRoom = rooms[playerRoom.GridPos.X + 1, playerRoom.GridPos.Y];
                madeValidMove = true;
            }

            // Check left keys
            else if ((KeyPressed(kbState, Keys.A) || KeyPressed(kbState, Keys.Left)) &&
                IsValidPos(new Point(playerRoom.GridPos.X, playerRoom.GridPos.Y - 1)))
            {
                playerRoom = rooms[playerRoom.GridPos.X, playerRoom.GridPos.Y - 1];
                madeValidMove = true;
            }

            // Check right keys
            else if ((KeyPressed(kbState, Keys.D) || KeyPressed(kbState, Keys.Right)) &&
                IsValidPos(new Point(playerRoom.GridPos.X, playerRoom.GridPos.Y + 1)))
            {
                playerRoom = rooms[playerRoom.GridPos.X, playerRoom.GridPos.Y + 1];
                madeValidMove = true;
            }

            return madeValidMove;
        }

        /// <summary>
        /// Sets difficulty mode to easy
        /// </summary>
        private void SetDifficultyEasy()
        {
            difficulty = DifficultyLevels.Easy;
            buttons[0][1].IsSelected = false;
            buttons[0][2].IsSelected = false;
            buttons[0][0].IsSelected = true;
        }

        /// <summary>
        /// Sets difficulty mode to medium
        /// </summary>
        private void SetDifficultyMedium()
        {
            difficulty = DifficultyLevels.Medium;
            buttons[0][0].IsSelected = false;
            buttons[0][2].IsSelected = false;
            buttons[0][1].IsSelected = true;
        }

        /// <summary>
        /// Sets difficulty mode to hard
        /// </summary>
        private void SetDifficultyHard()
        {
            difficulty = DifficultyLevels.Hard;
            buttons[0][0].IsSelected = false;
            buttons[0][1].IsSelected = false;
            buttons[0][2].IsSelected = true;
        }

        /// <summary>
        /// Changes state to prompt screen
        /// </summary>
        private void MainMenu()
        {
            state = GameState.Prompt;
            buttons[0][0].IsSelected = false;
            buttons[0][1].IsSelected = false;
            buttons[0][2].IsSelected = false;
        }

        /// <summary>
        /// Quits the program
        /// </summary>
        private void QuitGame()
        {
            this.Exit();
        }

        /// <summary>
        /// Method to update the text descriptions for the surroundings
        /// </summary>
        private void UpdateTextDescription()
        {
            // Clears the list of text
            textDescriptions.Clear();

            // Checks for maelstrom teleport
            if (hitMaelstrom)
            {
                textDescriptions.Add("You have hit a maelstrom and been blown to another room!");
            }

            // If in fountain room
            if (playerRoom == fountainRoom)
            {
                textDescriptions.Add("You have found the Fountain! It has been restored!");
            }

            // If pit near, add text
            if (CheckSurroundingsFor(RoomState.Pit))
            {
                textDescriptions.Add("You feel a draft. There is a pit in a nearby room.");
            }

            // If maelstrom near, add text
            if (CheckSurroundingsFor(RoomState.Maelstrom))
            {
                textDescriptions.Add("You hear the growling and groaning of a maelstrom nearby.");
            }

            // If amarok near, add text
            if (CheckSurroundingsFor(RoomState.Amarok))
            {
                textDescriptions.Add("You can smell the rotten stench of an amarok in a nearby room.");
            }

            // If broken fountain near, add text
            if (CheckSurroundingsFor(RoomState.DrippingFountain))
            {
                textDescriptions.Add("You hear water dripping nearby. The Fountain must be close!");
            }

            // If fixed fountain near, add text
            if (CheckSurroundingsFor(RoomState.FountainOfObjects))
            {
                textDescriptions.Add("You hear the rushing water of the restored fountain nearby!");
            }

            // Resets hitMaelstrom to false
            hitMaelstrom = false;

        }

        /// <summary>
        /// Method to check the surroundings of the player for a specific RoomState
        /// </summary>
        /// <param name="roomState"></param>
        /// <returns></returns>
        private bool CheckSurroundingsFor(RoomState roomState)
        {
            Point point;
            bool isNear = false;
            int[] xRange = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] yRange = { -1, -1, -1, 0, 0, 1, 1, 1 };

            for (int count = 0; count < 8 && !isNear; count++)
            {
                point = new Point(playerRoom.GridPos.X + xRange[count],
                    playerRoom.GridPos.Y + yRange[count]);
                if (IsValidPos(point) && rooms[point.X, point.Y].State == roomState)
                {
                    isNear = true;
                    break;
                }
            }

            return isNear;
        }

        /// <summary>
        /// Method to make the rooms visibility the given value
        /// </summary>
        /// <param name="visible"></param>
        private void ToggleRoomVisibility(bool visible)
        {
            foreach (Room room in rooms)
            {
                // Changes room visibility, except for empty's when going visible
                if (!visible || room.State != RoomState.Empty)
                    room.Visited = visible;
            }
            if (visible)
                fountainRoom.State = RoomState.FountainOfObjects;
        }

    }
}
