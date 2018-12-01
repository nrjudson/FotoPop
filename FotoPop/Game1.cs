using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FotoPop
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // The dimensions of the viewport
        Rectangle screenRect;

        // The image to show
        Texture2D foto;
        Rectangle fotoRect;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            //graphics.ToggleFullScreen();
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();


            


        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Grab the screen dimensions
            screenRect = GraphicsDevice.Viewport.Bounds;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            // Load the photo to show and scale it to ~70% of the screen width
            foto = this.Content.Load<Texture2D>("selfie");
            float fotoToScreenWidthPercentage = 0.7f;
            float fotoTargetWidth = fotoToScreenWidthPercentage * (float)screenRect.Width;
            float fotoScale = fotoTargetWidth / (float)foto.Width;
            int fotoXPos = (int) (((1.0f - fotoToScreenWidthPercentage) / 2.0f) * screenRect.Width); // Center the X position
            int fotoYPos = (int)(0.1f * screenRect.Height); // Start the img 10% from the top of the screen
            fotoRect = new Rectangle(fotoXPos, fotoYPos, (int) fotoTargetWidth, (int) (fotoScale * (float) foto.Height));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            spriteBatch.Begin();

            // Draw the photo
            spriteBatch.Draw(foto, fotoRect, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
