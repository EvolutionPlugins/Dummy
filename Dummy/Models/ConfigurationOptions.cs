namespace Dummy.Models
{
    public class ConfigurationOptions
    {
        public byte AmountDummies { get; set; }
        public float KickDummyAfterSeconds { get; set; }
        public bool IsAdmin { get; set; }
        public bool CanExecuteCommands { get; set; }
        public bool DisableSimulations { get; set; }
    }
}
