// Copyright 2024 James Keesey
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

using System;
using Dalamud.Bindings.ImGui;

namespace Chatter.ImGuiX;

public static class ImGuiWith
{
    public static IDisposable TextWrapPos(float value)
    {
        return new WithTextWrapPosDisposable(value);
    }

    public static IDisposable ItemWidth(float value)
    {
        return new WithItemWidthDisposable(value);
    }

    private sealed class WithTextWrapPosDisposable : IDisposable
    {
        public WithTextWrapPosDisposable(float value)
        {
            ImGui.PushTextWrapPos(value);
        }

        public void Dispose()
        {
            ImGui.PopTextWrapPos();
        }
    }

    private sealed class WithItemWidthDisposable : IDisposable
    {
        public WithItemWidthDisposable(float value)
        {
            ImGui.PushItemWidth(value);
        }

        public void Dispose()
        {
            ImGui.PopItemWidth();
        }
    }
}
