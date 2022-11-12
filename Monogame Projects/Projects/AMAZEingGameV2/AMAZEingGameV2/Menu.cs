using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AMAZEingGameV2
{
    public class Menu : GameObject
    {
        // 
        public Button[] menuButtons { get; set; }

        public Menu(GraphicsDevice graphicsDevice, Color background, Rectangle pos, Button[] buttons)
            : base(null, pos)
        {
            // Create a 1x1 white pixel texture, to be tinted later, and save it
            Texture2D pixel = new Texture2D(graphicsDevice, 1, 1);
            pixel.SetData<Color>(new Color[] { background });

            Asset = pixel;

            // Save the given buttons
            menuButtons = buttons;
        }


        public override void Update()
        {
            foreach (Button button in menuButtons)
            {
                button.Update();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            foreach (Button button in menuButtons)
            {
                button.Draw(spriteBatch);
            }
        }
    }
}
