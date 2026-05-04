using System;
using System.Collections.Generic;
using System.Linq;
using SimpleNotificationSystem.Models;
using SimpleNotificationSystem.Services;

namespace SimpleNotificationSystem.UI.Commands
{
    internal class UpdateUserCommand : ICommand
    {
        private List<User> _users;
        private UserService _userService;

        public UpdateUserCommand(List<User> users, UserService userService)
        {
            _users = users;
            _userService = userService;
        }

        public string Description => "Update User Details";

        public void Execute()
        {
            Console.Write("Enter Email of user to update: ");
            string email = Console.ReadLine() ?? "";
            User? user = _userService.GetUserByEmail(_users, email);
            if (user != null)
            {
                while (true)
                {
                    Console.WriteLine("\nWhat would you like to update?");
                    Console.WriteLine("1. First Name");
                    Console.WriteLine("2. Last Name");
                    Console.WriteLine("3. Country Code");
                    Console.WriteLine("4. Phone Number");
                    Console.WriteLine("5. Done Updating");
                    Console.Write("Choice: ");

                    if (!int.TryParse(Console.ReadLine(), out int choice))
                    {
                        Console.WriteLine("Invalid Input. Please try again.");
                        continue;
                    }

                    switch (choice)
                    {
                        case 1:
                            Console.Write("New First Name: ");
                            string newFirstName = Console.ReadLine() ?? "";
                            if (!string.IsNullOrEmpty(newFirstName)) user.Name.FirstName = newFirstName;
                            Console.WriteLine("First Name updated.");
                            break;
                        case 2:
                            Console.Write("New Last Name: ");
                            string newLastName = Console.ReadLine() ?? "";
                            if (!string.IsNullOrEmpty(newLastName)) user.Name.LastName = newLastName;
                            Console.WriteLine("Last Name updated.");
                            break;
                        case 3:
                            string newCountryCode = InputValidator.GetValidCountryCode();
                            if (!string.IsNullOrEmpty(newCountryCode)) 
                            {
                                user.Phone.CountryCode = newCountryCode;
                                Console.WriteLine("Country Code updated.");
                            }
                            break;
                        case 4:
                            string newPhoneNumber = InputValidator.GetValidPhoneNumber();
                            if (!string.IsNullOrEmpty(newPhoneNumber)) 
                            {
                                user.Phone.Number = newPhoneNumber;
                                Console.WriteLine("Phone Number updated.");
                            }
                            break;
                        case 5:
                            Console.WriteLine("User update complete.\n");
                            return;
                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("User not found\n");
            }
        }
    }
}
