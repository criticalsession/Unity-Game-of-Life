using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _minSize;
    [SerializeField] private float _maxSize;
    [SerializeField] private Grid _grid;
    [SerializeField] private GameObject _pauseMenuPanel;

    private Camera _camera;
    private float _size;
    private Vector3 _dragOrigin;

    private float _maxY, _maxX;

    private void Awake()
    {
        Screen.SetResolution((int)Math.Floor((16f / 9f) * Screen.height), Screen.height, true);

        _camera = GetComponent<Camera>();
        _size = _camera.orthographicSize;
    }

    void Update()
    {
        ScrollCamera();
        if (_size < _maxSize) // can't pan at max zoom out
            PanCamera();
    }

    private void PanCamera()
    {
        if (Input.GetMouseButtonDown(2)) // first time clicked
            _dragOrigin = _camera.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(2)) // button held down
        {
            Vector3 diff = _dragOrigin - _camera.ScreenToWorldPoint(Input.mousePosition);
            ClampCamera(diff);
        }
    }

    private void ClampCamera(Vector3 toMove)
    {
        Vector3 pos = _camera.transform.position + toMove;

        float width = _size * _camera.aspect;
        float height = _size;

        float buffer = 5f;

        if (_size == _maxSize) // no buffer at max zoom out
            buffer = 0;

        float minX = width - 0.5f - buffer;
        float minY = height - 0.5f - buffer;
        float maxX = _grid.Width - width - 0.5f + buffer;
        float maxY = _grid.Height - height - 0.5f + buffer;

        if (pos.x < minX) pos.x = minX;
        if (pos.y < minY) pos.y = minY;
        if (pos.x > maxX) pos.x = maxX;
        if (pos.y > maxY) pos.y = maxY;

        _camera.transform.position = pos;
    }

    private void ScrollCamera()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            _size = Mathf.Clamp(_size - 1, _minSize, _maxSize);
            _camera.orthographicSize = _size;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            _size = Mathf.Clamp(_size + 1, _minSize, _maxSize);
            _camera.orthographicSize = _size;
        }

        ClampCamera(Vector3.zero);
    }
}
