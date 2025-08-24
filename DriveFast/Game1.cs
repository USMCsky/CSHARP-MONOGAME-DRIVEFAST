using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace DriveFast
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        //The scrolling road
        Texture2D mRoad;           //800x600  ... so we change screen size to match

        float mVelocityY;          //the speed of the vertical scrolling road
        int[] mRoadY = new int[3]; //we are going to piece together the road 3 times

        //The car driving on the road and its position
        Texture2D mCar;
        Rectangle mCarPosition; //calculated in StartGame() below

        //We are going to use the keyboard to move the car from left to right and right to left (not up and down)
        KeyboardState mPreviousKeyboardState;
        int mMoveCarX;

        //We are going to place a hazard at random locations on the scrolling road
        Texture2D mHazard;
        Rectangle mHazardPosition; //calculated in StartGame() below

        Random mRandom = new Random();

        //We will keep track of the number of hazards we have passed sucessfully without hitting
        int mHazardsPassed;

        SpriteFont mFont;

        //Enums are strongly typed constants which makes the code more readable and less prone to errors. 
        //By default, the first enumerator has the value 0, and the value of each successive enumerator is increased by 1
        //The main advantage of Enum is make it easy to change values in the future, 
        //also you can reduces errors caused by transposing or mistyping numbers.
        enum State
        {
            Running,
            Crash
        }

        State mCurrentState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;


            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            //instead of declaring all the initializations here we call a separate method
            StartGame();     

            base.Initialize();
        }

        protected void StartGame()
        {
            mRoad = Content.Load<Texture2D>("Road");
            mCar = Content.Load<Texture2D>("Car");
            mHazard = Content.Load<Texture2D>("Hazard");

            //piece together the scrolling road ... placing it up and offscreen... 
            //we will later move it DOWNWARD on the screen ... it will kind of feel like you are driving forward   
            mRoadY[0] = 0;
            mRoadY[1] = mRoadY[0] + -mRoad.Height + 2;
            mRoadY[2] = mRoadY[1] + -mRoad.Height + 2;

            mVelocityY = 0.3F;

            //Remember default screen size is 800x480 ... changed to 800x600 
            //Actual dimensions of car is 501x737 ... to big for either screen ... so 
            //Couldn't use Vector2 here to position since we also needed to scale + we use it for our collision detection

            mCarPosition = new Rectangle(280, 440, (int)(mCar.Width * 0.2f), (int)(mCar.Height * 0.2f));

            //car doesn't actually move vertically...always near the bottom of the screen ... just screen scrolls
            //we just move it side to side 

            //This is how much OVER we move the car to avoid obstacles when we press the spacebar...we go -mMoveCarX if we are on the right side
            mMoveCarX = 160;

            //hazard not viewable initially ... up and offscreen
            //original dimensions of hazard is 256x256
            //Couldn't use Vector2 here to position since we also needed to scale + we use it for our collision detection

            mHazardPosition = new Rectangle(275, -mHazard.Height, (int)(mHazard.Width * 0.4f), (int)(mHazard.Height * 0.4f));

            //Hazard moves down screen ... position updated in UpdateHazard() method below

            mHazardsPassed = 0;

            mCurrentState = State.Running;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            //mRoad = Content.Load<Texture2D>("Road");
            //mCar = Content.Load<Texture2D>("Car");
            //mHazard = Content.Load<Texture2D>("Hazard");

            mFont = Content.Load<SpriteFont>("Font");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            KeyboardState aCurrentKeyboardState = Keyboard.GetState();

            if (aCurrentKeyboardState.IsKeyDown(Keys.Escape) == true)
            {
                this.Exit();
            }

            switch (mCurrentState)
            {
                case State.Running:
                    {
                        if ((aCurrentKeyboardState.IsKeyDown(Keys.Space) == true && mPreviousKeyboardState.IsKeyDown(Keys.Space) == false))

                        {
                            mCarPosition.X += mMoveCarX; //move car over
                            mMoveCarX *= -1;             //reverse direction for next time
                        }

                        ScrollRoad(gameTime);

                        if (mHazardPosition.Intersects(mCarPosition) == false)
                        {
                            UpdateHazard(gameTime);
                        }
                        else
                        {
                            mCurrentState = State.Crash;
                        }

                        break;
                    }
                case State.Crash:
                    {
                        if (aCurrentKeyboardState.IsKeyDown(Keys.Enter) == true)
                        {
                            StartGame();
                        }
                        break;
                    }
            }

            //make current keyboard state old keyboard state
            mPreviousKeyboardState = aCurrentKeyboardState;


            base.Update(gameTime);
        }

        private void ScrollRoad(GameTime theTime)
        {
            //Loop the road when it reaches the bottom
            for (int aIndex = 0; aIndex < mRoadY.Length; aIndex++)  //remember mRoadY is an array ... so Length is the size of the array 3
            {
                //if a road gets to the bottom of the screen
                if (mRoadY[aIndex] >= this.Window.ClientBounds.Height)
                {
                    //now found the smallest mRoadY[] at this moment ... position will be at aLastRoadIndex
                    int aLastRoadIndex = aIndex;
                    for (int aCounter = 0; aCounter < mRoadY.Length; aCounter++)
                    {
                        if (mRoadY[aCounter] < mRoadY[aLastRoadIndex])
                        {
                            aLastRoadIndex = aCounter;
                        }
                    }
                    //so first time through mRoadY[0] = mRoadY[2] - 600 + 2
                    mRoadY[aIndex] = mRoadY[aLastRoadIndex] - mRoad.Height + 2;
                    //move the piece of the road that has reached the bottom of the screen                                                                                
                    //backaround to the very top offscreen ... end of the line
                    //Recall
                    //mRoadY[0] = 0;
                    //mRoadY[1] = mRoadY[0] + -mRoad.Height + 2;
                    //mRoadY[2] = mRoadY[1] + -mRoad.Height + 2;
                }
            }

            //Move the road DOWN

            //Update these numbers
            //mRoadY[0] = 0;
            //mRoadY[1] = mRoadY[0] + -mRoad.Height + 2;
            //mRoadY[2] = mRoadY[1] + -mRoad.Height + 2;

            for (int aIndex = 0; aIndex < mRoadY.Length; aIndex++)
            {
                mRoadY[aIndex] += (int)(mVelocityY * theTime.ElapsedGameTime.TotalMilliseconds); //remember mVelocityY=0.3f
            }
            //Recall: ElapsedGameTime.TotalMilliseconds is ... the amount of elapsed game time since the last update.
        }

        private void UpdateHazard(GameTime theTime)
        {
            mHazardPosition.Y += (int)(mVelocityY * theTime.ElapsedGameTime.TotalMilliseconds); //move hazard down the screen

            if (mHazardPosition.Y > this.Window.ClientBounds.Height)  //if hazard reaches bottom of screen
            {
                mHazardPosition.X = 275;                              //reposition either on left or right side
                if (mRandom.Next(1, 3) == 2)
                {
                    mHazardPosition.X = 440;
                }

                mHazardPosition.Y = -mHazard.Height;   //offscreen for a second

                mHazardsPassed += 1;                   //update points ... reached bottom without collision
                mVelocityY += 0.1F;                    //update speed..faster and faster
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            _spriteBatch.Begin();

            //draw the road 3 times ... one will be onscreen the other two above the screen
            for (int aIndex = 0; aIndex < mRoadY.Length; aIndex++)
            {
                _spriteBatch.Draw(mRoad, new Vector2(0, mRoadY[aIndex]), Color.White);
            }

            _spriteBatch.Draw(mCar, mCarPosition, Color.White);
            _spriteBatch.Draw(mHazard, mHazardPosition, Color.White);

            _spriteBatch.DrawString(mFont, "Hazards: " + mHazardsPassed.ToString(), new Vector2(5, 25), Color.White,
                0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);

            if (mCurrentState == State.Crash)
            {
                _spriteBatch.DrawString(mFont, "Crash!", new Vector2(5, 200), Color.White, 0, new Vector2(0, 0), 1.0f,
                    SpriteEffects.None, 0);
                _spriteBatch.DrawString(mFont, "'Enter' to play again.", new Vector2(5, 230), Color.White, 0,
                    new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
                _spriteBatch.DrawString(mFont, "'Escape' to exit.", new Vector2(5, 260), Color.White, 0,
                    new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}