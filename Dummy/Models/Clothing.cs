namespace Dummy.Models;

/// <summary>
/// Skins wrapper.<br/>
/// To find skin id open a file <b>EconInfo.json</b> in Unturned folder and find a skin that you like, then copy the value of <b>item_id</b> parameter to set it.
/// </summary>
public class Clothing
{
    // todo: add support for item_effect

    public uint Shirt { get; set; }
    public uint Pants { get; set; }
    public uint Hat { get; set; }
    public uint Backpack { get; set; }
    public uint Vest { get; set; }
    public uint Mask { get; set; }
    public uint Glasses { get; set; }
}
