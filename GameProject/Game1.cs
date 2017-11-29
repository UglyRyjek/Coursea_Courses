using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace GameProject
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // game objects. Using inheritance would make this
        // easier, but inheritance isn't a GDD 1200 topic
        Burger burger;
        List<TeddyBear> bears = new List<TeddyBear>();
        static List<Projectile> projectiles = new List<Projectile>();
        List<Explosion> explosions = new List<Explosion>();

        // projectile and explosion sprites. Saved so they don't have to
        // be loaded every time projectiles or explosions are created
        static Texture2D frenchFriesSprite;
        static Texture2D teddyBearProjectileSprite;
        static Texture2D explosionSpriteStrip;

        

        static Texture2D teddySprite;
        // scoring support
        int score = 0;
        string scoreString = GameConstants.ScorePrefix + 0;

        // health support
        string healthString = GameConstants.HealthPrefix; //+ GameConstants.BurgerInitialHealth;
        bool burgerDead = false;

        // text display support
        SpriteFont font;

        // sound effects
        SoundEffect burgerDamage;
        SoundEffect burgerDeath;
        SoundEffect burgerShot;
        SoundEffect explosion;
        SoundEffect teddyBounce;
        SoundEffect teddyShot;

        static int winHeight = GameConstants.WindowHeight;
        static int winWidth = GameConstants.WindowWidth;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // set resolution
            graphics.PreferredBackBufferWidth = GameConstants.WindowWidth;
            graphics.PreferredBackBufferHeight = GameConstants.WindowHeight;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            RandomNumberGenerator.Initialize();

            

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // load audio content
            burgerDamage = Content.Load<SoundEffect>("bin/audio/BurgerDamage");
            burgerDeath = Content.Load<SoundEffect>("bin/audio/BurgerDeath");
            burgerShot = Content.Load<SoundEffect>("bin/audio/BurgerShot");
            explosion = Content.Load<SoundEffect>("bin/audio/Explosion");
            teddyBounce = Content.Load<SoundEffect>("bin/audio/TeddyBounce");
            teddyShot = Content.Load<SoundEffect>("bin/audio/TeddyShot");
            
            // load sprite font
            font = Content.Load<SpriteFont>("bin/fonts/Arial20");
            
            // load projectile and explosion sprites
            frenchFriesSprite = Content.Load<Texture2D>("bin/graphics/frenchfries");
            teddyBearProjectileSprite = Content.Load<Texture2D>("bin/graphics/teddybearprojectile"); 
            explosionSpriteStrip = Content.Load<Texture2D>("bin/graphics/explosion");

            teddySprite = Content.Load<Texture2D>("bin/graphics/burger");


            // add initial game objects
            for (int i = 0; i < GameConstants.MaxBears; i++)
            {
                SpawnBear(teddyBounce, teddyShot);
            }


            burger = new Burger(Content, "bin/graphics/burger", winWidth / 2, (winHeight / 4) * 3, burgerShot);

            // set initial health and score strings
            healthString = GameConstants.HealthPrefix + burger.HP ;
            scoreString = GameConstants.ScorePrefix + score;
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

            bool burgerGotHitThisFrame = false;


            CheckBurgerKill();
            if (burgerDead)
            {
                foreach (Projectile projectile in projectiles)
                {
                    projectile.Update(gameTime);
                }
                foreach (Explosion explosion in explosions)
                {
                    explosion.Update(gameTime);
                }
                return;
            }
                


            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // get current mouse state and update burger

            MouseState mysz = Mouse.GetState();
            KeyboardState klawiatura = Keyboard.GetState();
            burger.Update(gameTime, mysz , klawiatura);

            // update other game objects
            foreach (TeddyBear bear in bears)
            {
                bear.Update(gameTime);
            }
            foreach (Projectile projectile in projectiles)
            {
                projectile.Update(gameTime);
            }
            foreach (Explosion explosion in explosions)
            {
                explosion.Update(gameTime);
            }

            // check and resolve collisions between teddy bears
            for (int i = 0; i < bears.Count; i++)
            {
                for (int j = i + 1; j < bears.Count; j++)
                {
                    if( (bears[i].Active && bears[j].Active) && i != j)
                    {
                        TeddyBear b1 = bears[i];
                        TeddyBear b2 = bears[j];
                        //if (b1.CollisionRectangle.Intersects(b2.CollisionRectangle)) ;
                        CollisionResolutionInfo colInfo = CollisionUtils.CheckCollision(gameTime.ElapsedGameTime.Milliseconds, GameConstants.WindowWidth, GameConstants.WindowHeight, b1.Velocity, b1.DrawRectangle, b2.Velocity, b2.DrawRectangle);
                        if (colInfo != null)
                        {
                            if(colInfo.FirstOutOfBounds)
                            {
                                Explosion exp = new Explosion(explosionSpriteStrip, b1.Location.X, b1.Location.Y, explosion);
                                explosions.Add(exp);
                                b1.Active = false;
                            }
                            else
                            {
                                b2.Velocity = colInfo.FirstVelocity;
                                b2.DrawRectangle = colInfo.FirstDrawRectangle;
                            }

                            if(colInfo.SecondOutOfBounds)
                            {
                                Explosion exp = new Explosion(explosionSpriteStrip, b2.Location.X, b2.Location.Y, explosion);
                                explosions.Add(exp);
                                b2.Active = false;

                            }
                            else
                            {
                                b1.Velocity = colInfo.SecondVelocity;
                                b1.DrawRectangle = colInfo.SecondDrawRectangle;

                            }
                            teddyBounce.Play();
                        }
                    }
                }
            }


            // check and resolve collisions between burger and teddy bears
            foreach (TeddyBear bear in bears)
            {
                    if (bear.Active)
                    {
                        if (bear.CollisionRectangle.Intersects(burger.CollisionRectangle))
                        {
                            bear.Active = false;
                            burger.HP = GameConstants.BearDamage;
                            healthString = "" + GameConstants.HealthPrefix + burger.HP;
                            Explosion exp = new Explosion(explosionSpriteStrip, bear.Location.X, bear.Location.Y, explosion);
                            explosions.Add(exp);
                            burgerGotHitThisFrame = true;
                        //TODO
                        }
                    }
            }

            // check and resolve collisions between burger and projectiles
            foreach (Projectile proj in projectiles)
            {
                if (proj.Type == ProjectileType.TeddyBear)
                {
                    if (proj.Active)
                    {
                        if (proj.CollisionRectangle.Intersects(burger.CollisionRectangle))
                        {
                            proj.Active = false;
                            burger.HP = GameConstants.TeddyBearProjectileDamage;
                            healthString = "" + GameConstants.HealthPrefix + burger.HP;
                            burgerGotHitThisFrame = true;
                            //TODO
                        }
                    }
                }
            }
            // check and resolve collisions between teddy bears and projectiles
            foreach (TeddyBear bear in bears)
            {
                foreach (Projectile proj in projectiles)
                {
                    if (proj.Type == ProjectileType.FrenchFries)
                    {
                        if (proj.Active && bear.Active)
                        {
                            if (bear.CollisionRectangle.Intersects(proj.CollisionRectangle))
                            {
                                proj.Active = false;
                                bear.Active = false;
                                Explosion exp = new Explosion(explosionSpriteStrip, bear.Location.X, bear.Location.Y, explosion);
                                explosions.Add(exp);
                                score += GameConstants.BearPoints;
                                scoreString = GameConstants.ScorePrefix + score;
                            }
                        }
                    }
                }
            }

            // clean out inactive teddy bears and add new ones as necessary
            for (int i = 0; i < bears.Count; i++)
            {
                if(bears[i].Active == false)
                {
                    bears.RemoveAt(i);
                }
            }

            while(bears.Count < GameConstants.MaxBears)
            {
                SpawnBear(teddyBounce, teddyShot);
            }


            // clean out inactive projectiles
            for (int i = 0; i < projectiles.Count; i++)
            {
                if (projectiles[i].Active == false)
                {
                    projectiles.RemoveAt(i);
                }
            }
            // clean out finished explosions
            for (int i = 0; i < explosions.Count; i++)
            {
                if(explosions[i].Finished)
                {
                    explosions.RemoveAt(i);
                }
            }


            //sounds
            if(burgerGotHitThisFrame)
            {
                burgerDamage.Play();
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

            spriteBatch.Begin();

            // draw game objects
            burger.Draw(spriteBatch);
            foreach (TeddyBear bear in bears)
            {
                bear.Draw(spriteBatch);
            }
            foreach (Projectile projectile in projectiles)
            {
                projectile.Draw(spriteBatch);
            }
            foreach (Explosion explosion in explosions)
            {
                explosion.Draw(spriteBatch);
            }

            // draw score and health
            spriteBatch.DrawString(font, healthString, GameConstants.HealthLocation, Color.White);
            spriteBatch.DrawString(font, scoreString, GameConstants.ScoreLocation, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Public methods

        /// <summary>
        /// Gets the projectile sprite for the given projectile type
        /// </summary>
        /// <param name="type">the projectile type</param>
        /// <returns>the projectile sprite for the type</returns>
        public static Texture2D GetProjectileSprite(ProjectileType type)
        {
            if (type == ProjectileType.FrenchFries)
                return frenchFriesSprite;
            else
                return teddyBearProjectileSprite;
        }

        /// <summary>
        /// Adds the given projectile to the game
        /// </summary>
        /// <param name="projectile">the projectile to add</param>
        public static void AddProjectile(Projectile projectile)
        {
            projectiles.Add(projectile);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Spawns a new teddy bear at a random location
        /// </summary>
        private void SpawnBear(SoundEffect boundeMP3, SoundEffect shootMP3)
        {
            // generate random locationSpawnBorderSize



            int border = GameConstants.SpawnBorderSize;



            int width = GetRandomLocation(0, GameConstants.WindowWidth);
            int height = GetRandomLocation(0, GameConstants.WindowHeight);

            Vector2 loacation = new Vector2(width, height);
            loacation = Vector2.Clamp(loacation, (new Vector2(0 + border, 0 + border)), new Vector2(winWidth - border, winHeight - border));

            float speed =  RandomNumberGenerator.NextFloat(GameConstants.BearSpeedRange);
            if (speed < GameConstants.MinBearSpeed)
            {
                speed = GameConstants.MinBearSpeed;
            }
            float angle = RandomNumberGenerator.NextFloat((float)Math.PI);

            // generate random velocity
            //OWN_NOTE - OK, so I figure I dont really like math hence here is my own, random velocity 
            // which gives exactly the same result as the one which Mr T wanted to see
            // Life is brutal :)
            int xx = (int)RandomNumberGenerator.RandomVelocity(-2, 3);
            int yy = (int)RandomNumberGenerator.RandomVelocity(-2, 3);
            xx = (xx < 1 && xx > -1) ? 1 : xx;
            yy = (yy < 1 && yy > -1) ? 1 : yy;
            Vector2 velocity = new Vector2(xx  * speed, yy * speed);

            // create new bear


            // make sure we don't spawn into a collision

            List<Rectangle> otherRec = new List<Rectangle>();
            foreach (var bear in bears)
            {
                if (bear.Active)
                {
                    otherRec.Add(bear.CollisionRectangle);
                }
            }
            foreach (var proj in projectiles)
            {
                if (proj.Type == ProjectileType.FrenchFries && proj.Active)
                    otherRec.Add(proj.CollisionRectangle);
            }
            
            if(burger != null)
            {
                otherRec.Add(burger.CollisionRectangle);
            }

            bool Ready = false;


            while(!Ready)
            {
                width = GetRandomLocation(0, GameConstants.WindowWidth);
                height = GetRandomLocation(0, GameConstants.WindowHeight);
                loacation = new Vector2(width, height);
                loacation = Vector2.Clamp(loacation, (new Vector2(0 + border, 0 + border)), new Vector2(winWidth - border, winHeight - border));

                Rectangle rect = new Rectangle((int)loacation.X, (int)loacation.Y, teddySprite.Width, teddySprite.Height);
                Ready = CollisionUtils.IsCollisionFree(rect, otherRec);

            }

            TeddyBear newBear = new TeddyBear(Content, "bin/graphics/teddybear", (int)loacation.X, (int)loacation.Y, velocity, boundeMP3, shootMP3);

            // add new bear to list
            bears.Add(newBear);
        }

        /// <summary>
        /// Gets a random location using the given min and range
        /// </summary>
        /// <param name="min">the minimum</param>
        /// <param name="range">the range</param>
        /// <returns>the random location</returns>
        private int GetRandomLocation(int min, int range)
        {
            return min + RandomNumberGenerator.Next(range);
        }

        /// <summary>
        /// Gets a list of collision rectangles for all the objects in the game world
        /// </summary>
        /// <returns>the list of collision rectangles</returns>
        private List<Rectangle> GetCollisionRectangles()
        {
            List<Rectangle> collisionRectangles = new List<Rectangle>();
            collisionRectangles.Add(burger.CollisionRectangle);
            foreach (TeddyBear bear in bears)
            {
                collisionRectangles.Add(bear.CollisionRectangle);
            }
            foreach (Projectile projectile in projectiles)
            {
                collisionRectangles.Add(projectile.CollisionRectangle);
            }
            foreach (Explosion explosion in explosions)
            {
                collisionRectangles.Add(explosion.CollisionRectangle);
            }
            return collisionRectangles;
        }

        /// <summary>
        /// Checks to see if the burger has just been killed
        /// </summary>
        private void CheckBurgerKill()
        {
            if (burger.HP <= 0 && !burgerDead)
            {
                int xx = burger.CollisionRectangle.X + burger.CollisionRectangle.Width / 2;
                int yy = burger.CollisionRectangle.Y + burger.CollisionRectangle.Height / 2;
                int offset = 30;
                Explosion exp1 = new Explosion(explosionSpriteStrip, xx - offset, yy - offset, explosion);
                Explosion exp2 = new Explosion(explosionSpriteStrip, xx + offset, yy - offset, explosion);
                Explosion exp3 = new Explosion(explosionSpriteStrip, xx - offset, yy + offset, explosion);
                Explosion exp4 = new Explosion(explosionSpriteStrip, xx + offset, yy + offset, explosion);
                explosions.Add(exp1);
                explosions.Add(exp2);
                explosions.Add(exp3);
                explosions.Add(exp4);
                burgerDead = true;
                burgerDeath.Play();
                
            }


           
        }

        #endregion
    }
}
