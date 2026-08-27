using EnglishC1.Client.Domain.PlacementTest;
using EnglishC1.Client.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishC1.Client.Infrastructure.PlacementTest;

// Hand-authored question bank: 4 questions per (level, skill) cell across
// A2/B1/B2/C1 x Grammar/Vocabulary = 32 questions. Runs once at startup
// (see Program.cs) and is a no-op if the table already has data, so
// editing this file and restarting locally does NOT re-seed - clear the
// Questions table first if you want to pick up content changes.
public static class QuestionSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Questions.AnyAsync()) return;

        foreach (var (text, level, skill, options, correctIndex) in Bank)
        {
            var question = new Question
            {
                Id = Guid.NewGuid(),
                Text = text,
                Level = level,
                SkillArea = skill,
            };
            question.Options = options
                .Select(optionText => new QuestionOption { Id = Guid.NewGuid(), QuestionId = question.Id, Text = optionText })
                .ToList();
            question.CorrectOptionId = question.Options[correctIndex].Id;

            db.Questions.Add(question);
        }

        await db.SaveChangesAsync();
    }

    private static readonly (string Text, CefrLevel Level, SkillArea Skill, string[] Options, int CorrectIndex)[] Bank =
    [
        // A2 - Grammar
        ("She ___ to the gym every morning.", CefrLevel.A2, SkillArea.Grammar, ["goes", "go", "going", "gone"], 0),
        ("There ___ a book on the table.", CefrLevel.A2, SkillArea.Grammar, ["is", "are", "be", "been"], 0),
        ("This bag is ___ than that one.", CefrLevel.A2, SkillArea.Grammar, ["bigger", "more big", "biggest", "big"], 0),
        ("They ___ dinner when I called.", CefrLevel.A2, SkillArea.Grammar, ["were having", "have had", "has", "having"], 0),

        // A2 - Vocabulary
        ("What is the opposite of \"cheap\"?", CefrLevel.A2, SkillArea.Vocabulary, ["expensive", "small", "heavy", "old"], 0),
        ("A place where you buy bread is called a ___.", CefrLevel.A2, SkillArea.Vocabulary, ["bakery", "library", "pharmacy", "bank"], 0),
        ("The first meal of the day is called ___.", CefrLevel.A2, SkillArea.Vocabulary, ["breakfast", "lunch", "dinner", "snack"], 0),
        ("Which word means the same as \"big\"?", CefrLevel.A2, SkillArea.Vocabulary, ["large", "small", "thin", "short"], 0),

        // B1 - Grammar
        ("I have never ___ sushi before.", CefrLevel.B1, SkillArea.Grammar, ["eaten", "eat", "ate", "eating"], 0),
        ("If it rains, we ___ the picnic.", CefrLevel.B1, SkillArea.Grammar, ["will cancel", "cancel", "would cancel", "cancelled"], 0),
        ("You ___ smoke here, it's against the rules.", CefrLevel.B1, SkillArea.Grammar, ["mustn't", "don't have to", "can", "should"], 0),
        ("She's been living here ___ 2020.", CefrLevel.B1, SkillArea.Grammar, ["since", "for", "from", "at"], 0),

        // B1 - Vocabulary
        ("\"To postpone\" means to ___.", CefrLevel.B1, SkillArea.Vocabulary, ["delay", "cancel", "start", "finish"], 0),
        ("\"Reliable\" is closest in meaning to ___.", CefrLevel.B1, SkillArea.Vocabulary, ["dependable", "expensive", "interesting", "difficult"], 0),
        ("\"To give up\" means to ___.", CefrLevel.B1, SkillArea.Vocabulary, ["stop trying", "start again", "try harder", "wait"], 0),
        ("The phrasal verb \"look into\" means to ___.", CefrLevel.B1, SkillArea.Vocabulary, ["investigate", "find", "ignore", "remember"], 0),

        // B2 - Grammar
        ("If I ___ more time, I would travel more.", CefrLevel.B2, SkillArea.Grammar, ["had", "have", "will have", "having"], 0),
        ("The report ___ by the team yesterday.", CefrLevel.B2, SkillArea.Grammar, ["was written", "wrote", "has written", "is writing"], 0),
        ("He said he ___ tired.", CefrLevel.B2, SkillArea.Grammar, ["was", "is", "has been", "were"], 0),
        ("The man ___ car was stolen called the police.", CefrLevel.B2, SkillArea.Grammar, ["whose", "who", "which", "that"], 0),

        // B2 - Vocabulary
        ("\"Meticulous\" is closest in meaning to ___.", CefrLevel.B2, SkillArea.Vocabulary, ["careful", "careless", "fast", "lazy"], 0),
        ("\"To procrastinate\" means to ___.", CefrLevel.B2, SkillArea.Vocabulary, ["delay doing something", "finish early", "work quickly", "plan carefully"], 0),
        ("She made a ___ decision without thinking it through.", CefrLevel.B2, SkillArea.Vocabulary, ["hasty", "firm", "wise", "careful"], 0),
        ("\"Ambiguous\" means ___.", CefrLevel.B2, SkillArea.Vocabulary, ["unclear or having more than one meaning", "very clear", "interesting", "boring"], 0),

        // C1 - Grammar
        ("Had I known, I ___ have come.", CefrLevel.C1, SkillArea.Grammar, ["would", "will", "can", "should"], 0),
        ("Not only ___ late, but he also forgot the documents.", CefrLevel.C1, SkillArea.Grammar, ["was he", "he was", "did he", "he did"], 0),
        ("It is essential that she ___ present at the meeting.", CefrLevel.C1, SkillArea.Grammar, ["be", "is", "was", "being"], 0),
        ("Rarely ___ such dedication.", CefrLevel.C1, SkillArea.Grammar, ["have I seen", "I have seen", "did I see", "I saw"], 0),

        // C1 - Vocabulary
        ("\"Ubiquitous\" means ___.", CefrLevel.C1, SkillArea.Vocabulary, ["present everywhere", "very rare", "dangerous", "expensive"], 0),
        ("\"To exacerbate\" means to ___.", CefrLevel.C1, SkillArea.Vocabulary, ["make worse", "improve", "ignore", "measure"], 0),
        ("\"Candid\" is closest in meaning to ___.", CefrLevel.C1, SkillArea.Vocabulary, ["frank and honest", "secretive", "nervous", "formal"], 0),
        ("A \"deft\" solution is one that is ___.", CefrLevel.C1, SkillArea.Vocabulary, ["skillful", "slow", "expensive", "risky"], 0),
    ];
}
