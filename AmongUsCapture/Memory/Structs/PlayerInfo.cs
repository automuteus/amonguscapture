using System;
using System.Runtime.InteropServices;
using AUOffsetManager;
using Discord;

namespace AmongUsCapture
{
    
    public class PlayerInfo
    {
        public byte PlayerId;

        public string PlayerName;
        public PlayerColor ColorId;
        public uint HatId;
        public uint PetId;
        public uint SkinId;

        public uint PlayerLevel;
        public bool Disconnected;

        public uint RoleType;
        public uint RoleTeamType;

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
                PlayerOutfitStructOffsets oOf = CurrentOffsets.PlayerOutfitStructOffsets;
                var outfit = MemInstance.Read<IntPtr>(baseAddrCopy, pOf.OutfitsOffset);
                fixed (byte* ptr = buffer) {
                    var buffptr = (IntPtr) ptr;
                    PlayerId = Marshal.ReadByte(buffptr, pOf.PlayerIDOffset);
                    Disconnected = Marshal.ReadByte(buffptr, pOf.DisconnectedOffset) > 0;
                    Tasks = Marshal.ReadIntPtr(buffptr, pOf.TasksOffset);
                    IsDead = Marshal.ReadByte(buffptr, pOf.IsDeadOffset) > 0;
                    _object = Marshal.ReadIntPtr(buffptr, pOf.ObjectOffset);

                    // Read from Role
                    RoleType = (uint)MemInstance.Read<int>(baseAddrCopy, pOf.RoleTypeOffset);
                    RoleTeamType = (uint)MemInstance.Read<int>(baseAddrCopy, pOf.RoleTeamTypeOffset);
                    IsImpostor = RoleTeamType == 1;

                    // Read from PlayerOutfit
                    PlayerName = MemInstance.ReadString(MemInstance.Read<IntPtr>(outfit, oOf.PlayerNameOffset), CurrentOffsets.StringOffsets[0], CurrentOffsets.StringOffsets[1]);
                    ColorId = (PlayerColor)(uint)MemInstance.Read<int>(outfit, oOf.ColorIDOffset);
                    // TODO: Since IDs are changed from enum to string like "hat_police", renaming or mapping existing svgs to string is required
                    // TODO: As a workaround just fill with 0 as IDs
                    //HatId = MemInstance.ReadString(MemInstance.Read<IntPtr>(outfit, oOf.HatIDOffset), CurrentOffsets.StringOffsets[0], CurrentOffsets.StringOffsets[1]);
                    //PetId = MemInstance.ReadString(MemInstance.Read<IntPtr>(outfit, oOf.PetIDOffset), CurrentOffsets.StringOffsets[0], CurrentOffsets.StringOffsets[1]);
                    //SkinId = MemInstance.ReadString(MemInstance.Read<IntPtr>(outfit, oOf.SkinIDOffset), CurrentOffsets.StringOffsets[0], CurrentOffsets.StringOffsets[1]);
                    HatId = 0;
                    PetId = 0;
                    SkinId = 0;
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
