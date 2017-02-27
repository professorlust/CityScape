﻿/*
 *       Class: Building
 *      Author: Harish Bhagat
 *        Year: 2017
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Used to define a building class.
/// </summary>
public class Building : MonoBehaviour
{
    // RTTI Type of building
    public TileType Type;

    // Variables
    public BuildingDirection Direction;

    public Vector2 OccupantMinMax = new Vector2(0, 100);
    public Vector2 LevelMinMax = new Vector2(1, 3);
    public Vector2 HappinessMinMax = new Vector2(0, 100);

    private int _happiness;
    private int _occupants;
    private int _level = 1;

    private HashSet<HappinessBooster> _boosters;

    // Game instance
    private Game _game;

    private void Awake()
    {
        // Intialise variables
        _game = Game.Instance;
        _boosters = new HashSet<HappinessBooster>();

        switch (Type)
        {
            case TileType.Residential:
                OccupantMinMax = new Vector2(0, 100);
                break;
            case TileType.Commercial:
                OccupantMinMax = new Vector2(0, 225);
                break;
            case TileType.Office:
                OccupantMinMax = new Vector2(0, 500);
                break;
        }
    }

    // Properties
    public float TaxPercentage
    {
        get
        {
            switch (Type)
            {
                case TileType.Residential:
                    return _game.ResidentialTax;
                case TileType.Commercial:
                    return _game.CommercialTax;
                case TileType.Office:
                    return _game.OfficeTax;
                default:
                    return 1;
            }
        }
    }

    public int Happiness
    {
        get
        {
            // Get clamped boosters sum
            _happiness = Mathf.Clamp(_boosters.Sum(booster => booster.Boost), 0, 100);
            // Return happiness
            return _happiness;
        }
    }

    public int Occupants
    {
        get
        {
            // Calculate number of occupants
            _occupants =  Mathf.RoundToInt(Happiness / 100f * (OccupantMinMax.y * (Level / LevelMinMax.y)));
            // Return occupants
            return _occupants;
        }
    }

    public int Level
    {
        get { return _level; }
        set { _level = (int)Mathf.Clamp(value, LevelMinMax.x, LevelMinMax.y); }
    }

    /// <summary>
    /// Calculates the amount of tax generated.
    /// </summary>
    /// <returns></returns>
    public virtual float CollectTax()
    {
        // Calculate the amount of tax generated
        float happy = _happiness == 0 ? 1 : _happiness;
        float occ = _occupants == 0 ? 1 : _occupants;

        float tax = happy * _level / (TaxPercentage / occ); // Check occupantCount and happiness to prevent dividing by 0 error
        return Mathf.Approximately(tax, 1f) ? 0f : tax; // If tax is 1 collect nothing
    }

    private void OnTriggerEnter(Collider other)
    {
        // Add booster to HashSet
        HappinessBooster booster = other.gameObject.GetComponent<HappinessBooster>();
        if (booster == null) return;
        _boosters.Add(booster);
    }

    private void OnTriggerExit(Collider other)
    {
        // Remove booster from HashSet
        HappinessBooster booster = other.gameObject.GetComponent<HappinessBooster>();
        if (booster == null) return;
        _boosters.Remove(booster);
    }
}