using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace Spellblade
{
    // Name: Will Doyle
    // Purpose: Player class contains the states that the player character is capable of being in,
    // as well as the code to perform tasks in response to user input.
    // Restrictions or known errors: Melee attack cooldown doesn't work.
    enum PlayerState
    {
        Idle,
        Walking,
        Jumping,
        Falling,
        Attacking,
        Shooting,
    }
    class Player : Character
    {
        // Fields
        private PlayerState state;
        private KeyboardState kbState;
        private KeyboardState prevKbState;
        private MouseState mouseState;
        private MouseState prevMouseState;
        private Rectangle playerSword;
        private bool activeAttack;
        private bool canAttack;
        private int windowHeight;
        private bool canShoot;
        private int maxMana;
        private int currentMana;
        private int attackTimer;
        private int range;
        private int attackFrames;
        private int rangedTimer;
        private bool damageDealt;

        private string filename;
        private int levelsUnlocked;
        private int bonusHealth;
        private int bonusDamage;
        private int bonusMana;
        private bool[] isOrbCollected;

        //      Animation Fields      //
        // Constants for each frame in the tile sheet
        const int playerSpriteWidth = 50;
        const int playerSpriteHeight = 37;

        // Constants for draw offset from position bottom middle
        const int playerSpriteOriginX = 25;
        const int playerSpriteOriginY = 35;

        // Properties:
        public PlayerState State
        {
            get { return state; }
            set { state = value; }
        }

        public Rectangle PlayerSword
        {
            get { return playerSword; }
            set { playerSword = value; }
        }

        public int CurrentMana
        {
            get { return currentMana; }
            set { currentMana = value; }
        }

        public bool ActiveAttack
        {
            get { return activeAttack; }
        }

        public int AttackTimer
        {
            get { return attackTimer; }
        }

        public int Range
        {
            get { return range; }
        }

        public int WindowHeight
        {
            get { return windowHeight; }
        }

        public int MaxMana
        {
            get { return maxMana; }
            set { maxMana = value; }
        }
        public int LevelsUnlocked
        {
            get { return levelsUnlocked; }
            set { levelsUnlocked = value; }
        }
        public int BonusHealth
        {
            get { return bonusHealth; }
            set { bonusHealth = value; }
        }
        public int BonusDamage
        {
            get { return bonusDamage; }
            set { bonusDamage = value; }
        }
        public int BonusMana
        {
            get { return bonusMana; }
            set { bonusMana = value; }
        }
        public bool CanShoot
        {
            get { return canShoot; }
            set { canShoot = value; }
        }

        public bool[] IsOrbCollected
        {
            get { return isOrbCollected; }
            set { isOrbCollected = value; }
        }

        public int Frame
        {
            get { return frame; }
        }

        public bool DamageDealt
        {
            get { return damageDealt; }
            set { damageDealt = value; }
        }

        // Parameterized Constructor
        public Player(int range, int health, int damage,
             int moveSpeed, int attackSpeed, int windowHeight, Texture2D playerSpriteSheet, Texture2D texture, Rectangle position)
            : base(health, damage, moveSpeed, attackSpeed, texture, playerSpriteSheet, position)
        {
            this.health = health;
            this.position = position;
            this.damage = damage;
            this.windowHeight = windowHeight;
            // Player starts in idle state.
            this.state = PlayerState.Idle;

            this.playerSword = new Rectangle(position.X, position.Y, range, position.Height);
            this.maxMana = 5;
            this.currentMana = maxMana;
            this.attackTimer = 0;
            this.range = range;
            this.levelsUnlocked = 0; // Will be 0 when only Level 1 is unlocked, and 3 when all 4 levels are unlocked.
            this.bonusDamage = 0;
            this.bonusHealth = 0; // 4 will be the highest obtainable value
            this.bonusMana = 0; // 4 will be the highest obtainable value
            this.canAttack = true;
            this.attackFrames = 30;
            this.canShoot = true;
            this.rangedTimer = 0;
            this.damageDealt = false;
            frame = 0;
            animated = true;
            stateFrameCount = 4;
            timePerFrame = 10;
            timeCounter = 0;
            isOrbCollected = new bool[4] {false, false, false, false }; // All values are initialized to false.
        }

        public override void Update()
        {
            kbState = Keyboard.GetState();
            mouseState = Mouse.GetState();

            // If the player exceeds the height of the screen (by falling off of the stage),
            // his health will be set to 0, and the game will end.
            if (position.Y > windowHeight)
            {
                health = 0;
            }

            if ((kbState.IsKeyDown(Keys.S) || kbState.IsKeyDown(Keys.Down)) && IsMidair)
            {
                verticalVelocity -= 1;
            }

            // Logic for melee attack cooldown
            if (attackTimer == 1)
            {
                canAttack = false;
                attackTimer++;
            }
            else if (attackTimer <= attackSpeed + attackFrames && attackTimer != 0)
            {
                attackTimer++;
            }
            else if (attackTimer > attackSpeed + attackFrames)
            {
                attackTimer = 0;
                canAttack = true;
            }


            switch (state)
            {
                // The player will be able to perform all actions while idle or walking
                case PlayerState.Idle:
                case PlayerState.Walking:
                    // Melee attack: 
                    if (((mouseState.LeftButton == ButtonState.Pressed &&
                        prevMouseState.LeftButton == ButtonState.Released) ||
                        (prevKbState.IsKeyUp(Keys.J) && kbState.IsKeyDown(Keys.J))) && canAttack)
                    {
                        state = PlayerState.Attacking;
                    }
                    // Ranged attack:
                    else if ((mouseState.RightButton == ButtonState.Pressed &&
                        prevMouseState.RightButton == ButtonState.Released) ||
                        (prevKbState.IsKeyUp(Keys.K) && kbState.IsKeyDown(Keys.K)) && canShoot && currentMana > 0)
                    {
                        state = PlayerState.Shooting;
                    }
                    // Jumping:
                    else if ((kbState.IsKeyDown(Keys.Up) || kbState.IsKeyDown(Keys.W)) && !isMidair)
                    {
                        state = PlayerState.Jumping;

                    }
                    // Walking left:
                    else if (kbState.IsKeyDown(Keys.Left) || kbState.IsKeyDown(Keys.A))
                    {
                        state = PlayerState.Walking;
                        facingLeft = true;
                    }
                    // Walking right:
                    else if (kbState.IsKeyDown(Keys.Right) || kbState.IsKeyDown(Keys.D))
                    {
                        state = PlayerState.Walking;
                        facingLeft = false;
                    }
                    else
                    {
                        state = PlayerState.Idle;
                    }
                    break;
                // While attacking or shooting, the player will be able to move horizontally (walk)
                case PlayerState.Attacking:
                    if (!activeAttack)
                    {
                        activeAttack = true;
                        attackTimer = 1;
                    }
                    else if (attackTimer >= attackFrames)
                    {
                        activeAttack = false;
                        damageDealt = false;
                        state = PlayerState.Idle;
                    }
                    break;
                case PlayerState.Shooting:
                    if(currentMana == 0)
                    {
                        state = PlayerState.Idle;
                    }
                    break;
                // If the player is jumping or otherwise midair, they will be able to influence their direction.
                case PlayerState.Jumping:
                    // Changes velocity to positive.
                    verticalVelocity = 8;
                    isMidair = true;
                    state = PlayerState.Idle;
                    // While we dont want the player character to change states whilst midair,
                    // We do want the player to be able to change their direction midair.
                    if (kbState.IsKeyDown(Keys.Left) || kbState.IsKeyDown(Keys.A))
                    {
                        facingLeft = true;
                    }
                    if (kbState.IsKeyDown(Keys.Right) || kbState.IsKeyDown(Keys.D))
                    {
                        facingLeft = false;
                    }
                    break;
            }

            // Call to private update animation method
            UpdateAnimation();

            // Gets the previous state of the mouse.
            prevMouseState = mouseState;
            prevKbState = kbState;
        }

        /// <summary>
        /// Update the the current animation frame based on state and time spent
        /// </summary>
        private void UpdateAnimation()
        {
            switch (state)
            {
                // The player will be able to perform all actions while idle or walking
                case PlayerState.Idle:
                    animated = true;
                    stateFrameCount = 4;
                    timePerFrame = 10;


                    break;
                case PlayerState.Walking:
                    animated = true;
                    stateFrameCount = 6;
                    timePerFrame = 8;

                    break;
                // While attacking or shooting, the player will be able to move horizontally (walk)
                case PlayerState.Attacking:
                    animated = true;
                    stateFrameCount = 7;
                    timePerFrame = 5;

                    break;
                case PlayerState.Shooting:
                    animated = true;
                    stateFrameCount = 9;
                    timePerFrame = 5;

                    break;
                // If the player is jumping or otherwise midair, they will be able to influence their direction.
                case PlayerState.Jumping:
                    animated = true;
                    stateFrameCount = 3;
                    timePerFrame = 12;
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
                    }

                    timeCounter -= timePerFrame;
                }
            }

        }

        // Draws and updates the health and mana bar.
        public void DrawHUD(Texture2D healthBar, Texture2D manaBar, Texture2D healthBarExtension, Texture2D manaBarExtension, Texture2D square, SpriteBatch sb)
        {
            int missingHP = (maxHealth + (bonusHealth * 10)) - health;
            int missingMP = (maxMana + bonusMana) - currentMana;

            // Health bar logic
            sb.Draw(healthBar, new Rectangle(10, 10, 260, 50), Color.White);
            for (int i = 0; i < bonusHealth; i++)
            {
                sb.Draw(healthBarExtension, new Rectangle(261 + (i * 21), 10, 31, 50), Color.White);
            }
            // First black square for health.
            //sb.Draw(square, new Rectangle(57, 32, 17, 17), Color.Black);
            // Second square.
            //sb.Draw(square, new Rectangle(79, 32, 17, 17), Color.Black);
            for (int i = 1; i <= maxHealth; i++)
            {
                if (missingHP >= (i + bonusHealth) * 10)
                {
                    sb.Draw(square, new Rectangle(242 + (bonusHealth * 21) - (21 * (i - 1)), 32, 16, 17), Color.Black);
                }
            }

            // Mana bar logic
            sb.Draw(manaBar, new Rectangle(12, 60, 150, 50), Color.White);
            for (int i = 0; i < bonusMana; i++)
            {
                sb.Draw(manaBarExtension, new Rectangle(153 + (i * 20), 60, 31, 50), Color.White);
            }
            // First black square for mana.
            // sb.Draw(square, new Rectangle(57, 78, 17, 17), Color.Black);
            // Second square.
            // sb.Draw(square, new Rectangle(79, 78, 17, 17), Color.Black);
            for (int i = 1; i <= maxMana; i++)
            {
                if (missingMP >= i + bonusMana)
                {
                    sb.Draw(square, new Rectangle(135 + (bonusMana * 20) - (20 * (i - 1)), 78, 16, 18), Color.Black);
                }
            }
        }

        /// <summary>
        /// Draws the player collision box and the player animation. 
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {

            if (drawCollisionBox)
            {
                base.Draw(spriteBatch);
            }

            switch (state)
            {
                // The player will be able to perform all actions while idle or walking
                case PlayerState.Idle:
                    if (!facingLeft)
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                            new Rectangle(
                                frame * playerSpriteWidth,
                                0,
                                playerSpriteWidth,
                                playerSpriteHeight),
                            Color.White,
                            0,
                            new Vector2(playerSpriteOriginX, playerSpriteOriginY),
                            2f,
                            SpriteEffects.None,
                            1);
                    }
                    else
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                            new Rectangle(
                                frame * playerSpriteWidth,
                                0,
                                playerSpriteWidth,
                                playerSpriteHeight),
                            Color.White,
                            0,
                            new Vector2(playerSpriteOriginX, playerSpriteOriginY),
                            2f,
                            SpriteEffects.FlipHorizontally,
                            1);
                    }
                    break;
                case PlayerState.Walking:
                    if (!facingLeft)
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                            new Rectangle(
                                frame * playerSpriteWidth,
                                1 * playerSpriteHeight,
                                playerSpriteWidth,
                                playerSpriteHeight),
                            Color.White,
                            0,
                            new Vector2(playerSpriteOriginX, playerSpriteOriginY),
                            2f,
                            SpriteEffects.None,
                            0);
                    }
                    else
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                            new Rectangle(
                                frame * playerSpriteWidth,
                                1 * playerSpriteHeight,
                                playerSpriteWidth,
                                playerSpriteHeight),
                            Color.White,
                            0,
                            new Vector2(playerSpriteOriginX, playerSpriteOriginY),
                            2f,
                            SpriteEffects.FlipHorizontally,
                            0);
                    }


                    break;
                // While attacking or shooting, the player will be able to move horizontally (walk)
                case PlayerState.Attacking:
                    if (!facingLeft)
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                            new Rectangle(
                                frame * playerSpriteWidth,
                                4 * playerSpriteHeight,
                                playerSpriteWidth,
                                playerSpriteHeight),
                            Color.White,
                            0,
                            new Vector2(playerSpriteOriginX, playerSpriteOriginY),
                            2f,
                            SpriteEffects.None,
                            0);
                    }
                    else
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                            new Rectangle(
                                frame * playerSpriteWidth,
                                4 * playerSpriteHeight,
                                playerSpriteWidth,
                                playerSpriteHeight),
                            Color.White,
                            0,
                            new Vector2(playerSpriteOriginX, playerSpriteOriginY),
                            2f,
                            SpriteEffects.FlipHorizontally,
                            0);
                    }
                    break;
                case PlayerState.Shooting:
                    if (!facingLeft)
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                            new Rectangle(
                                frame * playerSpriteWidth,
                                3 * playerSpriteHeight,
                                playerSpriteWidth,
                                playerSpriteHeight),
                            Color.White,
                            0,
                            new Vector2(playerSpriteOriginX, playerSpriteOriginY),
                            2f,
                            SpriteEffects.None,
                            0);
                    }
                    else
                    {
                        spriteBatch.Draw(
                            spriteSheet,
                            new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                            new Rectangle(
                                frame * playerSpriteWidth,
                                3 * playerSpriteHeight,
                                playerSpriteWidth,
                                playerSpriteHeight),
                            Color.White,
                            0,
                            new Vector2(playerSpriteOriginX, playerSpriteOriginY),
                            2f,
                            SpriteEffects.FlipHorizontally,
                            0);
                    }

                    break;
                // If the player is jumping or otherwise midair, they will be able to influence their direction.
                case PlayerState.Jumping:
                    if (verticalVelocity > 0)
                    {
                        if (!facingLeft)
                        {
                            spriteBatch.Draw(
                                spriteSheet,
                                new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                                new Rectangle(
                                    2 * playerSpriteWidth + (frame * playerSpriteWidth),
                                    2 * playerSpriteHeight,
                                    playerSpriteWidth,
                                    playerSpriteHeight),
                                Color.White,
                                0,
                                new Vector2(playerSpriteOriginX, playerSpriteOriginY),
                                2f,
                                SpriteEffects.None,
                                0);
                        }
                        else
                        {
                            spriteBatch.Draw(
                                spriteSheet,
                                new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                                new Rectangle(
                                    2 * playerSpriteWidth + (frame * playerSpriteWidth),
                                    2 * playerSpriteHeight,
                                    playerSpriteWidth,
                                    playerSpriteHeight),
                                Color.White,
                                0,
                                new Vector2(playerSpriteOriginX, playerSpriteOriginY),
                                2f,
                                SpriteEffects.FlipHorizontally,
                                0);
                        }
                    }
                    else
                    {
                        if (!facingLeft)
                        {
                            spriteBatch.Draw(
                                spriteSheet,
                                new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                                new Rectangle(
                                    5 * playerSpriteWidth + (frame * playerSpriteWidth),
                                    2 * playerSpriteHeight,
                                    playerSpriteWidth,
                                    playerSpriteHeight),
                                Color.White,
                                0,
                                new Vector2(playerSpriteOriginX, playerSpriteOriginY),
                                2f,
                                SpriteEffects.None,
                                0);
                        }
                        else
                        {
                            spriteBatch.Draw(
                                spriteSheet,
                                new Vector2(position.X + (position.Width / 2), position.Y + position.Height),
                                new Rectangle(
                                    5 * playerSpriteWidth + (frame * playerSpriteWidth),
                                    2 * playerSpriteHeight,
                                    playerSpriteWidth,
                                    playerSpriteHeight),
                                Color.White,
                                0,
                                new Vector2(playerSpriteOriginX, playerSpriteOriginY),
                                2f,
                                SpriteEffects.FlipHorizontally,
                                0);
                        }
                    }

                    break;
            }
        }

        public override void Attack()
        {
            if (FacingLeft)
            {
                playerSword.X = Position.X - Range;
                playerSword.Y = Position.Y;
            }
            else
            {
                playerSword.X = Position.X + Position.Width;
                playerSword.Y = Position.Y;
            }
        }

        public override void Move()
        {
            // Exists to satify override requisite.
        }

        public void Move(int speed)
        {
            position.X += speed;
        }

        // Writes player's progression stats to a save file.
        public void SaveData()
        {
            filename = "../../../../../data_files/Player.txt";
            StreamWriter output = null;

            try
            {
                output = new StreamWriter(filename);

                // All data is written to the save file.
                output.WriteLine(levelsUnlocked);
                output.WriteLine(bonusHealth);
                output.WriteLine(bonusDamage);
                output.WriteLine(bonusMana);
                output.WriteLine(isOrbCollected[0]);
                output.WriteLine(isOrbCollected[1]);
                output.WriteLine(isOrbCollected[2]);
                output.WriteLine(isOrbCollected[3]);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error saving data: " + e.Message);
            }

            if (output != null)
            {
                output.Close();
            }

        }

        // Clears the save file by restoring all values to their defaults.
        public void ResetSave()
        {
            filename = "../../../../../data_files/Player.txt";
            StreamWriter output = null;

            try
            {
                output = new StreamWriter(filename);

                // All save data is reset.
                output.WriteLine(0);
                output.WriteLine(0);
                output.WriteLine(0);
                output.WriteLine(0);
                output.WriteLine("false");
                output.WriteLine("false");
                output.WriteLine("false");
                output.WriteLine("false");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error saving data: " + e.Message);
            }

            if (output != null)
            {
                output.Close();
            }
        }

        // Reads the save file and assigns data accordingly.
        public void LoadSave()
        {
            filename = "../../../../../data_files/Player.txt";
            StreamReader input = null;

            // Attempts to read data from the file.
            try
            {
                input = new StreamReader(filename);

                // Each line represents different data pertaining to the player.
                for (int i = 0; i <= 8; i++)
                {
                    switch (i)
                    {
                        // Number of levels Unlocked (0-3)
                        case 0:
                            this.levelsUnlocked = int.Parse(input.ReadLine());
                            break;
                        // Amount of bonus health (0 - 40)
                        case 1:
                            this.bonusHealth = int.Parse(input.ReadLine());
                            break;
                        // Amount of bonus damage (0 - 20)
                        case 2:
                            this.bonusDamage = int.Parse(input.ReadLine());
                            break;
                        // Amount of bonus mana (0 - 4)
                        case 3:
                            this.bonusMana = int.Parse(input.ReadLine());
                            break;
                        // Boolean for if you have collected the orb for level 1
                        case 4:
                            this.isOrbCollected[0] = bool.Parse(input.ReadLine());
                            break;
                        // Boolean for if you have collected the orb for level 2
                        case 5:
                            this.isOrbCollected[1] = bool.Parse(input.ReadLine());
                            break;
                        // Boolean for if you have collected the orb for level 3
                        case 6:
                            this.isOrbCollected[2] = bool.Parse(input.ReadLine());
                            break;
                        // Boolean for if you have collected the orb for level 4
                        case 7:
                            this.isOrbCollected[3] = bool.Parse(input.ReadLine());
                            break;

                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading save: " + e.Message);
            }

            if (input != null)
            {
                input.Close();
            }

        }

        // Updates the players stats by incorporating bonus values.
        public void UpdateStats()
        {
            maxHealth += bonusHealth * 10;
            health += bonusHealth * 10;
            maxMana += bonusMana;
            currentMana += bonusMana;
            damage += (bonusDamage * 5);
        }

        // Resets player to their default position, health, and mana.
        public void ResetPlayer()
        {
            health = maxHealth;
            currentMana = maxMana;
            position.X = 40;
            position.Y = 460;
            facingLeft = false;
            drawCollisionBox = false;
        }

    }

}
