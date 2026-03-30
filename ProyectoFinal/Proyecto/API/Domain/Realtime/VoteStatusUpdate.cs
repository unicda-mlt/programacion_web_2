namespace Domain.Realtime
{
    public class VoteStatusUpdate
    {
        public List<ScrutinyVoteStatus> Scrutinies { get; set; } = [];
    }

    public class ScrutinyVoteStatus
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ImageUrl { get; set; }
        public int TotalVotes { get; set; }
        public List<SlateVoteStatus> Slates { get; set; } = [];
    }

    public class SlateVoteStatus
    {
        public Guid Id { get; set; }
        public short Position { get; set; }
        public int VoteCount { get; set; }
        public FirstCandidacy? FirstCandidacy { get; set; }
    }

    public class FirstCandidacy
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? ImageUrl { get; set; }
    }
}
