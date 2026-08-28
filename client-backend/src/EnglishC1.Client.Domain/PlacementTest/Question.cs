namespace EnglishC1.Client.Domain.PlacementTest;

public class Question
{
    public Guid Id { get; init; }
    public required string Text { get; set; }
    public SkillArea SkillArea { get; set; }
    public CefrLevel Level { get; set; }
    public Guid CorrectOptionId { get; set; }
    public List<QuestionOption> Options { get; set; } = [];

    // Shown only after a wrong answer is submitted (never during the
    // test itself) - a short reason so a failed reinforcement attempt
    // teaches something instead of just reporting a score.
    public string? Explanation { get; set; }

    // True for a question Groq generated on demand for reinforcement
    // practice (see IAiQuestionGeneratorService), false for the
    // hand-written seeded bank. Surfaced in the admin Content tab so
    // it's clear which is which - not otherwise treated differently
    // (grading, sampling, everything else works identically either way).
    public bool IsAiGenerated { get; set; }

    // A short paragraph shown above the question - only Reading
    // questions use this (Grammar/Vocabulary stay one-liners). Null for
    // every other skill.
    public string? Passage { get; set; }

    // A URL to a short spoken audio clip, played instead of showing a
    // passage - only Listening questions use this. Null for every other
    // skill. Served as a static file (wwwroot/audio) - see
    // Program.cs's UseStaticFiles and QuestionSeeder's ListeningBank.
    public string? AudioUrl { get; set; }
}
