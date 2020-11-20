using System;
using System.Collections.Generic;
using AUOffsetManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AUOffsetHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            var a = AUOffsetManager.GameOffsetsHelper.FromDefault(
                0x144BB30,
                0x144BA30,
                0x144B7CC,
                0x13A715C,
                0x139B29C,
                0x139CF7c);
            Console.Write(JsonConvert.SerializeObject(a, Formatting.Indented));
            var b = new OffsetManager("");
            b.AddToLocalIndex("95A833FBE31A614FB3E50A3153D512E781107BD1D518FA3DAD9F034E2B30F2D7", a);
            Console.ReadLine();

            
        }
    }
}
