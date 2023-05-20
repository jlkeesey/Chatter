// Copyright 2023 James Keesey
// 
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS”
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Chatter.System;

namespace Chatter.Chat;

/// <summary>
///     Defines the handler for a single log file.
/// </summary>
public interface IChatLog
{
    /// <summary>
    ///     <c>true</c> if this log is open i.e. there is an open file for this log.
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    ///     The name of the file this log is writing to. This is only valid if <see cref="IsOpen" /> is <c>true</c>.
    ///     If <see cref="IsOpen" /> is false this may return a previous name of no name at all.
    /// </summary>
    string FileName { get; }

    /// <summary>
    ///     Dumps the information about this logger to the dev log.
    /// </summary>
    /// <param name="logger">Where to send the information.</param>
    void DumpLog(ILogger logger);

    /// <summary>
    ///     Closes this log if open.
    /// </summary>
    void Close();

    /// <summary>
    ///     Logs the chat information to the target log.
    /// </summary>
    /// <remarks>
    ///     The configuration defines whether the given message should be logged as well as what massaging of the data
    ///     is required.
    /// </remarks>
    /// <param name="chatMessage">The chat message information.</param>
    void LogInfo(ChatMessage chatMessage);

    /// <summary>
    ///     Determines if the give log information should be sent to the log output by examining the configuration.
    /// </summary>
    /// <param name="chatMessage">The chat message information.</param>
    /// <returns><c>true</c> if this message should be logged.</returns>
    public bool ShouldLog(ChatMessage chatMessage);

    /// <summary>
    ///     Formats the chat message using this logs format string and sends it to the log output.
    /// </summary>
    /// <param name="chatMessage">The chat message information.</param>
    /// <param name="sender">The sender after processing based on the configuration.</param>
    /// <param name="body">The body after processing based on the configuration.</param>
    public void WriteLog(ChatMessage chatMessage, string sender, string body);
}
