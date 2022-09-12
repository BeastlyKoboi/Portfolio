using System;

namespace AMAZEingGameV2
{
    // Enum to hold the different types of cells
    public enum CellType
    {
        Start,
        Empty,
        Wall,
        Exit
    }

    /// <summary>
    /// Class to hold each vertex's cell type, position,
    /// and whether it has been visited.
    /// </summary>
    public class Vertex
    {
        // Fields to hold the cell type, position, and visited status
        private CellType type;
        private int x;
        private int y;
        private Boolean visited;

        #region Properties for the fields
        /// <summary>
        /// Property to get and set the cell type
        /// </summary>
        public CellType Type
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// Property to get and set X position
        /// </summary>
        public int X
        {
            get { return x; }
            set { x = value; }
        }

        /// <summary>
        /// Property to get and set Y position
        /// </summary>
        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        /// <summary>
        /// Property to get and set whether it has been visited
        /// </summary>
        public Boolean Visited
        {
            get { return visited; }
            set { visited = value; }
        }
        #endregion 

        public Vertex(CellType type, int x, int y)
        {
            this.type = type;
            this.x = x;
            this.y = y;
            this.visited = false; 
        }
    }
}
