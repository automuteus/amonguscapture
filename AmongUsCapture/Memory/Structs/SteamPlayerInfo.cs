using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AmongUsCapture.Memory.Structs
{
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
    public struct SteamPlayerInfo : PlayerInfo
    {
        [System.Runtime.InteropServices.FieldOffset(8)] public byte PlayerId;
        [System.Runtime.InteropServices.FieldOffset(12)] public uint PlayerName;
        [System.Runtime.InteropServices.FieldOffset(16)] public byte ColorId;
        [System.Runtime.InteropServices.FieldOffset(20)] public uint HatId;
        [System.Runtime.InteropServices.FieldOffset(24)] public uint PetId;
        [System.Runtime.InteropServices.FieldOffset(28)] public uint SkinId;
        [System.Runtime.InteropServices.FieldOffset(32)] public byte Disconnected;
        [System.Runtime.InteropServices.FieldOffset(36)] public IntPtr Tasks;
        [System.Runtime.InteropServices.FieldOffset(40)] public byte IsImpostor;
        [System.Runtime.InteropServices.FieldOffset(41)] public byte IsDead;
        [System.Runtime.InteropServices.FieldOffset(44)] public IntPtr _object;

        byte PlayerInfo.PlayerId => PlayerId;

        IntPtr PlayerInfo.PlayerName => (IntPtr)PlayerName;

        byte PlayerInfo.ColorId => ColorId;

        uint PlayerInfo.HatId => HatId;

        uint PlayerInfo.PetId => PetId;

        uint PlayerInfo.SkinId => SkinId;

        byte PlayerInfo.Disconnected => Disconnected;

        IntPtr PlayerInfo.Tasks => Tasks;

        byte PlayerInfo.IsImpostor => IsImpostor;

        byte PlayerInfo.IsDead => IsDead;

        IntPtr PlayerInfo._object => _object;
        public string GetPlayerName()
        {
            return ProcessMemory.getInstance().ReadString((IntPtr)this.PlayerName);
        }
    }
}
