using LibraryManagement.BLL.Session;

namespace LibraryManagement.UI.Menus
{
    /// <summary>
    /// The main post-login menu loop for an authenticated administrator.
    /// Runs until the admin logs out.
    /// </summary>
    public static class AdminDashboard
    {
        /// <summary>
        /// Displays the admin dashboard and routes to admin feature submenus.
        /// Loops until the admin selects Logout (option 0).
        /// </summary>
        public static void Show()
        {
            while (UserSession.IsLoggedIn)
            {
                var member = UserSession.CurrentMember!;
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine($"  ADMIN DASHBOARD - {member.FirstName} {member.LastName}");
                Console.WriteLine("========================================");
                Console.WriteLine("1. Manage Members");
                Console.WriteLine("2. Manage Books");
                Console.WriteLine("3. Reports");
                Console.WriteLine("0. Logout");
                Console.WriteLine("========================================");
                Console.Write("Select an option: ");

                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1": AdminMemberUI.DisplayMenu(); break;
                    case "2": AdminBookUI.DisplayMenu(); break;
                    case "3": ReportsUI.DisplayMenu(); break;
                    case "0":
                        UserSession.Logout();
                        Console.WriteLine("You have been logged out. Goodbye!");
                        Console.ReadKey();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press any key...");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}
