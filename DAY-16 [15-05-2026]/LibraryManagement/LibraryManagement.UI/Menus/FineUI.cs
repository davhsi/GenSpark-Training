using LibraryManagement.BLL.Interfaces;
using LibraryManagement.BLL.Services;
using LibraryManagement.BLL.Session;
using LibraryManagement.Model.Exceptions;

namespace LibraryManagement.UI.Menus
{
    public static class FineUI
    {
        private static readonly IFineService _fineService = new FineService();

        public static void DisplayMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- My Fines ---");
                Console.WriteLine("1. View Unpaid Fines");
                Console.WriteLine("2. View Fine History");
                Console.WriteLine("0. Back");
                Console.Write("Select: ");
                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1": ShowUnpaidFines(); break;
                    case "2": ShowFineHistory(); break;
                    case "0": return;
                    default:
                        Console.WriteLine("Invalid option. Press any key...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static void ShowUnpaidFines()
        {
            Console.Clear();
            Console.WriteLine("--- Unpaid Fines ---");

            var fines = _fineService.GetUnpaidFines(UserSession.CurrentMember!.Id);
            if (fines.Count == 0)
            {
                Console.WriteLine("You have no unpaid fines.");
                Console.WriteLine("Press any key...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"{"ID",-5} {"Book",-35} {"Amount",-12} {"Issued",-12}");
            Console.WriteLine(new string('-', 65));
            foreach (var f in fines)
                Console.WriteLine($"{f.Id,-5} {f.Borrowing?.BookCopy?.Book?.Title,-35} ₹{f.Amount,-11} {f.IssuedDate:yyyy-MM-dd}");

            decimal total = fines.Sum(f => f.Amount);
            Console.WriteLine(new string('-', 65));
            Console.WriteLine($"Total Outstanding: ₹{total}");

            Console.WriteLine("\n1. Pay a Fine");
            Console.WriteLine("0. Back");
            Console.Write("Select: ");
            string choice = Console.ReadLine() ?? "";

            if (choice != "1") return;

            Console.Write("\nEnter Fine ID to pay: ");
            if (!int.TryParse(Console.ReadLine(), out int fineId))
            {
                Console.WriteLine("Invalid Fine ID. Press any key...");
                Console.ReadKey();
                return;
            }

            var selected = fines.FirstOrDefault(f => f.Id == fineId);
            if (selected == null)
            {
                Console.WriteLine("That fine does not belong to your account. Press any key...");
                Console.ReadKey();
                return;
            }

            Console.Write($"Pay ₹{selected.Amount} for '{selected.Borrowing?.BookCopy?.Book?.Title}'? (y/n): ");
            string confirm = Console.ReadLine()?.Trim().ToLower() ?? "";
            if (confirm != "y" && confirm != "yes")
            {
                Console.WriteLine("Cancelled. Press any key...");
                Console.ReadKey();
                return;
            }

            try
            {
                _fineService.PayFine(fineId);
                Console.WriteLine("Fine paid successfully.");
            }
            catch (FineNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static void ShowFineHistory()
        {
            Console.Clear();
            Console.WriteLine("--- Fine History ---");

            var fines = _fineService.GetAllFines(UserSession.CurrentMember!.Id);
            if (fines.Count == 0)
            {
                Console.WriteLine("You have no fine history.");
                Console.WriteLine("Press any key...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"{"ID",-5} {"Book",-35} {"Amount",-10} {"Status",-10} {"Issued",-12}");
            Console.WriteLine(new string('-', 72));
            foreach (var f in fines)
            {
                string status = f.IsPaid ? "Paid" : "Unpaid";
                Console.WriteLine($"{f.Id,-5} {f.Borrowing?.BookCopy?.Book?.Title,-35} ₹{f.Amount,-9} {status,-10} {f.IssuedDate:yyyy-MM-dd}");
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }
    }
}
