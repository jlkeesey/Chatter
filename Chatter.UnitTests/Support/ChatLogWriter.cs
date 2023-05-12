using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Chatter.UnitTests.Support;

public class ChatLogWriter : TextWriter
{
    public List<string> Lines { get; } = new();

    public override Encoding Encoding => Encoding.UTF8;

    public override void WriteLine(string? str)
    {
        Lines.Add(str ?? "?null?");
    }
}