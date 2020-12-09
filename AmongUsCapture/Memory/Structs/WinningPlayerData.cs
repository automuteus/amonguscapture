using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AmongUsCapture.Memory.Structs
{
	[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
	public struct WinningPlayerData
	{
		[FieldOffset(0x8)]
		public uint Name;
		[FieldOffset(0xC)]
		public bool IsDead;
		[FieldOffset(0xD)]
		public bool IsImpostor;
		[FieldOffset(0x10)]
		public int ColorId;
		[FieldOffset(0x14)]
		public uint SkinId;
		[FieldOffset(0x18)]
		public uint HatId;
		[FieldOffset(0x1C)]
		public uint PetId;
		[FieldOffset(0x20)]
		public bool IsYou;

		public string GetPlayerName()
		{
			return ProcessMemory.getInstance().ReadString((IntPtr)this.Name);
		}

		public string Display()
		{
			return this.GetPlayerName() + ":" + (this.IsImpostor ? "yes" : "no");
		}
	}
}