using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Tile : MonoBehaviour
{
    public enum TileStatus
    {
        Alive, Dead, None
    }

    private TileStatus _status;
    private SpriteRenderer _spriteRenderer;

    public TileStatus NextStatus;
    public Sprite DeadSprite, AliveSprite;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public TileStatus GetStatus() => _status;
    public void SetStatus(TileStatus status)
    {
        _status = status;
        if (status == TileStatus.Alive)
        {
            _spriteRenderer.sprite = AliveSprite;
        } 
        else
        {
            _spriteRenderer.sprite = DeadSprite;
        }
    }

}
