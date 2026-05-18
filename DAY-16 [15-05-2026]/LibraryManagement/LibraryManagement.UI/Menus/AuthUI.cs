using LibraryManagement.BLL.Interfaces;
using LibraryManagement.BLL.Services;
using LibraryManagement.BLL.Session;
using LibraryManagement.Model.Exceptions;
using LibraryManagement.Model.Models;

namespace LibraryManagement.UI.Menus
{
    public static class AuthUI
    {
        private static readonly IMemberService _memberService = new MemberService();

        public static void ShowRegister()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("          REGISTER NEW MEMBER           ");
            Console.WriteLine("========================================");

            Console.Write("First Name : ");
            string firstName = Console.ReadLine() ?? "";
            Console.Write("Last Name  : ");
            string lastName = Console.ReadLine() ?? "";
            Console.Write("Phone      : ");
            string phone = Console.ReadLine() ?? "";
            Console.Write("Email      : ");
            string email = Console.ReadLine() ?? "";
            Console.Write("Password   : ");
            string password = ReadPassword();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("\nAll fields are required. Press any key...");
                Console.ReadKey();
                return;
            }

            var types = _memberService.GetMembershipTypes();
            Console.WriteLine("\nSelect Membership Type:");
            foreach (var t in types)
                Console.WriteLine($"  {t.Id}. {t.Name} (Borrow limit: {t.MaxBorrowings}, Max days: {t.MaxBorrowDays})");
            Console.Write("Choice: ");
            int typeId = int.TryParse(Console.ReadLine(), out int tid) ? tid : 0;

            try
            {
                _memberService.RegisterMember(new Member
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = phone,
                    Email = email,
                    MembershipTypeId = typeId,
                    JoinDate = DateTime.UtcNow
                }, password);

                Console.WriteLine("\nRegistration successful! You can now log in.");
            }
            catch (InvalidEmailException ex)
            {
                Console.WriteLine($"\n{ex.Message}");
            }
            catch (InvalidPhoneException ex)
            {
                Console.WriteLine($"\n{ex.Message}");
            }
            catch (DuplicateEmailException ex)
            {
                Console.WriteLine($"\n{ex.Message}");
            }
            catch (InvalidMembershipTypeException ex)
            {
                Console.WriteLine($"\n{ex.Message}");
            }
            catch (LibraryException ex)
            {
                Console.WriteLine($"\n{ex.Message}");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static bool ShowLogin()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("                 LOGIN                  ");
            Console.WriteLine("========================================");

            Console.Write("Email    : ");
            string email = Console.ReadLine() ?? "";
            Console.Write("Password : ");
            string password = ReadPassword();

            try
            {
                var member = _memberService.Login(email, password);
                UserSession.Login(member);
                Console.WriteLine($"\nWelcome back, {member.FirstName}!");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return true;
            }
            catch (MemberInactiveException ex)
            {
                Console.WriteLine($"\n{ex.Message}");
            }
            catch (InvalidCredentialsException ex)
            {
                Console.WriteLine($"\n{ex.Message}");
            }
            catch (MemberNotFoundException)
            {
                Console.WriteLine("\nInvalid email or password. Please try again.");
            }
            catch (LibraryException ex)
            {
                Console.WriteLine($"\n{ex.Message}");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return false;
        }

        // Reads password as masked input (shows * instead of characters)
        private static string ReadPassword()
        {
            var password = new System.Text.StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(intercept: true); // Don't show the key
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
