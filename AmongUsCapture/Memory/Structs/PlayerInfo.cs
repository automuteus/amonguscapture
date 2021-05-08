using System;
using System.Runtime.InteropServices;
using AUOffsetManager;
using Discord;

namespace AmongUsCapture
{
    
    public class PlayerInfo
    {
        public byte PlayerId;
        public String PlayerName;
        public PlayerColor ColorId;
        public uint HatId;
        public uint PetId;
        public uint SkinId;
        public bool Disconnected;
        public IntPtr Tasks;
        public bool IsImpostor;
        public bool IsDead;
        public IntPtr _object; //Assume this always has largest offset
        public PlayerInfo(IntPtr baseAddr, ProcessMemory MemInstance, GameOffsets CurrentOffsets) {
            unsafe {
                var baseAddrCopy = baseAddr;
                int last = MemInstance.OffsetAddress(ref baseAddrCopy, 0, 0);
                var intPtrSize = MemInstance.is64Bit ? 8 : 4;
                int size = ((int)Math.Ceiling((decimal) ((intPtrSize + CurrentOffsets.PlayerInfoStructOffsets.ObjectOffset)/8)))*8; //Find the nearest multiple of 8
                byte[] buffer = MemInstance.Read(baseAddrCopy + last, size);
                PlayerInfoStructOffsets pOf = CurrentOffsets.PlayerInfoStructOffsets;
                fixed (byte* ptr = buffer) {
                    var buffptr = (IntPtr) ptr;
                    PlayerId = Marshal.ReadByte(buffptr, pOf.PlayerIDOffset);
                    var NamePTR = MemInstance.is64Bit ? (IntPtr) Marshal.ReadInt64(buffptr, pOf.PlayerNameOffset) : (IntPtr) Marshal.ReadInt32(buffptr, pOf.PlayerNameOffset);
                    PlayerName = NamePTR == IntPtr.Zero ? "" : MemInstance.ReadString(NamePTR, CurrentOffsets.StringOffsets[0], CurrentOffsets.StringOffsets[1]);
                    ColorId = (PlayerColor)(uint)Marshal.ReadInt32(buffptr, pOf.ColorIDOffset);
                    HatId = (uint) Marshal.ReadInt32(buffptr, pOf.HatIDOffset);
                    PetId = (uint) Marshal.ReadInt32(buffptr, pOf.PetIDOffset);
                    SkinId = (uint) Marshal.ReadInt32(buffptr, pOf.SkinIDOffset);
                    Disconnected = Marshal.ReadByte(buffptr, pOf.DisconnectedOffset) > 0;
                    Tasks = Marshal.ReadIntPtr(buffptr, pOf.TasksOffset);
                    IsImpostor = Marshal.ReadByte(buffptr, pOf.ImposterOffset) == 1;
                    IsDead = Marshal.ReadByte(buffptr, pOf.DeadOffset) > 0;
                    _object = Marshal.ReadIntPtr(buffptr, pOf.ObjectOffset);
                }
            }
        }
        public string GetPlayerName() {
            return PlayerName;
        }
        public bool GetIsDead()
        {
            return IsDead;
        }
        
        public bool GetIsImposter()
        {
            return IsImpostor;
        }

        public PlayerColor GetPlayerColor()
        {
            return ColorId;
        }

        public bool GetIsDisconnected()
        {
            return Disconnected;
        }
    }
}
