using Chatter.Model;
using Chatter.System;
using static Chatter.Configuration;

namespace Chatter.Chat;

public interface IChatLogGenerator
{
    /// <summary>
    ///     Creates a new <see cref="IChatLog" /> of the appropriate implementation for the given parameters.
    /// </summary>
    /// <param name="logConfiguration">The <see cref="ChatLogConfiguration" /> defining the log required.</param>
    /// <param name="logFileInfo">The common file information for logs.</param>
    /// <param name="dateHelper">The generator of dates and times.</param>
    /// <param name="fileHelper">The access to the file system.</param>
    /// <param name="myself">The <see cref="IPlayer" /> that defines the current user.</param>
    /// <returns>A new <see cref="IChatLog" /> implementation instance.</returns>
    IChatLog Create(ChatLogConfiguration logConfiguration, LogFileInfo logFileInfo, IDateHelper dateHelper,
        FileHelper fileHelper, IPlayer myself);
}