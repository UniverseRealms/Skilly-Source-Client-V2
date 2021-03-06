using System;
using System.Collections.Generic;
using Game.Entities;
using Models.Static;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

namespace Game
{
    public class Square : Tile
    {
        private static readonly List<int> _LookUp = new List<int>
        {
            26171, 44789, 20333, 70429, 98257, 59393, 33961
        };
        
        public TileDesc Desc { get; private set; }
        public ushort Type { get; private set; }
        public Entity StaticObject;
        public int SinkLevel { get; private set; }
        public Map Map { get; private set; }

        [NonSerialized]
        public Vector3Int Position;

        public void Init(Map map, TileDesc desc, int x, int y)
        {
            Map = map;
            Desc = desc;
            Type = desc.Type;
            Position = new Vector3Int(x, y, 0);
            sprite = desc.TextureData.GetTexture(Hash(x, y));
            Redraw();

            if (Desc.Sink)
            {
                SinkLevel = 12;
            }
        }

        private static int Hash(int x, int y)
        {
            var l = _LookUp[(x + y) % _LookUp.Count];
            var val = (x << 16 | y) ^ 81397550L;
            val = val * l % 65535;
            return (int)val;
        }

        public void Redraw()
        {
            var redrawnTexture = TileRedrawer.Redraw(this, true);
            if (Desc.Sink)
                SinkLevel = redrawnTexture == null ? 12 : 6;

            if (redrawnTexture)
                sprite = redrawnTexture;
        }
    }
}