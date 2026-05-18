using LibraryManagement.BLL.Interfaces;
using LibraryManagement.BLL.Services;
using LibraryManagement.BLL.Session;
using LibraryManagement.Model.Exceptions;

namespace LibraryManagement.UI.Menus
{
    /// <summary>
    /// The main post-login menu loop for an authenticated member.
    /// Runs until the member logs out.
    /// </summary>
    public static class MemberDashboard
    {
        private static readonly IMemberService _memberService = new MemberService();

        /// <summary>
        /// Displays the member dashboard and routes to feature submenus.
        /// Loops until the member selects Logout (option 0).
        /// </summary>
        public static void Show()
        {
            while (UserSession.IsLoggedIn)
            {
                var member = UserSession.CurrentMember!;
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine($"  Welcome, {member.FirstName} {member.LastName}");
                Console.WriteLine($"  Membership: {member.MembershipType?.Name}");
                Console.WriteLine("========================================");
                Console.WriteLine("1. Browse / Search Books");
                Console.WriteLine("2. My Active Borrowings");
                Console.WriteLine("3. My Borrowing History");
                Console.WriteLine("4. Borrow a Book");
                Console.WriteLine("5. Return a Book");
                Console.WriteLine("6. My Fines");
                Console.WriteLine("7. Edit My Profile");
                Console.WriteLine("8. Change Password");
                Console.WriteLine("0. Logout");
                Console.WriteLine("========================================");
                Console.Write("Select an option: ");

                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1": BookUI.DisplayMenu(); break;
                    case "2": BorrowingUI.ShowMyActiveBorrowings(); break;
                    case "3": BorrowingUI.ShowMyBorrowingHistory(); break;
                    case "4": BorrowingUI.BorrowBook(); break;
                    case "5": BorrowingUI.ReturnBook(); break;
                    case "6": FineUI.DisplayMenu(); break;
                    case "7": EditProfile(); break;
                    case "8": ChangePassword(); break;
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

        private static void EditProfile()
        {
            Console.Clear();
            Console.WriteLine("--- Edit My Profile ---");
            var member = UserSession.CurrentMember!;

            Console.WriteLine($"\nCurrent profile:");
            Console.WriteLine($"  Name            : {member.FirstName} {member.LastName}");
            Console.WriteLine($"  Email           : {member.Email}");
            Console.WriteLine($"  Phone           : {member.Phone}");
            Console.WriteLine($"  Membership Type : {member.MembershipType?.Name}");
            Console.WriteLine("\nEnter new values (press Enter to keep current):");

            Console.Write($"First Name [{member.FirstName}]: ");
            string firstName = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(firstName)) firstName = member.FirstName;

            Console.Write($"Last Name [{member.LastName}]: ");
            string lastName = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(lastName)) lastName = member.LastName;

            Console.Write($"Phone [{member.Phone}]: ");
            string phone = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(phone)) phone = member.Phone;

            Console.Write($"Email [{member.Email}]: ");
            string email = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(email)) email = member.Email ?? "";

            var types = _memberService.GetMembershipTypes();
            Console.WriteLine("\nMembership Types:");
            foreach (var t in types)
                Console.WriteLine($"  {t.Id}. {t.Name} ({t.MaxBorrowings} books / {t.MaxBorrowDays} days)");
            Console.Write($"Membership Type ID [{member.MembershipTypeId}]: ");
            string typeInput = Console.ReadLine() ?? "";
            int membershipTypeId = int.TryParse(typeInput, out int parsedType) && types.Any(t => t.Id == parsedType)
                ? parsedType
                : member.MembershipTypeId;

            try
            {
                _memberService.UpdateProfile(member.Id, firstName, lastName, phone, email, membershipTypeId);

                // Refresh the session with updated data
                var updated = _memberService.GetMemberById(member.Id);
                if (updated != null) UserSession.Login(updated);

                Console.WriteLine("\nProfile updated successfully.");
            }
            catch (LibraryException ex)
            {
                Console.WriteLine($"\n{ex.Message}");
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static void ChangePassword()
        {
            Console.Clear();
            Console.WriteLine("--- Change Password ---");

            Console.Write("Current Password: ");
            string current = ReadPassword();

            Console.Write("New Password    : ");
            string newPass = ReadPassword();

            Console.Write("Confirm New     : ");
            string confirm = ReadPassword();

            if (newPass != confirm)
            {
                Console.WriteLine("\nNew passwords do not match. Press any key...");
                Console.ReadKey();
                return;
            }

            if (string.IsNullOrWhiteSpace(newPass))
            {
                Console.WriteLine("\nPassword cannot be empty. Press any key...");
                Console.ReadKey();
                return;
            }

            try
            {
                _memberService.ChangePassword(UserSession.CurrentMember!.Id, current, newPass);
                Console.WriteLine("\nPassword changed successfully.");
            }
            catch (InvalidCredentialsException)
            {
                Console.WriteLine("\nCurrent password is incorrect.");
            }
            catch (LibraryException ex)
            {
                Console.WriteLine($"\n{ex.Message}");
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static string ReadPassword()
        {
            var password = new System.Text.StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter) break;
                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else if (key.Key != ConsoleKey.Backspace)
                {
                    password.Append(key.KeyChar);
                    Console.Write('*');
                }
            }
            Console.WriteLine();
            return password.ToString();
        }
    }
}
