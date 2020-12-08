using SDG.Unturned;
using Steamworks;

namespace Dummy.Models
{
    public class ConfigurationSettings
    {
        public bool IsPro { get; set; }
        public byte CharacterId { get; set; }
        public string PlayerName { get; set; }
        public string CharacterName { get; set; }
        public string NickName { get; set; }
        public string IP { get; set; }
        public ushort Port { get; set; }
        public string HWID { get; set; }
        public CSteamID SteamGroupId { get; set; }
        public string SkinColor { get; set; }
        public string Color { get; set; }
        public byte BeardId { get; set; }
        public string BeardColor { get; set; }
        public byte HairId { get; set; }
        public string HairColor { get; set; }
        public byte FaceId { get; set; }
        public string MarkerColor { get; set; }
        public bool IsLeftHanded { get; set; }
        public Clothing Skins { get; set; }
        public EPlayerSkillset PlayerSkillset { get; set; }
        public string Language { get; set; }
        public CSteamID LobbyId { get; set; }
    }
}