using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class Grid : MonoBehaviour
{
    [SerializeField] private Sprite _tileAliveSprite, _tileDeadSprite, _tileDeadSpriteB;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private int _updateSpeed = 3;

    [SerializeField] private TextMeshProUGUI _speedDisplay;
    [SerializeField] private TextMeshProUGUI _generationDisplay;
    [SerializeField] private TextMeshProUGUI _editModeText;

    [SerializeField] private Camera _camera;

    private Tile[,] _tiles;
    private float _lastUpdate;
    private int _generation;
    private bool _pauseAfterGenerate, _pauseAfterEdit;

    private float _actualUpdateSpeed
    {
        get
        {
            if (_updateSpeed == 1) return 0.01f;
            else if (_updateSpeed == 2) return 0.05f;
            else if (_updateSpeed == 3) return 0.1f;
            else if (_updateSpeed == 4) return 0.5f;
            else return 1f;
        }
    }

    public enum GameState
    {
        Generating,
        EditMode,
        Running,
        Paused
    };

    public GameState State;
    public int Width, Height;

    private void Awake()
    {
        _pauseAfterGenerate = true;
        State = GameState.Generating;
    }

    private void Update()
    {
        if (State == GameState.Generating)
        {
            if (_tiles == null) _tiles = new Tile[Width, Height];

            _generation = 0;

            GenerateTiles();
            UpdateTilesNextStatus();
            UpdateDisplayText();

            if (_pauseAfterGenerate)
            {
                State = GameState.Paused;
                _pauseAfterGenerate = false;
            }
            else
            {
                _lastUpdate = -1f;
                State = GameState.Running;
            }
        }
        else if (State == GameState.EditMode)
        {
            HandleMouseInput();
        }
        else if (State == GameState.Running)
        {
            _lastUpdate += Time.deltaTime;
            if (_lastUpdate > _actualUpdateSpeed)
            {
                _lastUpdate = 0;

                UpdateTilesAliveDisplay();
                UpdateTilesNextStatus();

                _generation++;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
                State = GameState.Paused;
        }
        else if (State == GameState.Paused)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                State = GameState.Running;
        }

        if (State != GameState.Generating)
            CheckInputs();

        if (State != GameState.Paused) _pauseMenu.SetActive(false);
        else _pauseMenu.SetActive(true);

        if (State == GameState.EditMode)
            _editModeText.enabled = true;
        else
            _editModeText.enabled = false;
    }

    private void HandleMouseInput()
    {
        if (State == GameState.EditMode)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                int tileX, tileY;
                tileX = (int)Mathf.Floor(worldPosition.x + 0.5f);
                tileY = (int)Mathf.Floor(worldPosition.y + 0.5f);

                if (tileX >= 0 && tileY >= 0 && tileX < Width && tileY < Height)
                {
                    Tile tile = _tiles[tileX, tileY];
                    if (tile != null)
                    {
                        tile.SetStatus(tile.GetStatus() == Tile.TileStatus.Alive ? Tile.TileStatus.Dead : Tile.TileStatus.Alive);
                    }
                }
            }
        }
    }

    private void UpdateDisplayText()
    {
        _speedDisplay.text = $"Simulation Speed: {UpdateSpeedToText}";
        _generationDisplay.text = $"Generation: {_generation}";
    }

    private void CheckInputs()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            Application.Quit();

        if (Input.GetKeyDown(KeyCode.C) && State == GameState.EditMode)
            ClearAllTiles();

        CheckSpeedChangeInput();
        CheckReset();
        CheckEditMode();

        UpdateDisplayText();
    }

    private void ClearAllTiles()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (_tiles[x, y] != null) Destroy(_tiles[x, y].gameObject);

                Tile newTile = Instantiate(_tilePrefab, new Vector2(x, y), Quaternion.identity);
                newTile.DeadSprite = (x + y) % 2 == 0 ? _tileDeadSprite : _tileDeadSpriteB;
                newTile.SetStatus(Tile.TileStatus.Dead);
                newTile.name = $"Tile {x},{y}";

                _tiles[x, y] = newTile;
            }
        }
    }

    private void CheckEditMode()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (State == GameState.Running)
            {
                _pauseAfterEdit = false;
                State = GameState.EditMode;
            }
            else if (State == GameState.Paused)
            {
                _pauseAfterEdit = true;
                State = GameState.EditMode;
            }
            else if (State == GameState.EditMode)
            {
                UpdateTilesNextStatus();
                State = _pauseAfterEdit ? GameState.Paused : GameState.Running;
            }
        }
    }

    private void CheckSpeedChangeInput()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Plus))
        {
            if (_updateSpeed > 1)
                _updateSpeed--;
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.Minus))
        {
            if (_updateSpeed < 5)
                _updateSpeed++;
        }
    }

    private string UpdateSpeedToText
    {
        get
        {
            return _updateSpeed == 3 ? "Normal"
                : _updateSpeed == 2 ? "Faster"
                : _updateSpeed == 1 ? "Fastest"
                : _updateSpeed == 4 ? "Slower"
                : "Slowest";
        }
    }

    private void CheckReset()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (State == GameState.Paused)
                _pauseAfterGenerate = true;
            else
                _pauseAfterGenerate = false;

            State = GameState.Generating;
        }
    }

    private void UpdateTilesAliveDisplay()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tile tile = _tiles[x, y];
                if (tile.NextStatus != Tile.TileStatus.None)
                {
                    tile.SetStatus(tile.NextStatus);
                }
            }
        }
    }

    private void UpdateTilesNextStatus()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tile tile = _tiles[x, y];
                bool isAlive = tile.GetStatus() == Tile.TileStatus.Alive;
                int totalAlive = GetTotalAdjacentTilesAlive(x, y);

                if (isAlive)
                {
                    if (totalAlive == 2 || totalAlive == 3) tile.NextStatus = Tile.TileStatus.Alive;
                    else tile.NextStatus = Tile.TileStatus.Dead;
                }
                else
                {
                    if (totalAlive == 3) tile.NextStatus = Tile.TileStatus.Alive;
                    else tile.NextStatus = Tile.TileStatus.Dead;
                }
            }
        }
    }

    private int GetTotalAdjacentTilesAlive(int x, int y)
    {
        int totalAlive = 0;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue; // skip current cell

                int calcX = x + i;
                int calcY = y + j;

                // wrap around
                if (calcX < 0) calcX = Width - 1;
                if (calcY < 0) calcY = Height - 1;
                if (calcX == Width) calcX = 0;
                if (calcY == Height) calcY = 0;

                totalAlive += _tiles[calcX, calcY].GetStatus() == Tile.TileStatus.Alive ? 1 : 0;
            }
        }

        return totalAlive;
    }

    void GenerateTiles()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (_tiles[x, y] != null) Destroy(_tiles[x, y].gameObject);

                Tile newTile = Instantiate(_tilePrefab, new Vector2(x, y), Quaternion.identity);
                newTile.DeadSprite = (x + y) % 2 == 0 ? _tileDeadSprite : _tileDeadSpriteB;
                newTile.SetStatus(Random.value > 0.9 ? Tile.TileStatus.Alive : Tile.TileStatus.Dead);
                newTile.name = $"Tile {x},{y}";

                _tiles[x, y] = newTile;
            }
        }
    }
}
