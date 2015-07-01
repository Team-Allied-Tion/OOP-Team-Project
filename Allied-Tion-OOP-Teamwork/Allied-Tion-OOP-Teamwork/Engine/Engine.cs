using System;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using AlliedTionOOP.GUI;
using AlliedTionOOP.MapNamespace;
using AlliedTionOOP.Objects.Creatures;
using AlliedTionOOP.Objects.Items;
using AlliedTionOOP.Objects.PlayerTypes;
using AlliedTionOOP.Physics;
using AlliedTionOOP.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AlliedTionOOP.Engine
{
    public class Engine : Game
    {
        protected static Texture2D BugImage;
        protected static Texture2D ExceptionImage;
        protected static Texture2D ExamBossImage;

        protected static Texture2D PlayerNerdSkin;
        protected static Texture2D PlayerNormalSkin;
        protected static Texture2D PlayerPartySkin;

        protected static Texture2D ProcessorUpgradeImage;
        protected static Texture2D MemoryUpgradeImage;
        protected static Texture2D DiskUpgradeImage;
        protected static Texture2D NakovBookImage;
        protected static Texture2D ResharperImage;
        protected static Texture2D BeerImage;
        protected static Texture2D RedBullImage;

        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        //private SpriteFont spriteFont;
        //private string noItemMessage = "";
        //private SpriteFont noItemMessageFont;

        private Sound getItemSound;
        private Sound musicTheme;
        private Sound killEnemy;

        private Map map;
        private Player player;
        private Vector2 mapPosition;

        private Creature closestCreature;

        private bool isKeyDownBeer = false;
        private bool isKeyDownRedBull = false;
        private bool isKeyDownAttack = false;

        public Engine()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            //window settings
            this.graphics.PreferredBackBufferWidth = MainClass.WindowWidth;
            this.graphics.PreferredBackBufferHeight = MainClass.WindowHeight;

            this.Window.Title = MainClass.WindowTitle;

            this.Window.Position = new Point((MainClass.CurrentScreenWidth - MainClass.WindowWidth) / 2,
                (MainClass.CurrentScreenHeight - MainClass.WindowHeight) / 2);

            //this.graphics.ToggleFullScreen();
            //this.Window.IsBorderless = true;

            LoadImages();

            // TODO: Add your initialization logic here

            this.getItemSound = new Sound(MainClass.GotItem);
            this.musicTheme = new Sound(MainClass.Music);
            this.killEnemy = new Sound(MainClass.KillEnemy);

            this.map = new Map();

            this.musicTheme.Play(true);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            map.SetMapBackground(Content.Load<Texture2D>("MapElementsTextures/map"));
            MapFactory.LoadMapObjectsFromTextFile(map, MainClass.MapCoordinates, this.Content);

            mapPosition = new Vector2(0, 0);

            //this.spriteFont = Content.Load<SpriteFont>("SpriteFont");
            //this.noItemMessageFont = Content.Load<SpriteFont>("Fonts/NoItemFont");

            player = new NormalStudent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (player.IsAlive && map.MapCreatures.Any(cr => cr is ExamBoss))
            {
                //TODO: Player gets stuck with objects in some pixels?
                CheckForPlayerMovementInput();

                #region CheckForCollisionWithItem

                int hashcodeOfCollidedItem;
                bool hasCollisionWithItem = CollisionDetector.HasCollisionWithItem(player, map, mapPosition,
                    out hashcodeOfCollidedItem);

                if (hasCollisionWithItem)
                {
                    Item collidedItem = map.MapItems.Single(x => x.GetHashCode() == hashcodeOfCollidedItem);
                    player.AddItemToInventory(collidedItem);

                    this.map.RemoveMapItemByHashCode(hashcodeOfCollidedItem);

                    new Thread(() => getItemSound.Play()).Start();
                }

                #endregion

                closestCreature = DistanceCalculator.GetClosestCreature(map, player, mapPosition);

                CheckForPlayerAttack();

                CheckForItemShortcutPressed();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                base.Exit();
            }

            base.Update(gameTime);
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);
            spriteBatch.Begin();

            map.Draw(spriteBatch, mapPosition, Content); // draw map with all its elements

            Target.DrawTarget(closestCreature, spriteBatch, Content, mapPosition);

            spriteBatch.Draw(player.Image, new Vector2(player.TopLeftX, player.TopLeftY)); // draw player

            StatBar.DrawEnergyBar(player, 10, spriteBatch, Content, Vector2.Zero);
            StatBar.DrawFocusBar(player, 13, spriteBatch, Content, Vector2.Zero);
            StatBar.DrawExperienceBar(player, 16, spriteBatch, Content, Vector2.Zero);

            InventoryBar.DrawInventory(player, spriteBatch, Content);

            if (!player.IsAlive || !map.MapCreatures.Any(cr => cr is ExamBoss))
            {
                spriteBatch.Draw(Content.Load<Texture2D>("MapElementsTextures/end"), Vector2.Zero);
            }

            ////draw some text
            //spriteBatch.DrawString(spriteFont, new StringBuilder("Player focus: " + player.CurrentFocus), new Vector2(100, 20), Color.WhiteSmoke);
            //spriteBatch.DrawString(spriteFont, new StringBuilder("Player is alive: " + player.IsAlive), new Vector2(100, 35), Color.WhiteSmoke);
            //spriteBatch.DrawString(spriteFont, new StringBuilder("Intersects: " + player.TotalFocus), new Vector2(100, 20), Color.WhiteSmoke);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void LoadImages()
        {
            BugImage = Content.Load<Texture2D>("CharacterTextures/bug");
            ExceptionImage = Content.Load<Texture2D>("CharacterTextures/exception");
            ExamBossImage = Content.Load<Texture2D>("CharacterTextures/exam");

            PlayerNerdSkin = Content.Load<Texture2D>("CharacterTextures/wizzard");
            PlayerNormalSkin = Content.Load<Texture2D>("CharacterTextures/wizzard");
            PlayerPartySkin = Content.Load<Texture2D>("CharacterTextures/wizzard");

            ProcessorUpgradeImage = Content.Load<Texture2D>("ItemsTextures/cpu-x35");
            MemoryUpgradeImage = Content.Load<Texture2D>("ItemsTextures/ram");
            DiskUpgradeImage = Content.Load<Texture2D>("ItemsTextures/hdd");
            NakovBookImage = Content.Load<Texture2D>("ItemsTextures/book");
            ResharperImage = Content.Load<Texture2D>("ItemsTextures/RSharper");
            BeerImage = Content.Load<Texture2D>("ItemsTextures/beerx32");
            RedBullImage = Content.Load<Texture2D>("ItemsTextures/redbull");
        }

        public void CheckForPlayerMovementInput()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.D))
            {
                if (player.TopLeftX < MainClass.WindowWidth / 2
                    || mapPosition.X + map.Background.Width < MainClass.WindowWidth)
                {
                    if (player.TopLeftX < MainClass.WindowWidth - player.Image.Width
                      && !CollisionDetector.HasCollisionWithObject(player, (player.TopLeftX + player.Speed.X), player.TopLeftY, map, mapPosition))
                    {
                        player.TopLeftX += player.Speed.X;
                    }
                }
                else
                {
                    mapPosition.X -= player.Speed.X;
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.A))
            {
                if (player.TopLeftX >= MainClass.WindowWidth / 2
                    || mapPosition.X >= map.Background.Bounds.Left)
                {
                    if (player.TopLeftX > 0
                        && !CollisionDetector.HasCollisionWithObject(player, (int)(player.TopLeftX - player.Speed.X), (int)player.TopLeftY, map, mapPosition))
                    {
                        player.TopLeftX -= player.Speed.X;
                    }
                }
                else
                {
                    mapPosition.X += player.Speed.X;
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down) || Keyboard.GetState().IsKeyDown(Keys.S))
            {
                if (player.TopLeftY < MainClass.WindowHeight / 2
                    || mapPosition.Y + map.Background.Height < MainClass.WindowHeight)
                {
                    if (player.TopLeftY < MainClass.WindowHeight - player.Image.Height
                        && !CollisionDetector.HasCollisionWithObject(player, (player.TopLeftX), (player.TopLeftY + player.Speed.Y), map, mapPosition))
                    {
                        player.TopLeftY += player.Speed.Y;
                    }
                }
                else
                {
                    mapPosition.Y -= player.Speed.Y;
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up) || Keyboard.GetState().IsKeyDown(Keys.W))
            {
                if (player.TopLeftY >= MainClass.WindowHeight / 2
                    || mapPosition.Y >= map.Background.Bounds.Top)
                {
                    if (player.TopLeftY > 0
                        && !CollisionDetector.HasCollisionWithObject(player, (int)(player.TopLeftX), (int)(player.TopLeftY - player.Speed.Y), map, mapPosition))
                    {
                        player.TopLeftY -= player.Speed.Y;
                    }
                }
                else
                {
                    mapPosition.Y += player.Speed.Y;
                }
            }
        }

        public void CheckForItemShortcutPressed()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Z))
            {
                isKeyDownBeer = true;
            }

            if (Keyboard.GetState().IsKeyUp(Keys.Z) && isKeyDownBeer)
            {
                Beer beerToUse = player.Inventory.FirstOrDefault(b => b is Beer) as Beer;

                if (beerToUse != null)
                {
                    player.GetFocus(beerToUse);
                }

                isKeyDownBeer = false;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.X))
            {
                isKeyDownRedBull = true;
            }

            if (Keyboard.GetState().IsKeyUp(Keys.X) && isKeyDownRedBull)
            {
                RedBull redbullToUse = player.Inventory.FirstOrDefault(b => b is RedBull) as RedBull;

                if (redbullToUse != null)
                {
                    player.GetEnergy(redbullToUse);
                }

                isKeyDownRedBull = false;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.C))
            {
                MemoryUpgrade memoryToUse = player.Inventory.FirstOrDefault(m => m is MemoryUpgrade) as MemoryUpgrade;

                if (memoryToUse != null)
                {
                    player.MemoryUpgrade(memoryToUse);
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.V))
            {
                DiskUpgrade diskToUse = player.Inventory.FirstOrDefault(d => d is DiskUpgrade) as DiskUpgrade;

                if (diskToUse != null)
                {
                    player.DiskUpgrade(diskToUse);
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.B))
            {
                ProcessorUpgrade processorToUse = player.Inventory.FirstOrDefault(p => p is ProcessorUpgrade) as ProcessorUpgrade;

                if (processorToUse != null)
                {
                    player.ProcessorUpgrade(processorToUse);
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.N))
            {
                NakovBook bookToUse = player.Inventory.FirstOrDefault(b => b is NakovBook) as NakovBook;

                if (bookToUse != null)
                {
                    player.NakovBook(bookToUse);
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.M))
            {
                Resharper resharperToUse = player.Inventory.FirstOrDefault(r => r is Resharper) as Resharper;
                if (resharperToUse != null)
                {
                    player.Resharper(resharperToUse);
                }
            }
        }

        private void CheckForPlayerAttack()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                isKeyDownAttack = true;
            }

            if (Keyboard.GetState().IsKeyUp(Keys.R) && isKeyDownAttack)
            {
                if (closestCreature != null &&
                    DistanceCalculator.GetDistanceBetweenObjects(player, closestCreature, mapPosition) < 80)
                {
                    player.Attack(closestCreature);

                    CheckIfCreatureIsAlive(closestCreature);
                }

                isKeyDownAttack = false;
            }
        }

        private void CheckIfCreatureIsAlive(Creature closestCreature)
        {
            if (!closestCreature.IsAlive)
            {
                new Thread(() => killEnemy.Play()).Start();

                int hashcodeOfKilledCreature = closestCreature.GetHashCode();
                map.RemoveMapCreatureByHashCode(hashcodeOfKilledCreature);
            }
        }
    }
}


