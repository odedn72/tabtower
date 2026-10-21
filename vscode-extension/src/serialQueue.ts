// One-at-a-time runner for the commands TabTower pushes down the pipe (0.7.1).
//
// Why it exists: every command that touches tabs (openSession, newSession, closeSession) works by
// moving the window's ACTIVE tab and then reading which tab is active, and the active tab is one
// value shared by the whole window. Run side by side, one command reads the result of another's
// move as its own. On 17-10-2026 TabTower sent nine closeSession-by-id requests to one window
// within a second; the extension started each the moment it arrived, their reveals and hand-backs
// interleaved, and one of them closed a live tab that none of the nine had named. Kept free of the
// vscode module so a plain node script can test it (serialQueue.test.ts).

export type Job = () => Promise<void>;

export class SerialQueue {
    private tail: Promise<void> = Promise.resolve();
    private pending = 0;

    /// @param onError called with a job's failure; the queue moves on to the next job either way.
    constructor(private readonly onError: (e: unknown) => void) { }

    /// Jobs waiting or running right now.
    get size(): number {
        return this.pending;
    }

    /// Run `job` after every job enqueued before it has settled. Resolves when `job` has.
    enqueue(job: Job): Promise<void> {
        this.pending++;
        const run = this.tail.then(job).catch((e) => this.onError(e)).finally(() => { this.pending--; });
        this.tail = run;
        return run;
    }
}
