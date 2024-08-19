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
using DamnedOfTheDeath.Core.Collectibles;
using DamnedOfTheDeath.Core.enemies;
using DamnedOfTheDeath.Core.entities;

namespace DamnedOfTheDeath
{
    public class Game1 : Game
    {
        #region Variables
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private PurplePortal _purplePortal;
        private GreenPortal _greenPortal;
        private Texture2D _fireTrapTexture;
        private List<FireTrap> _fireTrapsLevel1;
        private List<FireTrap> _fireTrapsLevel2;

        #region CoinVariables
        List<Coin> _coinsLevel1;  // List to hold multiple coins
        List<Coin> _coinsLevel2;  // List to hold multiple coins
        int _score;
        const int MaxScore = 10;
        #endregion

        #region AlienVariables
        Texture2D alienSpritesheet;
        Alien alienLevel1;
        Alien alienLevel2;
        #endregion

        #region DemonVariables
        Texture2D demonSpritesheet;
        Demon demonLevel1;
        Demon demonLevel2;
        #endregion

        #region UI
        ScoreOverlay _scoreOverlay;
        WinScreen _winScreen;
        StartScreen _startScreen;
        HealthOverlay _healthOverlay;
        GameOverScreen _gameOverScreen;

        bool _gameWon;
        bool _gameLost;
        private bool _gameStarted = false;

        private Texture2D _background;
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
        private int _playerHealth = 3;
        private int _minPlayerHealth = 0;
        private bool _isInvulnerable = false;
        private double _invulnerabilityTime;
        private double _flickerInterval = 0.1; // Interval between visibility toggles
        private double _flickerTime = 0;
        private bool _isVisible = true;
        bool canJump = true; // To allow the player to jump only when grounded
        // Level 1
        List<Rectangle> groundCollisionsLevel1; // List to store multiple ground rectangles
        // Level 2
        List<Rectangle> groundCollisionsLevel2; // List to store multiple ground rectangles
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
        private Texture2D _levelsBackground;
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
            _playerHealth = 3;
            _gameWon = false;
            _gameLost = false;
            // Set the initial position of the sprite
            _position = new Vector2(-40,350);
            _velocity = Vector2.Zero;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the PurplePortal sprite sheet
            var purplePortalSpriteSheet = Content.Load<Texture2D>("Purple Portal Sprite Sheet");
            var purplePortalFrames = CreatePortalFrames(purplePortalSpriteSheet, 8, 64, 64);
            var hitboxTexture = CreateHitboxTexture();

            // Create PurplePortal instance
            _purplePortal = new PurplePortal(purplePortalSpriteSheet, purplePortalFrames, new Vector2(1530, 370), 0.1, _spriteBatch, hitboxTexture);

            // Load the GreenPortal sprite sheet
            var greenPortalSpriteSheet = Content.Load<Texture2D>("Green Portal Sprite Sheet");
            var greenPortalFrames = CreatePortalFrames(greenPortalSpriteSheet, 8, 64, 64);

            // Create GreenPortal instance
            _greenPortal = new GreenPortal(greenPortalSpriteSheet, greenPortalFrames, new Vector2(1530, 300), 0.1, _spriteBatch, hitboxTexture);

            _fireTrapTexture = Content.Load<Texture2D>("FireTrap");

            // Initialize fire traps for level 1
            _fireTrapsLevel1 = new List<FireTrap>
        {
            new FireTrap(_fireTrapTexture, 32, 41, 14, 0.07f) { Position = new Vector2(540, 350) }
        };

            // Initialize fire traps for level 2
            _fireTrapsLevel2 = new List<FireTrap>
        {
            new FireTrap(_fireTrapTexture, 32, 41, 14, 0.07f) { Position = new Vector2(1035, 320) }
        };

            // Load the start screen background
            _background = Content.Load<Texture2D>("DawnBackground");

            #region Player
            // Load the player's sprite sheet
            _spriteSheet = Content.Load<Texture2D>("GoblinMechRiderSpriteSheet");
            #endregion

            #region groundCollisionsLevel1
            // Level 1
            // Initialize the list of ground collisions
            groundCollisionsLevel1 = new List<Rectangle>
            {
                new Rectangle(320, 310, 65, 20),
                new Rectangle(550, 340, 25, 100),
                new Rectangle(750, 340, 20, 100),
                new Rectangle(930, 240, 320, 20),
            };
            #endregion

