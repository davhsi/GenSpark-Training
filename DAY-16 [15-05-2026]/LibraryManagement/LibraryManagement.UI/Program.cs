using LibraryManagement.DAL;
using LibraryManagement.UI.Menus;

namespace LibraryManagement.UI
{
    /// <summary>
    /// Application entry point. Displays the main menu and routes to
    /// Register, Login, or Exit. After successful login, hands control
    /// to MemberDashboard for the session duration.
    ///
    /// Pass --seed on the command line to wipe and repopulate test data:
    ///     dotnet run --seed
    /// </summary>
    internal class Program
    {
        /// <summary>Runs the application's outer loop until the user chooses to exit.</summary>
        static void Main(string[] args)
        {
            // ── Developer shortcut: reset to a known test state ──────────────
            if (args.Contains("--seed"))
            {
                DataSeeder.Seed();
                Console.WriteLine("\nPress any key to continue to the menu…");
                Console.ReadKey();
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("    DAVE LIBRARY MANAGEMENT SYSTEM");         
                Console.WriteLine("========================================");
                Console.WriteLine("1. Register");
                Console.WriteLine("2. Login");
                Console.WriteLine("0. Exit");
                Console.WriteLine("========================================");
                Console.Write("Select an option: ");

                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        AuthUI.ShowRegister();
                        break;
                    case "2":
                        bool loggedIn = AuthUI.ShowLogin();
                        if (loggedIn)
                        {
                            if (BLL.Session.UserSession.CurrentMember!.IsAdmin)// null forgiving operator is needed to satisfy the compiler, but we know CurrentMember is not null here because ShowLogin only returns true if login succeeded                         // null forgiving operator is needed to satisfy the compiler, but we know CurrentMember is not null here because ShowLogin only returns true if login succeeded.
                            {
                                AdminDashboard.Show();
                            }
                            else
                            {
                                MemberDashboard.Show();
                            }
                        }
                        break;
                    case "0":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Press any key...");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}
