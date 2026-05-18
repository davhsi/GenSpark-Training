using LibraryManagement.BLL.Interfaces;
using LibraryManagement.BLL.Services;
using LibraryManagement.Model.Exceptions;
using LibraryManagement.Model.Models;

namespace LibraryManagement.UI.Menus
{
    public static class AdminMemberUI
    {
        private static readonly IMemberService _memberService = new MemberService();

        public static void DisplayMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("           MANAGE MEMBERS               ");
                Console.WriteLine("========================================");
                Console.WriteLine("1. List All Members");
                Console.WriteLine("2. Search Member by Phone / Email");
                Console.WriteLine("3. Edit Member Profile");
                Console.WriteLine("4. Activate / Deactivate Member");
                Console.WriteLine("0. Back");
                Console.WriteLine("========================================");
                Console.Write("Select: ");
                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1": ListAllMembers(); break;
                    case "2": SearchMember(); break;
                    case "3": EditMemberProfile(); break;
                    case "4": UpdateMemberStatus(); break;
                    case "0": return;
                    default:
                        Console.WriteLine("Invalid option. Press any key...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static void ListAllMembers()
        {
            Console.Clear();
            Console.WriteLine("--- All Members ---");
            var members = _memberService.GetAllMembers();

            if (members.Count == 0)
            {
                Console.WriteLine("No members found.");
            }
            else
            {
                Console.WriteLine($"{"ID",-5} {"Name",-25} {"Email",-25} {"Phone",-15} {"Type",-10} {"Status",-10}"); // left align with padding for columns
                Console.WriteLine(new string('-', 95)); // separator line
                foreach (var m in members)
                {
                    string status = m.IsAdmin ? "Admin" : (m.IsActive ? "Active" : "Inactive");
                    Console.WriteLine($"{m.Id,-5} {m.FirstName + " " + m.LastName,-25} {m.Email,-25} {m.Phone,-15} {m.MembershipType?.Name,-10} {status,-10}");
                }
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        private static void SearchMember()
        {
            Console.Clear();
            Console.WriteLine("--- Search Member ---");
            Console.Write("Enter phone number or email: ");
            string keyword = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(keyword))
            {
                Console.WriteLine("Search keyword cannot be empty. Press any key...");
                Console.ReadKey();
                return;
            }

            var members = _memberService.SearchMembers(keyword);

            if (members.Count == 0)
            {
                Console.WriteLine($"No members found matching '{keyword}'.");
            }
            else
            {
                Console.WriteLine($"\n{"ID",-5} {"Name",-25} {"Email",-25} {"Phone",-15} {"Type",-10} {"Status",-10}");
                Console.WriteLine(new string('-', 95));
                foreach (var m in members)
                {
                    string status = m.IsAdmin ? "Admin" : (m.IsActive ? "Active" : "Inactive");
                    Console.WriteLine($"{m.Id,-5} {m.FirstName + " " + m.LastName,-25} {m.Email,-25} {m.Phone,-15} {m.MembershipType?.Name,-10} {status,-10}");
                }
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        private static void EditMemberProfile()
        {
            Console.Clear();
            Console.WriteLine("--- Edit Member Profile ---");
            Console.Write("Enter Member ID: ");

            if (!int.TryParse(Console.ReadLine(), out int memberId))
            {
                Console.WriteLine("Invalid ID. Press any key...");
                Console.ReadKey();
                return;
            }

            var member = _memberService.GetMemberById(memberId);
            if (member == null)
            {
                Console.WriteLine("Member not found. Press any key...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nCurrent profile for ID {member.Id}:");
            Console.WriteLine($"  Name            : {member.FirstName} {member.LastName}");
            Console.WriteLine($"  Email           : {member.Email}");
            Console.WriteLine($"  Phone           : {member.Phone}");
            Console.WriteLine($"  Membership Type : {member.MembershipType?.Name}");
            Console.WriteLine("\nEnter new values (press Enter to keep current):");

            Console.Write($"First Name [{member.FirstName}]: ");
            string firstNameInput = Console.ReadLine() ?? "";
            string firstName = string.IsNullOrWhiteSpace(firstNameInput) ? member.FirstName : firstNameInput;

            Console.Write($"Last Name [{member.LastName}]: ");
            string lastNameInput = Console.ReadLine() ?? "";
            string lastName = string.IsNullOrWhiteSpace(lastNameInput) ? member.LastName : lastNameInput;

            Console.Write($"Phone [{member.Phone}]: ");
            string phoneInput = Console.ReadLine() ?? "";
            string phone = string.IsNullOrWhiteSpace(phoneInput) ? member.Phone : phoneInput;

            Console.Write($"Email [{member.Email}]: ");
            string emailInput = Console.ReadLine() ?? "";
            string email = string.IsNullOrWhiteSpace(emailInput) ? member.Email ?? "" : emailInput;

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
                _memberService.UpdateProfile(memberId, firstName, lastName, phone, email, membershipTypeId);
                Console.WriteLine("\nProfile updated successfully.");
            }
            catch (LibraryException ex)
            {
                Console.WriteLine($"\n{ex.Message}");
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static void UpdateMemberStatus()
        {
            Console.Clear();
            Console.WriteLine("--- Activate / Deactivate Member ---");
            Console.Write("Enter Member ID: ");
            if (!int.TryParse(Console.ReadLine(), out int memberId))
            {
                Console.WriteLine("Invalid ID. Press any key...");
                Console.ReadKey();
                return;
            }

            var member = _memberService.GetMemberById(memberId);
            if (member == null)
            {
                Console.WriteLine("Member not found. Press any key...");
                Console.ReadKey();
                return;
            }

            if (member.IsAdmin)
            {
                Console.WriteLine("Cannot deactivate an administrator. Press any key...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"\nCurrent Status for {member.FirstName} {member.LastName}: {(member.IsActive ? "Active" : "Inactive")}");
            Console.Write($"Change status to {(member.IsActive ? "Inactive (Deactivate)" : "Active (Activate)")}? (y/n): ");
            string confirm = Console.ReadLine()?.ToLower() ?? "";

            if (confirm != "y" && confirm != "yes")
            {
                Console.WriteLine("Operation cancelled.");
                Console.WriteLine("Press any key...");
                Console.ReadKey();
                return;
            }

            try
            {
                _memberService.UpdateMemberStatus(memberId, !member.IsActive);
                Console.WriteLine("Status updated successfully.");
            }
            catch (MemberHasActiveBorrowingsException ex)
            {
                Console.WriteLine($"\n{ex.Message}");
            }
            catch (MemberHasUnpaidFinesException ex)
            {
                Console.WriteLine($"\n{ex.Message}");
            }
            catch (LibraryException ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
    }
}
