[![CI](https://github.com/mxmgorin/csbf/actions/workflows/ci.yml/badge.svg)](https://github.com/mxmgorin/csbf/actions)


# csbf
`csbf` is a CLI tool for the Brainfuck language written in C#.
It implements a full language pipeline:

```Source → Parser → IR → VM / Codegen```

> **[Try in your browser](https://mxmgorin.github.io/csbf/)** — run client-side via Blazor WebAssembly (no install).

# Features
- Interactive CLI with REPL
- Peephole optimizer (run-length folding; opposing moves like `><` and `+-` cancel)
- Code generation for emitting target-language source files (e.g. Go)
- Debugger with instruction-index breakpoints and time-travel (reverse stepping)
- Brainfuck parser producing a structured IR
- Virtual machine with step-by-step execution, registers and memory inspection

# Requirements
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (pinned via `global.json`)

# Getting started
Run from source:

```sh
# Start the interactive REPL
dotnet run --project src/Csbf.Cli

# One-shot mode: any REPL command also works as CLI arguments
dotnet run --project src/Csbf.Cli -- run samples/hello_world.bf
dotnet run --project src/Csbf.Cli -- eval '++++++++[>++++++++<-]>+++++.'

# Run the tests
dotnet test
```

> A `dotnet tool` package (`dotnet tool install -g csbf`) is planned; for now, run from source.

# Quickstart: debugging a program
On `load`, source is parsed and optimized, so instruction indices below refer to the
**optimized** ops (inspect them with `bytecode`).

```
> load samples/hello_world.bf
> break 21
breakpoint set at 0x15
> run
hit breakpoint at 0x15
> regs
IP: 0x15
DP: 0x0
CELL: 0x0
> step
0x15: ADD_BYTE 8
> run
Hello World!
```

> Note: `break` takes a **decimal** instruction index, while `regs`/`step`/`bytecode` print
> addresses in hex (so `break 21` sets a breakpoint at `0x15`).

# Commands
| Command | Aliases | Description |
| --- | --- | --- |
| `load <file>` | | Load a Brainfuck source file into the VM |
| `run [file]` | | Optionally load `file`, then run until a breakpoint or program end |
| `step` | | Execute one instruction and print it |
| `back [n]` | `stepb` | Step backward `n` instructions (default 1), restoring tape + registers |
| `break <n>` | | Toggle a breakpoint at instruction index `n` (decimal) |
| `regs` | | Show registers: IP, DP, and the current cell |
| `mem <from> <len>` | | Dump `len` bytes of tape memory starting at offset `from` |
| `eval <src>` | | Load and run inline Brainfuck source |
| `emit <lang> <in> [out]` | | Compile `in` to `<lang>` (currently `go`); prints to stdout, or writes `out` |
| `bytecode` | `bc` | Disassemble the loaded program |
| `help` | | List commands |
| `exit` | `quit` | Exit the CLI |

# Code generation
Compile a Brainfuck program to Go and run it:

```sh
dotnet run --project src/Csbf.Cli -- emit go samples/hello_world.bf hello.go
go run hello.go   # -> Hello World!
```

# References
- [The language](https://www.muppetlabs.com/~breadbox/bf/)
- [Basics of BrainFuck](https://gist.github.com/roachhd/dce54bec8ba55fb17d3a)
- [Wiki](https://en.wikipedia.org/wiki/Brainfuck)
- [Brainfuck Online](https://copy.sh/brainfuck/)
- [Brainfuck Programs](https://brainfuck.org/)
