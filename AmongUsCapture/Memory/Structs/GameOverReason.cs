using System;
using System.Collections.Generic;
using System.Text;

namespace AmongUsCapture.Memory.Structs
{
    public enum GameOverReason
    {
		HumansByVote,
		HumansByTask,
		ImpostorByVote,
		ImpostorByKill,
		ImpostorBySabotage,
		ImpostorDisconnect,
		HumansDisconnect,
		Unknown
	}
}
