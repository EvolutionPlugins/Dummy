using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using HarmonyLib;
using SDG.NetTransport;
using SDG.Unturned;

namespace EvolutionPlugins.Dummy.Extensions.Network.Patches
{
    public class Patch_ClientInstanceMethod
    {
        
        public Patch_ClientInstanceMethod(Harmony harmony)
        {
            harmony.Patch(typeof(ClientInstanceMethod<>)
                .GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance), new HarmonyMethod(typeof(Patch_ClientInstanceMethod).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static)));
        }
        
        //https://www.codegrepper.com/code-examples/csharp/c%23+class+to+byte+array
        private static byte[] ObjectToByteArray(object obj)
        {
            if(obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }
        
        private static void Prefix<T>(NetId netId, ENetReliability reliability, ITransportConnection transportConnection, T arg)
        {
            DummyNetworkStream stream = DummyNetworkStream.Streams.FirstOrDefault(c => c.Equals(netId));
            stream?.Write(ObjectToByteArray(arg));
        }
    }
}