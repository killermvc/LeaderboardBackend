namespace Leaderboard.Models;

/// <summary>
/// Represents the approval status of a submitted score.
/// </summary>
public enum ScoreStatus
{
    /// <summary>
    /// Score is pending review by a moderator.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Score has been approved and is visible on the leaderboard.
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Score has been rejected by a moderator.
    /// </summary>
    Rejected = 2
}
