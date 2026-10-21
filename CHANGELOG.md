# Changelog

Every version of TabTower, newest first. Versions up to v0.9.9 shipped under the
project's former name, SessionDeck.

Only the current release stays on the
[Releases page](https://github.com/tabtower/tabtower/releases) - older ones are deleted
so the page never accumulates self-contained zips nobody downloads. Their **tags survive**,
so any version below can be rebuilt from source with `git checkout v<version>` followed by
a publish. This file is the history the Releases page cannot hold; `release.ps1` prepends
to it automatically.

<!-- new releases are inserted directly below this line -->

## v0.11.44 - unreleased

- fix(cards): a VS Code window that its extension already reports is no longer turned into a
  second card when it is dragged over the deck. With a custom `window.title` the drag-in found
  no card whose title pattern matched, so it cut a name from the title and made a card with no
  folder: no hook could fill its path (they all resolve to the real card by folder) and no
  rebind could match it, so it read "no window" for good while the window was open. The drag-in
  now refuses such a window and names the card that has it. A folderless card of that kind that
  already exists (never used, no sessions, nothing set on it, and its window reported by the
  connector of a card with a folder) is retired on the 10 s metadata tick, with one log line.

## v0.11.43 - unreleased

- security(phone): a paired phone now makes the PC run a tasks-file link only when the link is a
  file or folder inside the tasks file's own folder, or below it. The path is made canonical
  first, so a `..` escape is caught; network shares, `\\?\` and `\\.\` paths, relative paths,
  alternate data streams and `something:` links are refused, and a sibling folder that shares
  the name prefix does not count as inside. A refused link answers `not_allowed` (409) with the
  reason, the page explains it, and the deck log gets one line. A click on the PC still opens
  any link. The rule is `Services/Phone/PhoneLinkPolicy.cs`; the phone suite covers it.

## v0.11.42 - unreleased

- fix(cards): a group card (one VS Code instance of a folder shared by several) now shows the
  live preview of its own window. It used to hide the preview band altogether, because a custom
  `window.title` per instance matches no title pattern and the card had no window to show. The
  window is found by the group's title marker: the connector's window once focus identified it,
  otherwise a window whose title carries the marker and names the folder. A group card with no
  such window keeps the band collapsed. Clicking a group card raises that same window, and its
  "Close window" item closes it rather than whatever window the parent card was bound to.

## v0.11.41 - unreleased

- docs: plain punctuation in the `manual-verify` and `release` skills under `.claude/skills/`.
  CHANGELOG.md marks v0.11.19, and the versions its release bundled, as released on 2026-10-11.

## v0.11.39 - unreleased

- fix(sessions): a session opened or resumed while the deck was not running fired its first
  hook into nothing and stayed off the deck until it fired another one, which an idle session
  never does (every VS Code start that restores its sessions before the deck is up). At startup,
  and once more a minute later, the deck now reads Claude Code's live-session registry and gives
  a card to each session whose identity is proven: a valid id, a running process whose start time
  matches the record, an interactive VS Code extension session, and a transcript on disk. A card
  the deck closed on a guess (orphaned, stale, exited) is reopened when its process is running; a
  session ended by its own hook, or replaced, is left to its next hook.
  `tests/session-recovery.tests.ps1` covers the rules and runs the reader over a fixture registry
  backed by real hidden processes.

## v0.11.38 - unreleased

- feat(deck): in a VS Code window that keeps a question board, a turn started by a message from
  another session no longer ends as "your turn": the card says "answered another session" in its
  own quiet colour and does not blink. A question for you goes on the board in such a window, so
  that turn's end asks nothing of you. Outside a window with a board nothing changes. The phone
  page words and colours it the same and counts it in no chip.

## v0.11.37 - unreleased

- fix(phone): the phone page shows a session the way its deck card does. An empty question board
  read "your turn", and so did a session waiting on a run, a monitor or a background job of its
  own; they now read "board empty" and name what they wait for, in the deck's colours. Pinned
  sessions come first, with a pin mark, and the chips count what the cards show.

## v0.11.35 - unreleased

- fix(close-tab): `session end --close-tab` and `session close-tab` could close a different,
  live tab. The connector ran every command from the deck the moment it arrived, so a burst of
  close-by-id requests to one window ran side by side; each one reads the window's active tab
  as the result of its own reveal, and one took another's hand-back for its own and closed a
  tab that no request had named. The connector (0.7.1) now runs commands one at a time, in
  order. And the deck no longer reveals a session whose Claude Code process is not running
  (read from Claude Code's live-session registry, process start time included): that is a
  no-op with a log line, no tab is touched, and the answer says so. A live session's tab is
  closed by id as before. `npm test` in `vscode-extension/` checks the queue, and the phone
  harness checks the deck's decision.
- docs: the README covers the cache-warmth countdown and row colours, pinning a session, board
  sessions, handing off to a fresh session, and the phone's Tasks tab; it states that TabTower
  makes no network calls of its own. `docs/phone-access.md` and `SECURITY.md` say what a paired
  phone can make the PC do, and `docs/board-session.md` explains board sessions with an example
  skill.

## v0.11.34 - unreleased

- feat(deck): pin a session to the top of its window. Right-click a session row: "Pin to the
  top of its window" / "Unpin". A pinned open session heads its window's block on the card with
  a pin chip and stops moving with activity; with two pins in one window the newer one is first,
  without taking the older one's pin away. A closed session ignores its pin and gets it back on
  resume. CLI: `tabtower session pin --id <sid> [--off]`. Not to be confused with
  `tabtower pin <target>`, which moves a window to the Stage.

## v0.11.33 - unreleased

- fix(deck): the borders of a card waiting on a machine (light blue) and of a forked session
  (red) drew plain gray: both colours were written as #AARRGGBB, which the colour parser does
  not read. They are #RRGGBB now. The same fix applies to the board-session colours below.

## v0.11.32 - unreleased

- feat(board): board sessions. A session whose job is to collect the questions the other
  sessions in its window are waiting on, and to carry the answers back, ends every turn with a
  message to you, so its card read "your turn" after every turn whether or not anything was
  open. It can now report its count: `tabtower session board --id <sid> --open <n>` (or
  `--off`). When its turn ends, the card reads "n questions for you" with a pink border, a
  board chip with the count, and blinks; an empty board reads "board empty" in steady slate,
  with no blink and no notification. A higher count than before blinks again. The count is
  kept across a restart. See `docs/board-session.md`.

## v0.11.30 - unreleased

- fix(deck): a background task (an agent, a shell, a monitor) that finished after a session
  was closed out woke the session, and the deck read that wakeup as real activity: the
  `wrapped` mark gave way to `working`, and the reply's Stop left the card on "your turn" for
  good. A wakeup that carries a task notification is now held off `wrapped` and `replaced`,
  like the Stop of the same turn already was. A prompt you type, a message from another
  session, a wait and an error still clear it.

## v0.11.26 - unreleased

- feat(phone): a task session opened from the phone returns the task's opening phrase, and the
  toast offers "Copy it and open in the app": VS Code types the phrase into the new tab without
  sending it, which is right at the desk but left a phone user with an empty session in the app.

## v0.11.25 - unreleased

- feat(phone): the window picker of the Tasks tab says which windows are up, which the PC
  starts first, and which cannot be started; the active navigation square scrolls into view.

## v0.11.24 - unreleased

- feat(phone): a Tasks tab on the phone page, mirroring the PC's tasks page: the level on
  screen, the top-level navigation squares, a Run box that opens a task by its number, and a
  "Session" button per card with a window picker per session group (the same window a click on
  the PC with that key held would use). A card's link is followed on the PC when it is not a
  web address (so both screens move together) and opened on the phone when it is. The phone
  sends a card's id and a window, never a link or any text: the launch phrase and the link are
  read from the tasks file on the PC. New endpoints `GET /api/tasks`, `GET /api/tasks/find`,
  `POST /api/tasks/start` and `POST /api/tasks/nav`, and 17 new harness checks.

## v0.11.23 - unreleased

- feat(deck): once the prompt cache has expired on a context of 50k or more, the session row
  takes a steady light-blue tint beside the snowflake chip: the next message writes the whole
  context into the cache again.

## v0.11.22 - unreleased

- feat(deck): in the last five minutes before the prompt cache expires, the session row pulses
  yellow behind its text and the chip reads "cools in 8m". Only for contexts of 50k or more;
  yellow, never orange, which is the waiting status.

## v0.11.21 - unreleased

- feat(deck): a session card shows how long its prompt cache stays warm while you hold the next
  move: a muted countdown, yellow in the last 15 minutes, and a snowflake with the context size
  once it has expired. The clock runs from the start of the newest request, with the lifetime
  of the newest cache write (one hour, or five minutes). Contexts under 50k never warn.
- fix(tokens): Claude Opus 5.5 cache reads weigh 0.05 of an input token in the token count
  ($0.20 against $4 per million), not 0.1.

## v0.11.20 - unreleased

- fix(deck): clicking the card of a session whose tab was closed, in a window that is still
  open, resumed it with `claude --resume` in a terminal. The deck now records which VS Code
  window last held each session's tab, and when that same window answers the click the session
  resumes into a Claude Code panel there. A window that died or reloaded keeps the terminal
  route, and so does a session still running in a terminal the deck opened.

## v0.11.19 - 2026-10-11

- chore(build): the download no longer includes `TabTower.pdb`, a debug file that recorded the
  folder the build ran in, and `TabTower.dll` no longer carries the path of that folder either.
  A Release build now makes no debug symbols. The app itself is unchanged: the compiled code is
  the same, method for method. A Debug build keeps its symbols.

## v0.11.18 - 2026-10-11, released in v0.11.19

- fix(install): installing rewrote the whole user PATH with every `%VARIABLE%` expanded and
  changed its registry type from REG_EXPAND_SZ to REG_SZ; uninstalling did the same. Both
  scripts now edit the registry value itself: one entry in, the same entry out, every other
  character and the value's type exactly as they were, and nothing written when there is
  nothing to change. `tests/user-path.tests.ps1` proves it against a scratch registry key.
  An install of 0.11.17 or earlier has already changed the PATH of that machine; this fix
  stops it from happening again and does not put the earlier text back.
- fix(cli): `session end --close-tab` on a session that had already ended answered with
  advice to pass `--close-tab` to `session end`, the command that had just been run, and
  said the tab was left open without knowing it. It now says that the session had already
  ended, that its tab was not asked for, and what to do if the tab is still open.
- docs(launch): the hero, the deck image, the tasks page and the blink clip are captures of
  0.11.17, in the app's own colours. Two images are new: the deck with window previews on,
  which is what a first run shows, and the token chip's hover text. The README says that
  the clip and the compact deck were recorded with previews switched off.
- docs(launch): the landing page plays a second video file in wide windows, the same take at
  twice the frame size, so the thin card borders keep the colours the app draws. Narrow
  windows keep the 1280 px file.
- docs: a click on a session "stops the blink" (the README said it "clears the alert"; the
  row stays orange until the prompt is answered). The landing page's legend dots are the
  app's own state colours. The README says that the `done` state reads "your turn" on the
  deck, and that the titles of a task list share one side.

## v0.11.17 - 2026-10-11, released in v0.11.19

- fix(cards): the line under a session waiting on a permission dialog had three wordings,
  depending on which source had spoken last: the `PermissionRequest` hook (`Bash: npm test`),
  the transcript scanner (`Waiting for permission: Bash`) and Claude Code's own notification
  (`Claude needs your permission to use Bash`). All three now open with the same words, on the
  deck and on the phone page. The stable line is `Waiting for permission: <tool>`. While only
  the hook has spoken, for up to ten seconds, the command it knows follows the tool:
  `Waiting for permission: Bash: npm test`.
- feat(tasks): in a task list where no title has a right-to-left letter, the names are drawn
  left-aligned and start on one vertical line. A list with any right-to-left title keeps the
  fixed right-aligned header on every card, exactly as before. The side is chosen once for the
  whole list, on every task in the file, so a search cannot change it.
- fix(deck): the search box was about 5 px wide on a narrow deck, such as a 40% zone. With no
  tasks file it now takes the whole row. With one, the Run box drops to a second line while the
  row is too narrow for both. The search box is at least 120 px at any window size.
- fix(ui): the token chip's tooltip says what its two numbers are: the tokens used so far,
  weighted by price, and how full the context window is now. "VS Code" is written as two words
  in the status line, the menus, the tooltips and the installer's output. The window-preview
  menu item said "off by default", although the preview has been on by default since 0.11.3.
- chore(ui): plain punctuation in the app's own text (tooltips, the status line, dialogs, the
  tasks-file contract behind Copy spec) and in the CLI's messages. The `list` output keeps its
  format, because scripts read it.
- docs: launch material. The README opens with a short recording, has new deck and tasks-page
  images and writes "VS Code" in its prose; the landing page under `site/` plays the same
  recording, gains a tile for the tasks page and no longer leads with window previews;
  CONTRIBUTING, SECURITY and the issue and pull-request templates are in. `hooks/README.md`,
  `ARCHITECTURE.md` and the public `CLAUDE.md` use plain punctuation. The social card from
  before the rename is removed; the card in use is under `assets/social/`.

## v0.11.15 - 2026-10-11, released in v0.11.19

- fix(cards): with the Claude in Chrome extension connected, every session's second line (on the
  deck and on the phone page) showed the start of the `<browser_instruction>` block Claude Code
  puts in front of each prompt, and the card title skipped those prompts. Complete leading
  blocks of that kind (`<browser_instruction>`, `<system-reminder>`, `<ide_selection>`,
  `<command-...>`, `<pasted_content>`) are now skipped, in the hook and in the app, and the
  user's own words are shown. A prompt that is only such a block, or that merely starts with
  `<`, is shown as before; a cut-off block saved by an older hook is no longer shown.

## v0.11.14 - 2026-10-11, released in v0.11.19

- fix(install): the installer showed an old hooks version after a version bump that was not
  cut by `release.ps1`, because only that script updated the `# Version:` header of
  `hooks/tabtower-hook.ps1`. The header now moves with every bump, and the build stops with a
  clear message while it differs from the app version.
- chore(release): the notes of a release that bundles several unreleased versions are
  taken from their sections in CHANGELOG.md instead of being written by hand.

## v0.11.13 - 2026-10-11, released in v0.11.19

- fix(phone): Reopen from the phone resumed the session with `claude --resume` in a VS Code
  terminal, so it ran with no Claude Code tab and its card showed no tab. A session with no
  tab in its window was always sent to the terminal, a route meant for a session whose window
  died. Reopen from the phone now resumes it in a Claude Code panel in its own window, the
  way New opens one. A click on the deck keeps its current behaviour.

## v0.11.12 - 2026-10-11, released in v0.11.19

- fix(phone): a session opened from the phone and closed before its first message vanished
  instead of appearing under "Recently closed", so there was nothing to reopen and Reopen
  looked broken. Such a session has no conversation to resume and the deck drops it on close,
  as it always did; the phone now says so when it closes one. An open "Recently closed" list
  refreshes after every close, so a session just closed is there to reopen at once. Every
  phone close and reopen now logs its outcome, and a refused one (bad id, busy, unknown
  route) logs why.

## v0.11.11 - 2026-10-11, released in v0.11.19

- fix(phone): a session opened from the phone did not appear in the phone's list until its
  first message, because a session with no transcript yet counts as a phantom and phantoms
  were hidden. Now a phantom is listed when it is evidently real (a live Claude Code process
  registered it, or it was opened from the phone), marked "new, no message yet"; a spare id
  with neither stays hidden, and headless runs stay hidden. After New, the page refreshes every
  3 seconds until the new row carries its Claude app link. Close follows the same rule.

## v0.11.10 - 2026-10-11, released in v0.11.19

- feat(phone): optional phone page, off by default (gear menu, Phone access). Served by
  TabTower itself on 127.0.0.1 and reached through `tailscale serve`. Lists open sessions as
  the deck groups them; opens a new session (confirmed on the phone, returns the real session
  id), closes a session with its tab, reopens a recently closed one, and links to the Claude
  app when Remote Control published a link. Every new device is approved on the PC with a match
  code and can be revoked in Phone access settings; Funnel traffic is refused. English, plus
  Hebrew (right to left) when the phone's language asks for it. Docs: docs/phone-access.md.
- fix(phone): hardening after review. Pairing is approved by TYPING the code shown on the
  device into the PC prompt (a look-alike device name cannot win), closing the prompt denies,
  and at most two requests wait at once. The Host must be a loopback name or exactly this PC's
  own tailnet name; with no X-Forwarded-For only a loopback Host is trusted (a tcp or
  tls-terminated forward is refused). Only listed sessions can be closed. At most 32
  connections at a time; Transfer-Encoding must be exactly chunked and Content-Length digits
  only. A failed `tailscale status` is cached for 10 seconds. Reopen no longer asks first;
  new and close still do.

## v0.9.9 - 2026-08-09

- chore: sync hook script version header to 0.9.9
- fix(sessions): close a workspace's sessions when its VSCode window exits (T-0362)

## v0.9.8 - 2026-08-09

- chore: sync hook script version header to 0.9.8
- fix(zone): remove the full-screen zone, cap custom at 90% (T-0364)

## v0.9.7 - 2026-08-09

- chore: sync hook script version header to 0.9.7
- feat(ui): dark native title bar matching the card header (T-0361)
- docs(claude): remove the hazak-uvaruch chapter (T-0337)
- docs(claude): add hazak-uvaruch task-finish chapter (T-0337)

## v0.9.6 - 2026-08-05

- chore: sync hook script version header to 0.9.6
- chore(release): one release on GitHub, permanent tags, CHANGELOG.md
- chore: prepare the repo and the Marketplace listing for promotion (T-0342)

## v0.9.5 - 2026-08-05

- chore: sync hook script version header to 0.9.5
- fix: sweep ghost sessions whose transcript was declared but never written

## v0.9.4 - 2026-08-04

- fix: sync the hook version header, and stop hardcoding a version in the README
- docs: sync the README status line to v0.9.4
- docs: say up front what SessionDeck expects you to be running
- docs: rebuild the README around real screenshots (T-0240)
- fix(sessions): remove ghost sessions that never carried a transcript path (v0.9.4)

## v0.9.3 - 2026-08-04

- chore(extension): sync package-lock to the 0.6.11 bump
- fix(extension): give the VSIX a repository field so vsce can package it (v0.9.3)

## v0.9.2 - 2026-08-04

- docs: replace the executed plan documents with CLAUDE.md and skills (v0.9.2)

## v0.9.1 - 2026-08-04

- docs(i18n): translate the docs to English, stage 2 of T-0239 (v0.9.1)

## v0.9.0 - 2026-08-04

- feat(i18n): move the whole UI to English, stage 1 of T-0239 (v0.9.0)

## v0.8.1 - 2026-08-04

- fix(hooks): scope the hook-confirmed dialog to its own pending call
- docs(hooks): describe the scan-mark bound and the subagent limitation
- fix(hooks): bound the permission-dialog hold to one scan cycle
- fix(hooks): hold a PermissionRequest wait until the scanner corroborates it

## v0.8.0 - 2026-08-04

- feat(hooks): PermissionRequest + StopFailure + Elicitation (7 -> 11 events)

## v0.7.9 - 2026-08-03

- feat(connector): SessionDeck logo in the extension icon and the Output log

## v0.7.8 - 2026-08-02

- chore: sync hook script version header to 0.7.8
- v0.7.8: bump for the docs release

## v0.7.7 - 2026-08-02

- docs: repo is public - correct the stale visibility facts
- fix(release): vsce stderr warning aborted the release under EAP=Stop
- v0.7.7: titled sessions don't match prompt-based tab labels (T-0313)

## v0.7.6 - 2026-08-02

- v0.7.6: empty string instead of null for non-nullable Detail (T-0313)

## v0.7.5 - 2026-08-02

- v0.7.5: fix fork-phantom waiting/working card states (T-0313)

## v0.7.4 - 2026-07-28

- v0.7.4: pressed look for card toggle buttons (T-0236)

## v0.7.3 - 2026-07-27

- chore: sync hook script version header to 0.7.3
- v0.7.3: slim dark scrollbars + tasks-page header polish (T-0234)

## v0.7.2 - 2026-07-27

- v0.7.2: rounded corners on every button (T-0233)

## v0.7.1 - 2026-07-27

- v0.7.1: tasks button left of pin + header overflow fixes (T-0233)

## v0.7.0 - 2026-07-27

- fix: order-insensitive pending-call scan in TranscriptReader
- v0.7.0: read-only tasks panel fed by an external JSON file (T-0116)

## v0.6.43 - 2026-07-27

- chore: sync hook script version header to 0.6.43
- v0.6.43: apply Stage on bind when the pinned workspace had no open window

## v0.6.42 - 2026-07-26

- v0.6.42: orphan-session reconciliation - close sessions whose host died without SessionEnd

## v0.6.41 - 2026-07-26

- v0.6.41: release.ps1 - skip local tag delete when the tag is already gone

## v0.6.40 - 2026-07-26

- chore: sync hook script version header to 0.6.40
- v0.6.40: remove empty VSCode warmup sessions from workspace history

## v0.6.39 - 2026-07-22

- v0.6.39: hide DWM thumbnail with a clean call when card scrolls out

## v0.6.38 - 2026-07-22

- v0.6.38: diagnostic logging (issues 2+3 groundwork)

## v0.6.37 - 2026-07-22

- v0.6.37: enforce a usable minimum width (window + zone) and smarter icon wrap

## v0.6.36 - 2026-07-21

- v0.6.36: rebind window to the right card on same-window Open Folder

## v0.6.35 - 2026-07-21

- v0.6.35: clip DWM thumbnails to the scroll viewport

## v0.6.34 - 2026-07-21

- chore: sync hook script version header to 0.6.34
- v0.6.34: release policy - one release per major.minor line

## v0.6.33 - 2026-07-21

- fix(release.ps1): publish into a clean output dir
- v0.6.33 packaging: release.ps1 - one-command GitHub release
- v0.6.33: zone mode stretches visible frame to fill the full zone

## v0.6.32 - 2026-07-21

- v0.6.32: zone quarters/custom modes + lock; UI style pass

## v0.6.29 - 2026-07-20

- v0.6.29: packaged release - install-hooks, quit, installer scripts

## v0.6.28 - 2026-07-20

- docs: retarget the packaging plan at v0.6.29
- docs: add the two silent-failure gaps the installer must fix
- v0.6.28: stop auto-acknowledge from silencing a blink after the user left the tab

## v0.6.27 - 2026-07-20

- docs: add packaging and distribution plan for v0.6.28
- docs: add MIT license, .gitattributes, accurate install steps
- docs: add README for the public GitHub repo
- v0.6.27: withdraw the Windows notification when its cause is gone

## v0.6.26 - 2026-07-20

- v0.6.26: escalate attention to Windows when the deck can be buried

## v0.6.25 - 2026-07-20

- v0.6.25: card title follows the VSCode tab label

## v0.6.24 - 2026-07-20

- v0.6.24: correlate tabs by a candidate set, not one title

## v0.6.23 - 2026-07-20

- v0.6.23: neutral flags redesign + app-wide RTL handling

## v0.6.19 - 2026-07-20

- v0.6.19: detect "waiting" from the transcript - VSCode UI fires no Notification hook

## v0.6.16 - 2026-07-20

- v0.6.16: add "copy path to clipboard" to workspace card menu

## v0.6.15 - 2026-07-20

- v0.6.15: fix auto-ack lost to tab-title drift + split-group false acks

## v0.6.14 - 2026-07-19

- v0.6.14: status-bar summary dots, toggle agent-prompt copy button, app icon

## v0.6.13 - 2026-07-19

- v0.6.13: custom toolbar toggles - flag files for external hooks, CLI, GUI editor

## v0.6.12 - 2026-07-19

- v0.6.12: settings toggle for collapsing VSCode panels on session open + RTL settings menu

## v0.6.11 - 2026-07-19

- v0.6.11: pin toggle (always on top) + pressed-state UI for pin and search toggles

## v0.6.10 - 2026-07-19

- v0.6.10: release DWM thumbnail when card is collapsed (search filter / hide)

## v0.6.9 - 2026-07-19

- v0.6.9: search/filter row - workspaces+sessions by fields, optional transcript content search

## v0.6.8 - 2026-07-19

- v0.6.8: session card slimmed - secondary title removed, tooltip trimmed to detail+times, tooltip RTL

## v0.6.7 - 2026-07-19

- v0.6.7: RTL for the no-window placeholder text

## v0.6.6 - 2026-07-19

- v0.6.6: Stage "full" performs a real maximize

## v0.6.5 - 2026-07-19

- v0.6.5: + New Session button on workspace cards

## v0.6.4 - 2026-07-19

- v0.6.4: workspace card actions consolidated into one menu + close-window action

## v0.6.3 - 2026-07-19

- v0.6.3: phantom sessions hidden + auto-closed (stale)

## v0.6.2 - 2026-07-19

- v0.6.2: tab switch bumps session to top (extreme activity sort)

## v0.6.1 - 2026-07-19

- v0.6.1: /rename support, truncated-label matching, question-form hooks, bottom status bar

## v0.6.0 - 2026-07-19

- v0.6.0: tab titles as primary, reliable session-tab correlation, auto-acknowledge, history, RTL

## v0.5.0 - 2026-07-19

- Stage D: VSCode extension connector, tab open/resume from deck, transcript titles (v0.5.0)

## v0.4.0 - 2026-07-19

- Stage C: Workspace/Session Cards, hooks integration, session engine (v0.4.0)

## v0.3.0 - 2026-07-17

- Rename WinGrid -> SessionDeck (v0.3.0, decisions 19-20)
