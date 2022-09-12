using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Text;

namespace Challenge_TheFountainOfObjects
{
    /// <summary>
    /// Class to represent a room
    /// </summary>
    public class Room : GameObject
    {
        // Fields to hold the room state
        public RoomState State { get; set; }

        // Fields to hold the rectangles for each color on the asset
        private static Rectangle[] unvisitedAssetPositions = {
            new Rectangle(16, 16, 16, 16),
            new Rectangle(0, 0, 16, 16),
            new Rectangle(0, 16, 16, 16),
            new Rectangle(0, 16, 16, 16),
            new Rectangle(16, 0, 16, 16),
            new Rectangle(16, 0, 16, 16),
            new Rectangle(16, 0, 16, 16), };
        private static Rectangle[] visitedAssetPositions = {
            new Rectangle(48, 16, 16, 16),
            new Rectangle(0, 0, 16, 16),
            new Rectangle(0, 16, 16, 16),
            new Rectangle(32, 16, 16, 16),
            new Rectangle(48, 0, 16, 16),
            new Rectangle(48, 0, 16, 16),
            new Rectangle(48, 0, 16, 16), };

        // Fields to hold the position of room in grid
        public Point GridPos { get; }

        // Field to hold whether the room has been visited
        public bool Visited { get; set; }

        /// <summary>
        /// Constructor to give the room its starting values
        /// </summary>
        /// <param name="Asset"></param>
        /// <param name="Position"></param>
        /// <param name="state"></param>
        public Room(Texture2D Asset, Rectangle Position, RoomState state, Point gridPos) 
            : base(Asset, Position)
        {
            State = state;
            GridPos = gridPos;
        }

        /// <summary>
        /// Method to update the state of this object
        /// </summary>
        public override void Update()
        {
            
        }

        /// <summary>
        /// Method to draw the room color
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visited)
                spriteBatch.Draw(Asset, position, visitedAssetPositions[(int)State], Color.White);
            else
                spriteBatch.Draw(Asset, position, unvisitedAssetPositions[(int)State], Color.White);
        }

        /// <summary>
        /// Method to draw the player in this room
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void DrawPlayer(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Asset, new Rectangle(position.X + position.Width / 4, position.Y + position.Height / 4,
                position.Width / 2, position.Height / 2), unvisitedAssetPositions[(int)RoomState.Start], Color.White);
        }

    }
}
