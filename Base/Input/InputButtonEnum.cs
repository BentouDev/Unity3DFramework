[System.Flags]
public enum InputButtonEnum 
{
    Dash = 1,
    Jump = 1 << 1,
    Block = 1 << 2,
    Burst = 1 << 3,
    Slash = 1 << 4,
    Kick = 1 << 5,
    Special = 1 << 7,
}
