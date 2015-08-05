
namespace ConferenceDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var helicopter = new Helicopter(150);

            //userCode(helicopter);
            demo(helicopter);
        }

        private static void userCode(Helicopter helicopter)
        {
            helicopter.TakeOff();

            helicopter.ChangeThrottle(550);

            //put your commands here:
            //type 'helicopter.' and then see the list of commands you can do.
            helicopter.TurnLeft();
            
            helicopter.Land();
        }

        private static void demo(Helicopter helicopter)
        {
            //Takeoff command - FLY!
            helicopter.TakeOff();

            //Throttle command - LIFT
            helicopter.ChangeThrottle(500);

            //Move forward - watch out im moving forward
            helicopter.MoveForward();

            //Move backward - backing up!
            helicopter.MoveBackward();
            helicopter.MoveBackward();
            helicopter.MoveBackward();

            //Turn Left - Think about looking out of the helicopter window
            //helicopter.TurnLeft();

            //Turn right - Think about looking out of the helicopter window
            //helicopter.TurnRight();

            //Landing command
            helicopter.Land();
        }
    }
}