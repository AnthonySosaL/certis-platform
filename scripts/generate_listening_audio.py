"""Generates the Listening comprehension audio bank via edge-tts (local,
free, no API key - see docs/PENDING_IDEAS.md for why this was picked over
Groq's TTS, which is blocked on terms acceptance, and ElevenLabs, which
isn't installed).

One-time/occasional script, not run at app startup - the output mp3s are
committed to wwwroot and served as static files. Re-run this only when a
transcript below changes or a new entry is added; it always overwrites
every file, so it's safe to re-run in full.

Usage:
    python scripts/generate_listening_audio.py

The (filename, transcript) pairs below MUST stay in sync with the
ListeningBank tuples in
client-backend/src/EnglishC1.Client.Infrastructure/PlacementTest/QuestionSeeder.cs
(matched by filename) - QuestionSeeder doesn't store the transcript text
itself, only the resulting AudioUrl, so this script is the one place the
spoken content lives.
"""

import asyncio
import pathlib

import edge_tts

OUTPUT_DIR = (
    pathlib.Path(__file__).resolve().parent.parent
    / "client-backend"
    / "src"
    / "EnglishC1.Client.Api"
    / "wwwroot"
    / "audio"
    / "listening"
)

VOICE = "en-US-AriaNeural"

ENTRIES = [
    ("a2-1.mp3", "Hi, this is a message for Sarah. The train to London leaves at nine fifteen, not nine o'clock. Please arrive at the station by nine."),
    ("a2-2.mp3", "Welcome to City Cafe. Today's special is tomato soup with fresh bread for five dollars. Coffee and tea are available all day."),
    ("a2-3.mp3", "Excuse me, where is the nearest supermarket? Go straight ahead, turn left at the bank, and it's on your right, next to the pharmacy."),
    ("a2-4.mp3", "This is the weather report for tomorrow. It will be sunny in the morning, but rain is expected in the afternoon. Remember to take an umbrella if you go out later."),
    ("b1-1.mp3", "I just got back from my trip to Portugal. I've never eaten such good seafood before! The people were incredibly friendly too, and I'm already planning to go back next year."),
    ("b1-2.mp3", "Good morning, this is an announcement for platform three. The nine forty train to Manchester has been delayed by twenty minutes due to a signal problem. We apologize for the inconvenience."),
    ("b1-3.mp3", "So, I've been thinking about changing jobs. My current one pays well, but I don't feel challenged anymore. I think I need something where I can actually grow and learn new skills."),
    ("b1-4.mp3", "Just to remind everyone, the team meeting has moved from Thursday to Friday this week because the manager is traveling. Please update your calendars and let me know if that's a problem."),
    ("b2-1.mp3", "Honestly, I was skeptical about working from home at first, but after a year I've completely changed my mind. Sure, I miss the casual chats with colleagues, but the flexibility has made a huge difference to my overall wellbeing."),
    ("b2-2.mp3", "The city council meeting ran later than expected last night. Although the budget proposal was approved, several members raised concerns about long-term funding, and a follow-up session has been scheduled for next month to address those issues."),
    ("b2-3.mp3", "When I started this business five years ago, I honestly thought it would fail within six months. What kept it going wasn't some brilliant strategy - it was simply refusing to give up whenever things got difficult."),
    ("b2-4.mp3", "Critics have praised the film's cinematography, though several have noted that the plot drags in the second half. Still, most agree the performances alone make it worth watching."),
    ("c1-1.mp3", "It's tempting to assume that automation simply eliminates jobs, but the reality is more nuanced. While certain roles do disappear, history suggests that new kinds of work tend to emerge alongside the technology, even if that transition is rarely painless for the people caught in the middle of it."),
    ("c1-2.mp3", "What strikes me most about the debate isn't the disagreement over facts, but the disagreement over what even counts as evidence in the first place. Until both sides can agree on that, I doubt the conversation will move forward at all."),
    ("c1-3.mp3", "The proposal sounds appealing on paper, but implementing it would require infrastructure that, quite frankly, doesn't exist yet in most regions. So while I don't doubt the good intentions behind it, I'm skeptical it could be rolled out on the timeline being suggested."),
    ("c1-4.mp3", "It would be a mistake to characterize her resignation as purely a political statement. Colleagues close to her suggest it had as much to do with long-standing frustration over the department's internal culture as with any disagreement over policy."),
]


async def generate(filename: str, text: str) -> None:
    path = OUTPUT_DIR / filename
    communicate = edge_tts.Communicate(text, voice=VOICE)
    await communicate.save(str(path))
    print(f"wrote {path} ({path.stat().st_size} bytes)")


async def main() -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    for filename, text in ENTRIES:
        await generate(filename, text)


if __name__ == "__main__":
    asyncio.run(main())
