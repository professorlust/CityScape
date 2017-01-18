﻿/*
 *       Class: Isometric
 *      Author: Harish Bhagat
 *        Year: 2016
 */

// Website used: https://gamedevelopment.tutsplus.com/tutorials/creating-isometric-worlds-a-primer-for-game-developers--gamedev-6511

using UnityEngine;

/// <summary>
/// A static class used for converting between 2D cartesian and isometric coordinates.
/// </summary>
public static class Isometric
{
    /// <summary>
    /// Used to convert cartesian coordinates to isometric coordinates.
    /// </summary>
    /// <param name="x">X coordinate to be converted.</param>
    /// <param name="y">Y coordinate to be converted.</param>
    /// <returns>Returns Vector2.</returns>
    public static Vector2 CartToIso(float x, float y)
    {
        // Convert the coordinates from cartesian to isometric
        Vector2 newCoordinates = new Vector2();
        newCoordinates.x = (x - y);
        newCoordinates.y = ((x + y) / 2);
        return newCoordinates;
    }

    /// <summary>
    /// Used to convert cartesian coordinates to isometric coordinates.
    /// </summary>
    /// <param name="coordinates">The coordinates to be converted.</param>
    /// <returns>Returns Vector2.</returns>
    public static Vector2 CartToIso(Vector2 coordinates)
    {
        // Convert the coordinates from cartesian to isometric
        return CartToIso(coordinates.x, coordinates.y);
    }

    /// <summary>
    /// Used to convert isometric coordinates to cartesian coordinates.
    /// </summary>
    /// <param name="x">X coordinate to be converted.</param>
    /// <param name="y">Y coordinate to be converted.</param>
    /// <returns>Returns Vector2.</returns>
    public static Vector2 IsoToCart(float x, float y)
    {
        // Convert the coordinates from cartesian to isometric
        Vector2 newCoordinates = new Vector2();
        newCoordinates.x = (2 * y + x) / 2;
        newCoordinates.y = (2 * y - x) / 2;
        return newCoordinates;
    }

    /// <summary>
    /// Used to convert isometric coordinates to cartesian coordinates.
    /// </summary>
    /// <param name="coordinates">The coordinates to be converted.</param>
    /// <returns>Returns Vector2.</returns>
    public static Vector2 IsoToCart(Vector2 coordinates)
    {
        // Convert the coordinates from cartesian to isometric
        return IsoToCart(coordinates.x, coordinates.y);
    }
}