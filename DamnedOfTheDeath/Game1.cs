using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System;
using static DamnedOfTheDeath.Game1;
using Microsoft.VisualBasic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Diagnostics;
using SharpDX.Direct2D1.Effects;
using SharpDX.Direct3D9;
using DamnedOfTheDeath.UI;

namespace DamnedOfTheDeath
{
    public class Coin
    {
        private Texture2D _hitboxCoinTexture;
        private int _frameWidth = 20; // 180px / 9 columns
        private int _frameHeight = 20; // 20px / 1 rows
        private int _currentFrame;
        private int _totalFrames;
        private float _frameTime;
        private float _elapsedTime;

        public Vector2 Position { get; set; }
        public bool IsVisible { get; set; } = true; // Property to manage visibility

        public Coin(Texture2D texture, int frameWidth, int frameHeight, int totalFrames, float frameTime)
        {
            _hitboxCoinTexture = texture;
            _frameWidth = frameWidth;
            _frameHeight = frameHeight;
            _totalFrames = totalFrames;
            _frameTime = frameTime;
            _elapsedTime = 0f;
            _currentFrame = 0;
        }

        public void Update(GameTime gameTime)
        {
            if (!IsVisible) return;

            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_elapsedTime >= _frameTime)
            {
                _elapsedTime -= _frameTime;
                _currentFrame = (_currentFrame + 1) % _totalFrames;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;

            int row = 0; // For a single row spritesheet
            int column = _currentFrame;
            Rectangle sourceRectangle = new Rectangle(column * _frameWidth, row * _frameHeight, _frameWidth, _frameHeight);
            spriteBatch.Draw(_hitboxCoinTexture, Position, sourceRectangle, Color.White);
        }

