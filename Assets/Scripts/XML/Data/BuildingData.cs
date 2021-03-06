﻿/*
 *       Class: TileData
 *      Author: Harish Bhagat
 *        Year: 2017
 */

using System.Xml.Serialization;

// Video used: https://www.youtube.com/watch?v=Y8Di-Q6qpU4

/// <summary>
/// Class used to hold Building data for serialisation.
/// </summary>
public class BuildingData
{
    [XmlElement("TileType")]
    public TileType TileType;

    [XmlAttribute("Level")]
    public int Level;

    [XmlElement("PosX")]
    public float PosX;

    [XmlElement("PosY")]
    public float PosY;

    [XmlElement("RotateY")]
    public float RotY;
}