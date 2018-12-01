using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace FotoPop
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont title;
        // The dimensions of the viewport
        Rectangle screenRect;

        // The image to show
        Texture2D photo;
        Rectangle fotoRect;
        float fotoScale;

        float timeForLevel = 10.0f;
        float elapsedTime = 0.0f;

        class Level
        {
            public string name;
            public List<Photo> photos;
        }

        class Photo
        {
            public string name;
            public List<Objective> objectives;
        }

        class Objective
        {
            public int x;
            public int y;
            public List<string> words;
        }

        Level level;

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

            // Load font
            title = this.Content.Load<SpriteFont>("Fonts/title");

            loadLevel("City");
            photo = this.Content.Load<Texture2D>("Levels/" + level.name + "/Photos/" + level.photos[0].name);
            setAndScalePhoto(photo);
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

            // Poll for current keyboard state
            KeyboardState state = Keyboard.GetState();

            // If they hit Tab, move to the City level
            if (state.IsKeyDown(Keys.Tab))
            {
                //Game1 g = new Game1();
                //GameStateManager gms = new GameStateManager();
                //gms.getLevel(1).LoadContent();
            }


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
            spriteBatch.Draw(photo, fotoRect, Color.White);

            // Draw the circle that goes over the photo 
            // (Examples for now) (The unscaled selfie image is 660 x 371)
            // TODO: Move some of this logic to UPDATE
            spriteBatch.DrawCircle(getCircle(0, 0), 100, Color.White);
            spriteBatch.DrawCircle(getCircle(330, 185), 100, Color.White);
            spriteBatch.DrawCircle(getCircle(660, 371), 100, Color.White);

            // Draw the rectangle that shows how much time is left
            // TODO: Move some of this logic to UPDATE
            elapsedTime += gameTime.GetElapsedSeconds();
            float timeLeft = timeForLevel - elapsedTime;
            Rectangle outerTimeRect = new Rectangle(fotoRect.X, (int)(0.04f * screenRect.Height), fotoRect.Width, (int)(0.03f * screenRect.Height));
            spriteBatch.FillRectangle(outerTimeRect, Color.White);

            float proportionTimeLeft = timeLeft / timeForLevel;
            Rectangle innerTimeRect = new Rectangle(fotoRect.X, (int)(0.04f * screenRect.Height), (int)(fotoRect.Width * proportionTimeLeft), (int)(0.03f * screenRect.Height));
            Color colorForTime;
            if (proportionTimeLeft > 0.5f)
                colorForTime = Color.Green;
            else if (proportionTimeLeft > 0.2f)
                colorForTime = Color.Yellow;
            else if (proportionTimeLeft > 0.1)
                colorForTime = Color.Orange;
            else
                colorForTime = Color.Red;
            spriteBatch.FillRectangle(innerTimeRect, colorForTime);

            // Draw the time left for the level
            //spriteBatch.DrawString()

               



            spriteBatch.End();


            //draw text title
            spriteBatch.Begin();
            spriteBatch.DrawString(title, "Hello Noah", new Vector2(1, 1), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }


        /// <summary>
        /// Used to get the dimensions of the circle to show on the photo
        /// Converts absolute positioning of the pixels in the original image to the scaled coordinates in whatever is being shown.
        /// </summary>
        /// <param name="origX">X Pos in unscaled photo</param>
        /// <param name="origY">Y Pos in unscaled photo</param>
        /// <returns>CircleF that should be drawn on the photo</returns>
        private CircleF getCircle(int origX, int origY)
        {
            int diameter = (int)(screenRect.Width / 20.0f);
            int radius = (int)(diameter / 2.0f);
            int newX = (int)(fotoScale * origX) + fotoRect.X;
            int newY = (int)(fotoScale * origY) + fotoRect.Y;

            return new CircleF(new Point2(newX, newY), radius);
        }


        /// <summary>
        /// Read the JSON file for the associated level name and set up the level object in the game code
        /// </summary>
        /// <param name="name"></param>
        private void loadLevel(string name)
        {
            // Open the file to read
            using (StreamReader r = new StreamReader("Levels/" + name + ".json"))
            {
                string json = r.ReadToEnd();
                JObject levelJson = (JObject)JsonConvert.DeserializeObject(json);

                level = new Level();

                level.name = levelJson.GetValue("levelName").ToString();
                JArray photos = (JArray)levelJson.GetValue("photos");

                level.photos = new List<Photo>();

                // Iterate over the photos
                using (var photoEnum = photos.GetEnumerator())
                {
                    while (photoEnum.MoveNext())
                    {
                        Photo photo = new Photo();

                        JObject photoJson = (JObject)photoEnum.Current;
                        photo.name = (string)photoJson.GetValue("photo");
                        JArray objectives = (JArray)photoJson.GetValue("objectives");

                        photo.objectives = new List<Objective>();

                        // Iterate over the objectives
                        using (var objectiveEnum = objectives.GetEnumerator())
                        {
                            while (objectiveEnum.MoveNext())
                            {
                                Objective objective = new Objective();

                                JObject objectiveJson = (JObject)objectiveEnum.Current;
                                objective.x = (int)objectiveJson.GetValue("x");
                                objective.y = (int)objectiveJson.GetValue("y");
                                JArray words = (JArray)objectiveJson.GetValue("words");

                                objective.words = new List<string>();

                                // Iterate over the words
                                using (var wordsEnum = words.GetEnumerator())
                                {
                                    while (wordsEnum.MoveNext())
                                    {
                                        objective.words.Add((string)wordsEnum.Current);
                                    }
                                }

                                photo.objectives.Add(objective);
                            }
                        }

                        level.photos.Add(photo);
                    }
                }
            }
        }


        /// <summary>
        /// Load the photo to show and scale it to ~70% of the screen width
        /// </summary>
        /// <param name="photo"></param>
        private void setAndScalePhoto(Texture2D photo)
        {
            float fotoToScreenWidthPercentage = 0.7f;
            float fotoTargetWidth = fotoToScreenWidthPercentage * (float)screenRect.Width;
            fotoScale = fotoTargetWidth / (float)this.photo.Width;
            int fotoXPos = (int)(((1.0f - fotoToScreenWidthPercentage) / 2.0f) * screenRect.Width); // Center the X position
            int fotoYPos = (int)(0.1f * screenRect.Height); // Start the img 10% from the top of the screen
            fotoRect = new Rectangle(fotoXPos, fotoYPos, (int)fotoTargetWidth, (int)(fotoScale * (float)this.photo.Height));
        }


    }
}
