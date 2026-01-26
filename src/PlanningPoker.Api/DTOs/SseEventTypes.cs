namespace PlanningPoker.Api.DTOs;

public static class SseEventTypes
{
    public const string UserJoined = "user_joined";
    public const string UserLeft = "user_left";
    public const string UserConnected = "user_connected";
    public const string UserDisconnected = "user_disconnected";
    public const string VotingStarted = "voting_started";
    public const string VoteSubmitted = "vote_submitted";
    public const string VotesRevealed = "votes_revealed";
    public const string VotesReset = "votes_reset";
    public const string SessionClosed = "session_closed";
}
