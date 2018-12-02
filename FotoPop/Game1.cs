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
        SpriteFont sm;
        // The dimensions of the viewport
        Rectangle screenRect;

        // The image showing
        Texture2D photo;
        Rectangle photoRect;
        float photoScale;

        //TextBox textbox;
        Rectangle textRect;

        // 
        string yourInput = "Start typing!";
        bool neverTyped = true;

        float maxPointsPerWord = 100;

        string yourName = "Bob";
        float score = 0;

        bool newLevelLoaded = false;
        int currentPhotoIndex = 0;
        int currentObjectiveIndex = 0;
        static float timeForLevel = 45.0f;
        static float timeForWord = 10.0F;
        float elapsedTimeForLevel = 0.0f;
        float elapsedTimeForWord = 0.0f;
        float timeLeftForLevel = 1.0f;
        float timeLeftForWord = 1.0f;

        List<string> correctAnswers;


        // High Scores: LevelName -> Map of high scores
        private Dictionary<string, SortedDictionary<string, float>> highScores;

        bool selectingLevel = true;
        bool enteringName = false;
        bool seeingHighScore = false;
        int levelIndex = 0;
        int numLevels = 4;

        float lastKeyPressTime = 0.0f;
        float lastWordCheckTime = 0.0f;
        static string gameName = "PhotoPop";

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
            // UNCOMMENT FOR WINDOWED-MODE
            graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width - 800;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height - 450;
            graphics.ApplyChanges();

            // UNCOMMENT FOR FULL SCREEN
            //graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            //graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            //graphics.ToggleFullScreen();

            // Grab the screen dimensions
            screenRect = GraphicsDevice.Viewport.Bounds;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load font
            title = this.Content.Load<SpriteFont>("Fonts/title");
            sm = this.Content.Load<SpriteFont>("Fonts/sm");

            // load high scores
            loadHighScores();

            loadLevel("Market");
            //loadLevel("Nature");
            //loadLevel("City");
            photo = this.Content.Load<Texture2D>(getCurrentPhotoUri());
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
            if (enteringName)
            {
                updateEnterName(gameTime);
            }
            else if (selectingLevel)
            {
                updateSelectLevel(gameTime);
            }
            else if (seeingHighScore)
            {
                updateSeeingHighScore(gameTime);
            }
            else
            {
                updateGame(gameTime);
            }
            
            base.Update(gameTime);
        }


        private void updateSeeingHighScore(GameTime gameTime)
        {
            float now = (float)gameTime.TotalGameTime.TotalSeconds;

            KeyboardState state = Keyboard.GetState();

            // If they hit Tab, move to the City level
            if (state.IsKeyDown(Keys.Space))
            {
                selectingLevel = true;
                seeingHighScore = false;
                score = 0.0f;
            }
        }


        private void updateSelectLevel(GameTime gameTime)
        {
            float now = (float)gameTime.TotalGameTime.TotalSeconds;

            float secondsBetweenLetters = 0.16f;

            // Check all key presses
            if (now > lastKeyPressTime + secondsBetweenLetters)
            {
                // Poll for current keyboard state
                KeyboardState state = Keyboard.GetState();

                // If they hit Tab, move to the City level
                if (state.IsKeyDown(Keys.Up))
                {
                    levelIndex--;
                    lastKeyPressTime = (float)(gameTime.TotalGameTime.TotalSeconds);

                }
                else if (state.IsKeyDown(Keys.Down))
                {
                    levelIndex++;
                    lastKeyPressTime = (float)(gameTime.TotalGameTime.TotalSeconds);
                }
                if (levelIndex < 0)
                {
                    levelIndex = numLevels - 1;
                }
                else if (levelIndex > numLevels - 1)
                {
                    levelIndex = 0;
                }

                if (state.IsKeyDown(Keys.Enter))
                {
                    if (levelIndex == 0)
                    {
                        loadLevel("City");
                    }
                    else if (levelIndex == 1)
                    {
                        loadLevel("Nature");
                    }
                    else if (levelIndex == 2)
                    {
                        loadLevel("Market");
                    }
                    else if (levelIndex == 3)
                    {
                        loadLevel("Verbs");
                    }
                    // Show and load the new photo
                    photo = this.Content.Load<Texture2D>(getCurrentPhotoUri());
                    setAndScalePhoto(photo);

                    enteringName = true;
                    selectingLevel = false;
                    lastKeyPressTime = (float)(gameTime.TotalGameTime.TotalSeconds);
                }
            }
        }


        private void updateEnterName(GameTime gameTime)
        {
            float now = (float)gameTime.TotalGameTime.TotalSeconds;

            float secondsBetweenLetters = 0.16f;

            // Check all key presses
            if (now > lastKeyPressTime + secondsBetweenLetters)
            {
                // Poll for current keyboard state
                KeyboardState state = Keyboard.GetState();

                Keys[] keys = state.GetPressedKeys();
                if (keys.Length > 0)
                {
                    if (neverTyped)
                    {
                        yourName = "";
                    }
                    neverTyped = false;
                }

                // Check all key presses
                if (now > lastKeyPressTime + secondsBetweenLetters)
                {
                    foreach (Keys key in keys)
                    {
                        // Is the key pressed a letter?
                        if (key >= Keys.A && key <= Keys.Z)
                        {
                            yourName += key;
                            lastKeyPressTime = (float)(gameTime.TotalGameTime.TotalSeconds);
                        }
                        if (key == Keys.Back)
                        {
                            if (yourName.Length > 0)
                            {
                                yourName = yourName.Substring(0, yourName.Length - 1);
                                lastKeyPressTime = (float)(gameTime.TotalGameTime.TotalSeconds);
                            }
                        }
                        if (key == Keys.Space)
                        {
                            yourName += " ";
                            lastKeyPressTime = (float)(gameTime.TotalGameTime.TotalSeconds);
                        }
                        if (key == Keys.Enter)
                        {
                            if (yourName.Equals(""))
                            {
                                yourName = "Bob";
                            }
                            enteringName = false;
                            elapsedTimeForLevel = 0.0f;
                            score = 0.0f;
                            neverTyped = true;
                            lastKeyPressTime = (float)(gameTime.TotalGameTime.TotalSeconds);
                        }
                    }
                }

            }
        }


        private void finishedLevel()
        {
            seeingHighScore = true;

            // Update your high score
            if (highScores[level.name].ContainsKey(yourName))
            {
                float yourPreviousHighScore = highScores[level.name][yourName];
                if (score > yourPreviousHighScore)
                {
                    highScores[level.name].Remove(yourName);
                    highScores[level.name].Add(yourName, score);
                }
            }
            else
            {
                highScores[level.name].Add(yourName, score);
            }
        }


        private void updateGame(GameTime gameTime)
        {
            // Grab the time difference
            elapsedTimeForLevel += gameTime.GetElapsedSeconds();
            elapsedTimeForWord += gameTime.GetElapsedSeconds();

            timeLeftForLevel = timeForLevel - elapsedTimeForLevel;
            timeLeftForWord = timeForWord - elapsedTimeForWord;

            if (timeLeftForLevel < 0)
            {
                finishedLevel();
                return;
            }
            if (timeLeftForWord < 0)
            {
                updateToNextWord();
            }

            // Esc is exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Poll for current keyboard state
            KeyboardState state = Keyboard.GetState();

            // Text entry
            Keys[] keys = state.GetPressedKeys();
            if (keys.Length > 0)
            {
                if (neverTyped)
                {
                    yourInput = "";
                }
                neverTyped = false;
            }


            float now = (float)gameTime.TotalGameTime.TotalSeconds;

            float secondsBetweenLetters = 0.16f;

            // Check all key presses
            if (now > lastKeyPressTime + secondsBetweenLetters)
            {
                foreach (Keys key in keys)
                {
                    // Is the key pressed a letter?
                    if (key >= Keys.A && key <= Keys.Z)
                    {
                        yourInput += key;
                        lastKeyPressTime = (float)(gameTime.TotalGameTime.TotalSeconds);
                    }
                    if (key == Keys.Back)
                    {
                        if (yourInput.Length > 0)
                        {
                            yourInput = yourInput.Substring(0, yourInput.Length - 1);
                            lastKeyPressTime = (float)(gameTime.TotalGameTime.TotalSeconds);
                        }
                    }
                    if (key == Keys.Space)
                    {
                        yourInput += " ";
                        lastKeyPressTime = (float)(gameTime.TotalGameTime.TotalSeconds);
                    }
                }
            }


            // Check if the word is correct every 100 ms
            float secondsBetweenWordChecks = 0.1f;
            if (now > lastWordCheckTime + secondsBetweenWordChecks)
            {
                foreach (string word in level.photos[currentPhotoIndex].objectives[currentObjectiveIndex].words)
                {
                    if (yourInput.ToLower().Equals(word.ToLower()))
                    {
                        correctAnswers.Add(word);
                        // The word is correct. Advance to the next word (objective).
                        updateToNextWord();
                    }
                    
                }
                lastWordCheckTime = now;
            }

            if (newLevelLoaded)
            {
                newLevelLoaded = false;
                elapsedTimeForLevel = 0.0f;

                // Set the time for a level to 5 seconds * the # of objectives
                timeForLevel = 0.0f;
                foreach (Photo photo in level.photos)
                {
                    foreach (Objective objective in photo.objectives)
                    {
                        timeForLevel += 5.0f;
                    }
                }
            }
        }

        private void updateToNextWord()
        {
            currentObjectiveIndex++;
            // Update your score
            score += (timeLeftForWord / timeForWord) * maxPointsPerWord;
            // Reset the prompt
            yourInput = "";
            // Set new word timer
            elapsedTimeForWord = 0.0f;
            // See if we need to change the photo
            if (currentObjectiveIndex >= level.photos[currentPhotoIndex].objectives.Count)
            {
                currentObjectiveIndex = 0;
                currentPhotoIndex++;
                // See if there are no more photos, go to see high scores
                if (currentPhotoIndex >= level.photos.Count)
                {
                    finishedLevel();
                }
                else
                {
                    // Show and load the new photo
                    photo = this.Content.Load<Texture2D>(getCurrentPhotoUri());
                    setAndScalePhoto(photo);
                }
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            // Draw Game Name
            Vector2 gameNameLoc = new Vector2(0.01f * screenRect.Width, 0.03f * screenRect.Height);
            spriteBatch.DrawString(title, gameName, gameNameLoc, Color.White);

            if (selectingLevel)
            {
                drawLevelSelection(gameTime);
            }
            else if (enteringName)
            {
                drawEnterName(gameTime);
            }
            else if (seeingHighScore)
            {
                drawSeeingHighScore(gameTime);
            }
            else
            {
                drawGame(gameTime);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }


        private void drawGame(GameTime gameTime)
        {

            // Draw the photo
            spriteBatch.Draw(photo, photoRect, Color.White);

            // Draw the rectangle that shows how much time is left in the level
            Rectangle outerTimeRect = new Rectangle(photoRect.X, (int)(0.04f * screenRect.Height), photoRect.Width, (int)(0.03f * screenRect.Height));
            spriteBatch.FillRectangle(outerTimeRect, Color.White);

            float proportionTimeLeft = timeLeftForLevel / timeForLevel;
            Rectangle innerTimeRect = new Rectangle(photoRect.X, (int)(0.04f * screenRect.Height), (int)(photoRect.Width * proportionTimeLeft), (int)(0.03f * screenRect.Height));

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
            // Draw time left for level text
            spriteBatch.DrawString(sm, "Level Over In:", new Vector2(photoRect.X + photoRect.Width + .04f * screenRect.Width, screenRect.Height * .03f), Color.Black);
            spriteBatch.DrawString(sm, String.Format("{0:0.00}", timeLeftForLevel) + " seconds", new Vector2(photoRect.X + photoRect.Width + .04f * screenRect.Width, screenRect.Height * .1f), Color.Black);


            // Draw the time left for the word
            float proportionWordTimeLeft = timeLeftForWord / timeForWord;
            Color colorForWordTime;
            if (proportionWordTimeLeft > 0.5f)
                colorForWordTime = Color.Green;
            else if (proportionWordTimeLeft > 0.3f)
                colorForWordTime = Color.Yellow;
            else if (proportionWordTimeLeft < 0.3)
                colorForWordTime = Color.Red;
            else
                colorForWordTime = Color.Purple;
            // Draw the time left for the word
            spriteBatch.DrawString(title, ((int)(timeLeftForWord)).ToString(), new Vector2((screenRect.Width - .1f * screenRect.Width), screenRect.Height - (screenRect.Height * proportionWordTimeLeft)), colorForWordTime);


            // Draw the text entry
            textRect = new Rectangle(photoRect.X, photoRect.Y + photoRect.Height + (int)(0.07f * screenRect.Height), photoRect.Width, (int)(0.05f * screenRect.Height));
            spriteBatch.FillRectangle(textRect, Color.Black);
            spriteBatch.DrawString(title, yourInput, new Vector2(textRect.X, textRect.Y), Color.White);


            // Draw the circle that goes over the photo
            spriteBatch.DrawCircle(getCircle(level.photos[currentPhotoIndex].objectives[currentObjectiveIndex].x, level.photos[currentPhotoIndex].objectives[currentObjectiveIndex].y), 100, Color.White, 10);

            // Draw the score
            Vector2 scoreLoc = new Vector2(0.01f * screenRect.Width, 0.2f * screenRect.Height);
            spriteBatch.DrawString(sm, "Score: " + ((int)(score)).ToString(), scoreLoc, Color.White);

            //Draw Correct Answers
            int correctAnswerLocX = (int)(.01f * screenRect.Width);
            int correctAnswerLocY = (int)(.25f * screenRect.Height);
            foreach(string correctAnswer in correctAnswers)
            {
                correctAnswerLocY += (int)(.06f * screenRect.Height);
                spriteBatch.DrawString(title, correctAnswer, new Vector2(correctAnswerLocX, correctAnswerLocY), Color.Green);
            }
        }


        private void drawLevelSelection(GameTime gameTime)
        {
            // Draw Instructions
            Vector2 instructionsLoc = new Vector2(0.3f * screenRect.Width, 0.1f * screenRect.Height);
            spriteBatch.DrawString(title, "Select your level", instructionsLoc, Color.White);

            Vector2 levelLoc = new Vector2(0.4f * screenRect.Width, 0.3f * screenRect.Height);
            spriteBatch.DrawString(title, "City", levelLoc, Color.White);

            levelLoc = new Vector2(0.4f * screenRect.Width, 0.4f * screenRect.Height);
            spriteBatch.DrawString(title, "Nature", levelLoc, Color.White);

            levelLoc = new Vector2(0.4f * screenRect.Width, 0.5f * screenRect.Height);
            spriteBatch.DrawString(title, "Market", levelLoc, Color.White);

            levelLoc = new Vector2(0.4f * screenRect.Width, 0.6f * screenRect.Height);
            spriteBatch.DrawString(title, "Verbs", levelLoc, Color.White);

            float levelBoxY = (0.27f + (0.1f * levelIndex)) * screenRect.Height;
            Rectangle selectedLevelBox = new Rectangle((int)(0.34f * screenRect.Width), (int)levelBoxY, (int)(screenRect.Width * 0.35f), (int)(0.15f * screenRect.Height));
            spriteBatch.DrawRectangle(selectedLevelBox, Color.White, 5);
        }


        private void drawEnterName(GameTime gameTime)
        {
            // Draw Instructions
            Vector2 instructionsLoc = new Vector2(0.3f * screenRect.Width, 0.1f * screenRect.Height);
            spriteBatch.DrawString(title, "Enter your name", instructionsLoc, Color.White);

            // Draw the text entry
            textRect = new Rectangle(photoRect.X, photoRect.Y + photoRect.Height + (int)(0.07f * screenRect.Height), photoRect.Width, (int)(0.05f * screenRect.Height));
            spriteBatch.FillRectangle(textRect, Color.Black);
            spriteBatch.DrawString(title, yourName, new Vector2(textRect.X, textRect.Y), Color.White);
        }


        private void drawSeeingHighScore(GameTime gameTime)
        {
            Vector2 yourScoreLoc = new Vector2(0.3f * screenRect.Width, 0.1f * screenRect.Height);
            spriteBatch.DrawString(title, "Your Score: " + ((int)score).ToString(), yourScoreLoc, Color.White);



            //highScores[level.name][yourName];
            int scoreListX = (int)(0.3f * screenRect.Width);
            int scoreListY = (int)(0.17f * screenRect.Height);
            List<KeyValuePair<string, float>> sortedScoresList = new List<KeyValuePair<string, float> >();
            foreach (KeyValuePair<string, float> scorePair in highScores[level.name])
            {
                sortedScoresList.Insert(0, scorePair);
            }
            foreach (KeyValuePair<string, float> scorePair in sortedScoresList)
            {
                scoreListY += (int)(0.06f * screenRect.Height);
                spriteBatch.DrawString(title, scorePair.Key + ": " + (int)scorePair.Value, new Vector2(scoreListX, scoreListY), Color.Green);
            }

            Vector2 exitInstructionLoc = new Vector2(0.3f * screenRect.Width, 0.8f * screenRect.Height);
            spriteBatch.DrawString(sm, "Press space to exit", exitInstructionLoc, Color.White);
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

            int newX = (int)(photoScale * origX) + photoRect.X;
            int newY = (int)(photoScale * origY) + photoRect.Y;
            
            return new CircleF(new Point2(newX, newY), radius);
        }


        private void loadHighScores()
        {
            // Open the file to read
            using (StreamReader r = new StreamReader("Levels/highScores.json"))
            {
                string json = r.ReadToEnd();
                JObject fileJson = (JObject)JsonConvert.DeserializeObject(json);

                highScores = new Dictionary<string, SortedDictionary<string, float>>();

                JArray highScoreLevelList = (JArray)fileJson.GetValue("highScores");

                // Iterate over the levels
                using (var highScoresLevelEnum = highScoreLevelList.GetEnumerator())
                {
                    while (highScoresLevelEnum.MoveNext())
                    {
                        SortedDictionary<string, float> highScoresForLevel = new SortedDictionary<string, float>();
                        
                        JObject highScoresLevel = (JObject)highScoresLevelEnum.Current;

                        string levelName = (string)highScoresLevel.GetValue("levelName");

                        object thisLevelsScoresJson2 = highScoresLevel.GetValue("highScores");

                        JObject thisLevelsScoresJson = (JObject)highScoresLevel.GetValue("highScores");

                        // Iterate over the high scores in this level
                        using (var scoresEnum = thisLevelsScoresJson.GetEnumerator())
                        {
                            while (scoresEnum.MoveNext())
                            {
                                KeyValuePair<string, JToken> nameAndScore = (KeyValuePair<string, JToken>) scoresEnum.Current;
                                string name = nameAndScore.Key;
                                float score = (float) nameAndScore.Value;
                                highScoresForLevel.Add(name, score);
                            }
                        }

                        highScores.Add(levelName, highScoresForLevel);
                    }
                }
            }
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

            currentPhotoIndex = 0;
            currentObjectiveIndex = 0;
            newLevelLoaded = true;
            correctAnswers = new List<string>();
            
        }


        /// <summary>
        /// Load the photo to show and scale it to ~70% of the screen width
        /// </summary>
        /// <param name="photo"></param>
        private void setAndScalePhoto(Texture2D photo)
        {
            float fotoToScreenWidthPercentage = 0.7f;
            float fotoToScreenHeightPercentage = 0.75f;
            float fotoTargetWidth = fotoToScreenWidthPercentage * (float)screenRect.Width;
            float fotoTargetHeight = fotoToScreenHeightPercentage * (float)screenRect.Height;

            photoScale = Math.Min(fotoTargetWidth / (float)this.photo.Width,
                fotoTargetHeight / (float)this.photo.Height);

            int newWidth = (int)(photoScale * (float)this.photo.Width);
            int newHeight = (int)(photoScale * (float)this.photo.Height);

            int fotoXPos = (int)((screenRect.Width - newWidth) / 2.0f); // Center the X
            int fotoYPos = (int)(0.1f * screenRect.Height); // Start the img 10% from the top of the screen
            photoRect = new Rectangle(fotoXPos, fotoYPos, newWidth, newHeight);        }


        private string getCurrentPhotoUri()
        {
            return "Levels/" + level.name + "/Photos/" + level.photos[currentPhotoIndex].name;
        }

    }
}
