// node out/serialQueue.test.js (npm test): the queue never runs two jobs at once, keeps their order,
// and a failing job does not stop the ones behind it. No VS Code needed.
import { SerialQueue } from './serialQueue';

let failed = 0;
function check(ok: boolean, what: string): void {
    console.log(`${ok ? 'PASS' : 'FAIL'}  ${what}`);
    if (!ok) {
        failed++;
    }
}

const sleep = (ms: number) => new Promise<void>((r) => setTimeout(r, ms));

async function main(): Promise<void> {
    const errors: unknown[] = [];
    const q = new SerialQueue((e) => errors.push(e));
    let running = 0;
    let maxRunning = 0;
    const order: number[] = [];
    // Nine jobs arriving 1 ms apart, each taking longer than the gap, as on 17-10-2026, with the
    // second-to-last one failing.
    const done: Promise<void>[] = [];
    for (let i = 0; i < 9; i++) {
        done.push(q.enqueue(async () => {
            running++;
            maxRunning = Math.max(maxRunning, running);
            await sleep(15 - i);           // later jobs are faster: a race would reorder them
            order.push(i);
            running--;
            if (i === 7) {
                throw new Error('job 7 failed');
            }
        }));
        await sleep(1);
    }
    check(q.size > 0, 'jobs are still waiting while the first ones run');
    await Promise.all(done);
    check(maxRunning === 1, `never more than one job at a time (saw ${maxRunning})`);
    check(order.join(',') === '0,1,2,3,4,5,6,7,8', `jobs run in arrival order (got ${order.join(',')})`);
    check(errors.length === 1 && String(errors[0]).includes('job 7'), 'a failed job is reported once');
    check(order.includes(8), 'the job behind a failed one still runs');
    check(q.size === 0, 'the queue is empty afterwards');
    console.log(failed === 0 ? 'all passed' : `${failed} failed`);
    process.exit(failed === 0 ? 0 : 1);
}

void main();
