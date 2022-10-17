using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Dummy.API;
using OpenMod.Core.Commands;

namespace Dummy.Commands.Tests;
[Command("test")]
internal class CommandTestSpawn : Command
{
    private readonly IDummyProvider m_DummyProvider;

    public CommandTestSpawn(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider)
    {
        m_DummyProvider = dummyProvider;
    }

    protected override async Task OnExecuteAsync()
    {
        var dummy = await m_DummyProvider.AddDummyAsync(null, null);
        var isKicked = await m_DummyProvider.RemoveDummyAsync(dummy.SteamID);
        Debug.Assert(isKicked);

        var dummy2 = await m_DummyProvider.AddDummyAsync(null, null);
        Debug.Assert(dummy.SteamID.m_SteamID == dummy2.SteamID.m_SteamID);
    }
}