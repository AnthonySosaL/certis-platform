using EnglishC1.Client.Domain.PlacementTest;
using EnglishC1.Client.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishC1.Client.Infrastructure.PlacementTest;

// Hand-authored question bank: 4 questions per (level, skill) cell across
// A2/B1/B2/C1 x Grammar/Vocabulary = 32 questions. SeedAsync runs once at
// startup (see Program.cs) and only inserts if the table is empty.
// BackfillExplanationsAsync runs every startup regardless - it matches
// existing rows by Text and fills in Explanation where missing, so
// adding explanations after the bank was already seeded (in dev AND in
// production - see docs/errors) doesn't need clearing and re-seeding,
// which would have orphaned the QuestionId references on every
// TestAttempt already recorded.
public static class QuestionSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Questions.AnyAsync())
        {
            foreach (var (text, level, skill, options, correctIndex, explanation) in Bank)
            {
                var question = new Question
                {
                    Id = Guid.NewGuid(),
                    Text = text,
                    Level = level,
                    SkillArea = skill,
                    Explanation = explanation,
                };
                question.Options = options
                    .Select(optionText => new QuestionOption { Id = Guid.NewGuid(), QuestionId = question.Id, Text = optionText })
                    .ToList();
                question.CorrectOptionId = question.Options[correctIndex].Id;

                db.Questions.Add(question);
            }

            await db.SaveChangesAsync();
            return;
        }

        await BackfillExplanationsAsync(db);
    }

    private static async Task BackfillExplanationsAsync(AppDbContext db)
    {
        var explanationByText = Bank.ToDictionary(b => b.Text, b => b.Explanation);
        var questions = await db.Questions.Where(q => q.Explanation == null).ToListAsync();
        if (questions.Count == 0) return;

        foreach (var question in questions)
        {
            if (explanationByText.TryGetValue(question.Text, out var explanation))
                question.Explanation = explanation;
        }

        await db.SaveChangesAsync();
    }

    private static readonly (string Text, CefrLevel Level, SkillArea Skill, string[] Options, int CorrectIndex, string Explanation)[] Bank =
    [
        // A2 - Grammar
        ("She ___ to the gym every morning.", CefrLevel.A2, SkillArea.Grammar, ["goes", "go", "going", "gone"], 0,
            "Third-person singular subjects (she, he, it) take an -s ending in the present simple."),
        ("There ___ a book on the table.", CefrLevel.A2, SkillArea.Grammar, ["is", "are", "be", "been"], 0,
            "\"A book\" is singular, so it takes \"is\", not \"are\"."),
        ("This bag is ___ than that one.", CefrLevel.A2, SkillArea.Grammar, ["bigger", "more big", "biggest", "big"], 0,
            "Short adjectives like \"big\" form the comparative by doubling the final consonant and adding -er."),
        ("They ___ dinner when I called.", CefrLevel.A2, SkillArea.Grammar, ["were having", "have had", "has", "having"], 0,
            "The past continuous describes an action already in progress when another event (the call) happened."),

        // A2 - Vocabulary
        ("What is the opposite of \"cheap\"?", CefrLevel.A2, SkillArea.Vocabulary, ["expensive", "small", "heavy", "old"], 0,
            "\"Cheap\" and \"expensive\" are opposites in price; the other options describe size, weight, or age."),
        ("A place where you buy bread is called a ___.", CefrLevel.A2, SkillArea.Vocabulary, ["bakery", "library", "pharmacy", "bank"], 0,
            "A bakery specifically sells bread and baked goods."),
        ("The first meal of the day is called ___.", CefrLevel.A2, SkillArea.Vocabulary, ["breakfast", "lunch", "dinner", "snack"], 0,
            "Breakfast is eaten first, in the morning."),
        ("Which word means the same as \"big\"?", CefrLevel.A2, SkillArea.Vocabulary, ["large", "small", "thin", "short"], 0,
            "\"Large\" and \"big\" both describe size; the other words mean the opposite or something unrelated."),

        // B1 - Grammar
        ("I have never ___ sushi before.", CefrLevel.B1, SkillArea.Grammar, ["eaten", "eat", "ate", "eating"], 0,
            "\"Have + past participle\" (present perfect) pairs with \"never... before\" to talk about life experience up to now."),
        ("If it rains, we ___ the picnic.", CefrLevel.B1, SkillArea.Grammar, ["will cancel", "cancel", "would cancel", "cancelled"], 0,
            "First conditional: \"if\" + present simple, then \"will\" + base verb for a likely future result."),
        ("You ___ smoke here, it's against the rules.", CefrLevel.B1, SkillArea.Grammar, ["mustn't", "don't have to", "can", "should"], 0,
            "\"Mustn't\" expresses a prohibition. \"Don't have to\" means something is optional, which contradicts \"against the rules\"."),
        ("She's been living here ___ 2020.", CefrLevel.B1, SkillArea.Grammar, ["since", "for", "from", "at"], 0,
            "\"Since\" marks the starting point of an action (\"since 2020\"); \"for\" is used with a duration (\"for six years\")."),

        // B1 - Vocabulary
        ("\"To postpone\" means to ___.", CefrLevel.B1, SkillArea.Vocabulary, ["delay", "cancel", "start", "finish"], 0,
            "\"Postpone\" means to move something to a later time, not to cancel it."),
        ("\"Reliable\" is closest in meaning to ___.", CefrLevel.B1, SkillArea.Vocabulary, ["dependable", "expensive", "interesting", "difficult"], 0,
            "\"Reliable\" describes something or someone you can count on - \"dependable\"."),
        ("\"To give up\" means to ___.", CefrLevel.B1, SkillArea.Vocabulary, ["stop trying", "start again", "try harder", "wait"], 0,
            "\"Give up\" means to quit an effort, the opposite of trying harder."),
        ("The phrasal verb \"look into\" means to ___.", CefrLevel.B1, SkillArea.Vocabulary, ["investigate", "find", "ignore", "remember"], 0,
            "\"Look into\" means to examine or research something."),

        // B2 - Grammar
        ("If I ___ more time, I would travel more.", CefrLevel.B2, SkillArea.Grammar, ["had", "have", "will have", "having"], 0,
            "Second conditional: \"if\" + past simple describes a hypothetical present, paired with \"would\"."),
        ("The report ___ by the team yesterday.", CefrLevel.B2, SkillArea.Grammar, ["was written", "wrote", "has written", "is writing"], 0,
            "Passive voice (\"was/were + past participle\") is used when the focus is on the report, not on who wrote it."),
        ("He said he ___ tired.", CefrLevel.B2, SkillArea.Grammar, ["was", "is", "has been", "were"], 0,
            "Reported speech shifts the present tense (\"am\") back one step into the past (\"was\")."),
        ("The man ___ car was stolen called the police.", CefrLevel.B2, SkillArea.Grammar, ["whose", "who", "which", "that"], 0,
            "\"Whose\" shows possession, linking \"the man\" to \"his car\"."),

        // B2 - Vocabulary
        ("\"Meticulous\" is closest in meaning to ___.", CefrLevel.B2, SkillArea.Vocabulary, ["careful", "careless", "fast", "lazy"], 0,
            "\"Meticulous\" means paying great attention to detail - the opposite of careless."),
        ("\"To procrastinate\" means to ___.", CefrLevel.B2, SkillArea.Vocabulary, ["delay doing something", "finish early", "work quickly", "plan carefully"], 0,
            "\"Procrastinate\" means to put off a task you should be doing."),
        ("She made a ___ decision without thinking it through.", CefrLevel.B2, SkillArea.Vocabulary, ["hasty", "firm", "wise", "careful"], 0,
            "\"Hasty\" describes something done too quickly, without enough thought - matching \"without thinking it through\"."),
        ("\"Ambiguous\" means ___.", CefrLevel.B2, SkillArea.Vocabulary, ["unclear or having more than one meaning", "very clear", "interesting", "boring"], 0,
            "\"Ambiguous\" describes something open to more than one interpretation."),

        // C1 - Grammar
        ("Had I known, I ___ have come.", CefrLevel.C1, SkillArea.Grammar, ["would", "will", "can", "should"], 0,
            "Third conditional (\"had + past participle\", \"would have + past participle\") talks about an unreal past situation."),
        ("Not only ___ late, but he also forgot the documents.", CefrLevel.C1, SkillArea.Grammar, ["was he", "he was", "did he", "he did"], 0,
            "A negative expression like \"Not only\" at the start of a clause triggers subject-auxiliary inversion."),
        ("It is essential that she ___ present at the meeting.", CefrLevel.C1, SkillArea.Grammar, ["be", "is", "was", "being"], 0,
            "The subjunctive mood after expressions like \"it is essential that\" uses the base form of the verb, with no -s or tense marking."),
        ("Rarely ___ such dedication.", CefrLevel.C1, SkillArea.Grammar, ["have I seen", "I have seen", "did I see", "I saw"], 0,
            "Negative adverbs like \"Rarely\" at the start of a sentence trigger subject-auxiliary inversion."),

        // C1 - Vocabulary
        ("\"Ubiquitous\" means ___.", CefrLevel.C1, SkillArea.Vocabulary, ["present everywhere", "very rare", "dangerous", "expensive"], 0,
            "\"Ubiquitous\" describes something found everywhere at once - the opposite of rare."),
        ("\"To exacerbate\" means to ___.", CefrLevel.C1, SkillArea.Vocabulary, ["make worse", "improve", "ignore", "measure"], 0,
            "\"Exacerbate\" means to make a bad situation worse."),
        ("\"Candid\" is closest in meaning to ___.", CefrLevel.C1, SkillArea.Vocabulary, ["frank and honest", "secretive", "nervous", "formal"], 0,
            "\"Candid\" describes someone speaking truthfully and openly."),
        ("A \"deft\" solution is one that is ___.", CefrLevel.C1, SkillArea.Vocabulary, ["skillful", "slow", "expensive", "risky"], 0,
            "\"Deft\" describes something done with skill and ease."),
    ];
}
