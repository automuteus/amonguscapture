using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using AUOffsetManager;

namespace AmongUsCapture.Memory.Structs
{
	
	public class WinningPlayerData
	{
		
        public string Name {get;}
		
        public bool IsDead{get;}
		
        public bool IsImpostor{get;}
		
        public int ColorId{get;}
		
        public uint SkinId{get;}
		
        public uint HatId{get;}
		
        public uint PetId{get;}
		
        public bool IsYou{get;}

        public WinningPlayerData(IntPtr baseAddr, ProcessMemory MemInstance, GameOffsets CurrentOffsets) {
	        unsafe {
		        var baseAddrCopy = baseAddr;
		        int last = MemInstance.OffsetAddress(ref baseAddrCopy, 0, 0);
		        int size = ((int)Math.Ceiling((decimal) ((1 + CurrentOffsets.WinningPlayerDataStructOffsets.IsYouOffset)/8)))*8; //Find the nearest multiple of 8
		        byte[] buffer = MemInstance.Read(baseAddrCopy + last, size);
		        WinningPlayerDataStructOffsets pOf = CurrentOffsets.WinningPlayerDataStructOffsets;
		        fixed (byte* ptr = buffer) {
			        var buffptr = (IntPtr) ptr;
			        var NamePTR = MemInstance.is64Bit ? (IntPtr) Marshal.ReadInt64(buffptr, pOf.NameOffset) : (IntPtr) Marshal.ReadInt32(buffptr, pOf.NameOffset);
			        Name = NamePTR == IntPtr.Zero ? "" : MemInstance.ReadString(NamePTR, CurrentOffsets.StringOffsets[0], CurrentOffsets.StringOffsets[1]);
			        ColorId = (int)Marshal.ReadInt32(buffptr, pOf.ColorOffset);
			        HatId = (uint) Marshal.ReadInt32(buffptr, pOf.HatOffset);
			        PetId = (uint) Marshal.ReadInt32(buffptr, pOf.PetOffset);
			        SkinId = (uint) Marshal.ReadInt32(buffptr, pOf.SkinOffset);
			        IsImpostor = Marshal.ReadByte(buffptr, pOf.ImposterOffset) == 1;
			        IsDead = Marshal.ReadByte(buffptr, pOf.DeadOffset) > 0;
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