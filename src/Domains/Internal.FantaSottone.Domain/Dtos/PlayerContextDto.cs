namespace Internal.FantaSottone.Domain.Dtos
{
    public sealed class PlayerContextDto
    {
        public int UserId { get; set; }

        public string Email { get; set; }

        public int PlayerId { get; set; }

        public int? GameId { get; set; }

        public bool IsCreator { get; set; }

        public int CurrentScore { get; set; }

        public string GameName { get; set; }

        public byte GameStatus { get; set; }

        public int? CreatorPlayerId { get; set; }
    }
}
