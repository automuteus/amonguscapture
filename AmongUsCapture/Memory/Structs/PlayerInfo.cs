using System;
using System.Runtime.InteropServices;

namespace AmongUsCapture
{
    
    public interface PlayerInfo
    {
        public abstract byte PlayerId { get; }
        public abstract IntPtr PlayerName {get; }
        public abstract byte ColorId { get; }
        public abstract uint HatId { get;  }
        public abstract uint PetId { get; }
        public abstract uint SkinId { get; }
        public abstract byte Disconnected { get; }
        public abstract IntPtr Tasks { get; }
        public abstract byte IsImpostor { get; }
        public abstract byte IsDead { get; }
        public abstract IntPtr _object { get; }

        public bool GetIsDead()
        {
            return this.IsDead > 0;
        }

        public string GetPlayerName()
        {
            return ProcessMemory.getInstance().ReadString((IntPtr)this.PlayerName, 0x10, 0x14);
        }

        public bool GetIsImposter()
        {
            return this.IsImpostor == 1;
        }

        public PlayerColor GetPlayerColor()
        {
            return (PlayerColor)this.ColorId;
        }

        public bool GetIsDisconnected()
        {
            return this.Disconnected > 0;
        }
    }
}
