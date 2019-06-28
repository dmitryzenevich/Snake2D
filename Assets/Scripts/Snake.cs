using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Snake : MonoBehaviour
{
    #region Variables

    [SerializeField] private Tilemap _level;
    [SerializeField] private Tilemap _items;

    [SerializeField] private TileBase _foodTile;

    [SerializeField] private List<SnakeTile> _snakeTiles = new List<SnakeTile>();

    [SerializeField] private float speed;

    private Vector3 _direction = Vector3.up;
    private Vector3 _position;
    private Vector3 _oldPosition;
    private Vector3 _newPosition;

    #endregion

    #region Properties

    public bool IsPositionDifference
    {
        get { return _oldPosition != _newPosition; }
    }

    #endregion

    #region Sprites

    private SpriteRenderer _spriteRenderer;

    [SerializeField] private Sprite _spriteHead;

    [SerializeField] private Sprite _spriteUp;

    [SerializeField] private Sprite _spriteRight;

    [SerializeField] private Sprite _spriteDown;

    [SerializeField] private Sprite _spriteLeft;

    [SerializeField] private Sprite _spriteHorizontal;

    [SerializeField] private Sprite _spriteVertical;

    [SerializeField] private Sprite _spriteTailEndUp;

    [SerializeField] private Sprite _spriteTailEndRight;

    [SerializeField] private Sprite _spriteTailEndDown;

    [SerializeField] private Sprite _spriteTailEndLeft;

    [SerializeField] private Sprite _spriteTailBendingUpToRight;

    [SerializeField] private Sprite _spriteTailBendingRightToDown;

    [SerializeField] private Sprite _spriteTailBendingDownToLeft;

    [SerializeField] private Sprite _spriteTailBendingLeftToUp;

    #endregion

    #region Buttons

    [SerializeField] private Button _buttonUp;

    [SerializeField] private Button _buttonRight;

    [SerializeField] private Button _buttonDown;

    [SerializeField] private Button _buttonLeft;

    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _buttonUp.onClick.AddListener(SetDirectionUp);
        _buttonRight.onClick.AddListener(SetDirectionRight);
        _buttonDown.onClick.AddListener(SetDirectionDown);
        _buttonLeft.onClick.AddListener(SetDirectionLeft);
    }

    private void OnDisable()
    {
        _buttonUp.onClick.RemoveAllListeners();
        _buttonRight.onClick.RemoveAllListeners();
        _buttonDown.onClick.RemoveAllListeners();
        _buttonLeft.onClick.RemoveAllListeners();
    }

    private void Start()
    {
        _position = transform.position;
        SetDirectionUp();
    }

    private void Update()
    {
        Move();
    }

    #endregion

    #region Food

    private void CheckFood()
    {
        Vector3Int position = _items.WorldToCell(_newPosition + Vector3.down);
        TileBase tile = _items.GetTile(position);
        if (tile is FoodTile)
        {
            var food = (FoodTile)tile;
//            AddSnakeTile();
            SetFoodNewPosition(food);
            _items.SetTile(position, null);
        }
    }

    private void SetFoodNewPosition(TileBase foodTile)
    {
        bool isFood = default;
        bool isSnake = default;
        Vector3Int randPos;
        do
        {
            randPos = GetRandomPosition(new Vector3Int(-8, -4, 0), new Vector3Int(8, 4, 0));
            TileBase tile = _items.GetTile(randPos);
            isFood = tile is FoodTile;
            isSnake = CheckSnakeTile(randPos);
        } while (isFood || isSnake);
        _items.SetTile(randPos, foodTile);
    }

    private Vector3Int GetRandomPosition(Vector3Int min, Vector3Int max)
    {
        int x = Random.Range(min.x, max.x + 1);
        int y = Random.Range(min.y, max.y + 1);
        return new Vector3Int(x, y, 0);
    }

    private bool CheckSnakeTile(Vector3Int position)
    {
        foreach (var tile in _snakeTiles)
            if (tile.transform.position == position)
                return true;
        return false;
    }

    #endregion

    #region Move

    private void Move()
    {
        _position += _direction * speed * Time.deltaTime;
        _newPosition = _level.WorldToCell(_position);

        if (IsPositionDifference)
        {
            MoveTail(_newPosition);
            CheckFood();
        }

        transform.position = _newPosition;
        _oldPosition = _newPosition;
    }

    #endregion

    #region SetDirection

    private void SetDirectionUp()
    {
        SetDirection(Vector3.up, _spriteUp);
    }

    private void SetDirectionRight()
    {
        SetDirection(Vector3.right, _spriteRight);
    }

    private void SetDirectionDown()
    {
        SetDirection(Vector3.down, _spriteDown);
    }

    private void SetDirectionLeft()
    {
        SetDirection(Vector3.left, _spriteLeft);
    }

    private void SetDirection(Vector3 direction, Sprite sprite)
    {
        if (Vector3.Dot(_direction, direction).Equals(-1))
            return;

        _direction = direction;
        _spriteHead = sprite;
    }

    #endregion

    #region Tail

    private void MoveTail(Vector3 target)
    {
        if (_snakeTiles[0].transform.position == target)
            return;
        for (int i = _snakeTiles.Count - 1; i > 0; i--)
        {
            _snakeTiles[i].transform.position = _snakeTiles[i - 1].transform.position;
        }

        _snakeTiles[0].transform.position = target;

        SetHead();
        SetTailsSprites();
    }

    private void SetHead()
    {
        _spriteRenderer.sprite = _spriteHead;
    }

    private void SetTailsSprites()
    {
        for (int i = _snakeTiles.Count - 2; i > 0; i--)
        {
            var prevTile = _snakeTiles[i + 1];
            var nextTile = _snakeTiles[i - 1];
            var currentTile = _snakeTiles[i];

            currentTile.SpriteRenderer.sprite =
                GetSprite(currentTile.transform.position, prevTile.transform.position, nextTile.transform.position);
        }

        _snakeTiles[_snakeTiles.Count - 1].SpriteRenderer.sprite =
            GetSpriteTailEnd(_snakeTiles[_snakeTiles.Count - 2].transform.position);
    }

    private void AddSnakeTile()
    {
        _snakeTiles.Add(_snakeTiles[1]);
    }

    private Sprite GetSprite(Vector3 current, Vector3 prev, Vector3 next)
    {
        if (prev.x.Equals(next.x))
            return _spriteVertical;
        else if (prev.y.Equals(next.y))
            return _spriteHorizontal;

        return GetSpriteBending(current, prev, next);
    }

    private Sprite GetSpriteTailEnd(Vector3 next)
    {
        var dir = (_snakeTiles[_snakeTiles.Count - 1].transform.position - next).normalized;

        if (dir.Equals(Vector3.right))
            return _spriteTailEndRight;
        else if (dir.Equals(Vector3.left))
            return _spriteTailEndLeft;
        else if (dir.Equals(Vector3.up))
            return _spriteTailEndUp;
        else if (dir.Equals(Vector3.down))
            return _spriteTailEndDown;

        return null;
    }

    private Sprite GetSpriteBending(Vector3 current, Vector3 prev, Vector3 next)
    {
        var dirCurToNext = (current - next).normalized;
        var dirCurToPrev = (current - prev).normalized;

        if (dirCurToNext.Equals(Vector3.right) && dirCurToPrev.Equals(Vector3.up) ||
            dirCurToNext.Equals(Vector3.up) && dirCurToPrev.Equals(Vector3.right))
            return _spriteTailBendingDownToLeft;
        else if (dirCurToNext.Equals(Vector3.down) && dirCurToPrev.Equals(Vector3.right) ||
                 dirCurToNext.Equals(Vector3.right) && dirCurToPrev.Equals(Vector3.down))
            return _spriteTailBendingLeftToUp;
        else if (dirCurToNext.Equals(Vector3.left) && dirCurToPrev.Equals(Vector3.down) ||
                 dirCurToNext.Equals(Vector3.down) && dirCurToPrev.Equals(Vector3.left))
            return _spriteTailBendingUpToRight;
        else if (dirCurToNext.Equals(Vector3.up) && dirCurToPrev.Equals(Vector3.left) ||
                 dirCurToNext.Equals(Vector3.left) && dirCurToPrev.Equals(Vector3.up))
            return _spriteTailBendingRightToDown;

        return null;
    }

    #endregion
}