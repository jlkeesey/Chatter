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

using Chatter.Model;
using Chatter.System;
using static Chatter.Configuration;

namespace Chatter.Chat;

public interface IChatLogGenerator
{
    /// <summary>
    ///     Creates a new <see cref="IChatLog" /> of the appropriate implementation for the given parameters.
    /// </summary>
    /// <param name="configuration">The <see cref="Configuration" /> defining the log required.</param>
    /// <param name="logConfiguration">The <see cref="ChatLogConfiguration" /> defining the log required.</param>
    /// <param name="logFileInfo">The common file information for logs.</param>
    /// <param name="dateHelper">The generator of dates and times.</param>
    /// <param name="fileHelper">The access to the file system.</param>
    /// <param name="myself">The <see cref="IPlayer" /> that defines the current user.</param>
    /// <returns>A new <see cref="IChatLog" /> implementation instance.</returns>
    IChatLog Create(Configuration configuration,
                    ChatLogConfiguration logConfiguration,
                    LogFileInfo logFileInfo,
                    IDateHelper dateHelper,
                    FileHelper fileHelper,
                    IPlayer myself);
}
