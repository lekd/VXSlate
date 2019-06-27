using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BoardGame
{
    class Piece
    {
        GameObject _gameObject;
        string _name;

        public Piece(GameObject gameObject, string name)
        {
            GameObject = gameObject;
            Name = name;
        }

        public GameObject GameObject { get => _gameObject; set => _gameObject = value; }
        public string Name { get => _name; set => _name = value; }
    }
}
