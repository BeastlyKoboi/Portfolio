using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AMAZEingGameV2
{
    /// <summary>
    /// Class generate, display, and handle the mazes
    /// </summary>
    public class MazeManager
    {
        // Fields for containing the current maze and special vertices
        private Vertex[,] mazeVertices;
        private Vertex startVertex;
        private Vertex finalVertex;

        // Fields for Display Info
        private const int VERTEX_PXL_SIZE = 10;
        private Color[] VERTEX_COLORS = { Color.Blue, Color.White, Color.Black, Color.Red };

        private Texture2D pixel;
        private Point offset;

        private int mazeSizeX;
        private int mazeSizeY;

        private int movementTimer;
        private int movementCooldown;

        // Random obj for rng
        private Random rng;

        // Location of player
        private Point playerLoc;

        /// <summary>
        /// Constructor to create white pizel and initialize rng
        /// </summary>
        public MazeManager(GraphicsDevice device)
        {
            // Create a 1x1 white pixel texture, to be tinted later
            pixel = new Texture2D(device, 1, 1);
            pixel.SetData<Color>(new Color[] { Color.White });

            // Initialize rng
            rng = new Random();

            // Initialize starting movement values
            movementTimer = 0;
            movementCooldown = 5;

        }

        /// <summary>
        /// Updates the maze, the position of the player.
        /// </summary>
        public void Update()
        {
            KeyboardState kbState = Keyboard.GetState();

            if (kbState.IsKeyDown(Keys.W) && movementTimer == 0)
            {
                MovePlayerUp();
            }
            else if (kbState.IsKeyDown(Keys.A) && movementTimer == 0)
            {
                MovePlayerLeft();
            }
            else if (kbState.IsKeyDown(Keys.S) && movementTimer == 0)
            {
                MovePlayerDown();
            }
            else if (kbState.IsKeyDown(Keys.D) && movementTimer == 0)
            {
                MovePlayerRight();
            }

            if (movementTimer > 0)
            {
                movementTimer--;
            }
        }

        /// <summary>
        /// Method to use Depth-First Search in order to create a random maze
        /// </summary>
        /// <param name="mazeSizeX"></param>
        /// <param name="mazeSizeY"></param>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        /// <param name="screenWidth"></param>
        /// <param name="screenHeight"></param>
        public void CreateDFSMaze(int mazeSizeX, int mazeSizeY, int originX, int originY, int screenWidth, int screenHeight)
        {
            // Overwrites the size of the maze
            this.mazeSizeX = mazeSizeX;
            this.mazeSizeY = mazeSizeY;

            // Offsets (for centering)
            offset.X = originX + (screenWidth - VERTEX_PXL_SIZE * mazeSizeX) / 2;
            offset.Y = originY + (screenHeight - VERTEX_PXL_SIZE * mazeSizeY) / 2;

            // Set up the Vertices
            mazeVertices = new Vertex[mazeSizeX, mazeSizeY];
            for (int y = 0; y < mazeSizeY; y++)
            {
                for (int x = 0; x < mazeSizeX; x++)
                {
                    // Set up the data to represent an empty space
                    CellType currentType = CellType.Wall;

                    // Set up this Vertex and check for start/end
                    mazeVertices[x, y] = new Vertex(currentType, x, y);
                }
            }

            // Saves the start and exit
            startVertex = mazeVertices[1, 1];
            startVertex.Type = CellType.Start;
            finalVertex = mazeVertices[mazeSizeX - 2, mazeSizeY - 2];
            finalVertex.Type = CellType.Exit;

            // Saves the player origin
            playerLoc.X = startVertex.X;
            playerLoc.Y = startVertex.Y;

            // Declare currentVertex and initialize new stack
            Vertex currentVertex;
            Stack<Vertex> stack = new Stack<Vertex>();

            // Push start vertex and mark it visited
            startVertex.Visited = true;
            stack.Push(startVertex);

            // Loop creates paths to exit and dead ends,
            // throughout entire maze 
            while (stack.Count != 0)
            {
                // Saves the new current vertex
                currentVertex = stack.Pop();

                // If exit is not found, keep going
                if (currentVertex.Type != CellType.Exit)
                {
                    // create list of unvisited neighbors
                    List<Vertex> neighbors = new List<Vertex>();

                    // Checks for neighbors that are not visited
                    if (TileExists(currentVertex.X - 2, currentVertex.Y) &&
                        !mazeVertices[currentVertex.X - 2, currentVertex.Y].Visited)
                        neighbors.Add(mazeVertices[currentVertex.X - 2, currentVertex.Y]);

                    if (TileExists(currentVertex.X, currentVertex.Y - 2) &&
                        !mazeVertices[currentVertex.X, currentVertex.Y - 2].Visited)
                        neighbors.Add(mazeVertices[currentVertex.X, currentVertex.Y - 2]);

                    if (TileExists(currentVertex.X + 2, currentVertex.Y) &&
                        !mazeVertices[currentVertex.X + 2, currentVertex.Y].Visited)
                        neighbors.Add(mazeVertices[currentVertex.X + 2, currentVertex.Y]);

                    if (TileExists(currentVertex.X, currentVertex.Y + 2) &&
                        !mazeVertices[currentVertex.X, currentVertex.Y + 2].Visited)
                        neighbors.Add(mazeVertices[currentVertex.X, currentVertex.Y + 2]);

                    // If there are remaining neighbors,
                    // push current vert, make neighbor empty,
                    // then push the neighbor too
                    if (neighbors.Count != 0)
                    {
                        // Declare vertex to hold random vertex
                        Vertex randVertex;

                        // Add current vertex to stack
                        stack.Push(currentVertex);

                        // Picks random neighbor and saves it
                        randVertex = neighbors[rng.Next(neighbors.Count)];

                        // Makes wall in between current and rand an empty cell and visited too
                        mazeVertices[currentVertex.X + (randVertex.X - currentVertex.X) / 2,
                            currentVertex.Y + (randVertex.Y - currentVertex.Y) / 2].Visited = true;
                        mazeVertices[currentVertex.X + (randVertex.X - currentVertex.X) / 2,
                            currentVertex.Y + (randVertex.Y - currentVertex.Y) / 2].Type = CellType.Empty;

                        randVertex.Visited = true;

                        if (randVertex.Type == CellType.Wall)
                        {
                            randVertex.Type = CellType.Empty;
                        }

                        // Add the random vertex to stack
                        stack.Push(randVertex);
                    }
                }
            }
            // end of while
            ResetAllVertices();
        }

        #region Methods to move player
        /// <summary>
        /// Moves player upwards once if possible
        /// </summary>
        public void MovePlayerUp()
        {
            if (TileExists(playerLoc.X, playerLoc.Y - 1) &&
                mazeVertices[playerLoc.X, playerLoc.Y - 1].Type != CellType.Wall)
            {
                playerLoc.Y -= 1;
                movementTimer += movementCooldown;

                if (!mazeVertices[playerLoc.X, playerLoc.Y].Visited)
                {
                    mazeVertices[playerLoc.X, playerLoc.Y].Visited = true;
                }
            }
        }

        /// <summary>
        /// Moves player downwards once if possible
        /// </summary>
        public void MovePlayerDown()
        {
            if (TileExists(playerLoc.X, playerLoc.Y + 1) &&
                mazeVertices[playerLoc.X, playerLoc.Y + 1].Type != CellType.Wall)
            {
                playerLoc.Y += 1;
                movementTimer += movementCooldown;

                if (!mazeVertices[playerLoc.X, playerLoc.Y].Visited)
                {
                    mazeVertices[playerLoc.X, playerLoc.Y].Visited = true;
                }
            }
        }

        /// <summary>
        /// Moves player to the left once if possible
        /// </summary>
        public void MovePlayerLeft()
        {
            if (TileExists(playerLoc.X - 1, playerLoc.Y) &&
                mazeVertices[playerLoc.X - 1, playerLoc.Y].Type != CellType.Wall)
            {
                playerLoc.X -= 1;
                movementTimer += movementCooldown;

                if (!mazeVertices[playerLoc.X, playerLoc.Y].Visited)
                {
                    mazeVertices[playerLoc.X, playerLoc.Y].Visited = true;
                }
            }
        }

        /// <summary>
        /// Moves player to the right once if possible
        /// </summary>
        public void MovePlayerRight()
        {
            if (TileExists(playerLoc.X + 1, playerLoc.Y) &&
                mazeVertices[playerLoc.X + 1, playerLoc.Y].Type != CellType.Wall)
            {
                playerLoc.X += 1;
                movementTimer += movementCooldown;

                if (!mazeVertices[playerLoc.X, playerLoc.Y].Visited)
                {
                    mazeVertices[playerLoc.X, playerLoc.Y].Visited = true;
                }
            }
        }
        #endregion

        /// <summary>
        /// Draws the vertices in the maze, with tinting
        /// to indicate their type.
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void DrawMaze(SpriteBatch spriteBatch)
        {
            // Initializes the rectangle to use for drawing
            Rectangle rect = new Rectangle();
            rect.Width = VERTEX_PXL_SIZE;
            rect.Height = VERTEX_PXL_SIZE;

            // Loop and draw
            for (int x = 0; x < mazeSizeX; x++)
            {
                for (int y = 0; y < mazeSizeY; y++)
                {
                    // Calculates the location of this rect
                    rect.X = x * VERTEX_PXL_SIZE + offset.X;
                    rect.Y = y * VERTEX_PXL_SIZE + offset.Y;

                    // Draws this vertex with tint
                    if (mazeVertices[x,y].Visited) 
                        spriteBatch.Draw(pixel, rect, Color.LightGreen);
                    else
                        spriteBatch.Draw(pixel, rect, VERTEX_COLORS[(int)mazeVertices[x, y].Type]);
                }
            }

            // Draw player
            rect.X = playerLoc.X * VERTEX_PXL_SIZE + offset.X;
            rect.Y = playerLoc.Y * VERTEX_PXL_SIZE + offset.Y;
            spriteBatch.Draw(pixel, rect, Color.Green);
        }

        /// <summary>
		/// Sets all Vertices to "not visited" and return the start Vertex
		/// </summary>
		public void ResetAllVertices()
        {
            for (int x = 0; x < mazeSizeX; x++)
            {
                for (int y = 0; y < mazeSizeY; y++)
                {
                    // Reset the Vertex
                    mazeVertices[x, y].Visited = false;
                }
            }

            playerLoc.X = startVertex.X;
            playerLoc.Y = startVertex.Y;
        }

        /// <summary>
        /// Returns true if x and y are within bounds of maze
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool TileExists(int x, int y)
        {
            // Valid indices?
            return (y >= 0 && x >= 0 &&
                y < mazeVertices.GetLength(0) &&
                x < mazeVertices.GetLength(1));
        }
    }
}
