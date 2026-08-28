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

        foreach (var (text, level, skill, options, correctIndex, explanation) in Bank)
        {
            if (existingTexts.Contains(text)) continue;

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

        // Reading is a separate array (see ReadingBank) rather than
        // folded into Bank's tuple shape - Bank's 6-element tuple has 64
        // existing entries that would all need a trailing null Passage
        // appended for no benefit, since Passage only ever applies here.
        foreach (var (passage, text, level, options, correctIndex, explanation) in ReadingBank)
        {
            if (existingTexts.Contains(text)) continue;

            var question = new Question
            {
                Id = Guid.NewGuid(),
                Text = text,
                Level = level,
                SkillArea = SkillArea.Reading,
                Explanation = explanation,
                Passage = passage,
            };
            question.Options = options
                .Select(optionText => new QuestionOption { Id = Guid.NewGuid(), QuestionId = question.Id, Text = optionText })
                .ToList();
            question.CorrectOptionId = question.Options[correctIndex].Id;

            db.Questions.Add(question);
        }

        await db.SaveChangesAsync();
        await BackfillExplanationsAsync(db);
    }

    private static async Task BackfillExplanationsAsync(AppDbContext db)
    {
        var explanationByText = Bank.ToDictionary(b => b.Text, b => b.Explanation);
        foreach (var (_, text, _, _, _, explanation) in ReadingBank)
            explanationByText[text] = explanation;

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

    // Reading comprehension (2026-08-28, first new skill beyond
    // Grammar/Vocabulary) - 4 passages per level, each with one
    // comprehension question. Passages grow in length/register with
    // level: simple present-tense narration at A2, up to dense
    // academic-register argument at C1.
    private static readonly (string Passage, string Text, CefrLevel Level, string[] Options, int CorrectIndex, string Explanation)[] ReadingBank =
    [
        // A2
        ("Maria works in a small bakery in the city center. She starts work at six o'clock every morning and finishes at two in the afternoon. On Saturdays, the bakery is very busy because many people buy bread for the weekend.",
            "What time does Maria finish work?", CefrLevel.A2,
            ["At two in the afternoon", "At six in the morning", "At six in the evening", "She doesn't work on Saturdays"], 0,
            "The passage says she \"finishes at two in the afternoon.\""),
        ("Tom has a dog called Max. Every evening, Tom takes Max for a walk in the park near his house. Max loves to run and play with other dogs. After the walk, Tom gives Max some food and water.",
            "Where does Tom walk his dog?", CefrLevel.A2,
            ["In the park near his house", "At the bakery", "At school", "In the kitchen"], 0,
            "The passage states Tom takes Max \"for a walk in the park near his house.\""),
        ("The weather this week is cold and windy. On Monday and Tuesday, it will rain a lot. On Wednesday, the sun will come out, but it will still be cold. Remember to bring an umbrella on Monday.",
            "What should you bring on Monday?", CefrLevel.A2,
            ["An umbrella", "Sunglasses", "A swimsuit", "A fan"], 0,
            "The passage advises to \"bring an umbrella on Monday\" because it will rain."),
        ("Anna's favorite subject at school is art. She draws pictures every day after school. Her teacher says she is very talented. Next month, Anna's paintings will be shown at the school exhibition.",
            "What is Anna's favorite subject?", CefrLevel.A2,
            ["Art", "Math", "Science", "History"], 0,
            "The passage says \"Anna's favorite subject at school is art.\""),

        // B1
        ("Last summer, James decided to learn how to cook. He had never made a meal before, so he started with simple recipes. After a few months of practice, he could prepare full dinners for his family. Now his friends often ask him for cooking advice.",
            "What can we infer about James's cooking skills?", CefrLevel.B1,
            ["They have improved a lot since last summer", "They have not changed at all", "He learned to cook from his friends", "He still can't cook a full dinner"], 0,
            "The passage shows progress from never cooking to preparing full dinners and giving advice - his skills clearly improved."),
        ("The city council has announced plans to build a new library downtown. The project will take about two years to complete and will include a large reading area, computer rooms, and a café. Local residents have reacted positively to the news, saying the area needs more public spaces.",
            "How have local residents reacted to the announcement?", CefrLevel.B1,
            ["Positively, because the area needs more public spaces", "Negatively, because they don't want a library", "They haven't reacted yet", "They are worried about the cost"], 0,
            "The passage says residents \"reacted positively... saying the area needs more public spaces.\""),
        ("Emma has worked as a nurse for over ten years. Although the job can be stressful, she says she has never regretted her choice of career. She finds it rewarding to help patients recover and often stays in touch with them after they leave the hospital.",
            "Why does Emma find her job rewarding?", CefrLevel.B1,
            ["Because she helps patients recover", "Because the job is not stressful", "Because she doesn't work long hours", "Because she wants to change careers"], 0,
            "The passage states she \"finds it rewarding to help patients recover.\""),
        ("When Carlos moved to a new country, he found it difficult to make friends at first because of the language barrier. However, he joined a local sports club, which helped him meet people who shared his interests. Within a year, he felt much more at home.",
            "How did Carlos overcome his difficulty making friends?", CefrLevel.B1,
            ["By joining a local sports club", "By avoiding social situations", "By learning to cook", "By moving back to his home country"], 0,
            "The passage explains he \"joined a local sports club, which helped him meet people.\""),

        // B2
        ("Remote work has become increasingly common in recent years, offering employees greater flexibility and eliminating long commutes. However, critics argue that it can lead to feelings of isolation and make collaboration between team members more difficult. Companies are now experimenting with hybrid models that combine the benefits of both office and remote work.",
            "According to the passage, what is one criticism of remote work?", CefrLevel.B2,
            ["It can make employees feel isolated", "It eliminates all flexibility", "It increases commute times", "It makes collaboration easier"], 0,
            "The passage lists isolation and difficulty in collaboration as criticisms raised by critics."),
        ("Despite significant advances in renewable energy technology, many countries still rely heavily on fossil fuels to meet their energy needs. Experts suggest that this dependence is due to a combination of infrastructure costs, political interests, and the slow pace of policy change, rather than a lack of viable alternatives.",
            "What do experts suggest is the main reason for continued reliance on fossil fuels?", CefrLevel.B2,
            ["A combination of costs, politics, and slow policy change", "A lack of renewable energy technology", "Renewable energy is too expensive to develop", "Fossil fuels are more efficient than alternatives"], 0,
            "The passage attributes it to \"infrastructure costs, political interests, and the slow pace of policy change\" - not a lack of alternatives."),
        ("The novel's protagonist initially appears confident and self-assured, but as the story unfolds, the reader discovers a deep sense of insecurity beneath the surface. This contrast becomes central to understanding her later decisions, which often seem irrational unless viewed through the lens of her hidden fears.",
            "Why is the contrast between the protagonist's confidence and insecurity important?", CefrLevel.B2,
            ["It helps explain her later, seemingly irrational decisions", "It shows she never changes throughout the novel", "It proves she is not a reliable narrator", "It has no real significance to the plot"], 0,
            "The passage says this contrast \"becomes central to understanding her later decisions.\""),
        ("While social media has undeniably transformed how people communicate, its impact on mental health remains a subject of ongoing debate. Some studies link heavy usage to increased anxiety and lower self-esteem, while others argue that the platforms simply reflect pre-existing issues rather than causing them.",
            "What is the ongoing debate mentioned in the passage about?", CefrLevel.B2,
            ["Whether social media causes or merely reflects mental health issues", "Whether social media has changed communication", "Whether social media should be banned", "Whether studies about social media are reliable"], 0,
            "The debate is whether heavy usage \"causes\" issues or the platforms \"simply reflect pre-existing issues.\""),

        // C1
        ("It would be an oversimplification to attribute the decline of traditional print journalism solely to the rise of digital media. While online platforms have undeniably disrupted established revenue models, the industry's struggles are equally rooted in a failure to adapt editorial practices to a rapidly evolving readership whose expectations of immediacy and interactivity print media was ill-equipped to satisfy.",
            "According to the passage, what is a mistaken view about print journalism's decline?", CefrLevel.C1,
            ["That digital media alone is responsible for it", "That readers no longer want immediacy", "That print journalism never had revenue problems", "That editorial practices were always well-adapted"], 0,
            "The passage opens by calling it \"an oversimplification\" to blame the decline \"solely\" on digital media."),
        ("Proponents of universal basic income argue that it would alleviate poverty and provide a safety net in an era of automation-driven job displacement. Detractors, meanwhile, contend that unconditional payments risk disincentivizing work and may prove fiscally unsustainable at scale, though empirical evidence from limited pilot programs remains inconclusive on both counts.",
            "What does the passage say about the evidence from pilot programs?", CefrLevel.C1,
            ["It is inconclusive regarding both the benefits and the risks", "It clearly proves UBI reduces poverty", "It clearly proves UBI discourages work", "No pilot programs have ever been conducted"], 0,
            "The passage states the evidence \"remains inconclusive on both counts\" - referring to both the benefits and risks mentioned."),
        ("Critics of the policy have been quick to characterize it as reactionary, yet such a label obscures the more nuanced motivations at play: a genuine, if perhaps misguided, attempt to address constituents' economic anxieties rather than a straightforward ideological retreat.",
            "What is the author's view of the \"reactionary\" label critics use?", CefrLevel.C1,
            ["It oversimplifies more complex motivations behind the policy", "It accurately describes the policy's ideology", "It was created by the policy's supporters", "It has no relation to constituents' concerns"], 0,
            "The author says the label \"obscures the more nuanced motivations at play\", implying it's an oversimplification."),
        ("The assumption that technological progress inevitably yields greater leisure time has been repeatedly contradicted by historical evidence; each wave of labor-saving innovation, from the industrial revolution onward, has tended to intensify productivity demands rather than diminish them, a paradox economists have struggled to satisfactorily explain.",
            "What paradox does the passage describe?", CefrLevel.C1,
            ["Technology increasing productivity demands instead of leisure time", "Economists agreeing on why technology increases leisure", "The industrial revolution reducing productivity", "Labor-saving innovation having no effect on work"], 0,
            "The passage describes how technological progress, expected to increase leisure, instead \"intensif[ies] productivity demands\" - a paradox."),
    ];
}
