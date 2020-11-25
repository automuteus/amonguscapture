using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmongUsCapture.Memory.Structs
{
    public class ImmutablePlayer
    {
        public string Name { get; set; }
        public bool IsImpostor { get; set; }

        public void SetImpostor (bool imp)
        {
            this.IsImpostor = imp;
        }
    }
}
