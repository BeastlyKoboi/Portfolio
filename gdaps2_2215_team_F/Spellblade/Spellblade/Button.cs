using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Spellblade
{
    public class Button
    {
        // Fields to hold button texture and position
        private Texture2D buttonTexture;
        private Rectangle position;

        // Fields to hold the button's text, font, and text location
        private string buttonText;
        private SpriteFont font;
        private Vector2 textPos;
        private Texture2D buttonSprite;
        private Rectangle rectangle;
        private Color aliceBlue;
        private string v;
        private SpriteFont defaultFont;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="buttonPos"></param>
        /// <param name="color"></param>
        /// <param name="buttonText"></param>
        /// <param name="font"></param>
        public Button(GraphicsDevice graphicsDevice, Texture2D buttonTexture, Rectangle buttonPos,
            Color color, String buttonText, SpriteFont font)
        {
            // 
            position = buttonPos;
            this.buttonText = buttonText;
            this.font = font;
            this.buttonTexture = buttonTexture;

            //
            Vector2 stringSize = font.MeasureString(buttonText);
            textPos = new Vector2((position.X + position.Width / 2) - stringSize.X / 2,
                (position.Y + position.Height / 2) - stringSize.Y / 2);

            //
            buttonTexture = new Texture2D(graphicsDevice, position.Width, position.Height, false, SurfaceFormat.Color);
            int[] colorData = new int[buttonTexture.Width * buttonTexture.Height]; 
            Array.Fill<int>(colorData, (int)color.PackedValue);
            buttonTexture.SetData<Int32>(colorData, 0, colorData.Length);

        }

        public Button(Texture2D buttonSprite, Rectangle rectangle, Color aliceBlue, string v, SpriteFont defaultFont)
        {
            this.buttonSprite = buttonSprite;
            this.rectangle = rectangle;
            this.aliceBlue = aliceBlue;
            this.v = v;
            this.defaultFont = defaultFont;
        }

        //
        public bool ButtonClicked(MouseState prevMState, MouseState mouseState)
        {
            return (prevMState.LeftButton == ButtonState.Pressed &&
                mouseState.LeftButton != ButtonState.Pressed &&
                position.Contains(mouseState.Position));
        }

        //
        public void Draw(SpriteBatch sb)
        {
            //
            sb.Draw(buttonTexture, position, Color.White);

            //
            sb.DrawString(font, buttonText, textPos, Color.White);
        }



    }
}
