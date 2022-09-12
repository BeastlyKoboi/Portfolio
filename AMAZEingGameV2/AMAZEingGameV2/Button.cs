using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AMAZEingGameV2
{
    /// <summary>
    /// Allows implementation of events with methods matching this delegate.
    /// The event being hte button being clicked.
    /// </summary>
    public delegate void OnButtonClickdelegate();

    /// <summary>
    /// Class to 
    /// </summary>
    public class Button : GameObject
    {
        // Fields to hold the button's text, font, and text location
        private string buttonText;
        private Color textColor;
        private SpriteFont font;
        private Vector2 textPos;
        private MouseState prevMouseState;
        public bool IsSelected { get; set; }
        private Texture2D selectedTexture;

        /// <summary>
        /// When left clicked, does all the methods attached
        /// </summary>
        public event OnButtonClickdelegate OnLeftClick;

        /// <summary>
        /// When right clicked, does all the methods attached
        /// </summary>
        public event OnButtonClickdelegate OnRightClick;

        /// <summary>
        /// Constructor to initilize the button's starting fields
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="buttonTexture"></param>
        /// <param name="buttonPos"></param>
        /// <param name="buttonColor"></param>
        /// <param name="buttonText"></param>
        /// <param name="font"></param>
        public Button(GraphicsDevice graphicsDevice, Texture2D buttonTexture, Rectangle buttonPos,
            Color buttonColor, String buttonText, Color textColor, SpriteFont font)
            : base(buttonTexture, buttonPos)
        {
            // Save info
            position = buttonPos;
            this.buttonText = buttonText;
            this.font = font;
            this.Asset = buttonTexture;

            // Sets text color
            if (textColor != null)
                this.textColor = textColor;
            else
                this.textColor = Color.Black;

            // Calculates and saves the position of the text
            Vector2 stringSize = font.MeasureString(buttonText);
            textPos = new Vector2((position.X + position.Width / 2) - stringSize.X / 2,
                (position.Y + position.Height / 2) - stringSize.Y / 2);

            selectedTexture = new Texture2D(graphicsDevice, position.Width, position.Height, false, SurfaceFormat.Color);
            int[] colorData = new int[selectedTexture.Width * selectedTexture.Height];
            Array.Fill<int>(colorData, (int)Color.Green.PackedValue);
            selectedTexture.SetData<Int32>(colorData, 0, colorData.Length);

            // If texture null, give it given color instead
            if (Asset == null)
            {
                Asset = new Texture2D(graphicsDevice, position.Width, position.Height, false, SurfaceFormat.Color);
                colorData = new int[Asset.Width * Asset.Height];
                Array.Fill<int>(colorData, (int)buttonColor.PackedValue);
                Asset.SetData<Int32>(colorData, 0, colorData.Length);
            }


        }

        /// <summary>
        /// Updates the button to check for clicks
        /// </summary>
        public override void Update()
        {
            // Saves the current mouse state
            MouseState mouseState = Mouse.GetState();

            // If mouse is left clicked and the button contains the mouse
            if (prevMouseState.LeftButton == ButtonState.Pressed &&
                mouseState.LeftButton != ButtonState.Pressed &&
                position.Contains(mouseState.Position))
            {
                if (OnLeftClick != null)
                {
                    OnLeftClick();
                }
            }

            // If mouse is right clicked and the button contains the mouse
            if (prevMouseState.RightButton == ButtonState.Pressed &&
                mouseState.RightButton != ButtonState.Pressed &&
                position.Contains(mouseState.Position))
            {
                if (OnRightClick != null)
                {
                    OnRightClick();
                }
            }


            // Makes the current mouse state the previous
            prevMouseState = mouseState;
        }

        /// <summary>
        /// Draws the button to the window
        /// </summary>
        /// <param name="sb"></param>
        public override void Draw(SpriteBatch sb)
        {
            // Draws the button texture
            if (IsSelected)
                sb.Draw(selectedTexture, position, Color.White);
            else
                sb.Draw(Asset, position, Color.White);

            // Draws the button text
            sb.DrawString(font, buttonText, textPos, textColor);
        }

    }
}
