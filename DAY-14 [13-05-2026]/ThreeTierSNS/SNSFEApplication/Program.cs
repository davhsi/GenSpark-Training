using SNSFEApplication.UI;

namespace SNSFEApplication
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // testing User Model after connecting model layer
            // User testUser = new User("Davish", "E", "dav@gmail.com", "91", "9876543210");
            // Console.WriteLine(testUser);

            // testing Notification Model after connecting model layer
            // Notification testNotif = new Notification("test import");
            // Console.WriteLine(testNotif);

            ConsoleUI consoleUI = new ConsoleUI();
            consoleUI.Start();

        }
    }
}