using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Snake : MonoBehaviour
{
    [SerializeField]
    private Tilemap _level;

    [SerializeField]
    private List<GameObject> _tail = new List<GameObject>();

    [SerializeField]
    private float speed;

    private Vector3 _direction = Vector3.up;
    private Vector3 _position;
    private Vector3 _oldPosition;

    #region Sprites

    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private Sprite _spriteHead;

    [SerializeField]
    private Sprite _spriteUp;

    [SerializeField]
    private Sprite _spriteRight;

    [SerializeField]
    private Sprite _spriteDown;

    [SerializeField]
    private Sprite _spriteLeft;

    [SerializeField]
    private Sprite _spriteHorizontal;

    [SerializeField]
    private Sprite _spriteVertical;

    [SerializeField]
    private Sprite _spriteTailEndUp;

    [SerializeField]
    private Sprite _spriteTailEndRight;

    [SerializeField]
    private Sprite _spriteTailEndDown;

    [SerializeField]
    private Sprite _spriteTailEndLeft;

    [SerializeField]
    private Sprite _spriteTailBendingUpToRight;

    [SerializeField]
    private Sprite _spriteTailBendingRightToDown;

    [SerializeField]
    private Sprite _spriteTailBendingDownToLeft;

    [SerializeField]
    private Sprite _spriteTailBendingLeftToUp;

    #endregion

    #region Buttons

    [SerializeField]
    private Button _buttonUp;

    [SerializeField]
    private Button _buttonRight;

    [SerializeField]
    private Button _buttonDown;

    [SerializeField]
    private Button _buttonLeft;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _buttonUp.onClick.AddListener(MoveUp);
        _buttonRight.onClick.AddListener(MoveRight);
        _buttonDown.onClick.AddListener(MoveDown);
        _buttonLeft.onClick.AddListener(MoveLeft);
    }

    private void Start()
    {
        _position = transform.position;
        MoveUp();
    }

    private void Update()
    {
        _position += _direction * speed * Time.deltaTime;
        Vector3 newPosition = _level.WorldToCell(_position);

        if (_oldPosition != newPosition)
            MoveTail(newPosition);

        transform.position = newPosition;
        _oldPosition = newPosition;
    }

    #endregion

    #region Move

    private void MoveUp()
    {
        Move(Vector3.up, _spriteUp);
    }

    private void MoveRight()
    {
        Move(Vector3.right, _spriteRight);
    }

    private void MoveDown()
    {
        Move(Vector3.down, _spriteDown);
    }

    private void MoveLeft()
    {
        Move(Vector3.left, _spriteLeft);
    }

    private void Move(Vector3 direction, Sprite sprite)
    {
        _direction = direction;
        _spriteHead = sprite;
    }

    #endregion

    #region Tail

    private void MoveTail(Vector3 target)
    {
        for (int i = _tail.Count - 1; i > 0; i--)
        {
            _tail[i].transform.position = _tail[i - 1].transform.position;
        }

        _tail[0].transform.position = target;

        SetHead();
        SetTailsSprites();
    }

    private void SetHead()
    {
        _spriteRenderer.sprite = _spriteHead;
    }

    private void SetTailsSprites()
    {
        for (int i = _tail.Count - 2; i > 0; i--)
        {
            var prevTransform = _tail[i + 1].transform;
            var nextTransform = _tail[i - 1].transform;
            var currentTransform = _tail[i].transform;

            currentTransform.GetComponent<SpriteRenderer>().sprite =
                GetSprite(currentTransform.position, prevTransform.position, nextTransform.position);
        }

        _tail[_tail.Count - 1].GetComponent<SpriteRenderer>().sprite = GetSpriteTailEnd(_tail[_tail.Count - 2].transform.position);
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
        var dir = (_tail[_tail.Count - 1].transform.position - next).normalized;

        if (dir.x.Equals(1))
            return _spriteTailEndRight;
        else if (dir.x.Equals(-1))
            return _spriteTailEndLeft;
        else if (dir.y.Equals(1))
            return _spriteTailEndUp;
        else if (dir.y.Equals(-1))
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