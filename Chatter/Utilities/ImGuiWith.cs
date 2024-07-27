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
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;

namespace Chatter.Utilities;

public static class ImGuiWith
{
    public static IDisposable Disabled(bool flag)
    {
        return new WithDisabledDisposable(flag);
    }

    public static IDisposable Color(ImGuiCol idx, Vector4 value)
    {
        return new WithColorDisposable(idx, value);
    }

    public static IDisposable Color(ImGuiCol idx, uint value)
    {
        return new WithColorDisposable(idx, value);
    }

    public static IDisposable Style(ImGuiStyleVar styleVar, Vector2 value)
    {
        return new WithStyleDisposable(styleVar, value);
    }

    public static IDisposable Style(ImGuiStyleVar styleVar, float value)
    {
        return new WithStyleDisposable(styleVar, value);
    }

    public static IDisposable TextWrapPos(float value)
    {
        return new WithTextWrapPosDisposable(value);
    }

    public static IDisposable Font(ImFontPtr value)
    {
        return new WithFontDisposable(value);
    }

    public static IDisposable ItemWidth(float value)
    {
        return new WithItemWidthDisposable(value);
    }

    public static IDisposable ID(int value)
    {
        return new WithIDDisposable(value);
    }

    public static IDisposable ID(string value)
    {
        return new WithIDDisposable(value);
    }

    private sealed class WithDisabledDisposable : IDisposable
    {
        private readonly bool _flag;

        public WithDisabledDisposable(bool flag)
        {
            _flag = flag;
            if (_flag) ImGui.BeginDisabled();
        }

        public void Dispose()
        {
            if (_flag) ImGui.EndDisabled();
        }
    }

    private sealed class WithColorDisposable : IDisposable
    {
        public WithColorDisposable(ImGuiCol idx, Vector4 value)
        {
            ImGui.PushStyleColor(idx, value);
        }

        public WithColorDisposable(ImGuiCol idx, uint value)
        {
            ImGui.PushStyleColor(idx, value);
        }

        public void Dispose()
        {
            ImGui.PopStyleColor();
        }
    }

    private sealed class WithStyleDisposable : IDisposable
    {
        public WithStyleDisposable(ImGuiStyleVar styleVar, Vector2 value)
        {
            ImGui.PushStyleVar(styleVar, value);
        }

        public WithStyleDisposable(ImGuiStyleVar styleVar, float value)
        {
            ImGui.PushStyleVar(styleVar, value);
        }

        public void Dispose()
        {
            ImGui.PopStyleVar();
        }
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

    private sealed class WithFontDisposable : IDisposable
    {
        public WithFontDisposable(ImFontPtr value)
        {
            ImGui.PushFont(value);
        }

        public void Dispose()
        {
            ImGui.PopFont();
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

    private sealed class WithIDDisposable : IDisposable
    {
        public WithIDDisposable(int value)
        {
            ImGui.PushID(value);
        }

        public WithIDDisposable(string value)
        {
            ImGui.PushID(value);
        }

        public void Dispose()
        {
            ImGui.PopID();
        }
    }
}
