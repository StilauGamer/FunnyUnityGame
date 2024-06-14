using PlayerScripts;
using UnityEngine;

namespace Game.Models
{
    public struct VotePlayer
    {
        public readonly string Name;
        
        public readonly uint NetId;
        public readonly uint TargetNetId;
        
        public readonly bool IsDead;
        public readonly bool IsReporting;

        
        public Color Color;


        public VotePlayer(Player player)
            : this(player.DisplayName, player.netId, player.PlayerVote.VotedFor, player.IsDead, player.IsReporting, player.BodyColor)
        {
        }

        public VotePlayer(string name, uint netId, uint targetNetId, bool isDead, bool isReporting, Color color)
        {
            Name = name;
            
            NetId = netId;
            TargetNetId = targetNetId;
            
            IsDead = isDead;
            IsReporting = isReporting;
            
            Color = color;
        }
    }
}