using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Patches;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using System;

[assembly: PluginMetadata("Dummy", Author = "EvolutionPlugins", DisplayName = "Dummy",
    Website = "https://discord.gg/6KymqGv")]

namespace Dummy
{
    internal delegate IDummyProvider NeedDummyProvider();

    public class Dummy : OpenModUnturnedPlugin
    {
        private readonly ILogger<Dummy> m_Logger;
        private readonly IDummyProvider m_DummyProvider;

        public Dummy(IServiceProvider serviceProvider, ILogger<Dummy> logger, IDummyProvider dummyProvider) : base(serviceProvider)
        {
            m_Logger = logger;
            m_DummyProvider = dummyProvider;
        }

        protected override UniTask OnLoadAsync()
        {
            Patch_Provider.OnNeedDummy += GiveProvider;
            Patch_EffectManager.OnNeedDummy += GiveProvider;
            Patch_PlayerVoice.OnNeedDummy += GiveProvider;

            var type = AccessTools.TypeByName("SDG.Unturned.ServerMessageHandler_ReadyToConnect");
            var orgMethod = AccessTools.Method(type, "ReadMessage");
            var patchMethod = SymbolExtensions.GetMethodInfo(() => Patch_ServerMessageHandler_ReadyToConnect.ReadMessage(null));
            Harmony.CreateProcessor(orgMethod).AddTranspiler(new HarmonyMethod(patchMethod));

            m_Logger.LogInformation("Made with <3 by Evolution Plugins");
            m_Logger.LogInformation("Owner of EvolutionPlugins: DiFFoZ");
            m_Logger.LogInformation("https://github.com/evolutionplugins \\ https://github.com/diffoz");
            m_Logger.LogInformation("Discord Support: https://discord.gg/6KymqGv");

            return UniTask.CompletedTask;
        }

        protected override UniTask OnUnloadAsync()
        {
            Patch_Provider.OnNeedDummy -= GiveProvider;
            Patch_EffectManager.OnNeedDummy -= GiveProvider;
            Patch_PlayerVoice.OnNeedDummy -= GiveProvider;
            return UniTask.CompletedTask;
        }

        private IDummyProvider GiveProvider()
        {
            return m_DummyProvider;
        }
    }
}