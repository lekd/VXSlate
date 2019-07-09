using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script.TestGame
{
    public class Piece
    {
        GameObject _gameObject;
        string _name;
        bool _isSelected;
        Texture2D _original;
        Texture2D _highlighted;
        float _currentScaleLevel;

        public Piece(GameObject gameObject, string name)
        {
            GameObject = gameObject;
            Name = name;
        }

        public Piece(GameObject gameObject, string name, bool isSelected, Texture2D original, Texture2D highlighted, float scaleRatio)
        {
            _gameObject = gameObject;
            _name = name;
            _isSelected = isSelected;
            _original = original;
            _highlighted = highlighted;
            _currentScaleLevel = scaleRatio;
        }

        public GameObject GameObject { get => _gameObject; set => _gameObject = value; }
        public string Name { get => _name; set => _name = value; }
        public bool IsSelected { get => _isSelected; set => _isSelected = value; }
        public Texture2D Original { get => _original; set => _original = value; }
        public Texture2D Highlighted { get => _highlighted; set => _highlighted = value; }
        public float CurrentScaleLevel { get => _currentScaleLevel; set => _currentScaleLevel = value; }
    }
}
