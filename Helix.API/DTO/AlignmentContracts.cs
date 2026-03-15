namespace Helix.API;

public record SmithWatermanAlignmentJobRequest(
    string SequenceA,
    string SequenceB,
    int MatchScore = 3,
    int MismatchPenalty = -3,
    int GapPenalty = -2
);

public record SmithWatermanAlignmentJobResponse(
    int Score,
    string AlignedSequenceA,
    string AlignedSequenceB
);