using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Spellblade
{
    /// <summary>
    /// Enum to know all the different GameStates possible
    /// </summary>
    enum GameState { MainMenu, Directions, LevelSelect, Gameplay, Pause, ProMenu, LevelResult, Quit }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Fields to hold the window dimensions
        private int windowWidth = 1920;
        private int windowHeight = 800;

        // Field to hold the current GameState and whether previous state was pause
        private GameState gameState;
        private bool stateWasPause;
        private bool hasWon;

        // Fields to remember previous keyboard input
        private KeyboardState prevKBState;
        private MouseState prevMouseState;

        // Fields for fonts that will be used 
        private SpriteFont defaultFont;
        private SpriteFont titleFont;

        // Fields to contain the Tile maps for the levels
        private Texture2D[] levelTileSheets;

        // Fields to contain the character spritesheet and current player 
        private Texture2D characterSheet;
        // Dimensions: 17 x 29
        private Texture2D spriteCollisionBox;
        private Texture2D playerSpriteSheet;

        // Rectangle to hold to dictate player movement
        private Rectangle playerRect;
        // Player object
        private Player player;

        // Enemy textures
        private Texture2D skeletonWizardTexture;
        private Texture2D skeletonWarriorTexture;
        private Texture2D enemyGrimTexture;
        private Texture2D enemyNecromancerTexture;

        // Projectile spritesheet
        private Texture2D projectileSpriteSheet;
        private Texture2D necromancerProjectileSpriteSheet;
        private Texture2D reaperProjectileSpriteSheet;

        // Textures for health and mana bar, respectfully.
        private Texture2D healthBar;
        private Texture2D healthBarExtension;
        // Dimensions: 280 x 50
        private Texture2D manaBar;
        private Texture2D manaBarExtension;
        // Dimensions: 167 x 50
        private Texture2D square;
        // Fields for level backgrounds
        private Texture2D backgroundOne;
        private Texture2D backgroundTwo;
        private Texture2D castleOne;
        private Texture2D spaceOne;

        // Field to hold the Level Renderer
        private LevelRenderer levelRenderer;

        // Fields for enemy and projectile managers:
        private EnemyManager enemyManager;
        private ProjectileManager projectileManager;

        // Field to hold the buttons for each state
        private List<Button>[] buttons;

        // Field to hold button sprite
        private Texture2D buttonSprite;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            _graphics.PreferredBackBufferWidth = windowWidth;
            _graphics.PreferredBackBufferHeight = windowHeight;
            _graphics.ApplyChanges();

            //playerRect = new Rectangle(0, 380, 31, 53);
            gameState = GameState.MainMenu;
            stateWasPause = false;

            // Initialize the arrays of button lists
            buttons = new List<Button>[8];
            buttons[(int)GameState.MainMenu] = new List<Button>();
            buttons[(int)GameState.Directions] = new List<Button>();
            buttons[(int)GameState.LevelSelect] = new List<Button>();
            buttons[(int)GameState.Gameplay] = new List<Button>();
            buttons[(int)GameState.Pause] = new List<Button>();
            buttons[(int)GameState.ProMenu] = new List<Button>();
            buttons[(int)GameState.LevelResult] = new List<Button>();
            buttons[(int)GameState.Quit] = new List<Button>();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            // Character spreadsheet: (Dimensions of 500 x 444, 10 x 9 sprites, each 50 x 37 pixels)
            characterSheet = Content.Load<Texture2D>("charSheet");
            skeletonWizardTexture = Content.Load<Texture2D>("Skeleton_Mage");
            skeletonWarriorTexture = Content.Load<Texture2D>("Skeleton_Warrior");
            enemyGrimTexture = Content.Load<Texture2D>("ReaperTexture");
            enemyNecromancerTexture = Content.Load<Texture2D>("NecromancerTexture");
            projectileSpriteSheet = Content.Load<Texture2D>("projectileSheet");
            necromancerProjectileSpriteSheet = Content.Load<Texture2D>("necromancer_projectile_spritesheet");
            reaperProjectileSpriteSheet = Content.Load<Texture2D>("reaper_projectile_spritesheet");
            spriteCollisionBox = Content.Load<Texture2D>("playerCharCollision");
            playerSpriteSheet = Content.Load<Texture2D>("charSheet");

            // Level background are loaded here.
            backgroundOne = Content.Load<Texture2D>("Level1Background");
            backgroundTwo = Content.Load<Texture2D>("Level2Background");
            castleOne = Content.Load<Texture2D>("CastleBackground");
            spaceOne = Content.Load<Texture2D>("space_background");

            playerRect = new Rectangle(40, 380, spriteCollisionBox.Width, spriteCollisionBox.Height);

            // Load the default font and save it
            defaultFont = Content.Load<SpriteFont>("DefaultFont");
            titleFont = Content.Load<SpriteFont>("TitleFont");

            // Load the Tile Sheet for the levels
            levelTileSheets = new Texture2D[4];
            levelTileSheets[0] = Content.Load<Texture2D>("tileset");
            levelTileSheets[2] = Content.Load<Texture2D>("castleTileset");
            levelTileSheets[3] = Content.Load<Texture2D>("space_tile_set");

            // Health bar and mana bar (with black square for both)
            healthBar = Content.Load<Texture2D>("healthBar");
            manaBar = Content.Load<Texture2D>("manaBar");
            square = Content.Load<Texture2D>("blackSquare");
            // Extensions for health and mana bar
            healthBarExtension = Content.Load<Texture2D>("healthBarExtension");
            manaBarExtension = Content.Load<Texture2D>("manaBarExtension");

            // Sprite for button
            buttonSprite = Content.Load<Texture2D>("HUD Text Box Alt");

            // Player object is created.
            player = new Player(30, 100, 10, 5, 10, windowHeight, playerSpriteSheet, spriteCollisionBox, playerRect);
            player.LoadSave();
            player.UpdateStats();

            // Create all the Buttons
            // Create Menu Buttons
            buttons[(int)GameState.MainMenu].Add(
               new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth / 2 - 150, (int)(.4 * windowHeight) - 50, 300, 100),
               Color.AliceBlue, "Level Select", defaultFont));
            buttons[(int)GameState.MainMenu].Add(
                new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth / 2 - 150, (int)(.6 * windowHeight) - 50, 300, 100),
                Color.CadetBlue, "Directions", defaultFont));
            buttons[(int)GameState.MainMenu].Add(
                new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth / 2 - 150, (int)(.8 * windowHeight) - 50, 300, 100),
                Color.Red, "Quit", defaultFont));
            buttons[(int)GameState.MainMenu].Add(
                new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth - 270, windowHeight - 100, 240, 80),
                Color.Red, "Reset Game", defaultFont));

            // Create Directions Buttons
            buttons[(int)GameState.Directions].Add(
                new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth / 2 - 120, 710, 240, 80),
                Color.PowderBlue, "Pause", defaultFont));
            buttons[(int)GameState.Directions].Add(
                new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth / 2 - 120, 710, 240, 80),
                Color.SkyBlue, "Main Menu", defaultFont));

            // Create Level Select Buttons
            buttons[(int)GameState.LevelSelect].Add(
                new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth / 2 - 400, 200, 300, 100),
                Color.RoyalBlue, "Level 1", defaultFont));
            buttons[(int)GameState.LevelSelect].Add(
                new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth / 2 + 100, 200, 300, 100),
                Color.RoyalBlue, "Level 2", defaultFont));
            buttons[(int)GameState.LevelSelect].Add(
                new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth / 2 - 400, 400, 300, 100),
                Color.RoyalBlue, "Level 3", defaultFont));
            buttons[(int)GameState.LevelSelect].Add(
                new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth / 2 + 100, 400, 300, 100),
                Color.RoyalBlue, "Level 4", defaultFont));
            buttons[(int)GameState.LevelSelect].Add(
                new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth / 2 - 120, 600, 240, 80),
                Color.SkyBlue, "Main Menu", defaultFont));


            // Create Progression Menu Buttons
            buttons[(int)GameState.ProMenu].Add(
                new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth / 2 - 600, 350, 300, 100),
                Color.RoyalBlue, "Increase Health", defaultFont));
            buttons[(int)GameState.ProMenu].Add(
                new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth / 2 - 150, 350, 300, 100),
                Color.RoyalBlue, "Increase Damage", defaultFont));
            buttons[(int)GameState.ProMenu].Add(
                new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth / 2 + 300, 350, 300, 100),
                Color.RoyalBlue, "Increase Mana", defaultFont));

            // Create Pause Buttons
            buttons[(int)GameState.Pause].Add(
                new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth / 2 - 150, (int)(.4 * windowHeight) - 50, 300, 100),
                Color.RoyalBlue, "Return To Game", defaultFont));
            buttons[(int)GameState.Pause].Add(
                new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth / 2 - 150, (int)(.6 * windowHeight) - 50, 300, 100),
                Color.CadetBlue, "Directions", defaultFont));
            buttons[(int)GameState.Pause].Add(
                new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth / 2 - 150, (int)(.8 * windowHeight) - 50, 300, 100),
                Color.SkyBlue, "Main Menu", defaultFont));

            // Create Level Result Buttons
            buttons[(int)GameState.LevelResult].Add(
                new Button(GraphicsDevice, buttonSprite, new Rectangle(windowWidth / 2 - 120, 600, 240, 80),
                Color.PowderBlue, "Main Menu", defaultFont));

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            // TODO: Add your update logic here

            // Save the current keyboard and mouse state
            KeyboardState currentKBState = Keyboard.GetState();
            MouseState currentMState = Mouse.GetState();

            // Used to toggle full screen mode - suggestion from Patrick
            if (Keyboard.GetState().IsKeyDown(Keys.F11))
            {
                _graphics.IsFullScreen = !_graphics.IsFullScreen;
                _graphics.ApplyChanges();
            }

            // Process update depending on the current state of the game
            switch (gameState)
            {
                case GameState.MainMenu:
                    ProcessMainMenuMode(currentKBState, currentMState);
                    break;
                case GameState.Directions:
                    ProcessDirectionsMode(currentKBState, currentMState);
                    break;
                case GameState.LevelSelect:
                    ProcessLevelSelectMode(currentKBState, currentMState);
                    break;
                case GameState.Gameplay:
                    ProcessGameplayMode(currentKBState, currentMState);
                    break;
                case GameState.Pause:
                    ProcessPauseMode(currentKBState, currentMState);
                    break;
                case GameState.ProMenu:
                    ProcessProMenuMode(currentKBState, currentMState);
                    break;
                case GameState.LevelResult:
                    ProcessLevelResultMode(currentKBState, currentMState);
                    break;
                case GameState.Quit:
                    Exit();
                    break;
            }

            // Save the current keyboard to the previous keyboard
            prevKBState = currentKBState;

            // Save the current mouse to the previous mouse
            prevMouseState = currentMState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            // Begins Drawing AND Stops the blurriness of drawing - suggestion from Patrick
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // Display message about the controls depending on the current game state
            switch (gameState)
            {
                // Draws Title and Main Menu Buttons
                case GameState.MainMenu:
                    GraphicsDevice.Clear(Color.Black);

                    _spriteBatch.Draw(castleOne, new Rectangle(0, 0, windowWidth, windowHeight), Color.White);

                    _spriteBatch.DrawString(titleFont, "Spellblade",
                        new Vector2(windowWidth / 2 - titleFont.MeasureString("Spellblade").X / 2, (int)(.1 * windowHeight)), Color.Black);

                    // Draws each of the buttons for this state
                    foreach (Button button in buttons[(int)gameState])
                    {
                        button.Draw(_spriteBatch);
                    }

                    break;

                // Displays the directions and the buttons on this screen
                case GameState.Directions:

                    _spriteBatch.Draw(castleOne, new Rectangle(0, 0, windowWidth, windowHeight), Color.White);

                    if (stateWasPause)
                    {
                        GraphicsDevice.Clear(Color.CadetBlue);
                        _spriteBatch.DrawString(titleFont, "Directions",
                                                new Vector2(windowWidth / 2 - titleFont.MeasureString("Directions").X / 2, 0), Color.Black);

                        // Draws the button to return to pause if was paused 
                        buttons[(int)gameState][0].Draw(_spriteBatch);
                    }
                    else
                    {
                        GraphicsDevice.Clear(Color.CadetBlue);
                        _spriteBatch.DrawString(titleFont, "Directions",
                                                new Vector2(windowWidth / 2 - titleFont.MeasureString("Directions").X / 2, 0), Color.Black);

                        // Draws button to return to main menu
                        buttons[(int)gameState][1].Draw(_spriteBatch);
                    }


                    // Box and lines of text detailing the usable keys and their corresponding inputs.
                    _spriteBatch.Draw(buttonSprite, new Rectangle(windowWidth / 2 - 500, 150, 975, 550), Color.White);
                    _spriteBatch.DrawString(defaultFont, "W/Up Arrow: Jump", new Vector2(windowWidth / 2 - 460, 195 + 25), Color.White);
                    _spriteBatch.DrawString(defaultFont, "S/Down Arrow: Fastfall", new Vector2(windowWidth / 2 - 460, 255 + 25), Color.White);
                    _spriteBatch.DrawString(defaultFont, "A/Left Arrow: Move Left", new Vector2(windowWidth / 2 - 460, 320 + 25), Color.White);
                    _spriteBatch.DrawString(defaultFont, "D/Right Arrow: Move Right", new Vector2(windowWidth / 2 - 460, 385 + 25), Color.White);
                    _spriteBatch.DrawString(defaultFont, "Left Click/J: Melee Attack", new Vector2(windowWidth / 2 - 460, 450 + 25), Color.White);
                    _spriteBatch.DrawString(defaultFont, "Right Click/K: Ranged Skill", new Vector2(windowWidth / 2 - 460, 515 + 25), Color.White);
                    _spriteBatch.DrawString(defaultFont, "U: Toggle Hitboxes", new Vector2(windowWidth / 2 - 460, 575 + 25), Color.White);

                    // Lines of text detailing the unique mechanics of the game.
                    _spriteBatch.DrawString(defaultFont, "-Slaying an enemy with a \nmelee attack will restore 1 \nmana (fireball charge).", new Vector2(windowWidth / 2, 195), Color.White);
                    _spriteBatch.DrawString(defaultFont, "-Once per level, you can \nincrease one of your stats \npermanently.", new Vector2(windowWidth / 2, 320), Color.White);
                    _spriteBatch.DrawString(defaultFont, "-The goal of each level is to \ndefeat the boss at the end.", new Vector2(windowWidth / 2, 445), Color.White);
                    _spriteBatch.DrawString(defaultFont, "-If you run out of health or \nfall out of bounds, you will \nhave to restart the level.", new Vector2(windowWidth / 2, 525), Color.White);


                    break;

                // Draws the Level Select and the level buttons
                case GameState.LevelSelect:

                    _spriteBatch.Draw(castleOne, new Rectangle(0, 0, windowWidth, windowHeight), Color.White);
                    _spriteBatch.DrawString(titleFont, "Level Select",
                                            new Vector2(windowWidth / 2 - titleFont.MeasureString("Level Select").X / 2, 10), Color.Black);

                    // Draws each of the buttons for this state
                    foreach (Button button in buttons[(int)gameState])
                    {
                        button.Draw(_spriteBatch);
                    }

                    break;

                // Draws level background and GameObjects, handles player attack, and draws the player HUD
                case GameState.Gameplay:
                    GraphicsDevice.Clear(Color.RoyalBlue);

                    // Switch statement for level backgrounds.
                    switch (levelRenderer.currentLevel)
                    {
                        case Levels.Forest:
                            _spriteBatch.Draw(backgroundTwo, new Rectangle(0, 0, windowWidth, windowHeight), Color.White);
                            break;
                        case Levels.Village:
                            // Draw village background here
                            break;
                        case Levels.Castle:
                            _spriteBatch.Draw(castleOne, new Rectangle(0, 0, windowWidth, windowHeight), Color.White);
                            break;
                        case Levels.Astral:
                            // Draw space background here
                            _spriteBatch.Draw(spaceOne, new Rectangle(0, 0, windowWidth, windowHeight), Color.White);
                            break;
                    }

                    // Draws the level's current state
                    levelRenderer.Draw(_spriteBatch);
                    enemyManager.Draw(_spriteBatch);
                    projectileManager.Draw(_spriteBatch);
                    player.Draw(_spriteBatch);

                    if (player.ActiveAttack && player.Frame == 4)
                    {
                        player.Attack();

                        if (!player.DamageDealt)
                        {
                            // Finds all the enemies hit by the player's sword and
                            // deals damage to them.
                            List<Enemy> hitEnemies = enemyManager.SwordCollision(player.PlayerSword);
                            for (int i = 0; i < hitEnemies.Count; i++)
                            {
                                hitEnemies[i].TakeDamage(player.Damage);
                                if (hitEnemies[i].Health <= 0 && player.CurrentMana < player.MaxMana)
                                {
                                    player.CurrentMana++;
                                }
                            }

                            player.DamageDealt = true;
                        }

                        for (int i = 0; i < projectileManager.Projectiles.Count; i++)
                        {
                            if (!projectileManager.Projectiles[i].PlayerProjectile &&
                                projectileManager.Projectiles[i].Position.Intersects(player.PlayerSword))
                            {
                                // Tracking projectiles explode on contact with
                                // the player's sword.
                                if (projectileManager.Projectiles[i] is TrackingProjectile)
                                {
                                    projectileManager.RemoveProjectile(i);
                                }

                                // Normal projectiles are deflected on contact
                                // with the player's sword.
                                else
                                {
                                    projectileManager.Projectiles[i].SwapDirection();
                                    projectileManager.Projectiles[i].SwapControl();
                                }
                            }
                        }
                    }

                    player.DrawHUD(healthBar, manaBar, healthBarExtension, manaBarExtension, square, _spriteBatch);

                    if (enemyManager.ShouldDrawHUD)
                    {
                        enemyManager.DrawHUD(healthBar, healthBarExtension, square, _spriteBatch);
                    }

                    break;

                // Draws the text and buttons for the pause state 
                case GameState.Pause:
                    GraphicsDevice.Clear(Color.PowderBlue);

                    _spriteBatch.Draw(castleOne, new Rectangle(0, 0, windowWidth, windowHeight), Color.White);

                    _spriteBatch.DrawString(titleFont, "PAUSE",
                                            new Vector2(windowWidth / 2 - titleFont.MeasureString("PAUSE").X / 2, (int)(.1 * windowHeight)), Color.Black);

                    // Draws each of the buttons for this state
                    foreach (Button button in buttons[(int)gameState])
                    {
                        button.Draw(_spriteBatch);
                    }

                    break;

                // Draws the prompt and options for the progression menu
                case GameState.ProMenu:
                    GraphicsDevice.Clear(Color.Green);

                    _spriteBatch.Draw(castleOne, new Rectangle(0, 0, windowWidth, windowHeight), Color.White);

                    _spriteBatch.DrawString(defaultFont, "Welcome to the Progression Menu. Choose one.",
                                            new Vector2(565, 100), Color.Black);

                    // Draws each of the buttons for this state
                    foreach (Button button in buttons[(int)gameState])
                    {
                        button.Draw(_spriteBatch);
                    }

                    break;

                // Draws the Win/Lose Text and the buttons for the level result state
                case GameState.LevelResult:
                    GraphicsDevice.Clear(Color.SteelBlue);

                    _spriteBatch.Draw(castleOne, new Rectangle(0, 0, windowWidth, windowHeight), Color.White);

                    if (hasWon)
                    {
                        _spriteBatch.DrawString(titleFont, "Level Completed!",
                                            new Vector2(windowWidth / 2 - titleFont.MeasureString("Level Completed!").X / 2, (int)(windowHeight * .1)), Color.Black);
                    }
                    else
                    {
                        _spriteBatch.DrawString(titleFont, "You Died",
                                            new Vector2(windowWidth / 2 - titleFont.MeasureString("You Died").X / 2, (int)(windowHeight * .1)), Color.Black);
                    }


                    // Draws each of the buttons for this state
                    foreach (Button button in buttons[(int)gameState])
                    {
                        button.Draw(_spriteBatch);
                    }

                    break;

                // Quits the game
                case GameState.Quit:
                    GraphicsDevice.Clear(Color.Black);
                    _spriteBatch.DrawString(defaultFont, "Welcome to the Quit, The Great Beyond!!",
                                            new Vector2(10, 10), Color.White);

                    // Draws each of the buttons for this state
                    foreach (Button button in buttons[(int)gameState])
                    {
                        button.Draw(_spriteBatch);
                    }

                    break;

            }

            // Ends Drawing
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Method to process all input and update any relevant variables while in Main Menu
        /// </summary>
        /// <param name="kbState"></param>
        private void ProcessMainMenuMode(KeyboardState currentKBState, MouseState currentMState)
        {
            if (SingleKeyPressed(Keys.L, currentKBState) ||
                buttons[(int)gameState][0].ButtonClicked(prevMouseState, currentMState))
            {
                gameState = GameState.LevelSelect;
            }
            else if (SingleKeyPressed(Keys.O, currentKBState) ||
                buttons[(int)gameState][1].ButtonClicked(prevMouseState, currentMState))
            {
                gameState = GameState.Directions;
            }
            else if (SingleKeyPressed(Keys.Q, currentKBState) ||
                buttons[(int)gameState][2].ButtonClicked(prevMouseState, currentMState))
            {
                gameState = GameState.Quit;
            }
            else if (SingleKeyPressed(Keys.R, currentKBState) ||
                buttons[(int)gameState][3].ButtonClicked(prevMouseState, currentMState))
            {
                gameState = GameState.MainMenu;
                player.ResetSave();
                player.LoadSave();
            }
        }

        /// <summary>
        /// Method to process all input and update any relevant variables while in Directions
        /// </summary>
        /// <param name="kbState"></param>
        private void ProcessDirectionsMode(KeyboardState currentKBState, MouseState currentMState)
        {
            if ((SingleKeyPressed(Keys.P, currentKBState) ||
                buttons[(int)gameState][0].ButtonClicked(prevMouseState, currentMState)) &&
                stateWasPause)
            {
                gameState = GameState.Pause;
                stateWasPause = false;
            }
            if (SingleKeyPressed(Keys.M, currentKBState) ||
                buttons[(int)gameState][1].ButtonClicked(prevMouseState, currentMState))
            {
                gameState = GameState.MainMenu;
                stateWasPause = false;
            }
        }

        /// <summary>
        /// Method to process all input and update any relevant variables while in Level Select
        /// </summary>
        /// <param name="kbState"></param>
        private void ProcessLevelSelectMode(KeyboardState currentKBState, MouseState currentMState)
        {
            // Resets the objects used for managing gameplay by overwriting, calling ResetPlayer(), and loading the level
            // Changes to gameplay of 1st level
            if ((SingleKeyPressed(Keys.D1, currentKBState) ||
                buttons[(int)gameState][0].ButtonClicked(prevMouseState,
                currentMState)) && player.LevelsUnlocked >= 0)
            {
                gameState = GameState.Gameplay;
                player.ResetPlayer();
                projectileManager = new ProjectileManager(projectileSpriteSheet,
                    necromancerProjectileSpriteSheet,
                    reaperProjectileSpriteSheet, player, windowWidth);
                enemyManager = new EnemyManager(projectileManager, windowWidth,
                    windowHeight, skeletonWizardTexture, skeletonWarriorTexture,
                    enemyNecromancerTexture, enemyGrimTexture,
                    spriteCollisionBox);
                levelRenderer = new LevelRenderer(Levels.Forest, levelTileSheets, windowWidth,
                    windowHeight, enemyManager, projectileManager, player);
                levelRenderer.LoadLevel();
            }
            // Changes to gameplay of 2nd level
            else if ((SingleKeyPressed(Keys.D2, currentKBState) ||
                buttons[(int)gameState][1].ButtonClicked(prevMouseState,
                currentMState)) && player.LevelsUnlocked >= 1)
            {
                player.ResetPlayer();
                hasWon = true;
                if(player.LevelsUnlocked <= 1)
                {
                    player.LevelsUnlocked++;
                }
                gameState = GameState.LevelResult;
            }
            // Changes to gameplay of 3rd level
            else if ((SingleKeyPressed(Keys.D3, currentKBState) ||
                buttons[(int)gameState][2].ButtonClicked(prevMouseState,
                currentMState)) && player.LevelsUnlocked >= 2)
            {
                gameState = GameState.Gameplay;
                player.ResetPlayer();
                projectileManager = new ProjectileManager(projectileSpriteSheet,
                    necromancerProjectileSpriteSheet,
                    reaperProjectileSpriteSheet, player, windowWidth);
                enemyManager = new EnemyManager(projectileManager, windowWidth,
                    windowHeight, skeletonWizardTexture, skeletonWarriorTexture,
                    enemyNecromancerTexture, enemyGrimTexture,
                    spriteCollisionBox);
                levelRenderer = new LevelRenderer(Levels.Castle, levelTileSheets, windowWidth,
                    windowHeight, enemyManager, projectileManager, player);

                levelRenderer.LoadLevel();
            }
            // Changes to gameplay of 4th level
            else if ((SingleKeyPressed(Keys.D4, currentKBState) ||
                buttons[(int)gameState][3].ButtonClicked(prevMouseState,
                currentMState)) && player.LevelsUnlocked >= 3)
            {
                gameState = GameState.Gameplay;
                player.ResetPlayer();
                projectileManager = new ProjectileManager(projectileSpriteSheet,
                    necromancerProjectileSpriteSheet,
                    reaperProjectileSpriteSheet, player, windowWidth);
                enemyManager = new EnemyManager(projectileManager, windowWidth,
                    windowHeight, skeletonWizardTexture, skeletonWarriorTexture,
                    enemyNecromancerTexture, enemyGrimTexture,
                    spriteCollisionBox);
                levelRenderer = new LevelRenderer(Levels.Astral, levelTileSheets, windowWidth,
                    windowHeight, enemyManager, projectileManager, player);

                levelRenderer.LoadLevel();
            }
            // Goes to main menu instead
            else if (SingleKeyPressed(Keys.M, currentKBState) ||
                buttons[(int)gameState][4].ButtonClicked(prevMouseState,
                currentMState))
            {
                gameState = GameState.MainMenu;
            }
        }

        /// <summary>
        /// Method to process all input and update any relevant variables while in Gameplay
        /// </summary>
        /// <param name="kbState"></param>
        private void ProcessGameplayMode(KeyboardState currentKBState, MouseState currentMState)
        {
            // Sets players frame movement to zero
            int playerXMovement = 0;

            // Checks if player x movement changes to go left or right
            if (currentKBState.IsKeyDown(Keys.Left) || currentKBState.IsKeyDown(Keys.A))
            {
                playerXMovement -= 6;
            }
            if (currentKBState.IsKeyDown(Keys.Right) || currentKBState.IsKeyDown(Keys.D))
            {
                //
                playerXMovement += 6;
            }

            // Player x movement is modified by the level renderer
            // if colliding on either with tangible tile, in
            // center of screen, or if movement would leave window.
            // Then updates the state of the player
            player.Move(levelRenderer.Update(playerXMovement));
            player.Update();

            // If the input for shooting is entered by the player, the projectile manager creates a new projectile at the player's location.
            if (player.State == PlayerState.Shooting && player.CurrentMana > 0 && player.CanShoot)
            {
                projectileManager.CreateProjectile(10, player.FacingLeft, true,
                    player.Position.X, player.Position.Y + player.Position.Height / 2);
                player.CurrentMana--;
                player.State = PlayerState.Idle;
            }

            // Updates every enemy's state and checks health
            enemyManager.Update();

            // Updates every projectile and checks projectile collisions
            projectileManager.Update(enemyManager);

            // Toggles drawing collision hitboxes for all characters 
            if (prevKBState.IsKeyUp(Keys.U) && currentKBState.IsKeyDown(Keys.U))
            {
                if (player.DrawCollisionBox)
                {
                    player.DrawCollisionBox = false;
                    enemyManager.ToggleHitBox();
                }
                else
                {
                    player.DrawCollisionBox = true;
                    enemyManager.ToggleHitBox();
                }
            }

            // Checks for win, death, or pause state
            if (enemyManager.CurrentBoss != null && enemyManager.CurrentBoss.Health <= 0)
            {
                hasWon = true;
                // Number of levels unlocked only increments if the current number of levels is less than or equal to the integer corresponding to the current level.
                if (player.LevelsUnlocked <= (int)levelRenderer.currentLevel)
                {
                    player.LevelsUnlocked++;
                }
                player.SaveData();
                gameState = GameState.LevelResult;
            }
            if (player.Health <= 0)
            {
                hasWon = false;
                player.SaveData();
                gameState = GameState.LevelResult;
            }
            else if (SingleKeyPressed(Keys.P, currentKBState) ||
                SingleKeyPressed(Keys.Escape, currentKBState))
            {
                gameState = GameState.Pause;
            }
            // Checks for whether player is touching collision orb that has not been collected, then goes to progression menu
            else if (levelRenderer.ProgressionTile != null)
            {
                if (player.Position.Intersects(levelRenderer.ProgressionTile.Position) && !player.IsOrbCollected[(int)levelRenderer.currentLevel])
                {
                    gameState = GameState.ProMenu;
                    player.IsOrbCollected[(int)levelRenderer.currentLevel] = true;
                }
            }
        }

        /// <summary>
        /// Method to process all input and update any relevant variables while paused
        /// </summary>
        /// <param name="kbState"></param>
        private void ProcessPauseMode(KeyboardState currentKBState, MouseState currentMState)
        {
            if (SingleKeyPressed(Keys.M, currentKBState) ||
                buttons[(int)gameState][1].ButtonClicked(prevMouseState, currentMState))
            {
                gameState = GameState.Directions;
                stateWasPause = true;
            }
            else if (SingleKeyPressed(Keys.G, currentKBState) ||
                buttons[(int)gameState][0].ButtonClicked(prevMouseState, currentMState))
            {
                gameState = GameState.Gameplay;
            }
            else if (SingleKeyPressed(Keys.O, currentKBState) ||
                buttons[(int)gameState][2].ButtonClicked(prevMouseState, currentMState))
            {
                gameState = GameState.MainMenu;
            }
        }


        /// <summary>
        /// Determines what to do after entering the Progression Menu. 
        /// Increments the Health, Damage, or MaxMana of player.
        /// </summary>
        /// <param name="currentKBState"></param>
        /// <param name="currentMState"></param>
        private void ProcessProMenuMode(KeyboardState currentKBState, MouseState currentMState)
        {
            if (buttons[(int)gameState][0].ButtonClicked(prevMouseState, currentMState))
            {
                gameState = GameState.Gameplay;
                player.BonusHealth += 1;
                player.MaxHealth += 1;
                player.Health += 1;
                player.SaveData();
            }
            else if (buttons[(int)gameState][1].ButtonClicked(prevMouseState, currentMState))
            {
                gameState = GameState.Gameplay;
                player.BonusDamage += 1;
                player.Damage += 5;
                player.SaveData();
            }
            else if (buttons[(int)gameState][2].ButtonClicked(prevMouseState, currentMState))
            {
                gameState = GameState.Gameplay;
                player.BonusMana += 1;
                player.MaxMana += 1;
                player.CurrentMana += 1;
                player.SaveData();
            }
        }

        /// <summary>
        /// Method to process all input and update any relevant variables while in win or lose screen
        /// </summary>
        /// <param name="kbState"></param>
        private void ProcessLevelResultMode(KeyboardState currentKBState, MouseState currentMState)
        {
            player.SaveData();
            if (SingleKeyPressed(Keys.M, currentKBState) ||
                buttons[(int)gameState][0].ButtonClicked(prevMouseState, currentMState))
            {
                gameState = GameState.MainMenu;
            }
        }

        /// <summary>
        /// Helper method to find out if key was pressed and released
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private bool SingleKeyPressed(Keys key, KeyboardState currentKBState)
        {
            return (prevKBState.IsKeyUp(key) && currentKBState.IsKeyDown(key));
        }


    }
}