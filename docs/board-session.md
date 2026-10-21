# Board sessions

With several sessions running in one VS Code window, the questions they ask you get lost. Each
one stops at "should I do A or B?", the question scrolls away under the next session's output,
and the session sits there until you happen to look at its tab.

A **board session** is one Claude Code session per window whose only job is to collect those
questions in one place. The other sessions send it every question they are waiting on you for;
the board shows them all, in full, numbered; you answer there ("1 yes, 2 option B"), and the
board carries each answer back to the session that asked. It does no work of its own.

## What TabTower does for it

TabTower does not run the board. The collecting and relaying is done by the session itself,
through Claude Code's own cross-session messages (the SendMessage tool, where your Claude Code
version offers it). What TabTower adds is a card that tells the truth about it.

A board ends every turn with a message to you (its board), so without help its card would read
*your turn* in purple after every turn, empty board or not. A board reports how many questions
are open on it, at the end of every turn:

```
tabtower session board --id <session_id> --open <n>
tabtower session board --id <session_id> --off        # no longer a board
```

When its turn ends, the card shows:

| Board | Card |
|---|---|
| `n` questions open | a pink border, *n questions for you*, a 📋n chip, and it blinks like any session waiting for you; a higher count than before blinks again |
| empty | a steady slate border, *board empty*, a 📋 chip, no blink and no Windows notification |

The status underneath is untouched, and the count is kept across a TabTower restart.

Inside a Claude Code session, the session's own id is in the `CLAUDE_CODE_SESSION_ID`
environment variable, so a board can report itself:

```powershell
tabtower session board --id $env:CLAUDE_CODE_SESSION_ID --open 3
```

A board usually also pins itself, so it is always the first row of its window's block:
`tabtower session pin --id $env:CLAUDE_CODE_SESSION_ID`.

## Setting one up

1. Copy [`examples/board-session/SKILL.md`](examples/board-session/SKILL.md) to
   `~/.claude/skills/board-session/SKILL.md` and adjust it to taste.
2. In the window you want a board for, open a new Claude Code session and start it with
   `board session`. It names itself, pins itself, reports an empty board and waits.
3. Tell your other sessions about it. One paragraph in your `CLAUDE.md` is enough, for example:

   > When you stop to wait for a decision from me, also send the full question to the board
   > session of this window with SendMessage. First line `BOARD-ASK`, then `qid: <a number
   > unique in this session>`, then a line `---`, then the question exactly as you showed it to
   > me. When the question no longer needs me, send `BOARD-WITHDRAW` with its `qid`. An answer
   > arrives as `BOARD-ANSWER` with the `qid` and my words verbatim: act on it as if I had
   > typed it in this session.

The protocol in the example is plain text with a keyword on its first line, so you can change
it freely; the only part TabTower reads is the `session board` count.

## Limits

- The board depends on Claude Code's cross-session messages. If your Claude Code version or
  setup does not offer them, the sessions cannot reach the board; the card side still works.
- The board knows only what the sessions send it. A question asked before the board existed,
  or by a session that does not follow the paragraph above, is not on it.
- An answer reaches a session as a message, which starts a turn there, as if you had typed it.
