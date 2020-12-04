namespace Dummy.Models
{
    public class Options
    {
        public byte AmountDummies { get; set; }
        public float KickDummyAfterSeconds { get; set; }
        public bool IsAdmin { get; set; }
        public bool CanExecuteCommands { get; set; }
        public bool DisableSimulations { get; set; }
    }
}
