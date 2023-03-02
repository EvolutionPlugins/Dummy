using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using System;

namespace Dummy.API.Exceptions
{
    [Serializable]
    public class DummyOverflowsException : UserFriendlyException
    {
        public byte DummiesCount { get; }
        public byte MaxDummies { get; }

        public DummyOverflowsException(IStringLocalizer stringLocalizer, byte dummiesCount, byte maxDummies) : base(stringLocalizer[
            "exceptions:overflow",
            new { DummiesCount = dummiesCount, MaxDummies = maxDummies }])
        {
            DummiesCount = dummiesCount;
            MaxDummies = maxDummies;
        }
    }
}