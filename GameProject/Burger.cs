﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace GameProject
{// / 5 * gameTime.ElapsedGameTime.Milliseconds;
    /// <summary>
    /// A burger
    /// </summary>
    public class Burger
    {
        #region Fields
        // graphic and drawing info
        Texture2D sprite;
        Rectangle drawRectangle;
        // burger stats
        int health = 100;
        // shooting support
        bool canShoot = true;
        int elapsedCooldownMilliseconds = 0;
        // sound effect
        SoundEffect shootSound;
        #endregion
        #region Constructors
        /// <summary>
        ///  Constructs a burger
        /// </summary>
        /// <param name="contentManager">the content manager for loading content</param>
        /// <param name="spriteName">the sprite name</param>
        /// <param name="x">the x location of the center of the burger</param>
        /// <param name="y">the y location of the center of the burger</param>
        /// <param name="shootSound">the sound the burger plays when shooting</param>
        public Burger(ContentManager contentManager, string spriteName, int x, int y,
            SoundEffect shootSound)
        {
            LoadContent(contentManager, spriteName, x, y);
            this.shootSound = shootSound;
        }
        #endregion
        #region Properties

        public int HP
        {
            get { return health; }
            set { health += -value;  if (health < 0) health = 0; }
        }

        /// <summary>
        /// Gets the collision rectangle for the burger
        /// </summary>
        public Rectangle CollisionRectangle
        {
            get { return drawRectangle; }
        }
        #endregion
        #region Public methods
        /// <summary>
        /// Updates the burger's location based on mouse. Also fires 
        /// french fries as appropriate
        /// </summary>
        /// <param name="gameTime">game time</param>
        /// <param name="mouse">the current state of the mouse</param>
        //public void Update(GameTime gameTime, MouseState mouse)
        public void Update(GameTime gameTime, MouseState mouse, KeyboardState klawiatura)

        {
            //burger should only respond to input if it still has health
            if (health <= 0)
            {
                return; // its passess the rest of the methond this way
            }


            bool up = klawiatura.IsKeyDown(Keys.W);
            bool down = klawiatura.IsKeyDown(Keys.S);
            bool right = klawiatura.IsKeyDown(Keys.D);
            bool left = klawiatura.IsKeyDown(Keys.A);
            int speed = GameConstants.BurgerMovementAmount;
            
            
            if (down)
            {
                if (right || left)
                {
                    drawRectangle.Y += (int)(Math.Sqrt((double)speed) + speed/2);
                }
                else
                {
                drawRectangle.Y += speed;// 
                }
            }

            if (up)
            {
                if (right || left)
                {
                    drawRectangle.Y -= (int)(Math.Sqrt((double)speed) + speed / 2);
                }
                else
                {
                drawRectangle.Y -= speed;// 

                }
            }

            if (right)
            {
                if (up || down)
                {
                    drawRectangle.X += (int)(Math.Sqrt((double)speed) + speed / 2);
                }
                else
                { 
                drawRectangle.X += speed;// 
                }
            }

            if (left)
            {
                if (up || down)
                {
                    drawRectangle.X -= (int)(Math.Sqrt((double)speed) + speed / 2);
                }
                else
                {
                    drawRectangle.X -= speed;// 
                }
            }


            if (klawiatura.IsKeyDown(Keys.S) && klawiatura.IsKeyDown(Keys.D))
            {


            }
            // move burger using mouse
            // clamp burger in window
            drawRectangle.X = MathHelper.Clamp(drawRectangle.X, 0, GameConstants.WindowWidth - sprite.Width);
            drawRectangle.Y = MathHelper.Clamp(drawRectangle.Y, 0, GameConstants.WindowHeight - sprite.Height);
            // update shooting allowed
            // timer concept (for animations) introduced in Chapter 7
            // shoot if appropriate
            if (mouse.LeftButton == ButtonState.Pressed && canShoot)
            {
                float vel = GameConstants.FrenchFriesProjectileSpeed;
                Projectile pocisk = new Projectile(ProjectileType.FrenchFries, Game1.GetProjectileSprite(ProjectileType.FrenchFries), drawRectangle.Center.X, drawRectangle.Center.Y + GameConstants.FrenchFriesProjectileOffset, -vel);
                Game1.AddProjectile(pocisk);
                shootSound.Play();
                canShoot = false;
                elapsedCooldownMilliseconds = 0;
            }
            else if (mouse.LeftButton == ButtonState.Released && !canShoot)
            {
                canShoot = true;
            }
            else
            {
                elapsedCooldownMilliseconds += gameTime.ElapsedGameTime.Milliseconds;
                if (elapsedCooldownMilliseconds > GameConstants.BurgerTotalCooldownMilliseconds)
                {
                    canShoot = true;
                }
            }

        }
        /// <summary>
        /// Draws the burger
        /// </summary>
        /// <param name="spriteBatch">the sprite batch to use</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, drawRectangle, Color.White);
        }
        #endregion
        #region Private methods
        /// <summary>
        /// Loads the content for the burger
        /// </summary>
        /// <param name="contentManager">the content manager to use</param>
        /// <param name="spriteName">the name of the sprite for the burger</param>
        /// <param name="x">the x location of the center of the burger</param>
        /// <param name="y">the y location of the center of the burger</param>
        private void LoadContent(ContentManager contentManager, string spriteName,
            int x, int y)
        {
            // load content and set remainder of draw rectangle
            sprite = contentManager.Load<Texture2D>(spriteName);
            drawRectangle = new Rectangle(x - sprite.Width / 2,
                y - sprite.Height / 2, sprite.Width,
                sprite.Height);
        }

        #endregion
    }
}