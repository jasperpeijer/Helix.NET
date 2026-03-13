namespace Helix.API;

public record AlignmentRequest(
    string SequenceA,
    string SequenceB,
    int MatchScore = 3,
    int MismatchPenalty = -3,
    int GapPenalty = -2
);

public record AlignmentResponse(
    int Score,
    string AlignedSequenceA,
    string AlignedSequenceB
);