            #region groundCollisionsLevel2
            // Level 2
            // Initialize the list of ground collisions
            groundCollisionsLevel2 = new List<Rectangle>
            {
                new Rectangle(720, 330, 10, 100),
                new Rectangle(800, 210, 120, 20),
                new Rectangle(1060, 300, 190, 20),
            };
            #endregion

            #region Alien
            alienSpritesheet = Content.Load<Texture2D>("AlienSpritesheet");

            // Define the path for each level
            Vector2[] alienPathLevel1 = new Vector2[]
            {
            new Vector2(950f, 250f),
            new Vector2(1150f, 250f)
            };

            Vector2[] alienPathLevel2 = new Vector2[]
            {
            new Vector2(1200f, 390f),
            new Vector2(1400f, 390f)
            };

            // Initialize the alien animation for each level with paths and speed
            alienLevel1 = new Alien(alienSpritesheet, alienPathLevel1, 100f);
            alienLevel2 = new Alien(alienSpritesheet, alienPathLevel2, 100f);
            #endregion

            #region Demon
            demonSpritesheet = Content.Load<Texture2D>("DemonSpritesheet");

            // Define the path for each level
            Vector2[] pathLevel1 = new Vector2[]
            {
            new Vector2(10f, 200f),
            new Vector2(100, 270f),
            new Vector2(150, 200f)
            };

            Vector2[] pathLevel2 = new Vector2[]
            {
            new Vector2(250f, 200f),
            new Vector2(400f, 270),
            new Vector2(500f, 200)
            };

            // Initialize the demon animation for each level with paths and speed
            demonLevel1 = new Demon(demonSpritesheet, pathLevel1[0], pathLevel1, 100f);
            demonLevel2 = new Demon(demonSpritesheet, pathLevel2[0], pathLevel2, 100f);
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

            // Load the font and button texture
            Texture2D buttonTexture = Content.Load<Texture2D>("Hover@3x");

            SpriteFont titleFont = Content.Load<SpriteFont>("TitleFont");

            #region ScoreOverlay
            SpriteFont scoreFont = Content.Load<SpriteFont>("ScoreFont");

            _scoreOverlay = new ScoreOverlay(scoreFont, new Vector2(10, 10));  // Initialize UI screen with font and position
            #endregion

            #region HealthOverlay
            _healthOverlay = new HealthOverlay(scoreFont, new Vector2(1500, 10));  // Initialize UI screen with font and position
            #endregion

            #region WinScreen
            SpriteFont font = Content.Load<SpriteFont>("Font");

            _winScreen = new WinScreen(font, titleFont, buttonTexture, GraphicsDevice, _background);     // Initialize the win screen
            #endregion

            #region GameOverScreen
            _gameOverScreen = new GameOverScreen(font, titleFont, buttonTexture, GraphicsDevice, _background);     // Initialize the game over screen
            #endregion

            #region StartScreen
            // Initialize the start screen
            _startScreen = new StartScreen(font, titleFont, buttonTexture, _background);

            buttonTexture = CreateButtonTexture(120, 50, Color.Gray);
            #endregion

            #region Tileset
            // Load the tileset texture
            _tileset = Content.Load<Texture2D>("Tiles");
            #endregion

