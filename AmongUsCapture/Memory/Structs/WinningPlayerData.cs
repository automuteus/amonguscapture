using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using AUOffsetManager;

namespace AmongUsCapture.Memory.Structs
{

    public class WinningPlayerData
    {
        public string Name {get; }
        public int ColorId { get; }
        public uint HatId { get; }
        public uint PetId { get; }
        public uint SkinId { get; }

        public bool IsYou { get; }
        public bool IsImpostor { get; }
        public bool IsDead{get;}

        public WinningPlayerData(IntPtr baseAddr, ProcessMemory MemInstance, GameOffsets CurrentOffsets) {
            unsafe {
                var baseAddrCopy = baseAddr;
                int last = MemInstance.OffsetAddress(ref baseAddrCopy, 0, 0);
                int size = ((int)Math.Ceiling((decimal) ((8 + CurrentOffsets.WinningPlayerDataStructOffsets.IsDeadOffset)/8)))*8; //Find the nearest multiple of 8
                byte[] buffer = MemInstance.Read(baseAddrCopy + last, size);
                PlayerOutfitStructOffsets oOf = CurrentOffsets.PlayerOutfitStructOffsets;
                WinningPlayerDataStructOffsets pOf = CurrentOffsets.WinningPlayerDataStructOffsets;
                fixed (byte* ptr = buffer) {
                    var buffptr = (IntPtr) ptr;
                    Name = MemInstance.ReadString(MemInstance.Read<IntPtr>(baseAddrCopy, oOf.PlayerNameOffset), CurrentOffsets.StringOffsets[0], CurrentOffsets.StringOffsets[1]);
                    ColorId = (int)Marshal.ReadInt32(buffptr, oOf.ColorIDOffset);
                    // TODO: Since IDs are changed from enum to string like "hat_police", renaming or mapping existing svgs to string is required
                    // TODO: As a workaround just fill with 0 as IDs
                    HatId = 0;
                    PetId = 0;
                    SkinId = 0;
                    IsImpostor = Marshal.ReadByte(buffptr, pOf.IsImposterOffset) == 1;
                    IsDead = Marshal.ReadByte(buffptr, pOf.IsDeadOffset) > 0;
                    IsYou = Marshal.ReadByte(buffptr, pOf.IsYouOffset) == 1;
                }
            }
        }

		public string GetPlayerName() {
			return this.Name;
		}

		public string Display()
		{
			return this.GetPlayerName() + ":" + (this.IsImpostor ? "yes" : "no");
		}
	}
}