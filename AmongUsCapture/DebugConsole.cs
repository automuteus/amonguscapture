using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AmongUsCapture
{
    class DebugConsole
    {
        private bool debugGui;

        public DebugConsole(bool debugGui)
        {
            this.debugGui = debugGui;
        }

        public void Run()
        {
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged += PlayerChangedHandler;

            //while (true)
            //{
            //    string command = Console.ReadLine();
            //    Console.WriteLine($"Received command: {command}");
            //    if (command == "list")
            //    {
            //        Console.WriteLine("Player List:");
            //        foreach (KeyValuePair<string, PlayerInfo> kvp in GameMemReader.getInstance().oldPlayerInfos) // thread safety ??
            //        {
            //            string playerName = kvp.Key;
            //            PlayerInfo pi = kvp.Value;
            //            Console.WriteLine(kvp.Key);
            //        }
            //    }
            //}

            Thread.Sleep(Timeout.Infinite);
        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            if(!debugGui)
            {
                Console.WriteLine("State changed to " + e.NewState);
            }
            
        }

        private void PlayerChangedHandler(object sender, PlayerChangedEventArgs e)
        {
            if (e.Name.Length != 0) // for some reason, sometimes there's events with players without a name
            {
                if (!debugGui)
                {
                    Console.WriteLine(e.Name + ": " + e.Action);
                }


            }
        }
    }
}
