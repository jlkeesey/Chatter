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

using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Chatter.Localization;

/// <summary>
///     A localized (translated) message and it's metadata.
/// </summary>
[PublicAPI]
public struct LocalizedMessage
{
    public LocalizedMessage(string key, string? message = null, string? description = null)
    {
        Key = key;
        Message = message ?? "??[key]??";
#if DEBUG
        Description = description ?? string.Empty;
#else
        Description = string.Empty; // We don't need the description at runtime
#endif
    }

    /// <summary>
    ///     The key used to look up this value. This key will be the same for all localized variations of this message.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    ///     The localized message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     A description of the purpose of this message, i.e. how it is actually used to help with the translation.
    /// </summary>
    /// <remarks>
    ///     This field is never loaded into the application, it is only used by the translators.
    /// </remarks>
    [JsonIgnore]
    public string Description { get; set; }
}
