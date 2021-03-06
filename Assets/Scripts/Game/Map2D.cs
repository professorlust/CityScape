﻿/*
 *       Class: Map2D
 *      Author: Harish Bhagat
 *        Year: 2017
 */

// Website used: https://gamedevelopment.tutsplus.com/tutorials/creating-isometric-worlds-a-primer-for-game-developers--gamedev-6511

using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Random = UnityEngine.Random;

/// <summary>
/// A class to be used for generating and loading 2D cartesian and isometric maps.
/// </summary>
public class Map2D : MonoBehaviour
{
    /// <summary>
    /// Indicates how the map is to be displayed.
    /// </summary>
    public bool Isometric;
    /// <summary>
    /// The size of each tile (x and y) in pixels.
    /// </summary>
    public int TileSizeX;

	/// <summary>
	/// Map size X in tiles.
	/// </summary>
	public int XSize;
	/// <summary>
	/// Map size Y in tiles.
	/// </summary>
	public int YSize;
    /// <summary>
    /// The prefab to be used for generation.
    /// </summary>
    public GameObject Prefab;
    /// <summary>
    /// Container for ground tiles.
    /// </summary>
    public Tile[,] GroundTiles;
    /// <summary>
    /// Container for buildings
    /// </summary>
    public List<Building> Buildings;

	/// <summary>
	/// Container for roads.
	/// </summary>
	public List<Tile> Roads;
	/// <summary>
	/// Container for decorations.
	/// </summary>
	public List<Tile> Decorations;

    /// <summary>
    /// The active state of tile highlighting.
    /// </summary>
    public bool EnableTileHighlighting;
    /// <summary>
    /// The tile highlight colour.
    /// </summary>
    public Color TileHighlightColour;

    private Game _game;
    private TileType _tileToBePlaced;

    // Properties

    /// <summary>
    /// Gets a value indicating whether [destroy mode] is active.
    /// </summary>
    /// <value>
    ///   <c>true</c> if [destroy mode]; otherwise, <c>false</c>.
    /// </value>
    public bool DestroyMode { get; private set; }
    /// <summary>
    /// Gets or sets the current tile.
    /// </summary>
    /// <value>
    /// The current tile.
    /// </value>
    public Tile CurrentTile { get; set; }
    /// <summary>
    /// Gets or sets the last tile clicked.
    /// </summary>
    /// <value>
    /// The last tile clicked.
    /// </value>
    public Tile LastTileClicked { get; set; }

    // Pathfinding variables
    private RoadPathFinder _roadPathFinder;
    private CountdownTimer _timer;

    /// <summary>
    /// Initialises this instance, called before Start().
    /// </summary>
    private void Awake()
    {
        // Set TimeScale | Bug Fix
        Time.timeScale = 1f;

        // Variable initialisation
        GroundTiles = new Tile[XSize, YSize];
        Buildings = new List<Building>();
        Roads = new List<Tile>();
        Decorations = new List<Tile>();

        _roadPathFinder = gameObject.GetComponent<RoadPathFinder>();
        _timer = new CountdownTimer {Seconds = 5f};
        _timer.Begin();

        _game = GameObject.Find("Game").GetComponent<Game>();
    }

    /// <summary>
    /// Initialises this instance.
    /// </summary>
    private void Start()
    {
        // Change depth of parent gameobjects to prvent rendering errors
        Vector3 pos = new Vector3(0f, 0f, -1f);

        GameObject roadsObj = GameObject.Find("Tiles/Roads");
        GameObject decorationObj = GameObject.Find("Tiles/Decoration");

        // Set new positions
        roadsObj.transform.position = pos;
        decorationObj.transform.position = pos;
    }

    /// <summary>
    /// Changes the position of the camera to the centre of the map.
    /// </summary>
    /// <param name="currentCamera">The camera to be positioned.</param>
    public void CentreCameraView(Camera currentCamera)
    {
        Vector3 mapCentre = GroundTiles[GroundTiles.GetLength(0) / 2, GroundTiles.GetLength(1) / 2].transform.position;
        mapCentre.z = -10f;
        currentCamera.transform.position = mapCentre;
    }

    /// <summary>
    /// Generates and returns a list of GroundTiles.
    /// </summary>
    public void Generate()
    {
        // Get pixels per unit
        float ppu = Prefab.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;
        float tileSizeInUnits = TileSizeX / ppu;

        // Create and place each tile
        for (int y = 0; y < YSize; y++)
            for (int x = 0; x < XSize; x++)
            {
                // Calculate position
                Vector3 position;
                float halfUnitTileSize = tileSizeInUnits * 0.5f;

                if (Isometric)
                    position = transform.position +
                               (Vector3)
                               global::Isometric.CartToIso(transform.right * (x * halfUnitTileSize) +
                                                           transform.up * (y * halfUnitTileSize));
                else
                    position = transform.position + transform.right * (x * tileSizeInUnits) +
                               transform.up * (y * tileSizeInUnits);

                // Create tile
                Tile tile = CreateTile(TileType.Grass, position);
                // Make GameObject a child object
                tile.gameObject.transform.parent = GameObject.Find("Tiles/Ground").transform;
                // Add tile to list
                GroundTiles[x, y] = tile;
            }
    }

