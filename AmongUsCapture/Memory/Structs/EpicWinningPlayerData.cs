using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AmongUsCapture.Memory.Structs
{
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
    public struct EpicWinningPlayerData : WinningPlayerData
    {
        [FieldOffset(0x10)]
        public long Name;
        [FieldOffset(0x18)]
        public bool IsDead;
        [FieldOffset(0x19)]
        public bool IsImpostor;
        [FieldOffset(0x1C)]
        public int ColorId;
        [FieldOffset(0x20)]
        public uint SkinId;
        [FieldOffset(0x24)]
        public uint HatId;
        [FieldOffset(0x28)]
        public uint PetId;
        [FieldOffset(0x2C)]
        public bool IsYou;

        long WinningPlayerData.Name => Name;
        bool WinningPlayerData.IsDead => IsDead;
        bool WinningPlayerData.IsImpostor => IsImpostor;
        int WinningPlayerData.ColorId => ColorId;
        uint WinningPlayerData.SkinId => SkinId;
        uint WinningPlayerData.HatId => HatId;
        uint WinningPlayerData.PetId => PetId;
        bool WinningPlayerData.IsYou => IsYou;

        public string GetPlayerName()
        {
            return ProcessMemory.getInstance().ReadString((IntPtr)this.Name, 0x10, 0x14);
        }

    }
}
