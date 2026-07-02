using System.Text;
using Csbf.Core;

namespace Csbf.Core.Tests;

/// <summary>
/// Test double for <see cref="IVmIo"/>: replays scripted input bytes and
/// captures everything the VM writes. Reads past the end of the input
/// return 0 (the conventional EOF value in Brainfuck).
/// </summary>
internal sealed class CapturingIo(params byte[] input) : IVmIo
{
    private readonly Queue<byte> _input = new(input);
    private readonly List<byte> _output = [];

    public IReadOnlyList<byte> Output => _output;

    public byte Read() => _input.Count > 0 ? _input.Dequeue() : (byte)0;

    public void Write(byte value) => _output.Add(value);

    public string OutputAsString() => Encoding.ASCII.GetString(_output.ToArray());
}
