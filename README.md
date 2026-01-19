[![CI](https://github.com/mxmgorin/csbf/actions/workflows/ci.yml/badge.svg)](https://github.com/mxmgorin/csbf/actions)


# csbf
`csbf` is a CLI tool for the Brainfuck language written in C#.
It implements a full language pipeline:

```Source → Parser → IR → VM / Codegen```

# Features
- Interactive CLI with REPL
- Code generation for emitting target-language source files (e.g. Go)
- Debugger with instruction-index breakpoints
- Brainfuck parser producing a structured IR
- Virtual machine with step-by-step execution, registers and memory inspection

# References
- [The language](https://www.muppetlabs.com/~breadbox/bf/)
