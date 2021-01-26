using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmongUsCapture.Memory.Structs
{
    public interface PlayerInfo
    {
        public abstract byte PlayerId { get; }
        public abstract IntPtr PlayerName { get; }
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
            return ProcessMemory.getInstance().ReadString((IntPtr)this.PlayerName);
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
