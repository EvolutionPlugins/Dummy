using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using System;

namespace EvolutionPlugins.Dummy.Providers
{
    [Serializable]
    public class DummyContainsException : UserFriendlyException
    {
        public ulong Id { get; }

        public DummyContainsException(IStringLocalizer stringLocalizer, ulong id) : base(stringLocalizer["exceptions:contains",
            new { Id = id }])
        {
            Id = id;
        }
    }
}