using Mirror;
using UnityEngine;

namespace Game.Models
{
    public struct DeadPlayer
    {
        public NetworkConnectionToClient Connection;
        
        public Vector3 Position;
        public Quaternion Rotation;
        
        public Color Color;
        
        public DeadPlayer(NetworkConnectionToClient connection, Vector3 position, Quaternion rotation, Color color)
        {
            Connection = connection;
            Position = position;
            Rotation = rotation;
            Color = color;
        }
    }
}