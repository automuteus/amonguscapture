using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AmongUsCapture.Memory.Structs
{
	
	public interface WinningPlayerData
	{
		
        public abstract long Name {get;}
		
        public abstract bool IsDead{get;}
		
        public abstract bool IsImpostor{get;}
		
        public abstract int ColorId{get;}
		
        public abstract uint SkinId{get;}
		
        public abstract uint HatId{get;}
		
        public abstract uint PetId{get;}
		
        public abstract bool IsYou{get;}

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