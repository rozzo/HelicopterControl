using System;
using System.Linq;
using System.Threading;
using OmniResources.InfraredControl.IguanaIr;
using OmniResources.InfraredControl.Syma;

namespace OmniResources.InfraredControl
{
    class Program
    {
        static void Main(string[] args)
        {
            var cmd = "t";

            if (args.Length > 0)
                cmd = args[0].ToLower();

            switch (cmd)
            {
                case "t":
                    Transmit();
                    break;

                case "r":
                    Receive();
                    break;
            }
        }

        private static void Receive()
        {
            using (var iguana = new IguanaIrInterop())
            {
                var bitConverter = new S107PulseToBitArrayConverter();
                iguana.EnableReceiver();
                
                while (true)
                {
                    var commands = bitConverter.Convert(iguana.ReadData(3000)).Select(x => new S107RawCommand(x));

                    foreach (var command in commands)
                    {
                        //Console.WriteLine(string.Join(string.Empty, command.RawData.Select(x => x ? "1" : "0")));
                        Console.WriteLine(command.ToString());
                    }
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static void Transmit()
        {
            using (var iguana = new IguanaIrInterop())
            {
                using (var heli = new HelicopterControl<S107HeliCommand>(iguana))
                {
                    heli.ChangeThrottle(700);
                    Console.WriteLine(heli.Command.ToString());
                    Thread.Sleep(2500);

                    Console.WriteLine("slowing down...");

                    heli.ChangeThrottle(-350);
                    Console.WriteLine(heli.Command.ToString());
                    Thread.Sleep(2500);
                }
            }
        }
    }
}
