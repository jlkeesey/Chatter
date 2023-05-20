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

using System;
using System.Collections.Generic;

namespace Chatter.Model;

/// <summary>
///     Represents a single FFXIV friend. FFXIV has it's own type but it's not easily consumable by C# so
///     when a friend is loaded from FFXIV it is converted to this object type.
/// </summary>
public class Friend : IPlayer, IComparable<Friend>, IComparable
{
    /// <summary>
    ///     This friend's content id.
    /// </summary>
    public readonly ulong ContentId;

    /// <summary>
    ///     The <see cref="World" /> this friend is currently on.
    /// </summary>
    public readonly World CurrentWorld;

    /// <summary>
    ///     This friend's free company tag.
    /// </summary>
    public readonly string FreeCompany;

    /// <summary>
    ///     True if this friend is online.
    /// </summary>
    public readonly bool IsOnline;

    public Friend(ulong contentId, string name, string freeCompany, World homeWorld, World currentWorld, bool isOnline)
    {
        ContentId = contentId;
        Name = name;
        FreeCompany = freeCompany;
        HomeWorld = homeWorld;
        CurrentWorld = currentWorld;
        IsOnline = isOnline;
    }

    /// <summary>
    ///     This friend's home <see cref="World" />.
    /// </summary>
    public World HomeWorld { get; }

    /// <summary>
    ///     This friend's name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     This friend's full name which is the <see cref="Name" /> and <see cref="HomeWorld" /> combined.
    /// </summary>
    public string FullName => $"{Name}@{HomeWorld.Name}";

    #region Equality

    public override bool Equals(object? other)
    {
        return other is Friend rhs && Equals(rhs);
    }

    protected bool Equals(Friend other)
    {
        return Name == other.Name
            && HomeWorld.Equals(other.HomeWorld)
            && FreeCompany == other.FreeCompany
            && ContentId == other.ContentId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, HomeWorld, FreeCompany, ContentId);
    }

    public static bool operator ==(Friend? left, Friend? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Friend? left, Friend? right)
    {
        return !Equals(left, right);
    }

    #endregion

    #region IComparable

    public int CompareTo(Friend? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other == null) return 1;
        var nameComparison = string.Compare(Name, other.Name, StringComparison.Ordinal);
        if (nameComparison != 0) return nameComparison;
        var homeWorldComparison = string.Compare(HomeWorld.Name, other.HomeWorld.Name, StringComparison.Ordinal);
        if (homeWorldComparison != 0) return homeWorldComparison;
        var freeCompanyComparison = string.Compare(FreeCompany, other.FreeCompany, StringComparison.Ordinal);
        return freeCompanyComparison != 0 ? freeCompanyComparison : ContentId.CompareTo(other.ContentId);
    }

    public int CompareTo(object? obj)
    {
        if (obj == null) return 1;
        if (ReferenceEquals(this, obj)) return 0;
        return obj is Friend other
                   ? CompareTo(other)
                   : throw new ArgumentException($"Object must be of type {nameof(Friend)}");
    }

    public static bool operator <(Friend? left, Friend? right)
    {
        return Comparer<Friend>.Default.Compare(left, right) < 0;
    }

    public static bool operator >(Friend? left, Friend? right)
    {
        return Comparer<Friend>.Default.Compare(left, right) > 0;
    }

    public static bool operator <=(Friend? left, Friend? right)
    {
        return Comparer<Friend>.Default.Compare(left, right) <= 0;
    }

    public static bool operator >=(Friend? left, Friend? right)
    {
        return Comparer<Friend>.Default.Compare(left, right) >= 0;
    }

    #endregion
}
