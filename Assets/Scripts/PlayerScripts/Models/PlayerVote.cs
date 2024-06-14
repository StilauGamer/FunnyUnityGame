namespace PlayerScripts.Models
{
    public struct PlayerVote
    {
        public bool HasVoted;
        public bool IsSkipping;
        public uint VotedFor;
        
        public PlayerVote(bool hasVoted, bool isSkipping, uint votedFor)
        {
            HasVoted = hasVoted;
            IsSkipping = isSkipping;
            VotedFor = votedFor;
        }

        public void ResetVote()
        {
            HasVoted = false;
            IsSkipping = false;
            VotedFor = 0;
        }
    }
}