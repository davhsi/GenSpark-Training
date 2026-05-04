using UnderstandingOOPSApp.Interfaces;
using UnderstandingOOPSApp.Services;

namespace UnderstandingOOPSApp
{
    internal class Program
    {
        ICustomerInteract customerInteract;
        public Program()
        {
            customerInteract = new CustomerService();
        }

        void DisplayMenu()
        {
            Console.WriteLine("Enter 1 to add a new account.\nEnter 2 to print account details using phone number.\n Enter 3 to print account details using account number.\n Enter 4 to quit the Application");
        }


        void DoBanking()
        {

            while (true)
            {
                DisplayMenu();
                if (int.TryParse(Console.ReadLine(), out int input))
                {
                    // switch (input)
                    // {
                    //     case 1:
                    //         customerInteract.OpensAccount();
                    //     case 2:
                    //         Console.WriteLine("Please Enter your account Number: ");
                    //         string accNo = Console.ReadLine();
                    //         if(strin)
                    //         customerInteract.PrintAccountDetailsByAccountNumber()

                    // }
                }
                else
                {
                    Console.WriteLine("Invalid option");
                    DisplayMenu();
                }

            }
            // var account = customerInteract.OpensAccount();
            // Console.WriteLine(account);
            Console.WriteLine("Please enter the account you like see");
            string accNum = Console.ReadLine() ?? "";
            customerInteract.PrintAccountDetailsByAccountNumber(accNum);

        }
        static void Main(string[] args)
        {
            new Program().DoBanking();
        }
    }
}