            #region Background
            // Load the background texture for both levels
            _levelsBackground = Content.Load<Texture2D>("DawnBackground");
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
                { TileType.Empty,                           TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                                 TileType.GrassFloorPillar_Tree_Top,     TileType.GrassFLoorPillar_Pillar_Top,   TileType.Empty,                             TileType.Empty,                                 TileType.Empty,                         TileType.Empty,                         TileType.StoneTopFloor_BottomLeft,          TileType.StoneTopFloor_BottomMid,       TileType.StoneTopFloor_Bottomright,         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.StoneTopFloor_BottomLeft,          TileType.StoneTopFloor_BottomMid,       TileType.StoneTopFloor_Bottomright,         TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                          TileType.Empty,                                     TileType.Empty,                                         TileType.GrassFloorPillar_Tree_Top,     TileType.BrokenPillar_Bottom,               TileType.Empty,                             TileType.Empty,                            TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty },
                { TileType.GrassFloor_TopLeft,              TileType.GrassFloor_TopMid,                 TileType.GrassFloor_TopRight,           TileType.GrassFloor_TopLeft,            TileType.GrassFloor_TopMid,             TileType.GrassFloor_TopRight,               TileType.GrassFloor_TopLeft,            TileType.GrassFloor_TopMid,                 TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,   TileType.GrassFloorPillar_Tree_Mid,     TileType.GrassFloorPillar_Pillar_Mid,   TileType.GrassFloorPillar_GrassRoad_Top,    TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.StoneTopFloor_TopMid,          TileType.StoneTopFloor_TopMid,          TileType.StoneLeftBottomFloor_TopRight,     TileType.StoneTopFloor_Wall1,           TileType.StoneRightBottomFloor_TopLeft,     TileType.StoneTopFloor_TopMid,          TileType.StoneTopFloor_TopMid,          TileType.StoneTopFloor_TopMid,          TileType.StoneLeftBottomFloor_TopRight,     TileType.StoneTopFloor_Wall1,           TileType.StoneRightBottomFloor_TopLeft,     TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,   TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,   TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,   TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,   TileType.GrassFloorPillar_StoneRoad_TopLeft,        TileType.GrassFloorPillar_StoneRoad_TopRight,            TileType.GrassFloorPillar_StoneRoad_TopLeft,        TileType.GrassFloorPillar_StoneRoad_TopRight,           TileType.GrassFloorPillar_Tree_Mid,     TileType.GrassFloorPillar_Pillar_Mid,       TileType.GrassFloorPillar_GrassRoad_Top,    TileType.GrassFloor_TopMid,                TileType.GrassFloor_TopRight,           TileType.GrassFloor_TopLeft,            TileType.GrassFloor_TopMid,             TileType.GrassFloor_TopRight,           TileType.GrassFloor_TopLeft,            TileType.GrassFloor_TopMid,             TileType.GrassFloor_TopRight,           TileType.GrassFloor_TopRight,           TileType.GrassFloor_TopLeft },
                { TileType.GrassFloor_BottomLeft,           TileType.GrassFloor_BottomMid,              TileType.GrassFloor_BottomRight,        TileType.GrassFloor_BottomLeft,         TileType.GrassFloor_BottomMid,          TileType.GrassFloor_BottomRight,            TileType.GrassFloor_BottomLeft,         TileType.GrassFloor_BottomMid,              TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,TileType.GrassFloorPillar_Tree_Bottom,  TileType.GrassFloorPillar_Pillar_Bottom,TileType.GrassFloorPillar_GrassRoad_Bottom, TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.StoneTopFloor_BottomMid,       TileType.StoneTopFloor_BottomMid,       TileType.StoneLeftBottomFloor_BottomRight,  TileType.StoneTopFloor_Wall2,           TileType.StoneRightBottomFloor_BottomLeft,  TileType.StoneTopFloor_BottomMid,       TileType.StoneTopFloor_BottomMid,       TileType.StoneTopFloor_BottomMid,       TileType.StoneLeftBottomFloor_BottomRight,  TileType.StoneTopFloor_Wall2,           TileType.StoneRightBottomFloor_BottomLeft,  TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,TileType.GrassFloorPillar_StoneRoad_BottomRight,TileType.GrassFloorPillar_StoneRoad_BottomRight,TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,TileType.GrassFloorPillar_StoneRoad_BottomLeft,     TileType.GrassFloorPillar_StoneRoad_BottomRight,         TileType.GrassFloorPillar_StoneRoad_BottomLeft,     TileType.GrassFloorPillar_StoneRoad_BottomRight,        TileType.GrassFloorPillar_Tree_Bottom,  TileType.GrassFloorPillar_Pillar_Bottom,    TileType.GrassFloorPillar_GrassRoad_Bottom, TileType.GrassFloor_BottomMid,             TileType.GrassFloor_BottomRight,        TileType.GrassFloor_BottomLeft,         TileType.GrassFloor_BottomMid,          TileType.GrassFloor_BottomRight,        TileType.GrassFloor_BottomLeft,         TileType.GrassFloor_BottomMid,          TileType.GrassFloor_BottomRight,        TileType.GrassFloor_BottomRight,        TileType.GrassFloor_BottomLeft },
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
                { TileType.Empty,                                   TileType.Empty,                                     TileType.Empty,                         TileType.Fence_BottomRight,                 TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.StoneTopFloor_TopLeft,         TileType.StoneTopFloor_TopMid,              TileType.StoneTopFloor_TopRight,            TileType.Empty,                         TileType.Empty,                         TileType.GrassFLoorPillar_Pillar_Top,       TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                         TileType.Empty,                                 TileType.Empty,                                     TileType.StoneBridge_BottomLeft,        TileType.StoneBridge_MidBottomRight,        TileType.StoneRamp_MidRight,                TileType.StoneRamp_MidLeft,             TileType.StoneRamp_MidRight,            TileType.StoneRamp_MidLeft,             TileType.StoneRamp_MidMid,                      TileType.StoneRamp_MidRight,                        TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty },
                { TileType.Empty,                                   TileType.Empty,                                     TileType.GrassFloorPillar_Tree_Top,     TileType.BrokenPillar_Bottom,               TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                         TileType.Empty,                             TileType.StoneTopFloor_BottomLeft,      TileType.StoneTopFloor_BottomMid,           TileType.StoneTopFloor_Bottomright,         TileType.Empty,                         TileType.GrassFloorPillar_Tree_Top,     TileType.GrassFLoorPillar_Pillar_Top,       TileType.Empty,                                 TileType.Empty,                                     TileType.GrassFloorPillar_Tree_Top,     TileType.Empty,                                 TileType.Empty,                                     TileType.GrassFloorPillar_Tree_Top,     TileType.GrassFLoorPillar_Pillar_Top,       TileType.StoneRamp_BottomRight,             TileType.StoneRamp_BottomLeft,          TileType.StoneRamp_BottomRight,         TileType.StoneRamp_BottomLeft,          TileType.Empty,                                 TileType.StoneRamp_BottomRight,                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty,                                     TileType.Empty,                                 TileType.Empty },
                { TileType.GrassFloorPillar_StoneRoad_TopLeft,      TileType.GrassFloorPillar_StoneRoad_TopRight,       TileType.GrassFloorPillar_Tree_Mid,     TileType.GrassFloorPillar_Pillar_Mid,       TileType.GrassFloorPillar_GrassRoad_Top,    TileType.GrassFloor_TopLeft,            TileType.GrassFloor_TopMid,             TileType.GrassFloor_TopRight,           TileType.GrassFloor_TopLeft,            TileType.GrassFloor_TopMid,             TileType.GrassFloor_TopRight,           TileType.GrassFloor_TopLeft,            TileType.GrassFloor_TopMid,             TileType.GrassFloor_TopRight,               TileType.GrassFloor_TopLeft,            TileType.GrassFloor_TopMid,             TileType.GrassFloor_TopRight,           TileType.GrassFloor_TopLeft,            TileType.GrassFloor_TopMid,             TileType.GrassFloor_TopRight,           TileType.GrassFloor_TopMid,                 TileType.StoneLeftBottomFloor_TopRight, TileType.StoneTopFloor_Wall1,               TileType.StoneRightBottomFloor_TopLeft,     TileType.StoneTopFloor_TopMid,          TileType.GrassFloorPillar_Tree_Mid,     TileType.GrassFloorPillar_Pillar_Mid,       TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,       TileType.GrassFloorPillar_Tree_Mid,     TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,       TileType.GrassFloorPillar_Tree_Mid,     TileType.GrassFloorPillar_Pillar_Mid,       TileType.GrassFloorPillar_GrassRoad_Top,    TileType.GrassFloor_TopLeft,            TileType.GrassFloor_TopMid,             TileType.GrassFloor_TopRight,           TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,       TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,       TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,       TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,       TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight,       TileType.GrassFloorPillar_StoneRoad_TopLeft,    TileType.GrassFloorPillar_StoneRoad_TopRight },
                { TileType.GrassFloorPillar_StoneRoad_BottomLeft,   TileType.GrassFloorPillar_StoneRoad_BottomRight,    TileType.GrassFloorPillar_Tree_Bottom,  TileType.GrassFloorPillar_Pillar_Bottom,    TileType.GrassFloorPillar_GrassRoad_Bottom, TileType.GrassFloor_BottomLeft,         TileType.GrassFloor_BottomMid,          TileType.GrassFloor_BottomRight,        TileType.GrassFloor_BottomLeft,         TileType.GrassFloor_BottomMid,          TileType.GrassFloor_BottomRight,        TileType.GrassFloor_BottomLeft,         TileType.GrassFloor_BottomMid,          TileType.GrassFloor_BottomRight,            TileType.GrassFloor_BottomLeft,         TileType.GrassFloor_BottomMid,          TileType.GrassFloor_BottomRight,        TileType.GrassFloor_BottomLeft,         TileType.GrassFloor_BottomMid,          TileType.GrassFloor_BottomRight,        TileType.GrassFloor_BottomMid,              TileType.StoneTopFloor_BottomMid,       TileType.StoneTopFloor_Wall1,               TileType.StoneRightBottomFloor_BottomLeft,  TileType.StoneTopFloor_BottomMid,       TileType.GrassFloorPillar_Tree_Bottom,  TileType.GrassFloorPillar_Pillar_Bottom,    TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,    TileType.GrassFloorPillar_Tree_Bottom,  TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,    TileType.GrassFloorPillar_Tree_Bottom,  TileType.GrassFloorPillar_Pillar_Bottom,    TileType.GrassFloorPillar_GrassRoad_Bottom, TileType.GrassFloor_BottomLeft,         TileType.GrassFloor_BottomMid,          TileType.GrassFloor_BottomRight,        TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,    TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,    TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,    TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,    TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight,    TileType.GrassFloorPillar_StoneRoad_BottomLeft, TileType.GrassFloorPillar_StoneRoad_BottomRight },
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
            if (!_gameStarted)
            {
                int startScreenResult = _startScreen.Update(gameTime);

                if (startScreenResult == 1)
                {
                    _currentLevel = 1;
                    isLevel2Active = false;
                    UpdateCoins(_coinsLevel1, gameTime);
                    _gameStarted = true;

                }
                else if (startScreenResult == 2)
                {
                    _currentLevel = 1;
                    isLevel2Active = false;
                    UpdateCoins(_coinsLevel1, gameTime);
                    _gameStarted = true;
                }
                else if (startScreenResult == 3)
                {
                    _currentLevel = 2;
                    isLevel2Active = true;
                    UpdateCoins(_coinsLevel2, gameTime);
                    _gameStarted = true;
                }
            }
            // Game Started
            else
            {
                #region IsKeyDown
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                    Exit();
                #endregion
                if (!_gameWon && !_gameLost)
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

                    #region UpdatePlayerHitbox
                    // Update the player hitbox position
                    _playerHitbox.X = (int)_position.X + (_frameWidth / 4) + 5;
                    _playerHitbox.Y = (int)_position.Y + _frameHeight / 2;

                    #endregion

                    #region Boundaries
                    // Define the margin for the left and right boundaries
                    int leftMargin = -50;  // Allow some space off the left side
                    int rightMargin = 50;  // Allow some space off the right side

                    // Ensure the player stays within the screen bounds with the added margins
                    _position.X = MathHelper.Clamp(_position.X, leftMargin, GraphicsDevice.Viewport.Width - _frameWidth + rightMargin);
                    _position.Y = MathHelper.Clamp(_position.Y, 0, GraphicsDevice.Viewport.Height - _frameHeight);
                    #endregion

                    #region groundCollisionsLevel1
                    if (_currentLevel == 1)
                    {
                        // Check for collisions with each ground rectangle
                        foreach (var groundCollision in groundCollisionsLevel1)
                        {
                            _isGrounded = false;
                            canJump = false; // Allow jumping again
                            _isJumping = true;

                            if (_playerHitbox.Intersects(groundCollision))
                            {
                                // Ensure player stays on top of the ground
                                _position.Y = groundCollision.Top - _playerHitbox.Height;
                                _velocity.Y = 0; // Stop falling
                                _isGrounded = true;
                                canJump = true; // Allow jumping again
                                _isJumping = false;
                                break; // Exit loop after detecting ground collision
                            }
                        }
                    }
                    #endregion

                    #region groundCollisionLevel2
                    if (_currentLevel == 2)
                    {
                        // Check for collisions with each ground rectangle
                        foreach (var groundCollision in groundCollisionsLevel2)
                        {
                            _isGrounded = false;
                            canJump = false; // Allow jumping again
                            _isJumping = true;

                            if (_playerHitbox.Intersects(groundCollision))
                            {
                                // Ensure player stays on top of the ground
                                _position.Y = groundCollision.Top - _playerHitbox.Height;
                                _velocity.Y = 0; // Stop falling
                                _isGrounded = true;
                                canJump = true; // Allow jumping again
                                _isJumping = false;
                                break; // Exit loop after detecting ground collision
                            }
                        }
                    }
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

                    #region Alien
                    if (_currentLevel == 1)
                    {
                        alienLevel1.Update(gameTime);
                        CheckCollisionAlien(alienLevel1.Hitbox);
                    }
                    else if (_currentLevel == 2)
                    {
                        alienLevel2.Update(gameTime);
                        CheckCollisionAlien(alienLevel2.Hitbox);
                    }
                    #endregion

                    #region Demon
                    if (_currentLevel == 1)
                    {
                        demonLevel1.Update(gameTime);
                        CheckCollision(demonLevel1.Hitbox);
                    }
                    else if (_currentLevel == 2)
                    {
                        demonLevel2.Update(gameTime);
                        CheckCollision(demonLevel2.Hitbox);
                    }
                    #endregion

                    #region HealthOverlay
                    _healthOverlay.UpdateHealth(_playerHealth);  // Update the UI screen with the latest health
                    #endregion

                    #region UpdateHealth
                    if (_playerHealth <= _minPlayerHealth)
                    {
                        _gameLost = true;
                    }
                    #endregion

                    #region FlickeringPlayer
                    double elapsed = gameTime.ElapsedGameTime.TotalSeconds;

                    // Handle invulnerability and flickering
                    if (_isInvulnerable)
                    {
                        _invulnerabilityTime -= elapsed;
                        _flickerTime += elapsed;

                        if (_flickerTime >= _flickerInterval)
                        {
                            _isVisible = !_isVisible; // Toggle visibility
                            _flickerTime = 0;
                        }

                        if (_invulnerabilityTime <= 0)
                        {
                            _isInvulnerable = false;
                            _isVisible = true; // Ensure visibility is reset
                        }
                    }
                    #endregion

                    if (_currentLevel == 1 || !isLevel2Active)
                    {
                        _purplePortal.Update(gameTime);

                        // Check for collision between player and purple portal
                        if (_playerHitbox.Intersects(_purplePortal.GetHitbox()))
                        {
                            _currentLevel = 2;
                            LoadLevel2();
                        }
                    }

                    if (_currentLevel == 2 || isLevel2Active)
                    {
                        _greenPortal.Update(gameTime);

                        // Check for collision between player and green portal
                        if (_playerHitbox.Intersects(_greenPortal.GetHitbox()))
                        {
                            _currentLevel = 1;
                            LoadLevel1();
                        }
                    }

                    List<FireTrap> currentFireTraps = _currentLevel == 1 ? _fireTrapsLevel1 : _fireTrapsLevel2;
                    UpdateFireTraps(currentFireTraps, gameTime);
                }

