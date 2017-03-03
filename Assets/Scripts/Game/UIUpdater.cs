﻿using UnityEngine;
using UnityEngine.UI;

public class UIUpdater : MonoBehaviour
{
    public InputField Field;

    private string _gameObjectName;
    private Game _game;

    // Use this for initialization
    private void Start()
    {
        // Get game instance
        _game = GameObject.Find("Game").GetComponent<Game>();
        // Get component
        Field = gameObject.GetComponent<InputField>();
        // Get the type of field
        _gameObjectName = gameObject.name;
    }

    // Update is called once per frame
    private void Update()
    {
        // Update field based on gameObject
        if (_gameObjectName == "Money_Display")
        {
            // Update the field to show the current amount of money the player has
            int money = _game.Money;
            Field.text = money.ToString();

            Text text = gameObject.transform.FindChild("Text").GetComponent<Text>();
            text.color = money < 0 ? Color.red : new Color(0f, 0.44f, 0.13f);
        }
        else if (_gameObjectName == "Population_Display")
        {
            // Update the field to show the current population
            Field.text = _game.Population.ToString();
        }
        else if (_gameObjectName == "Happiness_Display")
        {
            // Update the field to show the current level of happiness
            int happiness = Mathf.Clamp(_game.Happiness, 0, 100);
            Field.text = happiness.ToString() + "%";
            
            // Update icon to reflect percentage of happiness
            Image image = gameObject.transform.FindChild("Icon").GetComponent<Image>();
            string path = happiness >= 67 ? "UI/Happy" : happiness >= 33 ? "UI/Neutral" : "UI/Sad";
            image.sprite = Resources.Load<Sprite>(path);
        }
    }
}