    /// <summary>
    /// Loads an existing map.
    /// </summary>
    /// <param name="path">The XML file path which contains the world data.</param>
    /// <returns></returns>
    public void Load(string path)
    {
        // Clear existing GroundTiles
        GroundTiles = new Tile[XSize, YSize];

        // Check if the file exists
        if (!File.Exists(path))
            return;

        // Deserialize data
        GameDataContainer tileDataContainer = XMLSerializer.Deserialize<GameDataContainer>(path);

        // Clear parent GameObject
        GameObject groundParentObj = GameObject.Find("Tiles/Ground");
        foreach (Transform child in groundParentObj.transform)
            Destroy(child.gameObject);

        // Create tile
        List<TileData> groundDataList = tileDataContainer.GroundDataList;
        int arrayCounter = 0;

        // Process ground tiles
        for (int y = 0; y < YSize; y++)
            for (int x = 0; x < XSize; x++)
            {
                // Create tile
                TileData tileData = groundDataList[arrayCounter];
                Tile tile = CreateTile(tileData);
                // Load data
                tile.Data = tileData;
                tile.LoadData();
                // Set parent
                tile.gameObject.transform.parent = GameObject.Find("Tiles/Ground").transform;
                // Add tile to array
                GroundTiles[x, y] = tile;
                // Update array counter
                arrayCounter++;
            }

        // Process buildings
        List<BuildingData> buildingDataList = tileDataContainer.BuildingDataList;

        foreach (BuildingData buildingData in buildingDataList)
            SpawnBuilding(buildingData);

        // Process roads
        List<TileData> roadDataList = tileDataContainer.RoadDataList;

        foreach (TileData data in roadDataList)
        {
            Tile tile = CreateTile(data);
            tile.Data = data;
            tile.LoadData();
            // Set parent
            tile.gameObject.transform.parent = GameObject.Find("Tiles/Roads").transform;
            // Add to list
            Roads.Add(tile);
        }

        // Process nodes
        ProcessNodes();

        // Process decorations
        List<TileData> decorationDataList = tileDataContainer.DecorationDataList;

        foreach (TileData data in decorationDataList)
        {
            Tile tile = CreateTile(data);
            tile.Data = data;
            tile.LoadData();
            // Set parent
            tile.gameObject.transform.parent = GameObject.Find("Tiles/Decoration").transform;
            // Add to list
            Decorations.Add(tile);
        }
    }

    /// <summary>
    /// Creates and returns a Tile.
    /// </summary>
    /// <param name="tileType">The type of tile to be created.</param>
    /// <param name="position">The position of the newly created tile.</param>
    /// <returns></returns>
    private Tile CreateTile(TileType tileType, Vector2 position)
    {
        // Get prefab of tileType
        GameObject prefab = Resources.Load<GameObject>(GetTilePrefabPath(tileType) + tileType);

        // Instantiate
        GameObject go = Instantiate(prefab, position, Quaternion.identity);
        // Get tile script
        Tile tile = go.GetComponent<Tile>();

        // Return tile
        return tile;
    }

    /// <summary>
    /// Creates and returns a Tile.
    /// </summary>
    /// <param name="data">A reference to an existing TileData object, that is to be used for the created Tile object.</param>
    /// <returns></returns>
    private Tile CreateTile(TileData data)
    {
        // Create tile
        return CreateTile(data.TileType, new Vector2(data.PosX, data.PosY));
    }

    /// <summary>
    /// Updates this instance.
    /// </summary>
    private void Update()
    {
        // Check gamestate
        if (_game.GameState == GameState.Paused) return;

        // Check for keyboard input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Disable destroy action
            if (DestroyMode)
                ToggleDestroyMode();
        }

