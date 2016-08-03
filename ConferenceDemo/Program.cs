
namespace ConferenceDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var helicopter = new Helicopter(120);

            //demo(helicopter);
            userCode(helicopter);
        }

        private static void userCode(Helicopter helicopter)
        {
            helicopter.TakeOff();

            helicopter.ChangeThrottle(550);

            //put your commands here:
            //type 'helicopter.' and then see the list of commands you can do.
            helicopter.sleep(10000);

            
            helicopter.Land();
        }

        private static void demo(Helicopter helicopter)
        {
            //Takeoff command - FLY!
            helicopter.TakeOff();

            //Throttle command - LIFT
            helicopter.ChangeThrottle(550);

            //Move forward - watch out im moving forward
            helicopter.MoveForward(1000);

            //Move backward - backing up!
            helicopter.MoveBackward(2000);

            //Turn Left - Think about looking out of the helicopter window
            helicopter.TurnLeft(2000);

            helicopter.ChangeThrottle(540);

            //Turn right - Think about looking out of the helicopter window
            helicopter.TurnRight(2000);

            //Landing command
            helicopter.Land();
        }
    }
}