                #region _gameLost
                if (_gameLost)
                {
                    if (_gameOverScreen.Update(gameTime))
                    {
                        RestartGame();
                        LoadLevel1();
                    }
                }
                #endregion

                #region _gameWon
                else if (_gameWon)
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
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            // Draw the ground collision rectangles (for debugging purposes)
            Texture2D groundTexture = new Texture2D(GraphicsDevice, 1, 1);
            //groundTexture.SetData(new[] { Color.Green });

            if (!_gameStarted)
            {
                _startScreen.Draw(_spriteBatch);
            }
            else if (!_gameWon && !_gameLost)
            {
                // Level 2
                if (_currentLevel == 2 || isLevel2Active == true)
                {
                    _spriteBatch.Draw(_levelsBackground, new Rectangle(0, 0, 1600, 480), Color.White);
                    DrawLevel2(gameTime);
                    _greenPortal.Draw();
                    DrawCoins(_coinsLevel2);
                    alienLevel2.Draw(_spriteBatch);
                    demonLevel2.Draw(_spriteBatch);

                    foreach(var groundcollision in groundCollisionsLevel2)
                    {
                        _spriteBatch.Draw(groundTexture, groundcollision, Color.White * 0.5f);
                    }
                }
                // Level 1
                else if (_currentLevel == 1 || isLevel2Active == false)
                {
                    _spriteBatch.Draw(_levelsBackground, new Rectangle(0, 0, 1600, 480), Color.White);
                    DrawLevel(gameTime);
                    _purplePortal.Draw();
                    DrawCoins(_coinsLevel1);
                    alienLevel1.Draw(_spriteBatch);
                    demonLevel1.Draw(_spriteBatch);

                    foreach (var groundcollision in groundCollisionsLevel1)
                    {
                        _spriteBatch.Draw(groundTexture, groundcollision, Color.White * 0.5f);
                    }
                }

                List<FireTrap> currentFireTraps = _currentLevel == 1 ? _fireTrapsLevel1 : _fireTrapsLevel2;
                DrawFireTraps(currentFireTraps);

                if (_isVisible)
                {
                    DrawPlayer();
                }

                _scoreOverlay.Draw(_spriteBatch);  // Draw the UI screen with the score
                _healthOverlay.Draw(_spriteBatch); // Draw the UI screen with the health
            }
            else if(_gameWon)
            {
                _winScreen.Draw(_spriteBatch);  // Draw the Win Screen
            }
            else if(_gameLost)
            {
                _gameOverScreen.Draw(_spriteBatch); // Draw the Game Over Screen
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private Rectangle[] CreatePortalFrames(Texture2D spriteSheet, int frameCount, int frameWidth, int frameHeight)
        {
            var frames = new Rectangle[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                frames[i] = new Rectangle(i * frameWidth, 0, frameWidth, frameHeight);
            }
            return frames;
        }

        private Texture2D CreateHitboxTexture()
        {
            var texture = new Texture2D(GraphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }

        private void UpdateFireTraps(IEnumerable<FireTrap> fireTraps, GameTime gameTime)
        {
            foreach (var fireTrap in fireTraps)
            {
                fireTrap.Update(gameTime);

                // Check for collision with the player
                if (fireTrap.GetHitbox().Intersects(_playerHitbox) && !_isInvulnerable)
                {
                    _playerHealth -= 1;
                    _isInvulnerable = true;
                    _invulnerabilityTime = 3.0; // 3 seconds of invulnerability
                    _flickerTime = 0;
                    _isVisible = true; // Reset visibility
                }
            }
        }

        private void DrawFireTraps(IEnumerable<FireTrap> fireTraps)
        {
            foreach (var fireTrap in fireTraps)
            {
                fireTrap.Draw(_spriteBatch);
            }
        }

        private void CheckCollision(Rectangle demonHitbox)
        {
            if (demonHitbox.Intersects(_playerHitbox) && !_isInvulnerable)
            {
                _playerHealth -= 1;
                _isInvulnerable = true;
                _invulnerabilityTime = 3.0; // 3 seconds of invulnerability
                _flickerTime = 0;
                _isVisible = true; // Reset visibility
            }
        }

        private void CheckCollisionAlien(Rectangle alienHitbox)
        {
            if (alienHitbox.Intersects(_playerHitbox) && !_isInvulnerable)
            {
                _playerHealth -= 1;
                _isInvulnerable = true;
                _invulnerabilityTime = 3.0; // 3 seconds of invulnerability
                _flickerTime = 0;
                _isVisible = true; // Reset visibility
            }
        }

        private void RestartGame()
        {
            _score = 0;
            _playerHealth = 3;
            _gameWon = false;
            _gameLost = false;
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

        private Texture2D CreateButtonTexture(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; ++i) data[i] = color;
            texture.SetData(data);
            return texture;
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
            //rect.SetData(new[] { Color.Red });
            // Draw the hitbox of the player
            _spriteBatch.Draw(rect, _playerHitbox, Color.White * 0.5f);
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