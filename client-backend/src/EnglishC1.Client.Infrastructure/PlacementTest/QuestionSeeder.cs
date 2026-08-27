using EnglishC1.Client.Domain.PlacementTest;
using EnglishC1.Client.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishC1.Client.Infrastructure.PlacementTest;

// Hand-authored question bank: 8 questions per (level, skill) cell across
// A2/B1/B2/C1 x Grammar/Vocabulary = 64 questions (doubled from the
// original 32 - see docs/PENDING_IDEAS.md, "thin for anything beyond a
// first estimate"). SeedAsync runs every startup and inserts whatever
// Bank entries aren't already in the database yet, matched by Text -
// the same match-by-Text approach BackfillExplanationsAsync already used
// for adding the Explanation column after the bank was live. This is
// what let the second batch of 32 questions get added here without
// clearing and re-seeding, which would have orphaned the QuestionId
// references on every TestAttempt already recorded (in dev AND
// production - see docs/errors).
public static class QuestionSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        var existingTexts = (await db.Questions.Select(q => q.Text).ToListAsync()).ToHashSet();
        var newEntries = Bank.Where(b => !existingTexts.Contains(b.Text)).ToList();

        foreach (var (text, level, skill, options, correctIndex, explanation) in newEntries)
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

        if (newEntries.Count > 0)
            await db.SaveChangesAsync();

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

        // --- Second batch (2026-08-27, doubling the bank) ---

        // A2 - Grammar
        ("I ___ like coffee, I prefer tea.", CefrLevel.A2, SkillArea.Grammar, ["don't", "doesn't", "not", "no"], 0,
            "\"I\" takes \"don't\" (do not) in present simple negatives; \"doesn't\" is only for he/she/it."),
        ("We ___ to the beach last weekend.", CefrLevel.A2, SkillArea.Grammar, ["went", "go", "goes", "going"], 0,
            "\"Went\" is the simple past of \"go\", used for a completed action (\"last weekend\")."),
        ("Is this ___ car parked outside?", CefrLevel.A2, SkillArea.Grammar, ["your", "you", "yours", "you're"], 0,
            "\"Your\" is a possessive adjective used directly before a noun (\"your car\"); \"yours\" stands alone."),
        ("The meeting is ___ Monday morning.", CefrLevel.A2, SkillArea.Grammar, ["on", "in", "at", "for"], 0,
            "\"On\" is used with specific days (\"on Monday\"); \"in\" is for months/years, \"at\" for clock times."),

        // A2 - Vocabulary
        ("What do you use to write a letter?", CefrLevel.A2, SkillArea.Vocabulary, ["a pen", "a fork", "a spoon", "a key"], 0,
            "A pen is a writing tool; the other objects are used for eating or opening locks."),
        ("Your mother's sister is your ___.", CefrLevel.A2, SkillArea.Vocabulary, ["aunt", "cousin", "niece", "grandmother"], 0,
            "\"Aunt\" specifically names a parent's sister."),
        ("What is the opposite of \"fast\"?", CefrLevel.A2, SkillArea.Vocabulary, ["slow", "quick", "early", "big"], 0,
            "\"Slow\" and \"fast\" describe opposite speeds; \"quick\" means the same as fast, not its opposite."),
        ("The day after Monday is ___.", CefrLevel.A2, SkillArea.Vocabulary, ["Tuesday", "Sunday", "Wednesday", "Friday"], 0,
            "Tuesday directly follows Monday in the week."),

        // B1 - Grammar
        ("I ___ play football every weekend when I was young.", CefrLevel.B1, SkillArea.Grammar, ["used to", "use to", "was using", "uses"], 0,
            "\"Used to\" + base verb describes a repeated past habit that no longer happens."),
        ("This exercise is ___ difficult as the last one.", CefrLevel.B1, SkillArea.Grammar, ["as", "so", "than", "more"], 0,
            "\"As...as\" compares two things that are equal; \"than\" is used with comparatives like \"more difficult than\"."),
        ("She's not answering her phone; she ___ be asleep.", CefrLevel.B1, SkillArea.Grammar, ["must", "can", "should", "would"], 0,
            "\"Must\" expresses a confident logical deduction based on the evidence available."),
        ("The woman ___ called earlier left a message.", CefrLevel.B1, SkillArea.Grammar, ["who", "which", "whom", "whose"], 0,
            "\"Who\" introduces a relative clause describing a person acting as the subject (\"the woman... called\")."),

        // B1 - Vocabulary
        ("\"To find out\" means to ___.", CefrLevel.B1, SkillArea.Vocabulary, ["discover", "hide", "forget", "lose"], 0,
            "\"Find out\" means to learn or discover a piece of information."),
        ("\"Exhausted\" is closest in meaning to ___.", CefrLevel.B1, SkillArea.Vocabulary, ["extremely tired", "very happy", "a little hungry", "slightly bored"], 0,
            "\"Exhausted\" describes an intense level of tiredness, stronger than just \"tired\"."),
        ("\"To turn down\" an offer means to ___ it.", CefrLevel.B1, SkillArea.Vocabulary, ["refuse", "accept", "discuss", "repeat"], 0,
            "\"Turn down\" means to reject or decline something offered."),
        ("A \"generous\" person is someone who ___.", CefrLevel.B1, SkillArea.Vocabulary, ["shares willingly", "never smiles", "works hard", "arrives late"], 0,
            "\"Generous\" describes willingness to give time, money, or help freely."),

        // B2 - Grammar
        ("She enjoys ___ novels in her free time.", CefrLevel.B2, SkillArea.Grammar, ["reading", "read", "to read", "reads"], 0,
            "\"Enjoy\" is followed by a gerund (verb + -ing), not an infinitive or base form."),
        ("I wish I ___ more free time these days.", CefrLevel.B2, SkillArea.Grammar, ["had", "have", "will have", "having"], 0,
            "\"Wish\" + past simple expresses a regret about the present, even though the meaning is not past."),
        ("We ___ our car repaired at the garage yesterday.", CefrLevel.B2, SkillArea.Grammar, ["had", "did", "made", "has"], 0,
            "The causative \"have something done\" (had + object + past participle) shows someone else performed the action for you."),
        ("You haven't finished the report yet, ___?", CefrLevel.B2, SkillArea.Grammar, ["have you", "haven't you", "did you", "don't you"], 0,
            "A negative statement takes a positive question tag (\"haven't... have you\")."),

        // B2 - Vocabulary
        ("\"Reluctant\" means ___ to do something.", CefrLevel.B2, SkillArea.Vocabulary, ["unwilling", "eager", "confident", "calm"], 0,
            "\"Reluctant\" describes hesitation or unwillingness, the opposite of \"eager\"."),
        ("\"To compensate\" someone means to ___.", CefrLevel.B2, SkillArea.Vocabulary, ["make up for a loss", "avoid them", "ignore them", "delay a payment"], 0,
            "\"Compensate\" means to give something to balance out a loss, damage, or inconvenience."),
        ("A \"versatile\" tool is one that ___.", CefrLevel.B2, SkillArea.Vocabulary, ["adapts to many uses", "never changes", "is very expensive", "breaks easily"], 0,
            "\"Versatile\" describes flexibility - being useful in many different situations."),
        ("\"Skeptical\" is closest in meaning to ___.", CefrLevel.B2, SkillArea.Vocabulary, ["doubtful", "confident", "excited", "careless"], 0,
            "\"Skeptical\" describes having doubts about whether something is true."),

        // C1 - Grammar
        ("It was in 1969 ___ the moon landing happened.", CefrLevel.C1, SkillArea.Grammar, ["that", "which", "when", "who"], 0,
            "This cleft sentence (\"It was... that...\") emphasizes \"in 1969\"; \"that\" is the standard connector in this structure."),
        ("If she had studied medicine, she ___ a doctor now.", CefrLevel.C1, SkillArea.Grammar, ["would be", "would have been", "will be", "had been"], 0,
            "A mixed conditional pairs a past unreal condition (\"had studied\") with a present unreal result (\"would be... now\")."),
        ("___ tired after the long journey, she decided to rest.", CefrLevel.C1, SkillArea.Grammar, ["Feeling", "Feel", "Felt", "To feel"], 0,
            "A present participle clause (\"Feeling tired...\") gives the reason for the main action in a single economical clause."),
        ("It was ___ cold that the lake froze overnight.", CefrLevel.C1, SkillArea.Grammar, ["so", "such", "too", "very"], 0,
            "\"So\" + adjective + \"that\" introduces a result clause; \"such\" would instead need a noun (\"such cold weather\")."),

        // C1 - Vocabulary
        ("An \"astute\" businessperson is one who is ___.", CefrLevel.C1, SkillArea.Vocabulary, ["sharp and perceptive", "careless", "generous", "talkative"], 0,
            "\"Astute\" describes someone with sharp judgment, quick to notice opportunities or risks."),
        ("\"To relinquish\" something means to ___ it.", CefrLevel.C1, SkillArea.Vocabulary, ["give up", "gain", "defend", "hide"], 0,
            "\"Relinquish\" means to formally give up control of or claim to something."),
        ("\"Tenacious\" is closest in meaning to ___.", CefrLevel.C1, SkillArea.Vocabulary, ["persistent", "weak", "lazy", "forgetful"], 0,
            "\"Tenacious\" describes holding firmly onto something, especially not giving up easily."),
        ("Something \"ephemeral\" is ___.", CefrLevel.C1, SkillArea.Vocabulary, ["short-lived", "permanent", "expensive", "dangerous"], 0,
            "\"Ephemeral\" describes something that lasts only a very short time."),
    ];
}