        public Rectangle GetHitbox()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, _frameWidth, _frameHeight);
        }
    }

    public class Game1 : Game
    {
        #region Variables
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        #region PurplePortalVariables
        // PurplePortal variables
        private Texture2D _purplePortalSpriteSheet;
        private Rectangle[] _purplePortalFrames;
        private int _purplePortalFrameWidth = 64; // 512px / 8 columns
        private int _purplePortalFrameHeight = 64; // 192px / 3 rows
        private int _purplePortalCurrentFrame;
        private Vector2 _purplePortalPosition = new Vector2(1530, 370); // Adjust this to where you want the portal on the right
        private double _purplePortalTimeElapsed;
        private double _purplePortalTimeToUpdate = 0.1; // Adjust the speed of the animation
        // PurplePortal hitbox
        private Rectangle _purplePortalHitbox;
        private Texture2D _hitboxPurplePortalTexture;
        #endregion

        #region GreenPortalVariables
        // GreenPrtal Variables
        private Texture2D _greenPortalSpriteSheet;
        private Rectangle[] _greenPortalFrames;
        private int _greenPortalFrameWidth = 64; // 512px / 8 columns
        private int _greenPortalFrameHeight = 64; // 192px / 3 rows
        private int _greenPortalCurrentFrame;
        private Vector2 _greenPortalPosition = new Vector2(1530, 300); // Adjust this to where you want the portal on the right
        private double _greenPortalTimeElapsed;
        private double _greenPortalTimeToUpdate = 0.1; // Adjust the speed of the animation
        // PurplePortal hitbox
        private Rectangle _greenPortalHitbox;
        private Texture2D _hitboxGreenPortalTexture;
        #endregion

        #region CoinVariables
        List<Coin> _coinsLevel1;  // List to hold multiple coins
        List<Coin> _coinsLevel2;  // List to hold multiple coins
        int _score;
        const int MaxScore = 10;
        SpriteFont _font;  // Font to display the score
        #endregion

        #region UI
        ScoreOverlay _scoreOverlay;
        WinScreen _winScreen;
        private StartScreen _startScreen;

        bool _gameWon;
        #endregion

        #region PlayerVariables
        private Rectangle _playerHitbox;
        private Texture2D _spriteSheet;
        private Vector2 _position;
        private float _maxSpeed = 300f;
        private float _acceleration = 300f;
        private float _deceleration = 500f;
        private float _jumpSpeed = -500f; // Initial speed for jumping
        private float _gravity = 800f; // Gravity strength
        private bool _isJumping = false;
        private bool _isGrounded = true;
        private bool _isAttacking = false;
        private int _frame;
        private double _timeSinceLastFrame;
        private double _millisecondsPerFrame = 100;
        private int _frameWidth;
        private int _frameHeight;
        private Vector2 _velocity;
        #endregion

        #region TilesetVariables
        private Texture2D _tileset;
        private TileType[,] _currentLevelData;
        private TileType[,] _level1Data;
        private TileType[,] _level2Data;
        private int _tileWidth = 32;
        private int _tileHeight = 32;
        private bool isLevel2Active = false;
        private int _currentLevel;
        #endregion

        #region DictionaryCollisionMasks
        private Dictionary<CollisionType, List<Rectangle>> _collisionMasks = new Dictionary<CollisionType, List<Rectangle>>();
        #endregion

        #region DictionaryTileCollisions
        // Initialize tile collision properties
        private Dictionary<TileType, CollisionType> _tileCollisions = new Dictionary<TileType, CollisionType>()
            {
                { TileType.Empty, CollisionType.Empty },
                { TileType.StoneTopFloor_TopLeft, CollisionType.HalfFilledBottom },
                { TileType.StoneTopFloor_TopMid, CollisionType.HalfFilledBottom },
                { TileType.StoneTopFloor_TopRight, CollisionType.HalfFilledBottom },
                { TileType.GrassFloor_TopLeft, CollisionType.HalfFilledBottom },
                { TileType.GrassFloor_TopMid, CollisionType.HalfFilledBottom },
                { TileType.GrassFloor_TopRight, CollisionType.HalfFilledBottom },
                { TileType.Empty1, CollisionType.Empty },
                { TileType.Empty2, CollisionType.Empty },
                { TileType.Empty3, CollisionType.Empty },
                { TileType.Empty4, CollisionType.Empty },
                { TileType.Empty5, CollisionType.Empty },
                { TileType.StoneTopFloor_BottomLeft, CollisionType.Full },
                { TileType.StoneTopFloor_BottomMid, CollisionType.Full },
                { TileType.StoneTopFloor_Bottomright, CollisionType.Full },
                { TileType.GrassFloor_BottomLeft, CollisionType.Full },
                { TileType.GrassFloor_BottomMid, CollisionType.Full },
                { TileType.GrassFloor_BottomRight, CollisionType.Full },
                { TileType.GrassFloorPillar_Tree_Top, CollisionType.HalfFilledBottom },
                { TileType.GrassFLoorPillar_Pillar_Top, CollisionType.Full },
                { TileType.Empty6, CollisionType.Empty },
                { TileType.Empty7, CollisionType.Empty },
                { TileType.StoneLeftBottomFloor_TopLeft, CollisionType.HalfFilledBottom },
                { TileType.StoneLeftBottomFloor_TopRight, CollisionType.Full },
                { TileType.StoneTopFloor_Wall1, CollisionType.Full },
                { TileType.StoneRightBottomFloor_TopLeft, CollisionType.Full },
                { TileType.StoneRightBottomFloor_TopRight, CollisionType.HalfFilledBottom },
                { TileType.GrassFloorPillar_StoneRoad_TopLeft, CollisionType.HalfFilledBottom },
                { TileType.GrassFloorPillar_StoneRoad_TopRight, CollisionType.HalfFilledBottom },
                { TileType.GrassFloorPillar_Tree_Mid, CollisionType.Full },
                { TileType.GrassFloorPillar_Pillar_Mid, CollisionType.Full },
                { TileType.GrassFloorPillar_GrassRoad_Top, CollisionType.HalfFilledBottom },
                { TileType.Empty8, CollisionType.Empty },
                { TileType.StoneLeftBottomFloor_BottomLeft, CollisionType.Full },
                { TileType.StoneLeftBottomFloor_BottomRight, CollisionType.Full },
                { TileType.StoneTopFloor_Wall2, CollisionType.Full },
                { TileType.StoneRightBottomFloor_BottomLeft, CollisionType.Full },
                { TileType.StoneRightBottomFloor_BottomRight, CollisionType.Full },
                { TileType.GrassFloorPillar_StoneRoad_BottomLeft, CollisionType.Full },
                { TileType.GrassFloorPillar_StoneRoad_BottomRight, CollisionType.Full },
                { TileType.GrassFloorPillar_Tree_Bottom, CollisionType.Full },
                { TileType.GrassFloorPillar_Pillar_Bottom, CollisionType.Full },
                { TileType.GrassFloorPillar_GrassRoad_Bottom, CollisionType.Full },
                { TileType.Empty9, CollisionType.Empty },
                { TileType.StoneLeftTopCeiling_TopLeft, CollisionType.Full },
                { TileType.StoneLeftTopCeiling_TopRight, CollisionType.Full },
                { TileType.StoneWall_BetweenTopCeiling, CollisionType.Full },
                { TileType.StoneRightTopCeiling_TopLeft, CollisionType.Full },
                { TileType.StoneRightTopCeiling_TopRight, CollisionType.Full },
                { TileType.Empty10, CollisionType.Empty },
                { TileType.GrassRamp_LeftMid_Top, CollisionType.DiagonalRightBottom },
                { TileType.GrassRamp_Mid_Top, CollisionType.HalfFilledBottom },
                { TileType.GrassRamp_RightMid_Top, CollisionType.DiagonalLeftBottom },
                { TileType.Empty11, CollisionType.Empty },
                { TileType.Empty12, CollisionType.Empty },
                { TileType.StoneLeftTopCeiling_BottomLeft, CollisionType.Full },
                { TileType.StoneLeftTopCeiling_BottomRight, CollisionType.Full },
                { TileType.StoneBottomCeiling_TopMid, CollisionType.Full },
                { TileType.StoneRightTopCeiling_BottomLeft, CollisionType.Full },
                { TileType.StoneRightTopCeiling_BottomRight, CollisionType.Full },
                { TileType.GrassRamp_TopLeft, CollisionType.DiagonalRightBottom },
                { TileType.GrassRamp_LeftMid_Mid, CollisionType.Full },
                { TileType.GrassRamp_Mid_Mid, CollisionType.Full },
                { TileType.GrassRamp_RightMid_Mid, CollisionType.Full },
                { TileType.GrassRamp_TopRight, CollisionType.DiagonalLeftBottom },
                { TileType.Empty13, CollisionType.Empty },
                { TileType.Empty14, CollisionType.Empty },
                { TileType.StoneBottomCeiling_BottomLeft, CollisionType.Full },
                { TileType.StoneBottomCeiling_BottomMid, CollisionType.Full },
                { TileType.StoneBottomCeiling_BottomRight, CollisionType.Full },
                { TileType.Empty15, CollisionType.Empty },
                { TileType.GrassRamp_BottomLeft, CollisionType.Full },
                { TileType.GrassRamp_LeftMid_Bottom, CollisionType.Full },
                { TileType.GrassRamp_Mid_Bottom, CollisionType.Full },
                { TileType.GrassRamp_RightMid_Bottom, CollisionType.Full },
                { TileType.GrassRamp_BottomRight, CollisionType.Full },
                { TileType.Empty16, CollisionType.Empty },
                { TileType.Fence_TopLeft, CollisionType.Empty },
                { TileType.Fence_TopRight, CollisionType.Empty },
                { TileType.StoneBridge_TopLeft, CollisionType.HalfFilledBottom },
                { TileType.StoneBridge_MidTopLeft, CollisionType.HalfFilledBottom },
                { TileType.StoneBridge_MidTopRight, CollisionType.HalfFilledBottom },
                { TileType.StoneBridge_TopRight, CollisionType.HalfFilledBottom },
                { TileType.Empty17, CollisionType.Empty },
                { TileType.Bush_TopLeft, CollisionType.Empty },
                { TileType.Bush_TopMid, CollisionType.Empty },
                { TileType.Bush_TopRight, CollisionType.Empty },
                { TileType.Empty18, CollisionType.Empty },
                { TileType.Fence_MidLeft, CollisionType.Empty },
                { TileType.Fence_MidRight, CollisionType.Empty },
                { TileType.StoneBridge_BottomLeft, CollisionType.HalfFilledTop },
                { TileType.StoneBridge_MidBottomLeft, CollisionType.HalfFilledTop },
                { TileType.StoneBridge_MidBottomRight, CollisionType.Full },
                { TileType.StoneBridge_BottomRight, CollisionType.HalfFilledTop },
                { TileType.Bush_Left_Top, CollisionType.Empty },
                { TileType.Bush_TopLeftCorner, CollisionType.Empty },
                { TileType.Bush_Black_Top, CollisionType.Empty },
                { TileType.Bush_TopRightCorner, CollisionType.Empty },
                { TileType.Bush_Right_Top, CollisionType.Empty },
                { TileType.Fence_BottomLeft, CollisionType.Empty },
                { TileType.Fence_BottomRight, CollisionType.Empty },
                { TileType.StoneRamp_TopLeft, CollisionType.Empty }, // Nearly invisible
                { TileType.StoneRamp_TopMid, CollisionType.HalfFilledBottom },
                { TileType.StoneRamp_TopRight, CollisionType.Empty }, // Nearly invisible
                { TileType.BrokenPillar_Top, CollisionType.HalfFilledBottom },
                { TileType.Bush_Left_Mid, CollisionType.Empty },
                { TileType.Bush_Black_Left, CollisionType.Empty },
                { TileType.Bush_Black_Mid, CollisionType.Empty },
                { TileType.Bush_Black_Right, CollisionType.Empty },
                { TileType.Bush_Right_Mid, CollisionType.Empty },
                { TileType.BlackPillar_TopLeft, CollisionType.Full },
                { TileType.BlackPillar_TopRight, CollisionType.Full },
                { TileType.StoneRamp_MidLeft, CollisionType.DiagonalRightBottom },
                { TileType.StoneRamp_MidMid, CollisionType.Full },
                { TileType.StoneRamp_MidRight, CollisionType.DiagonalLeftBottom },
                { TileType.BrokenPillar_Bottom, CollisionType.Full },
                { TileType.Bush_Left_Bottom, CollisionType.Empty },
                { TileType.Bush_BottomLeftCorner, CollisionType.Empty },
                { TileType.Bush_Black_Bottom, CollisionType.Empty },
                { TileType.Bush_BottomRightCorner, CollisionType.Empty },
                { TileType.Bush_Right_Bottom, CollisionType.Empty },
                { TileType.BlackPillar_BottomLeft, CollisionType.Full },
                { TileType.BlackPillar_BottomRight, CollisionType.Full },
                { TileType.StoneRamp_BottomLeft, CollisionType.Empty }, // Nearly invisible
                { TileType.Empty19, CollisionType.Empty },
                { TileType.StoneRamp_BottomRight, CollisionType.Empty }, // Nearly invisible
                { TileType.Empty20, CollisionType.Empty },
                { TileType.Empty21, CollisionType.Empty },
                { TileType.Bush_BottomLeft, CollisionType.Empty },
                { TileType.Bush_BottomMid, CollisionType.Empty },
                { TileType.Bush_BottomRight, CollisionType.Empty },
                { TileType.Empty22, CollisionType.Empty }
            };
        #endregion

        #region enumTileType
        public enum TileType
        {
            Empty = 0,
            StoneTopFloor_TopLeft = 1,
            StoneTopFloor_TopMid = 2,
            StoneTopFloor_TopRight = 3, 
            GrassFloor_TopLeft = 4,
            GrassFloor_TopMid = 5,
            GrassFloor_TopRight = 6,
            Empty1 = 7,
            Empty2 = 8,
            Empty3 = 9,
            Empty4 = 10,
            Empty5 = 11,
            StoneTopFloor_BottomLeft = 12,
            StoneTopFloor_BottomMid = 13,
            StoneTopFloor_Bottomright = 14,
            GrassFloor_BottomLeft = 15,
            GrassFloor_BottomMid = 16,
            GrassFloor_BottomRight = 17,
            GrassFloorPillar_Tree_Top = 18,
            GrassFLoorPillar_Pillar_Top = 19,
            Empty6 = 20,
            Empty7 = 21,
            StoneLeftBottomFloor_TopLeft = 22,
            StoneLeftBottomFloor_TopRight = 23,
            StoneTopFloor_Wall1 = 24,
            StoneRightBottomFloor_TopLeft = 25,
            StoneRightBottomFloor_TopRight = 26,
            GrassFloorPillar_StoneRoad_TopLeft = 27,
            GrassFloorPillar_StoneRoad_TopRight= 28,
            GrassFloorPillar_Tree_Mid = 29,
            GrassFloorPillar_Pillar_Mid= 30,
            GrassFloorPillar_GrassRoad_Top = 31,
            Empty8 = 32,
            StoneLeftBottomFloor_BottomLeft = 33,
            StoneLeftBottomFloor_BottomRight = 34,
            StoneTopFloor_Wall2= 35,
            StoneRightBottomFloor_BottomLeft = 36,
            StoneRightBottomFloor_BottomRight = 37,
            GrassFloorPillar_StoneRoad_BottomLeft = 38,
            GrassFloorPillar_StoneRoad_BottomRight = 39,
            GrassFloorPillar_Tree_Bottom = 40,
            GrassFloorPillar_Pillar_Bottom = 41,
            GrassFloorPillar_GrassRoad_Bottom = 42,
            Empty9 = 43,
            StoneLeftTopCeiling_TopLeft = 44,
            StoneLeftTopCeiling_TopRight = 45,
            StoneWall_BetweenTopCeiling= 46,
            StoneRightTopCeiling_TopLeft = 47,
            StoneRightTopCeiling_TopRight = 48,
            Empty10 = 49,
            GrassRamp_LeftMid_Top = 50,
            GrassRamp_Mid_Top = 51,
            GrassRamp_RightMid_Top = 52,
            Empty11 = 53,
            Empty12 = 54,
            StoneLeftTopCeiling_BottomLeft = 55,
            StoneLeftTopCeiling_BottomRight = 56,
            StoneBottomCeiling_TopMid = 57,
            StoneRightTopCeiling_BottomLeft = 58,
            StoneRightTopCeiling_BottomRight = 59,
            GrassRamp_TopLeft = 60,
            GrassRamp_LeftMid_Mid = 61,
            GrassRamp_Mid_Mid = 62,
            GrassRamp_RightMid_Mid = 63,
            GrassRamp_TopRight = 64,
            Empty13 = 65,
            Empty14 = 66,
            StoneBottomCeiling_BottomLeft = 67,
            StoneBottomCeiling_BottomMid = 68,
            StoneBottomCeiling_BottomRight = 69,
            Empty15 = 70,
            GrassRamp_BottomLeft = 71,
            GrassRamp_LeftMid_Bottom = 72,
            GrassRamp_Mid_Bottom = 73,
            GrassRamp_RightMid_Bottom = 74,
            GrassRamp_BottomRight = 75,
            Empty16 = 76,
            Fence_TopLeft = 77,
            Fence_TopRight = 78,
            StoneBridge_TopLeft= 79,
            StoneBridge_MidTopLeft = 80,
            StoneBridge_MidTopRight = 81,
            StoneBridge_TopRight = 82,
            Empty17 = 83,
            Bush_TopLeft = 84,
            Bush_TopMid = 85,
            Bush_TopRight = 86,
            Empty18 = 87,
            Fence_MidLeft = 88,
            Fence_MidRight = 89,
            StoneBridge_BottomLeft = 90,
            StoneBridge_MidBottomLeft = 91,
            StoneBridge_MidBottomRight = 92,
            StoneBridge_BottomRight = 93,
            Bush_Left_Top= 94,
            Bush_TopLeftCorner = 95,
            Bush_Black_Top = 96,
            Bush_TopRightCorner = 97,
            Bush_Right_Top= 98,
            Fence_BottomLeft = 99,
            Fence_BottomRight = 100,
            StoneRamp_TopLeft = 101,
            StoneRamp_TopMid = 102,
            StoneRamp_TopRight = 103,
            BrokenPillar_Top= 104,
            Bush_Left_Mid = 105,
            Bush_Black_Left = 106,
            Bush_Black_Mid = 107,
            Bush_Black_Right = 108,
            Bush_Right_Mid = 109,
            BlackPillar_TopLeft = 110,
            BlackPillar_TopRight = 111,
            StoneRamp_MidLeft = 112,
            StoneRamp_MidMid = 113,
            StoneRamp_MidRight = 114,
            BrokenPillar_Bottom = 115,
            Bush_Left_Bottom = 116,
            Bush_BottomLeftCorner = 117,
            Bush_Black_Bottom= 118,
            Bush_BottomRightCorner = 119,
            Bush_Right_Bottom = 120,
            BlackPillar_BottomLeft = 121,
            BlackPillar_BottomRight = 122,
            StoneRamp_BottomLeft = 123,
            Empty19 = 124,
            StoneRamp_BottomRight = 125,
            Empty20 = 126,
            Empty21 = 127,
            Bush_BottomLeft = 128,
            Bush_BottomMid = 129,
            Bush_BottomRight = 130,
            Empty22 = 131
        }
        #endregion

        #region CollisionType
        public enum CollisionType
        {
            Empty,
            Full,
            HalfFilledTop,
            HalfFilledBottom,
            DiagonalLeftBottom,
            DiagonalRightBottom
        }
        #endregion

        #endregion

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Set the window size
            _graphics.PreferredBackBufferWidth = 1600; // Width of the window
            _graphics.PreferredBackBufferHeight = 480; // Height of the window
            _graphics.ApplyChanges(); // Apply the changes to the graphics device
        }

        protected override void Initialize()
        {
            _tileWidth = 32;
            _tileHeight = 32;

            _currentLevel = 1;

            _score = 0;  // Initialize score to 0

            _gameWon = false;

            // Set the initial position of the sprite
            _position = new Vector2(-40,350);

            _velocity = Vector2.Zero;

            InitializeCollisionMasks();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            #region WhiteTexture
            // Create a 1x1 white texture to use for drawing rectangles
            _hitboxPurplePortalTexture = new Texture2D(GraphicsDevice, 1, 1);
            _hitboxPurplePortalTexture.SetData(new[] { Color.White });

            _hitboxGreenPortalTexture = new Texture2D(GraphicsDevice, 1, 1);
            _hitboxGreenPortalTexture.SetData(new[] { Color.White });
            #endregion

            #region Player
            // Load the player's sprite sheet
            _spriteSheet = Content.Load<Texture2D>("GoblinMechRiderSpriteSheet");
            #endregion

            #region PurplePortal
            // Load the PurplePortal sprite sheet
            _purplePortalSpriteSheet = Content.Load<Texture2D>("Purple Portal Sprite Sheet");

            // Initialize the PurplePortal hitbox
            _purplePortalHitbox = new Rectangle((int)_purplePortalPosition.X, (int)_purplePortalPosition.Y, _purplePortalFrameWidth, _purplePortalFrameHeight);

            // Example: Initialize the hitbox for the other sprite (e.g., a portal)
            //_purplePortalHitbox = new Rectangle((int)_purplePortalPosition.X, (int)_purplePortalPosition.Y, _purplePortalFrameWidth / 2, _purplePortalFrameHeight); // Adjust as necessary

            // Initialize the PurplePortal frames for the idle animation (first row)
            _purplePortalFrames = new Rectangle[8];
            for (int i = 0; i < 8; i++)
            {
                _purplePortalFrames[i] = new Rectangle(i * _purplePortalFrameWidth, 0, _purplePortalFrameWidth, _purplePortalFrameHeight);
            }
            #endregion

            #region GreenPortal
            // Load the GreenPortal sprite sheet
            _greenPortalSpriteSheet = Content.Load<Texture2D>("Green Portal Sprite Sheet");

            // Initialize the GreenPortal hitbox
            _greenPortalHitbox = new Rectangle((int)_greenPortalPosition.X, (int)_greenPortalPosition.Y, _greenPortalFrameWidth, _greenPortalFrameHeight);

            // Initialize the GreenPortal frames for the idle animation (first row)
            _greenPortalFrames = new Rectangle[8];
            for (int i = 0; i < 8; i++)
            {
                _greenPortalFrames[i] = new Rectangle(i * _greenPortalFrameWidth, 0, _greenPortalFrameWidth, _greenPortalFrameHeight);
            }
            #endregion

            #region Coin
            // Load the coin spritesheet (Make sure the file is in your Content folder)
            Texture2D coinTexture = Content.Load<Texture2D>("coin2_20x20");

            // Initialize the list of coins for level 1
            _coinsLevel1 = new List<Coin>()
            {
                // Add multiple coins with different positions
                new Coin(coinTexture, 20, 20, 9, 0.1f) { Position = new Vector2(935, 250) },
                new Coin(coinTexture, 20, 20, 9, 0.1f) { Position = new Vector2(340, 310) },
                new Coin(coinTexture, 20, 20, 9, 0.1f) { Position = new Vector2(650, 400) },
                new Coin(coinTexture, 20, 20, 9, 0.1f) { Position = new Vector2(1200, 250) },
                new Coin(coinTexture, 20, 20, 9, 0.1f) { Position = new Vector2(1410, 370) },
            };

            // Initialize the list of coins for level 2
            _coinsLevel2 = new List<Coin>()
            {
                // Add multiple coins with different positions
            new Coin(coinTexture, 20, 20, 9, 0.1f) { Position = new Vector2(400, 320) },
            new Coin(coinTexture, 20, 20, 9, 0.1f) { Position = new Vector2(710, 340) },
            new Coin(coinTexture, 20, 20, 9, 0.1f) { Position = new Vector2(850, 230) }, // 850, 220 Testing
            new Coin(coinTexture, 20, 20, 9, 0.1f) { Position = new Vector2(1140, 310) },
            new Coin(coinTexture, 20, 20, 9, 0.1f) { Position = new Vector2(1410, 370) },
        };
            #endregion

            #region ScoreOverlay
            SpriteFont scoreFont = Content.Load<SpriteFont>("ScoreFont");

            _scoreOverlay = new ScoreOverlay(scoreFont, new Vector2(10, 10));  // Initialize UI screen with font and position
            #endregion

            #region WinScreen
            SpriteFont font = Content.Load<SpriteFont>("Font");

            _winScreen = new WinScreen(font, GraphicsDevice);     // Initialize the win screen
            #endregion

            #region StartScreen
            // Load the font and button texture
            Texture2D buttonTexture = Content.Load<Texture2D>("Hover@3x");

            // Initialize the start screen
            _startScreen = new StartScreen(font, buttonTexture);
            #endregion

            #region Tileset
            // Load the tileset texture
            _tileset = Content.Load<Texture2D>("Tiles");
            #endregion

            #region Level1
            // Define the level data (mocked for testing purposes)
            _level1Data = new TileType[15,50]
            {
                { TileType.StoneLeftTopCeiling_TopRight,    TileType.StoneWall_BetweenTopCeiling,       TileType.StoneRightTopCeiling_TopLeft,  TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,                    TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,                   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,      TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling },
                { TileType.StoneWall_BetweenTopCeiling,     TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,                    TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,                   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,      TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling },
                { TileType.StoneWall_BetweenTopCeiling,     TileType.StoneBottomCeiling_BottomMid,      TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,      TileType.StoneBottomCeiling_BottomMid,  TileType.StoneRightTopCeiling_BottomLeft,   TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,                    TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,                   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,      TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling },
                { TileType.StoneRightTopCeiling_BottomLeft, TileType.StoneRightTopCeiling_BottomRight,  TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,      TileType.StoneLeftTopCeiling_BottomRight,       TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,              TileType.StoneBottomCeiling_BottomMid,                   TileType.StoneBottomCeiling_BottomMid,              TileType.StoneBottomCeiling_BottomMid,                  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,      TileType.StoneBottomCeiling_BottomMid,      TileType.StoneBottomCeiling_BottomMid,     TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling },
                { TileType.Empty,                           TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.StoneLeftTopCeiling_BottomLeft,    TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,  TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneBottomCeiling_BottomMid,      TileType.StoneRightTopCeiling_BottomRight,      TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                          TileType.Empty,                                     TileType.Empty,                                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                             TileType.StoneLeftTopCeiling_BottomLeft,   TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling },
                { TileType.Empty,                           TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                                 TileType.StoneLeftTopCeiling_BottomLeft,TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,      TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,      TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,      TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,      TileType.StoneBottomCeiling_BottomMid,  TileType.StoneRightTopCeiling_BottomRight,  TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                          TileType.Empty,                                     TileType.Empty,                                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                             TileType.Empty,                            TileType.Empty,                         TileType.StoneLeftTopCeiling_BottomLeft,TileType.StoneBottomCeiling_BottomMid,  TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling },
                { TileType.Empty,                           TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                          TileType.Empty,                                     TileType.Empty,                                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                             TileType.Empty,                            TileType.Empty,                         TileType.Empty,                         TileType.StoneLeftTopCeiling_BottomLeft,TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid },
                { TileType.Empty,                           TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                          TileType.Empty,                                     TileType.Empty,                                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                             TileType.Empty,                            TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty },
                { TileType.Empty,                           TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.StoneRamp_TopLeft,                     TileType.StoneRamp_TopMid,                      TileType.StoneRamp_TopRight,                    TileType.StoneRamp_TopLeft,                     TileType.StoneRamp_TopMid,                      TileType.StoneRamp_TopRight,                        TileType.StoneRamp_TopLeft,                              TileType.StoneRamp_TopMid,                          TileType.StoneBridge_TopLeft,                           TileType.StoneBridge_MidTopLeft,        TileType.StoneRamp_TopMid,                  TileType.StoneRamp_TopRight,                TileType.Empty,                            TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty },
                { TileType.Empty,                           TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.StoneRamp_MidLeft,                     TileType.StoneRamp_MidMid,                      TileType.StoneRamp_MidRight,                    TileType.StoneRamp_MidLeft,                     TileType.StoneRamp_MidMid,                      TileType.StoneRamp_MidRight,                        TileType.StoneRamp_MidLeft,                              TileType.StoneRamp_MidMid,                          TileType.StoneBridge_BottomLeft,                        TileType.StoneBridge_MidBottomLeft,     TileType.StoneBridge_MidBottomRight,        TileType.StoneRamp_MidRight,                TileType.Empty,                            TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty },
                { TileType.Empty,                           TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.StoneBridge_TopLeft,                   TileType.StoneBridge_MidTopLeft,        TileType.StoneBridge_MidTopRight,       TileType.StoneBridge_TopRight,              TileType.Empty,                                 TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.StoneRamp_BottomLeft,                  TileType.Empty,                                 TileType.StoneRamp_BottomRight,                 TileType.StoneRamp_BottomLeft,                  TileType.Empty,                                 TileType.StoneRamp_BottomRight,                     TileType.StoneRamp_BottomLeft,                           TileType.Empty,                                     TileType.Empty,                                         TileType.Empty,                         TileType.GrassFLoorPillar_Pillar_Top,       TileType.StoneRamp_BottomRight,             TileType.Empty,                            TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty },
                { TileType.Empty,                           TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.StoneBridge_BottomLeft,                TileType.StoneBridge_MidBottomLeft,     TileType.StoneBridge_MidBottomRight,    TileType.StoneBridge_BottomRight,           TileType.Empty,                                 TileType.Empty,                         TileType.Empty,                         TileType.StoneTopFloor_TopLeft,             TileType.StoneTopFloor_TopMid,          TileType.StoneTopFloor_TopRight,            TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.StoneTopFloor_TopLeft,             TileType.StoneTopFloor_TopMid,          TileType.StoneTopFloor_TopRight,            TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                          TileType.Empty,                                     TileType.Empty,                                         TileType.Empty,                         TileType.GrassFLoorPillar_Pillar_Top,       TileType.Empty,                             TileType.Empty,                            TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty },
                { TileType.Empty,                           TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.GrassRamp_LeftMid_Top,         TileType.GrassRamp_Mid_Top,                 TileType.GrassRamp_RightMid_Top,        TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                                 TileType.GrassFloorPillar_Tree_Top,     TileType.GrassFLoorPillar_Pillar_Top,   TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                         TileType.Empty,                         TileType.StoneTopFloor_BottomLeft,          TileType.StoneTopFloor_BottomMid,       TileType.StoneTopFloor_Bottomright,         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.StoneTopFloor_BottomLeft,          TileType.StoneTopFloor_BottomMid,       TileType.StoneTopFloor_Bottomright,         TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                          TileType.Empty,                                     TileType.Empty,                                         TileType.GrassFloorPillar_Tree_Top,     TileType.BrokenPillar_Bottom,               TileType.Empty,                             TileType.Empty,                            TileType.Empty,                         TileType.Empty,                         TileType.GrassRamp_LeftMid_Top,         TileType.GrassRamp_Mid_Top,             TileType.GrassRamp_RightMid_Top,        TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty },
                { TileType.GrassFloor_TopLeft,              TileType.GrassFloor_TopMid,                 TileType.GrassFloor_TopRight,           TileType.GrassRamp_TopLeft,             TileType.GrassRamp_LeftMid_Mid,         TileType.GrassRamp_Mid_Mid,                 TileType.GrassRamp_RightMid_Mid,        TileType.GrassRamp_TopRight,                TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,   TileType.GrassFloorPillar_Tree_Mid,     TileType.GrassFloorPillar_Pillar_Mid,   TileType.GrassFloorPillar_GrassRoad_Top,    TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.StoneTopFloor_TopMid,          TileType.StoneTopFloor_TopMid,          TileType.StoneLeftBottomFloor_TopRight,     TileType.StoneTopFloor_Wall1,           TileType.StoneRightBottomFloor_TopLeft,     TileType.StoneTopFloor_TopMid,          TileType.StoneTopFloor_TopMid,          TileType.StoneTopFloor_TopMid,          TileType.StoneLeftBottomFloor_TopRight,     TileType.StoneTopFloor_Wall1,           TileType.StoneRightBottomFloor_TopLeft,     TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,   TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,   TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,   TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,   TileType.GrassFloorPillar_StoneRoad_TopLeft,        TileType.GrassFloorPillar_StoneRoad_TopRight,            TileType.GrassFloorPillar_StoneRoad_TopLeft,        TileType.GrassFloorPillar_StoneRoad_TopRight,           TileType.GrassFloorPillar_Tree_Mid,     TileType.GrassFloorPillar_Pillar_Mid,       TileType.GrassFloorPillar_GrassRoad_Top,    TileType.GrassFloor_TopMid,                TileType.GrassFloor_TopRight,           TileType.GrassRamp_TopLeft,             TileType.GrassRamp_LeftMid_Mid,         TileType.GrassRamp_Mid_Mid,             TileType.GrassRamp_RightMid_Mid,        TileType.GrassRamp_TopRight,            TileType.GrassFloor_TopLeft,            TileType.GrassFloor_TopMid,             TileType.GrassFloor_TopRight },
                { TileType.GrassFloor_BottomLeft,           TileType.GrassFloor_BottomMid,              TileType.GrassFloor_BottomRight,        TileType.GrassRamp_BottomLeft,          TileType.GrassRamp_LeftMid_Bottom,      TileType.GrassRamp_Mid_Bottom,              TileType.GrassRamp_RightMid_Bottom,     TileType.GrassRamp_BottomRight,             TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,TileType.GrassFloorPillar_Tree_Bottom,  TileType.GrassFloorPillar_Pillar_Bottom,TileType.GrassFloorPillar_GrassRoad_Bottom, TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.StoneTopFloor_BottomMid,       TileType.StoneTopFloor_BottomMid,       TileType.StoneLeftBottomFloor_BottomRight,  TileType.StoneTopFloor_Wall2,           TileType.StoneRightBottomFloor_BottomLeft,  TileType.StoneTopFloor_BottomMid,       TileType.StoneTopFloor_BottomMid,       TileType.StoneTopFloor_BottomMid,       TileType.StoneLeftBottomFloor_BottomRight,  TileType.StoneTopFloor_Wall2,           TileType.StoneRightBottomFloor_BottomLeft,  TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,TileType.GrassFloorPillar_StoneRoad_BottomRight,TileType.GrassFloorPillar_StoneRoad_BottomRight,TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,TileType.GrassFloorPillar_StoneRoad_BottomLeft,     TileType.GrassFloorPillar_StoneRoad_BottomRight,         TileType.GrassFloorPillar_StoneRoad_BottomLeft,     TileType.GrassFloorPillar_StoneRoad_BottomRight,        TileType.GrassFloorPillar_Tree_Bottom,  TileType.GrassFloorPillar_Pillar_Bottom,    TileType.GrassFloorPillar_GrassRoad_Bottom, TileType.GrassFloor_BottomMid,             TileType.GrassFloor_BottomRight,        TileType.GrassRamp_BottomLeft,          TileType.GrassRamp_LeftMid_Bottom,      TileType.GrassRamp_Mid_Bottom,          TileType.GrassRamp_RightMid_Bottom,     TileType.GrassRamp_BottomRight,         TileType.GrassFloor_BottomLeft,         TileType.GrassFloor_BottomMid,          TileType.GrassFloor_BottomRight },
                };
            #endregion

            #region Level2
            // Define the level data
            _level2Data = new TileType[15, 50]
            {
                { TileType.StoneWall_BetweenTopCeiling,             TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling },
                { TileType.StoneWall_BetweenTopCeiling,             TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling },
                { TileType.StoneWall_BetweenTopCeiling,             TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,       TileType.StoneBottomCeiling_BottomMid,      TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,      TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,              TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,              TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,      TileType.StoneWall_BetweenTopCeiling,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling },
                { TileType.StoneBottomCeiling_BottomMid,            TileType.StoneBottomCeiling_BottomMid,              TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,      TileType.StoneBottomCeiling_BottomMid,      TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneLeftTopCeiling_BottomRight,   TileType.StoneBottomCeiling_TopMid,     TileType.StoneBottomCeiling_TopMid,     TileType.StoneBottomCeiling_TopMid,     TileType.StoneBottomCeiling_TopMid,     TileType.StoneBottomCeiling_TopMid,     TileType.StoneBottomCeiling_TopMid,     TileType.StoneBottomCeiling_TopMid,         TileType.StoneBottomCeiling_TopMid,     TileType.StoneRightTopCeiling_TopRight,     TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                         TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                         TileType.Empty,                             TileType.StoneLeftTopCeiling_TopLeft,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling },
                { TileType.Empty,                                   TileType.Empty,                                     TileType.Empty,                         TileType.Empty,                             TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.StoneBottomCeiling_BottomLeft,     TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,      TileType.StoneBottomCeiling_BottomMid,  TileType.StoneRightTopCeiling_BottomRight,  TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                         TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                         TileType.Empty,                             TileType.StoneLeftTopCeiling_TopLeft,       TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,   TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling,               TileType.StoneWall_BetweenTopCeiling,           TileType.StoneWall_BetweenTopCeiling },
                { TileType.Empty,                                   TileType.StoneBridge_TopLeft,                       TileType.StoneBridge_MidTopLeft,        TileType.StoneBridge_MidTopRight,           TileType.StoneBridge_TopRight,              TileType.StoneRamp_TopRight,            TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                         TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                         TileType.Empty,                             TileType.StoneBottomCeiling_BottomLeft,     TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,  TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,              TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,              TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,              TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,              TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid,              TileType.StoneBottomCeiling_BottomMid,          TileType.StoneBottomCeiling_BottomMid },
                { TileType.Empty,                                   TileType.StoneBridge_BottomLeft,                    TileType.StoneBridge_MidBottomLeft,     TileType.StoneBridge_MidBottomRight,        TileType.StoneBridge_BottomRight,           TileType.StoneRamp_MidRight,            TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                         TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                         TileType.Empty,                             TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty },
                { TileType.Empty,                                   TileType.Empty,                                     TileType.Empty,                         TileType.BrokenPillar_Bottom,               TileType.Empty,                             TileType.StoneRamp_BottomRight,         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                             TileType.StoneBridge_TopLeft,           TileType.StoneBridge_MidTopLeft,        TileType.StoneBridge_MidTopRight,           TileType.StoneBridge_MidTopLeft,                TileType.StoneBridge_MidTopLeft,                    TileType.StoneBridge_TopRight,          TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                         TileType.Empty,                             TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty },
                { TileType.Empty,                                   TileType.Empty,                                     TileType.Empty,                         TileType.Fence_MidRight,                    TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                             TileType.StoneBridge_BottomLeft,        TileType.StoneBridge_MidBottomLeft,     TileType.StoneBridge_MidBottomRight,        TileType.StoneBridge_MidBottomLeft,             TileType.StoneBridge_MidBottomLeft,                 TileType.StoneBridge_BottomRight,       TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                         TileType.Empty,                             TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty },
                { TileType.Empty,                                   TileType.Empty,                                     TileType.Empty,                         TileType.Fence_MidRight,                    TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.GrassFLoorPillar_Pillar_Top,       TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                         TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                         TileType.Empty,                             TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty },
                { TileType.Empty,                                   TileType.Empty,                                     TileType.Empty,                         TileType.Fence_MidRight,                    TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.GrassFLoorPillar_Pillar_Top,       TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                         TileType.Empty,                                 TileType.Empty,                                     TileType.StoneBridge_TopLeft,           TileType.StoneBridge_MidTopRight,           TileType.StoneRamp_TopRight,                TileType.StoneRamp_TopLeft,             TileType.StoneRamp_TopRight,            TileType.StoneRamp_TopLeft,             TileType.StoneRamp_TopMid,                      TileType.StoneRamp_TopRight,                        TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty },
                { TileType.Empty,                                   TileType.Empty,                                     TileType.Empty,                         TileType.Fence_BottomRight,                 TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.GrassRamp_LeftMid_Top,         TileType.GrassRamp_Mid_Top,             TileType.GrassRamp_RightMid_Top,            TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.StoneTopFloor_TopLeft,         TileType.StoneTopFloor_TopMid,              TileType.StoneTopFloor_TopRight,            TileType.Empty,                         TileType.Empty,                         TileType.GrassFLoorPillar_Pillar_Top,       TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                         TileType.Empty,                                 TileType.Empty,                                     TileType.StoneBridge_BottomLeft,        TileType.StoneBridge_MidBottomRight,        TileType.StoneRamp_MidRight,                TileType.StoneRamp_MidLeft,             TileType.StoneRamp_MidRight,            TileType.StoneRamp_MidLeft,             TileType.StoneRamp_MidMid,                      TileType.StoneRamp_MidRight,                        TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty },
                { TileType.Empty,                                   TileType.Empty,                                     TileType.GrassFloorPillar_Tree_Top,     TileType.BrokenPillar_Bottom,               TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.GrassRamp_LeftMid_Top,         TileType.GrassRamp_TopLeft,             TileType.GrassRamp_LeftMid_Mid,         TileType.GrassRamp_Mid_Mid,             TileType.GrassRamp_RightMid_Mid,            TileType.GrassRamp_TopRight,            TileType.GrassRamp_RightMid_Top,        TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.StoneTopFloor_BottomLeft,      TileType.StoneTopFloor_BottomMid,           TileType.StoneTopFloor_Bottomright,         TileType.Empty,                         TileType.GrassFloorPillar_Tree_Top,     TileType.GrassFLoorPillar_Pillar_Top,       TileType.Empty,                                 TileType.Empty,                                     TileType.GrassFloorPillar_Tree_Top,     TileType.Empty,                                 TileType.Empty,                                     TileType.GrassFloorPillar_Tree_Top,     TileType.GrassFLoorPillar_Pillar_Top,       TileType.StoneRamp_BottomRight,             TileType.StoneRamp_BottomLeft,          TileType.StoneRamp_BottomRight,         TileType.StoneRamp_BottomLeft,          TileType.Empty,                                 TileType.StoneRamp_BottomRight,                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty },
                { TileType.GrassFloorPillar_StoneRoad_TopLeft,      TileType.GrassFloorPillar_StoneRoad_TopRight,       TileType.GrassFloorPillar_Tree_Mid,     TileType.GrassFloorPillar_Pillar_Mid,       TileType.GrassFloorPillar_GrassRoad_Top,    TileType.GrassFloor_TopLeft,            TileType.GrassFloor_TopMid,             TileType.GrassFloor_TopRight,           TileType.GrassRamp_TopLeft,             TileType.GrassRamp_LeftMid_Mid,         TileType.GrassRamp_BottomLeft,          TileType.GrassRamp_Mid_Bottom,          TileType.GrassRamp_Mid_Bottom,          TileType.GrassRamp_RightMid_Bottom,         TileType.GrassRamp_BottomRight,         TileType.GrassRamp_RightMid_Mid,        TileType.GrassRamp_TopRight,            TileType.GrassRamp_RightMid_Top,        TileType.Empty,                         TileType.Empty,                         TileType.StoneLeftBottomFloor_TopLeft,      TileType.StoneLeftBottomFloor_TopRight, TileType.StoneTopFloor_Wall1,               TileType.StoneRightBottomFloor_TopLeft,     TileType.StoneTopFloor_TopMid,          TileType.GrassFloorPillar_Tree_Mid,     TileType.GrassFloorPillar_Pillar_Mid,       TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,       TileType.GrassFloorPillar_Tree_Mid,     TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,       TileType.GrassFloorPillar_Tree_Mid,     TileType.GrassFloorPillar_Pillar_Mid,       TileType.GrassFloorPillar_GrassRoad_Top,    TileType.GrassFloor_TopLeft,            TileType.GrassFloor_TopMid,             TileType.GrassFloor_TopRight,           TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,       TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,       TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,       TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,       TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,       TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight },
                { TileType.GrassFloorPillar_StoneRoad_BottomLeft,   TileType.GrassFloorPillar_StoneRoad_BottomRight,    TileType.GrassFloorPillar_Tree_Bottom,  TileType.GrassFloorPillar_Pillar_Bottom,    TileType.GrassFloorPillar_GrassRoad_Bottom, TileType.GrassFloor_BottomLeft,         TileType.GrassFloor_BottomMid,          TileType.GrassFloor_BottomRight,        TileType.GrassRamp_BottomLeft,          TileType.GrassRamp_LeftMid_Bottom,      TileType.GrassRamp_Mid_Bottom,          TileType.GrassRamp_Mid_Bottom,          TileType.GrassRamp_Mid_Bottom,          TileType.GrassRamp_RightMid_Bottom,         TileType.GrassRamp_RightMid_Bottom,     TileType.GrassRamp_RightMid_Bottom,     TileType.GrassRamp_BottomRight,         TileType.GrassRamp_RightMid_Mid,        TileType.GrassRamp_TopRight,            TileType.GrassRamp_RightMid_Top,        TileType.StoneLeftBottomFloor_BottomLeft,   TileType.StoneTopFloor_BottomMid,       TileType.StoneTopFloor_Wall1,               TileType.StoneRightBottomFloor_BottomLeft,  TileType.StoneTopFloor_BottomMid,       TileType.GrassFloorPillar_Tree_Bottom,  TileType.GrassFloorPillar_Pillar_Bottom,    TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,    TileType.GrassFloorPillar_Tree_Bottom,  TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,    TileType.GrassFloorPillar_Tree_Bottom,  TileType.GrassFloorPillar_Pillar_Bottom,    TileType.GrassFloorPillar_GrassRoad_Bottom, TileType.GrassFloor_BottomLeft,         TileType.GrassFloor_BottomMid,          TileType.GrassFloor_BottomRight,        TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,    TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,    TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,    TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,    TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,    TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight },
            };
            #endregion

            #region PlayerFrameDimensions
            // Calculate frame dimensions
            _frameWidth = _spriteSheet.Width / 8; // 8 columns in the movement row -> 160
            _frameHeight = _spriteSheet.Height / 5; // 5 rows -> 96

            // Assuming _playerPosition and _playerTexture are already defined
            _playerHitbox = new Rectangle((int)_position.X - (_frameWidth / 2), (int)_position.Y + _frameHeight / 2, _frameWidth / 2, _frameHeight / 2); // Adjust the width division based on the number of frames
            Console.WriteLine(_playerHitbox);
            #endregion

        }

        protected override void Update(GameTime gameTime)
        {
            #region IsKeyDown
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            #endregion

            if (!_gameWon)
            {

                #region delta
                // Get the elapsed time since the last update normally var now float
                float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
                #endregion

                #region HandleInputMovement
                // Handle input for movement
                var direction = Vector2.Zero;
                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                {
                    direction.X = -1;
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.Right))
                {
                    direction.X = 1;
                }
                #endregion

                #region HandleAttacking
                // Handle attacking
                if (Keyboard.GetState().IsKeyDown(Keys.Space) && !_isAttacking)
                {
                    _isAttacking = true;
                    _frame = 0; // Start from the first frame of the attack animation
                }
                #endregion

                #region HandleJumping
                // Handle jumping
                if (Keyboard.GetState().IsKeyDown(Keys.Up) && _isGrounded && !_isJumping)
                {
                    _velocity.Y = _jumpSpeed;
                    _isJumping = true;
                    _isGrounded = false;
                }
                #endregion

                #region isAttacking
                if (_isAttacking)
                {
                    AnimateAttack(gameTime);
                }
                else
                {
                    // Update horizontal velocity and animation
                    if (direction.X != 0)
                    {
                        _velocity.X += direction.X * _acceleration * delta;
                        _velocity.X = MathHelper.Clamp(_velocity.X, -_maxSpeed, _maxSpeed);
                        AnimateMovement(gameTime);
                    }
                    else
                    {
                        if (_velocity.X > 0)
                        {
                            _velocity.X -= _deceleration * delta;
                            if (_velocity.X < 0) _velocity.X = 0;
                        }
                        else if (_velocity.X < 0)
                        {
                            _velocity.X += _deceleration * delta;
                            if (_velocity.X > 0) _velocity.X = 0;
                        }

                        if (!_isJumping)
                        {
                            _frame = 0; // Reset to idle frame if no movement
                        }
                    }
                }
                #endregion

                #region isJumping
                if (_isJumping)
                {
                    // Apply gravity while jumping
                    _velocity.Y += _gravity * delta;
                }
                #endregion

                #region UpdatePosition
                // Update position
                _position.X += _velocity.X * delta;
                _position.Y += _velocity.Y * delta;


                #endregion

                #region UpdatePositionCodeStein
                /*
                // First calculate the future hitbox of the player
                float newPosX = _position.X + _velocity.X * delta;
                float newPosY = _position.Y + _velocity.Y * delta;

                // Create a hitbox that only moves horizontally, if we collide
                // with something after we move then we know we hit something and should 
                // change our position to be right next to the element we collided with

                Rectangle futureHorizontalHitbox =
                    new Rectangle((int)newPosX, _playerHitbox.Y, _playerHitbox.Width, _playerHitbox.Height);
                if ((CollidesWithTerrain(futureHorizontalHitbox) is var otherHorizontalHitbox) && otherHorizontalHitbox != null)
                {
                    // We are intersecting, if we are moving to the right (positive velocity) we should move
                    // right until we are hitting the other element
                    if (_velocity.X > 0) // Moving to the right
                    {
                        _position.X = otherHorizontalHitbox.Value.X - _tileWidth - 1;
                    }
                    else // Moving left
                    {
                        _position.X = otherHorizontalHitbox.Value.X + 1;
                    }
                    _velocity.X = 0;
                }
                else
                {
                    _position.X = newPosX;
                }

                // Create a hitbox that only moves vertically, if we collide
                // with something after we move then we know we hit something and should 
                // change our position to be right next to the element we collided with

                Rectangle futureVerticalHitbox =
                    new Rectangle((int)_position.X, (int)newPosY, _playerHitbox.Width, _playerHitbox.Height);
                if ((CollidesWithTerrain(futureVerticalHitbox) is var otherVerticalHitbox) && otherVerticalHitbox != null)
                {
                    // We are intersecting, if we are moving down (positive velocity) we should move
                    // up until we are hitting the other element
                    if (_velocity.Y > 0) // Moving down
                    {
                        _position.Y = otherVerticalHitbox.Value.Y - _tileHeight - 1;
                    }
                    else // Moving up
                    {
                        _position.Y = otherVerticalHitbox.Value.Y + 1;
                    }
                    _velocity.Y = 0;
                    _isGrounded = true;
                    _isJumping = false;
                    Debug.WriteLine("Terrain collision");
                }
                else if (futureVerticalHitbox.Y >= GraphicsDevice.Viewport.Height - _playerHitbox.Height) // Falling off bottom of screen
                {
                    _position.Y = GraphicsDevice.Viewport.Height - _playerHitbox.Height - 1;
                    Debug.WriteLine("Bottom screen collision");
                }
                else if (futureVerticalHitbox.Y <= 0)
                {
                    _position.Y = 1;
                    Debug.WriteLine("Top screen colission");
                }
                else
                {
                    _position.Y = newPosY;
                    _velocity.Y += _gravity * delta;
                    Debug.WriteLine("No colission");
                }
                */
                #endregion

                #region UpdatePlayerHitbox
                // Update the player hitbox position
                _playerHitbox.X = (int)_position.X + (_frameWidth / 4) + 5;
                _playerHitbox.Y = (int)_position.Y + _frameHeight / 2;

                #endregion

                #region UpdatePurplePortalHitbox
                // Update the other sprite's hitbox position
                _purplePortalHitbox.X = (int)_purplePortalPosition.X;
                _purplePortalHitbox.Y = (int)_purplePortalPosition.Y;
                #endregion

                #region Boundaries
                // Define the margin for the left and right boundaries
                int leftMargin = -50;  // Allow some space off the left side
                int rightMargin = 50;  // Allow some space off the right side

                // Ensure the player stays within the screen bounds with the added margins
                _position.X = MathHelper.Clamp(_position.X, leftMargin, GraphicsDevice.Viewport.Width - _frameWidth + rightMargin);
                _position.Y = MathHelper.Clamp(_position.Y, 0, GraphicsDevice.Viewport.Height - _frameHeight);
                #endregion

                #region GroundCollision
                // Simulate ground collision
                if (_position.Y >= 350) // GraphicsDevice.Viewport.Height - _frameHeight
                {
                    _position.Y = 350; // GraphicsDevice.Viewport.Height - _frameHeight
                    _velocity.Y = 0;
                    _isGrounded = true;
                    _isJumping = false;
                }
                #endregion

                #region CollisionPlayerAndPurplePortal
                // Check for collisions with between player and purple portal only if on level 1
                if (_currentLevel == 1 || isLevel2Active == false)
                {
                    // Update portal animation if necessary
                    UpdatePurplePortalAnimation(gameTime);

                    // Check for collision between player and portal
                    if (_playerHitbox.Intersects(_purplePortalHitbox))
                    {
                        // Move to level 2
                        _currentLevel = 2;
                        LoadLevel2();
                    }
                }
                #endregion

                #region CollisionPlayerAndGreenPortal
                // Check for collisions with between player and green portal only if on level 2
                if (_currentLevel == 2 || isLevel2Active == true)
                {
                    // Update portal animation if necessary
                    UpdateGreenPortalAnimation(gameTime);

                    // Check for collision between player and portal
                    if (_playerHitbox.Intersects(_greenPortalHitbox))
                    {
                        // Move to level 1
                        _currentLevel = 1;
                        LoadLevel1();
                    }
                }
                #endregion

                #region UpdateCurrentLevel
                // Update the current level (this will draw the new level after transition)
                if (isLevel2Active)
                {
                    // Update the level 2 logic
                    UpdateLevel2(gameTime);
                }
                else
                {
                    // Update the level 1 logic
                    UpdateLevel1(gameTime);
                }
                #endregion

                /*
                // Check collision with each tile
                for (int y = 0; y < _levelData.GetLength(0); y++)
                {
                    for (int x = 0; x < _levelData.GetLength(1); x++)
                    {
                        TileType tileType = _levelData[y, x];
                        Vector2 tilePosition = new Vector2(x * _tileWidth, y * _tileHeight);

                        if (CheckTileCollision(_playerHitbox, tilePosition, tileType))
                        {
                            // Handle the collision (e.g., stop the player's movement, adjust position, etc.)
                            HandleCollision(gameTime);
                        }
                    }
                }
                */

                #region UpdateCoins
                // Update the coins based on the current level
                if (_currentLevel == 1)
                {
                    UpdateCoins(_coinsLevel1, gameTime);
                }
                else if (_currentLevel == 2)
                {
                    UpdateCoins(_coinsLevel2, gameTime);
                }
                #endregion

                #region ScoreOverlay
                _scoreOverlay.UpdateScore(_score);  // Update the UI screen with the latest score
                #endregion

                #region UpdateScore
                if (_score >= MaxScore)
                {
                    _gameWon = true;
                }
                #endregion
            }
            #region _gameWon
            else
            {
                if (_winScreen.Update(gameTime))
                {
                    RestartGame();
                    LoadLevel1();
                }
            }
            #endregion

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            if (!_gameWon)
            {
                // Level 2
                if (_currentLevel == 2 || isLevel2Active == true)
                {
                    DrawLevel2(gameTime);
                    DrawGreenPortal();
                    DrawCoins(_coinsLevel2);
                }
                // Level 1
                else if (_currentLevel == 1 || isLevel2Active == false)
                {
                    DrawLevel(gameTime);
                    DrawPurplePortal();
                    DrawCoins(_coinsLevel1);
                }

                DrawPlayer();

                _scoreOverlay.Draw(_spriteBatch);  // Draw the UI screen with the score
            }
            else
            {
                _winScreen.Draw(_spriteBatch);  // Draw the Win Screen
            }

                _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void RestartGame()
        {
            _score = 0;
            _gameWon = false;
            _position = new Vector2(-40, 350);
            isLevel2Active = false;

            // Set the game back to level 1
            _currentLevel = 1;

            // Reset coin visibility
            foreach (var coin in _coinsLevel1)
            {
                coin.IsVisible = true;
            }
            foreach (var coin in _coinsLevel2)
            {
                coin.IsVisible = true;
            }
        }

        private void UpdateCoins(List<Coin> coins, GameTime gameTime)
        {
            // Update all coins in the current list and check for collision
            foreach (var coin in coins)
            {
                coin.Update(gameTime);

                // Check for collision between player and each coin
                if (coin.IsVisible && coin.GetHitbox().Intersects(_playerHitbox))
                {
                    coin.IsVisible = false; // Make the coin disappear

                    if (_score < MaxScore) // Increment score if it's less than MaxScore
                    {
                        _score += 1;
                    }
                }
            }
        }

        private void DrawCoins(List<Coin> coins)
        {
            // Draw all visible coins in the current list
            foreach (var coin in coins)
            {
                coin.Draw(_spriteBatch);
            }
        }

        private void DrawLevel(GameTime gameTime)
        {
            for (int y = 0; y < _level1Data.GetLength(0); y++)
            {
                for (int x = 0; x < _level1Data.GetLength(1); x++)
                {
                    TileType tileType = _level1Data[y, x];

                    // Only draw non-empty tiles
                    if (tileType != TileType.Empty)
                    {
                        int tileIndex = (int)tileType;

                        int tilesetColumns = _tileset.Width / _tileWidth;
                        int sourceX = (tileIndex % tilesetColumns) * _tileWidth;
                        int sourceY = (tileIndex / tilesetColumns) * _tileHeight;

                        Rectangle sourceRectangle = new Rectangle(sourceX, sourceY, _tileWidth, _tileHeight);
                        Vector2 position = new Vector2(x * _tileWidth, y * _tileHeight);

                        _spriteBatch.Draw(_tileset, position, sourceRectangle, Color.White);
                    }
                }
            }
        }

        private void DrawLevel2(GameTime gameTime)
        {
            // Draw all the tiles and objects for level 2
            // Similar to what you do in level 1's draw method
            for (int y = 0; y < _level2Data.GetLength(0); y++)
            {
                for (int x = 0; x < _level2Data.GetLength(1); x++)
                {
                    TileType tileType = _level2Data[y, x];

                    // Only draw non-empty tiles
                    if (tileType != TileType.Empty)
                    {
                        int tileIndex = (int)tileType;

                        int tilesetColumns = _tileset.Width / _tileWidth;
                        int sourceX = (tileIndex % tilesetColumns) * _tileWidth;
                        int sourceY = (tileIndex / tilesetColumns) * _tileHeight;

                        Rectangle sourceRectangle = new Rectangle(sourceX, sourceY, _tileWidth, _tileHeight);
                        Vector2 position = new Vector2(x * _tileWidth, y * _tileHeight);

                        _spriteBatch.Draw(_tileset, position, sourceRectangle, Color.White);
                    }
                }
            }
        }

        private void DrawPlayer()
        {
            Rectangle sourceRectangle;

            if (_isAttacking)
            {
                // Use the attack animation frames (third row)
                sourceRectangle = new Rectangle(_frame * _frameWidth, _frameHeight * 2, _frameWidth, _frameHeight); // Attack animation
            }
            else if (_isJumping)
            {
                // Use the same animation frame during jump if moving
                if (_velocity.X != 0)
                {
                    sourceRectangle = new Rectangle(_frame * _frameWidth, _frameHeight, _frameWidth, _frameHeight); // Movement animation
                }
                else
                {
                    sourceRectangle = new Rectangle(0, 0, _frameWidth, _frameHeight); // Default frame or idle frame during jump
                }
            }
            else
            {
                // Calculate the source rectangle for the current frame in the movement row (index 1)
                sourceRectangle = new Rectangle(_frame * _frameWidth, _frameHeight, _frameWidth, _frameHeight);
            }

            // Draw the current frame
            _spriteBatch.Draw(_spriteSheet, _position, sourceRectangle, Color.White);

            // Set the data for the rectangle hitbox of the player
            Texture2D rect = new Texture2D(GraphicsDevice, 1, 1);
            rect.SetData(new[] { Color.Red });
            // Draw the hitbox of the player
            _spriteBatch.Draw(rect, _playerHitbox, Color.White * 0.5f);
        }

        private void DrawPurplePortal()
        {
            // Draw the PurplePortal sprite
            _spriteBatch.Draw(_purplePortalSpriteSheet, _purplePortalPosition, _purplePortalFrames[_purplePortalCurrentFrame], Color.White);

            // Draw the hitbox for the portal in red
            _spriteBatch.Draw(_hitboxPurplePortalTexture, _purplePortalHitbox, Color.Purple * 0.5f); // semi-transparent red
        }

        private void DrawGreenPortal()
        {
            // Draw the GreenPortal sprite
            _spriteBatch.Draw(_greenPortalSpriteSheet, _greenPortalPosition, _greenPortalFrames[_greenPortalCurrentFrame], Color.White);

            // Draw the hitbox for the portal in red
            _spriteBatch.Draw(_hitboxGreenPortalTexture, _greenPortalHitbox, Color.Green * 0.5f); // semi-transparent red
        }

        private Texture2D CreateRectangleTexture(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; ++i) data[i] = color;
            texture.SetData(data);
            return texture;
        }

        private void AnimateAttack(GameTime gameTime)
        {
            _timeSinceLastFrame += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_timeSinceLastFrame >= _millisecondsPerFrame)
            {
                _frame++;
                if (_frame > 6) // There are 7 frames in the attack row
                {
                    _frame = 0;
                    _isAttacking = false; // End attack animation
                }

                _timeSinceLastFrame = 0;
            }
        }
        
        private void AnimateMovement(GameTime gameTime)
        {
            _timeSinceLastFrame += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_timeSinceLastFrame >= _millisecondsPerFrame)
            {
                _frame++;
                if (_frame > 7) // There are 8 frames in the movement row
                    _frame = 0;

                _timeSinceLastFrame = 0;
            }
        }

        private void UpdatePurplePortalAnimation(GameTime gameTime)
        {
            // Update the PurplePortal animation
            _purplePortalTimeElapsed += gameTime.ElapsedGameTime.TotalSeconds;

            if (_purplePortalTimeElapsed >= _purplePortalTimeToUpdate)
            {
                _purplePortalTimeElapsed -= _purplePortalTimeToUpdate;

                _purplePortalCurrentFrame++;
                if (_purplePortalCurrentFrame >= _purplePortalFrames.Length)
                {
                    _purplePortalCurrentFrame = 0;
                }
            }
        }

        private void UpdateGreenPortalAnimation(GameTime gameTime)
        {
            // Update the PurplePortal animation
            _greenPortalTimeElapsed += gameTime.ElapsedGameTime.TotalSeconds;

            if (_greenPortalTimeElapsed >= _greenPortalTimeToUpdate)
            {
                _greenPortalTimeElapsed -= _greenPortalTimeToUpdate;

                _greenPortalCurrentFrame++;
                if (_greenPortalCurrentFrame >= _greenPortalFrames.Length)
                {
                    _greenPortalCurrentFrame = 0;
                }
            }
        }

        private void InitializeCollisionMasks()
        {
            /*
            _collisionMasks = new Dictionary<CollisionType, List<Rectangle>>()
            {
                // Full tile - covers the entire tile area
                {
                    CollisionType.Full, new List<Rectangle>
                    {
                        new Rectangle(0, 0, _tileWidth, _tileHeight)
                    }
                },

                // Empty tile - no collision
                {
                    CollisionType.Empty, new List<Rectangle>()
                    // No rectangles, meaning no collision area
                },

                // Top half filled - collision only on the top half of the tile
                {
                    CollisionType.HalfFilledTop, new List<Rectangle>
                    {
                        new Rectangle(0, 0, _tileWidth, _tileHeight / 2)
                    }
                },

                // Bottom half filled - collision only on the bottom half of the tile
                {
                    CollisionType.HalfFilledBottom, new List<Rectangle>
                    {
                        new Rectangle(0, _tileHeight / 2, _tileWidth, _tileHeight / 2)
                    }
                },

                // Diagonal from bottom-left to top-right - approximated with two triangles
                {
                    CollisionType.DiagonalLeftBottom, new List<Rectangle>
                    {
                        // Bottom-left triangle, approximated with a rectangular mask
                        new Rectangle(0, _tileHeight / 2, _tileWidth / 2, _tileHeight / 2),
                        // Top-right triangle, approximated with a rectangular mask
                        new Rectangle(_tileWidth / 2, 0, _tileWidth / 2, _tileHeight / 2)
                    }
                },

                // Diagonal from bottom-right to top-left - approximated with two triangles
                {
                    CollisionType.DiagonalRightBottom, new List<Rectangle>
                    {
                        // Bottom-right triangle, approximated with a rectangular mask
                        new Rectangle(_tileWidth / 2, _tileHeight / 2, _tileWidth / 2, _tileHeight / 2),
                        // Top-left triangle, approximated with a rectangular mask
                        new Rectangle(0, 0, _tileWidth / 2, _tileHeight / 2)
                    }
                }
            };
            */

            int tileWidth = 32;  // Assuming each tile is 32x32 pixels
            int tileHeight = 32;

            _collisionMasks[CollisionType.Empty] = new List<Rectangle>(); // No collision

            _collisionMasks[CollisionType.Full] = new List<Rectangle>
    {
        new Rectangle(0, 0, tileWidth, tileHeight) // Full tile collision
    };

            _collisionMasks[CollisionType.HalfFilledTop] = new List<Rectangle>
    {
        new Rectangle(0, 0, tileWidth, tileHeight / 2) // Top half collision
    };

            _collisionMasks[CollisionType.HalfFilledBottom] = new List<Rectangle>
    {
        new Rectangle(0, tileHeight / 2, tileWidth, tileHeight / 2) // Bottom half collision
    };

            _collisionMasks[CollisionType.DiagonalLeftBottom] = new List<Rectangle>
    {
        new Rectangle(0, tileHeight / 2, tileWidth / 2, tileHeight / 2),  // Left bottom collision (half diagonal)
        new Rectangle(tileWidth / 2, tileHeight / 2, tileWidth / 2, tileHeight / 2) // Extend to cover entire bottom
    };

            _collisionMasks[CollisionType.DiagonalRightBottom] = new List<Rectangle>
    {
        new Rectangle(tileWidth / 2, tileHeight / 2, tileWidth / 2, tileHeight / 2), // Right bottom collision (half diagonal)
        new Rectangle(0, tileHeight / 2, tileWidth / 2, tileHeight / 2) // Extend to cover entire bottom
    };
        }

        private bool CheckTileCollision(Rectangle playerRect, Vector2 tilePosition, TileType tileType)
        {
            if (_tileCollisions.TryGetValue(tileType, out CollisionType collisionType))
            {
                if (_collisionMasks.TryGetValue(collisionType, out List<Rectangle> masks))
                {
                    foreach (var mask in masks)
                    {
                        Rectangle tileCollisionRect = new Rectangle(
                            (int)tilePosition.X + mask.X,
                            (int)tilePosition.Y + mask.Y,
                            mask.Width,
                            mask.Height
                        );

                        if (playerRect.Intersects(tileCollisionRect))
                        {
                            return true; // Collision detected
                        }
                    }
                }
            }

            return false; // No collision
        }

        private void HandleCollision(GameTime gameTime)
        {
            // Define the player's hitbox based on its position and size
            Rectangle playerRect = new Rectangle((int)_position.X, (int)_position.Y, _frameWidth, _frameHeight);

            // Store the player's position before collision handling
            Vector2 newPlayerPosition = _position;

            // Reset grounded state
            bool wasGrounded = _isGrounded;  // Store the previous grounded state
            _isGrounded = false;

            // Loop through the 2D array of tiles
            for (int y = 0; y < 15; y++)
            {
                for (int x = 0; x < 50; x++)
                {
                    TileType tileType = _currentLevelData[y, x];

                    if (tileType != TileType.Empty) // Skip empty tiles
                    {
                        // Get the corresponding CollisionType for this tile
                        if (_tileCollisions.TryGetValue(tileType, out CollisionType collisionType))
                        {
                            // Calculate the tile's position based on its indices in the 2D array
                            Vector2 tilePosition = new Vector2(x * _tileWidth, y * _tileHeight);

                            // Get the list of rectangles that define the collision masks for this tile
                            List<Rectangle> collisionMasks = _collisionMasks[collisionType];

                            // Check each collision mask against the player's rectangle
                            foreach (var mask in collisionMasks)
                            {
                                // Calculate the actual position of the collision mask
                                Rectangle tileRect = new Rectangle(
                                    (int)tilePosition.X + mask.X,
                                    (int)tilePosition.Y + mask.Y,
                                    mask.Width,
                                    mask.Height);

                                // Check if the player's rectangle intersects with the tile's collision mask
                                if (playerRect.Intersects(tileRect))
                                {
                                    // Get the intersection depth of the collision
                                    Vector2 depth = GetIntersectionDepth(playerRect, tileRect);

                                    // Resolve collision on the Y axis first (vertical collision handling)
                                    if (Math.Abs(depth.Y) < Math.Abs(depth.X))
                                    {
                                        if (depth.Y < 0) // Player is moving down (colliding from above)
                                        {
                                            newPlayerPosition.Y += depth.Y;
                                            _isGrounded = true; // Player is grounded
                                            _velocity.Y = 0;
                                        }
                                        else if (depth.Y > 0) // Player is moving up (colliding from below)
                                        {
                                            newPlayerPosition.Y += depth.Y;
                                            _velocity.Y = 0;
                                        }
                                    }
                                    else
                                    {
                                        // Handle horizontal collision (X axis)
                                        newPlayerPosition.X += depth.X;
                                        _velocity.X = 0;
                                    }

                                    // Update the player rectangle to the new position after collision
                                    playerRect = new Rectangle((int)newPlayerPosition.X, (int)newPlayerPosition.Y, _frameWidth, _frameHeight);
                                }
                            }
                        }
                    }
                }
            }

            // Apply the new position after resolving all collisions
            _position = newPlayerPosition;

            // Check if the player was grounded before and is still grounded after resolving collisions
            if (wasGrounded && !_isGrounded)
            {
                _velocity.Y += _gravity * (float)gameTime.ElapsedGameTime.TotalSeconds; // Apply gravity if player is no longer grounded
            }

            // Handle jump input (if the jump key is pressed and the player is grounded)
            if (_isGrounded && _isJumping)
            {
                _velocity.Y += _gravity * (float)gameTime.ElapsedGameTime.TotalSeconds; // Apply jump force
                _isGrounded = false; // The player is no longer grounded after jumping
            }

            // Update the player's position based on velocity
            _position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Apply gravity if the player is not grounded
            if (!_isGrounded)
            {
                _velocity.Y += _gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        private Vector2 GetIntersectionDepth(Rectangle rectA, Rectangle rectB)
        {
            // Calculate half sizes
            float halfWidthA = rectA.Width / 2.0f;
            float halfHeightA = rectA.Height / 2.0f;
            float halfWidthB = rectB.Width / 2.0f;
            float halfHeightB = rectB.Height / 2.0f;

            // Calculate centers
            Vector2 centerA = new Vector2(rectA.Left + halfWidthA, rectA.Top + halfHeightA);
            Vector2 centerB = new Vector2(rectB.Left + halfWidthB, rectB.Top + halfHeightB);

            // Calculate current and minimum non-intersecting distances between centers
            float distanceX = centerA.X - centerB.X;
            float distanceY = centerA.Y - centerB.Y;
            float minDistanceX = halfWidthA + halfWidthB;
            float minDistanceY = halfHeightA + halfHeightB;

            // Calculate and return intersection depths
            float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
            float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;

            return new Vector2(depthX, depthY);
        }

        private Rectangle? CollidesWithTerrain(Rectangle hitbox)
        {
            for (int y = 0; y < _currentLevelData.GetLength(0); y++)
            {
                for (int x = 0; x < _currentLevelData.GetLength(1); x++)
                {
                    TileType tileType = _currentLevelData[y, x];
                    // Only draw non-empty tiles
                    if (tileType != TileType.Empty)
                    {
                        Vector2 position = new Vector2(x * _tileWidth, y * _tileHeight);
                        Rectangle ourHitBox = new Rectangle((int)position.X, (int)position.Y, _tileWidth, _tileHeight);
                        if (ourHitBox.Intersects(hitbox))
                        {
                            Debug.WriteLine("---( hitbox of player )---");
                            Debug.WriteLine(hitbox);
                            Debug.WriteLine("---( hitbox of tile )---");
                            Debug.WriteLine(ourHitBox);
                            Debug.WriteLine("---( tile type )---");
                            Debug.WriteLine((tileType));
                            return ourHitBox;
                        };
                    }
                }
            }
            return null;
        }

        private void LoadLevel1()
        {
            // Set the flag to indicate level 1 is active
            isLevel2Active = false;
            _currentLevel = 1;

            // Load the level 1 data
            // This could be similar to how you load the first level
            _currentLevelData = _level1Data;

            // Reset the player's position to the start position in level 2
            _position = new Vector2(-40, 350); // Coordinates for level 2
        }

        private void LoadLevel2()
        {
            // Set the flag to indicate level 2 is active
            isLevel2Active = true;
            _currentLevel = 2;

            // Load the level 2 data
            // This could be similar to how you load the first level
            _currentLevelData = _level2Data;

            // Reset the player's position to the start position in level 2
            _position = new Vector2(-40, 350); // Coordinates for level 2
        }

        private void UpdateLevel1(GameTime gameTime)
        {
            // Handle input, collisions, and any specific logic for level 2
            // Similar to what you have for level 1, but tailored to level 2's layout and rules
        }

        private void UpdateLevel2(GameTime gameTime)
        {
            // Handle input, collisions, and any specific logic for level 2
            // Similar to what you have for level 1, but tailored to level 2's layout and rules
        }

    }
}