        // Spawn traffic
        if (_timer != null)
            if (_timer.IsDone())
            {
                SpawnTraffic();

                // Reset timer
                _timer.ResetClock();
                _timer.Begin();
            }
            else
                _timer.Update();
    }

    /// <summary>
    /// Spawns a random building.
    /// </summary>
    public void SpawnRandomBuilding()
    {
        // Pick a random tile
        Tile randTile;

        while (true)
        {
            // Get a random tile
            randTile = GroundTiles[Random.Range(0, GroundTiles.GetLength(0)), Random.Range(0, GroundTiles.GetLength(1))];

            // Check if the tile is buildable
            Vector3 randTilePosition = randTile.transform.position;

            // Check if the tile is buildable, occupied and whether the map is full
            if (!randTile.Buildable ||
                Buildings.Count != GroundTiles.Length &&
                Buildings.Any(building => building.transform.position == randTilePosition)) continue;

            break;
        }

        // Get tile position
        Vector3 tilePosition = randTile.transform.position;

        // Pick random building
        TileType randTileType = (TileType)Random.Range(0, 3);
        const int level = 1;

        // Spawn building
        SpawnBuilding(randTileType, level, tilePosition, Quaternion.identity);
    }

    /// <summary>
    /// Spawns a building.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="level">The level.</param>
    /// <param name="position">The position.</param>
    /// <param name="rotation">The rotation.</param>
    public void SpawnBuilding(TileType type, int level, Vector2 position, Quaternion rotation)
    {
        // Place building
        GameObject go = Resources.Load<GameObject>("Prefabs/Buildings/" + type.ToString() + "_" + level);
        GameObject newBuilding = Instantiate(go, position, rotation, GetTileParent(go).transform);

        Building building = newBuilding.GetComponent<Building>();
        building.Level = level;
        building.TileType = type;

        // Add to list
        Buildings.Add(building);
    }

    /// <summary>
    /// Spawns a building.
    /// </summary>
    /// <param name="data">The data.</param>
    public void SpawnBuilding(BuildingData data)
    {
        SpawnBuilding(data.TileType, data.Level, new Vector2(data.PosX, data.PosY), Quaternion.Euler(0, data.RotY, 0));
    }

    /// <summary>
    /// Gets the tile to be placed.
    /// </summary>
    /// <returns></returns>
    public TileType GetTileToBePlaced()
    {
        // Return tile type and reset variable
        TileType tileType = _tileToBePlaced;
        _tileToBePlaced = TileType.None;
        return tileType;
    }

    /// <summary>
    /// Sets the highlight colour.
    /// </summary>
    /// <param name="r">Red</param>
    /// <param name="g">Green</param>
    /// <param name="b">Blue</param>
    /// <param name="a">Alpha</param>
    public void SetHighlightColour(float r, float g, float b, float a)
    {
        TileHighlightColour = new Color(r, g, b, a);
    }

    /// <summary>
    /// Sets the highlight colour.
    /// </summary>
    /// <param name="colour">The colour.</param>
    public void SetHighlightColour(string colour)
    {
        // Prevents case sensitivity
        colour = colour.ToLower();

        switch (colour)
        {
            case "cyan":
                TileHighlightColour = Color.cyan;
                break;
            case "green":
                TileHighlightColour = Color.green;
                break;
            case "orange":
                TileHighlightColour = new Color(1f, 0.5f, 0f, 1f);
                break;
            case "red":
                TileHighlightColour = Color.red;
                break;
            default:
                TileHighlightColour = Color.white;
                break;
        }
    }

    /// <summary>
    /// Sets the tile template.
    /// </summary>
    /// <param name="tileType">Type of the tile.</param>
    public void SetTileTemplate(string tileType)
    {
        _tileToBePlaced = (TileType)Enum.Parse(typeof(TileType), tileType);
    }

    /// <summary>
    /// Toggles the destroy mode.
    /// </summary>
    public void ToggleDestroyMode()
    {
        DestroyMode = !DestroyMode;
        // Set highlight colour
        EnableTileHighlighting = DestroyMode;
        SetHighlightColour(DestroyMode ? "red" : "");
    }

    /// <summary>
    /// Processes the traversibility of node, used when loading a world.
    /// </summary>
    public void ProcessNodes()
    {
        // Processes nodes during loading
        foreach (Tile tile in Roads)
        {
            Road road = tile.gameObject.GetComponent<Road>();
            Vector2 roadPosition = road.transform.position;

            foreach (Tile ground in GroundTiles)
                if ((Vector2) ground.transform.position == roadPosition)
                {
                    // Mark node
                    Node node = ground.GetComponent<Node>();
                    node.TraversableUp = road.TraversableUp;
                    node.TraversableDown = road.TraversableDown;
                    node.TraversableLeft = road.TraversableLeft;
                    node.TraversableRight = road.TraversableRight;
                }
        }
    }

    /// <summary>
    /// Used to determine the parent GameObject of a tile based on its type. 
    /// </summary>
    /// <param name="go">The go.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public GameObject GetTileParent(GameObject go)
    {
        // Get the GroundTiles parent and return it, this is done so that gameobjects are organised
        Tile tile = go.GetComponent<Tile>();
        TileType type = tile.TileType;

        switch (type)
        {
            case TileType.None:
                break;
            case TileType.Commercial:
            case TileType.CommercialMarker:
            case TileType.Office:
            case TileType.OfficeMarker:
            case TileType.Residential:
            case TileType.ResidentialMarker:
                return GameObject.Find("Game/Tiles/Buildings");
            case TileType.CrossRoad:
            case TileType.StraightRoad:
            case TileType.StraightTurnRoadX:
            case TileType.StraightTurnRoadY:
                return GameObject.Find("Game/Tiles/Roads");
            case TileType.Grass:
            case TileType.Sand:
            case TileType.SandWater:
            case TileType.Water:
                return GameObject.Find("Game/Tiles/Ground");
            case TileType.Pavement:
            case TileType.Tree:
                return GameObject.Find("Game/Tiles/Decoration");
            default:
                throw new ArgumentOutOfRangeException();
        }

        return GameObject.Find("Game/Tiles");
    }

    /// <summary>
    /// Used to determine the prefab path of a tile based on its type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>A path.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public string GetTilePrefabPath(TileType type)
    {
        switch (type)
        {
            case TileType.None:
                break;
            case TileType.Commercial:
            case TileType.CommercialMarker:
            case TileType.Office:
            case TileType.OfficeMarker:
            case TileType.Residential:
            case TileType.ResidentialMarker:
                return "Prefabs/Buildings/";
            case TileType.CrossRoad:
            case TileType.StraightRoad:
            case TileType.StraightTurnRoadX:
            case TileType.StraightTurnRoadY:
                return "Prefabs/Roads/";
            case TileType.Grass:
            case TileType.Sand:
            case TileType.SandWater:
            case TileType.Water:
                return "Prefabs/World/";
            case TileType.Pavement:
            case TileType.Tree:
                return "Prefabs/Decorative/";
            default:
                throw new ArgumentOutOfRangeException();
        }

        return "Prefabs/";
    }

    /// <summary>
    /// Generates a random path.
    /// </summary>
    /// <returns>A random path along road tiles.</returns>
    public bool GenerateRandomPath()
    {
        // Find a path

        // Check the number of tiles available
        Node[] roadNodes = GameObject.Find("Game/Tiles/Roads").GetComponentsInChildren<Node>();
        int noOfNodes = roadNodes.Length;
        if (noOfNodes < 3) return false;

        int firstIndex;
        int secondIndex;

        while (true)
        {
            // Find a random path
            firstIndex = Random.Range(0, noOfNodes);
            secondIndex = Random.Range(0, noOfNodes);

            // Check the indices aren't the same and that there is a sufficent amount of space to travel
            if (firstIndex != secondIndex && Mathf.Abs(firstIndex - secondIndex) >= 1) break;
        }

        // Get the nodes underneath the road nodes
        Node[] groundNodes = GameObject.Find("Game/Tiles/Ground").GetComponentsInChildren<Node>();
        Node firstNode = roadNodes[firstIndex];
        Node secondNode = roadNodes[secondIndex];

        foreach (Node node in groundNodes)
        {
            // Make node's z position 0 to prevent error
            Vector2 nodePos = node.transform.position;
            node.transform.position = nodePos;

            if (nodePos == (Vector2) firstNode.gameObject.transform.position)
                firstNode = node;
            else if (nodePos == (Vector2) secondNode.gameObject.transform.position)
                secondNode = node;
        }

        // Find path
        _roadPathFinder.FindPath(firstNode, secondNode);

        return _roadPathFinder.GetPath().Count > 0;
    }

    /// <summary>
    /// Spawns a vehicle which follows a random path.
    /// </summary>
    public void SpawnTraffic()
    {
        // Get random path, check for errors
        if (!GenerateRandomPath()) return;
        List<Node> path = _roadPathFinder.GetPath();

        // Get random car
        int randomInt = Random.Range(0, 4);
        string carString = "";

        switch (randomInt)
        {
            case 0:
                carString = "Car_Red";
                break;
            case 1:
                carString = "Car_Blue";
                break;
            case 2:
                carString = "Car_Green";
                break;
            case 3:
                carString = "Car_Black";
                break;
        }

        GameObject res = Resources.Load<GameObject>("Prefabs/Vehicles/" + carString);

        // Spawn car
        GameObject car = Instantiate(res, path[0].transform.position, Quaternion.identity, GameObject.Find("Game/Tiles/Traffic").transform);
        // Set car's path
        Vehicle vehicle = car.GetComponent<Vehicle>();
        vehicle.Path = path;
        vehicle.Stationary = false;
    }
}