# Benchmark — optimizer passes

Measures the effect of the loop-idiom passes (`clear`, `scan`) on top of the
run-length `collapse` pass.

## Method

- Reproduce with the built-in `bench` command:
  `dotnet run -c Release --project src/Csbf.Cli -- bench <file> [runs]`.
- `bench` runs each pass combination on a fresh VM with a discarding I/O sink and
  **no** step-recording, so timing reflects the interpreter, not console I/O.
- `ops` is the optimized instruction count (deterministic). `steps` is the number
  of executed ops to completion. `time` is the best of N runs. `speedup` is
  relative to `none`.
- `bench` caps each level at 2,000,000,000 steps to stay responsive; the full
  mandelbrot exceeds that, so its runtime figures below come from an **uncapped**
  `run` (output discarded) timed end-to-end instead.

## Results

### mandelbrot (heavy — full render to completion)

Static op count shrinks 3.2× as idioms are lowered:

| pass            | ops    |
| --------------- | ------ |
| none            | 11,451 |
| collapse        | 4,115  |
| collapse+clear  | 3,867  |
| collapse+scan   | 3,867  |
| all             | 3,619  |

Full-render wall-clock (uncapped `run`, output to `/dev/null`):

| passes | time     | speedup |
| ------ | -------- | ------- |
| none   | 299.72 s | 1.00×   |
| all    | 60.30 s  | **4.97×** |

### sierpinski (`bench samples/sierpinski.bf 3`)

| level          | ops | steps   | time (ms) | speedup |
| -------------- | --- | ------- | --------- | ------- |
| none           | 114 | 319,292 | 45.16     | 1.00×   |
| collapse       | 76  | 244,323 | 29.34     | 1.54×   |
| collapse+clear | 72  | 92,023  | 10.78     | 4.19×   |
| collapse+scan  | 72  | 238,484 | 28.32     | 1.59×   |
| all            | 68  | 86,184  | 11.60     | 3.89×   |

### hello_world (`bench samples/hello_world.bf 5`)

| level          | ops | steps | time (ms) | speedup |
| -------------- | --- | ----- | --------- | ------- |
| none           | 129 | 970   | 0.19      | 1.00×   |
| collapse       | 80  | 647   | 0.13      | 1.44×   |
| collapse+clear | 80  | 647   | 0.14      | 1.30×   |
| collapse+scan  | 78  | 535   | 0.08      | 2.22×   |
| all            | 78  | 535   | 0.09      | 2.19×   |

## Takeaways

- The loop-idiom passes give a large, real speedup on loop-heavy programs — ~5× on
  the full mandelbrot, ~3.9× on sierpinski — on top of the `collapse` pass.
- Which pass dominates depends on the program's idioms: sierpinski leans on
  clear loops (`clear` alone is 4.2×), while hello_world is dominated by a single
  scan (`scan` alone is 2.2×).
- Because a lowered `SET`/`SCAN` does in one op what was previously a whole loop,
  the executed-step count drops sharply (mandelbrot's static op count alone falls
  3.2×), which is where most of the wall-clock win comes from.
