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

using Chatter.Chat;
using Chatter.Reporting;
using Chatter.System;
using static Chatter.Configuration;

namespace Chatter.UnitTests.Support;

internal class TestChatLog : ChatLog
{
    public TestChatLog(string name) : this(new ChatLogConfiguration(name))
    {
        LogConfiguration.InitializeTypeFlags();
        LogConfiguration.IsActive = true;
    }

    public TestChatLog(ChatLogConfiguration configuration) : this(configuration,
                                                                  new LogFileInfo(),
                                                                  new DateHelperFake(),
                                                                  new FileSystemFake(),
                                                                  new ErrorWriter())
    {
    }

    private TestChatLog(ChatLogConfiguration configuration,
                        LogFileInfo logFileInfo,
                        IDateHelper dateHelper,
                        FileSystemFake fileSystem,
                        IErrorWriter errorWriter) : base(configuration,
                                                         logFileInfo,
                                                         dateHelper,
                                                         new FileHelper(fileSystem),
                                                         errorWriter)
    {
        FileSystem = fileSystem;
        Configuration = new Configuration();
        Configuration.Initialize(FileHelper);
        logFileInfo.UpdateConfigValues(Configuration);
        logFileInfo.StartTime = dateHelper.ZonedNow;
    }

    public Configuration Configuration { get; }

    public FileSystemFake FileSystem { get; }

    public new DateHelperFake DateHelper => (base.DateHelper as DateHelperFake)!;

    public new ChatLogConfiguration LogConfiguration => base.LogConfiguration;

    public new LogFileInfo LogFileInfo => base.LogFileInfo;

    protected override string DefaultFormat => "{6} {4}: {5}";
}
