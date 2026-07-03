using System.Text;
using Csbf.Core;

namespace Csbf.Web;

/// <summary>
/// Browser-side <see cref="IVmIo"/>: buffers output and feeds input on demand.
/// Input can be pre-seeded and topped up interactively while a program runs.
/// </summary>
public sealed class WebIo : IVmIo
{
    private readonly Queue<byte> _input;
    private readonly List<byte> _output = [];

    public WebIo(string input = "")
        => _input = new Queue<byte>(Encoding.UTF8.GetBytes(input));

    /// <summary>Set once the user signals end-of-input; then reads return 0.</summary>
    public bool AtEof { get; private set; }

    public bool HasInput => _input.Count > 0;

    public string Output => Encoding.UTF8.GetString(_output.ToArray());

    public byte Read() => _input.Count > 0 ? _input.Dequeue() : (byte)0;

    public void Write(byte value) => _output.Add(value);

    public void Feed(string text)
    {
        foreach (var b in Encoding.UTF8.GetBytes(text))
        {
            _input.Enqueue(b);
        }
    }

    public void SignalEof() => AtEof = true;
}
