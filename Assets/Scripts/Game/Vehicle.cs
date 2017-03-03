﻿using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public List<Node> Path;
    public float Speed = 1f;
    public bool Stationary = true;

    private Vector3 _currentPos;
    private Vector3 _targetPos;
    private RotatableSprite2D _rotatableSprite2D;

    private void Start()
    {
        // Initialise variables
        _targetPos = gameObject.transform.position;
        _rotatableSprite2D = gameObject.GetComponent<RotatableSprite2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Check if the vehicle needs to despawn
        if (Path.Count == 0)
            Destroy(gameObject);

        // Update current position
        _currentPos = gameObject.transform.position;

        // Check for a path or if stationary or if the last move is done
        if (!MoveDone() || Stationary) return;

        // Get the next node
        Node node = Path[0];
        // Get the position of the node
        Vector3 nodePos = node.gameObject.transform.position;
        // Set targetPos
        _targetPos = nodePos;
        // Check if the sprite needs to be changed/rotated
        ProcessRotation();

        // Remove the node from the list
        Path.RemoveAt(0);
    }

    private bool MoveDone()
    {
        // Check if the move to the target is done
        if (_currentPos == _targetPos)
            return true;

        // Travel from your current position to the node
        gameObject.transform.position = Vector2.MoveTowards(_currentPos, _targetPos, Time.deltaTime * (Speed * 0.1f));

        return false;
    }

    private void ProcessRotation()
    {
        // Get the X and Y difference between the current position and the target position

        // Get the direction of travel
        Direction2D direction;

        bool posX = _targetPos.x > _currentPos.x;
        bool posY = _targetPos.y > _currentPos.y;

        if (posX && posY)
            direction = Direction2D.Up;
        else if (!posX && !posY)
            direction = Direction2D.Down;
        else if (!posX && posY)
            direction = Direction2D.Left;
        else
            direction = Direction2D.Right;

        // Change the sprite
        Debug.Log(direction);
        _rotatableSprite2D.SetRotation(direction);
    }
}
