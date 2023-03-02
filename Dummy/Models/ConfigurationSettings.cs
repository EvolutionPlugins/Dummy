extern alias JetBrainsAnnotations;

using System.Net;
using JetBrainsAnnotations::JetBrains.Annotations;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace Dummy.Models
{
    [UsedImplicitly]
    public class ConfigurationSettings
    {
        public bool IsPro { get; set; } = true;
        public byte CharacterId { get; set; }
        public string PlayerName { get; set; } = "dummy (steam)";
        public string CharacterName { get; set; } = "dummy";
        public string NickName { get; set; } = "dummy (group)";
        public IPAddress IP { get; set; } = IPAddress.None;
        public ushort Port { get; set; }
        // todo: add support for multiple hwids
        public string? Hwid { get; set; }
        public CSteamID SteamGroupId { get; set; }
        public Color SkinColor { get; set; }
        public byte BeardId { get; set; }
        public Color BeardColor { get; set; }
        public byte HairId { get; set; }
        public Color HairColor { get; set; }
        public byte FaceId { get; set; }
        public Color MarkerColor { get; set; }
        public bool IsLeftHanded { get; set; }
        public Clothing Skins { get; set; } = new();
        public EPlayerSkillset PlayerSkillset { get; set; }
        public string Language { get; set; } = "english";
        public CSteamID LobbyId { get; set; }
    }
}