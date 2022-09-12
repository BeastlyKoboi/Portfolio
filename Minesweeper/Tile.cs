using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Text;

namespace Minesweeper
{
    /// <summary>
    /// Enum to list the type of tile
    /// </summary>
    public enum TileType { Grass, Mine }

    //
    public delegate void OnTileClickDelegate(Point point);

    /// <summary>
    /// Class to represent a tile on the board
    /// </summary>
    public class Tile : GameObject
    {
        //
        private static Rectangle[] textureLocs = {
            new Rectangle(0, 0, 16, 16),
            new Rectangle(0, 16, 16, 16),
            new Rectangle(0, 32, 16, 16),
            new Rectangle(0, 48, 16, 16)};

        //
        public event OnTileClickDelegate OnLeftClick;
        public event OnTileClickDelegate OnRightClick;

        private bool isCovered;
        private bool isFlagged;
        private int numMinesNear;

        /// <summary>
        /// Auto-property to save the type of tile
        /// </summary>
        public TileType Type { get; set; }

        /// <summary>
        /// Auto-property to save the number of mines near the tile
        /// </summary>
        public int NumMinesNear
        {
            get
            {
                return numMinesNear;
            }
            set
            {
                numMinesNear = value;
                Vector2 size = numFont.MeasureString(numMinesNear.ToString());
                numPos.X = position.X + (position.Width - size.X) / 2;
                numPos.Y = position.Y + (position.Height - size.Y) / 2;
            }
        }

        /// <summary>
        /// Auto-property to save whether the tile is still covered or not
        /// </summary>
        public bool IsCovered
        {
            get
            {
                return isCovered;
            }
            set
            {
                if (value)
                {
                    CurrentTextureLoc = textureLocs[0];
                }
                else
                {
                    if (Type == TileType.Grass)
                        CurrentTextureLoc = textureLocs[3];
                    else
                        CurrentTextureLoc = textureLocs[2];
                }

                isCovered = value;
            }
        }

        /// <summary>
        /// Auto-property to save whether the tile has been flagged
        /// </summary>
        public bool IsFlagged
        {
            get
            {
                return isFlagged;
            }

            set
            {
                if (IsCovered)
                {
                    isFlagged = value;

                    if (isFlagged)
                        CurrentTextureLoc = textureLocs[1];
                    else
                        CurrentTextureLoc = textureLocs[0];
                }
                else
                {
                    isFlagged = false;
                }
            }
        }

        /// <summary>
        /// Save the position of the current texture
        /// </summary>
        private Rectangle CurrentTextureLoc;

        // Saves the state of the mouse for the previous frame
        private MouseState prevMouseState;

        //
        Point gridPos;
        Vector2 numPos;

        //
        private SpriteFont numFont;
        

        /// <summary>
        /// Parameterized constructor to initialize the tile to a default state
        /// </summary>
        /// <param name="Asset"></param>
        /// <param name="Position"></param>
        public Tile(Texture2D Asset, Rectangle Position, Point gridPos, SpriteFont numFont)
            : base(Asset, Position)
        {
            Type = TileType.Grass;
            numMinesNear = 0;
            IsCovered = true;
            IsFlagged = false;
            this.gridPos = gridPos;
            this.numFont = numFont;
        }

        /// <summary>
        /// Method to override update, and check the mouse for clicks
        /// </summary>
        public override void Update()
        {
            // Gets vurrent mouse state
            MouseState mouseState = Mouse.GetState();
            
            // Checks if covered by grass and left clicked
            if (IsCovered && LeftClicked(mouseState))
            { 
                if (OnLeftClick != null)
                    OnLeftClick(gridPos);
            }

            // Checks if covered by grass and right clicked
            if (IsCovered && RightClicked(mouseState))
            {
                CurrentTextureLoc = textureLocs[1];
                IsFlagged = !IsFlagged;

                if (OnRightClick != null)
                    OnRightClick(gridPos);
            }


            // Saves current mouse state as previous before ending
            prevMouseState = mouseState;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Asset, Position, CurrentTextureLoc, Color.White);

            if (!IsCovered && NumMinesNear != 0 && Type != TileType.Mine)
            {
                spriteBatch.DrawString(numFont, NumMinesNear.ToString(), numPos, Color.Black);
            }
        }

        /// <summary>
        /// Method to check whether tile has been left clicked
        /// </summary>
        /// <param name="mouseState"></param>
        /// <returns></returns>
        private bool LeftClicked(MouseState mouseState)
        {
            return (prevMouseState.LeftButton == ButtonState.Pressed &&
                mouseState.LeftButton != ButtonState.Pressed &&
                position.Contains(mouseState.Position));
        }

        /// <summary>
        /// Method to check whether tile has been right clicked
        /// </summary>
        /// <param name="mouseState"></param>
        /// <returns></returns>
        private bool RightClicked(MouseState mouseState)
        {
            return (prevMouseState.RightButton == ButtonState.Pressed &&
                mouseState.RightButton != ButtonState.Pressed &&
                position.Contains(mouseState.Position));
        }
    }
}
