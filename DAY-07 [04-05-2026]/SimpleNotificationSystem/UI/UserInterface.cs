using SimpleNotificationSystem.Models;
using SimpleNotificationSystem.Services;
using System.Linq;
using System;
using System.Collections.Generic;

namespace SimpleNotificationSystem.UI
{
    internal class UserInterface
    {
        
        private List<User> _users;
        private UserService _userService;
        private NotificationManager _notificationManager;

        // constructor overriding the default constructor
        public UserInterface()
        {
            _users = new List<User>();
            _userService = new UserService();
            _notificationManager = new NotificationManager();
        }

        public void Start()
        {
            Console.WriteLine("Simple Notification System");
            Console.WriteLine("-----------------------------");

            while (true)
            {
                DisplayMenu();
                if (!int.TryParse(Console.ReadLine(), out int input))
                {
                    Console.WriteLine("Invalid Input. Please try again\n");
                    continue;
                }

                switch (input)
                {
                    case 1: AddUserUI(); break;
                    case 2: GetUserDetailsByEmailUI(); break;
                    case 3: GetUserDetailsByPhoneNumberUI(); break;
                    case 4: UpdateUserDetailsUI(); break;
                    case 5: DeleteUserUI(); break;
                    case 6: NotifyViaSMSUI(); break;
                    case 7: NotifyViaEmailUI(); break;
                    case 8: Console.WriteLine("Exiting Application. ThankYou!"); return;
                    default: Console.WriteLine("Invalid option. Please try again\n"); break;
                }
            }
        }

        private void AddUserUI()
        {
            Console.WriteLine("Enter user details: ");
            Console.Write("First Name: ");
            string firstName = Console.ReadLine() ?? "";
            Console.Write("Last Name: ");
            string lastName = Console.ReadLine() ?? "";

            // validation for email
            string email = "";
            while (true)
            {
                Console.Write("Email: ");
                email = Console.ReadLine() ?? "";
                if (email.Contains("@") && email.Contains("."))
                    break;
                Console.WriteLine("Invalid email format. Please ensure it contains '@' and '.'.");
            }

            Console.Write("Country Code (don't include +): ");
            string countryCode = Console.ReadLine() ?? "";
            if (!string.IsNullOrEmpty(countryCode) && !countryCode.StartsWith("+"))
            {
                countryCode = "+" + countryCode;
            }

            // validation for phone number
            string phoneNumber = "";
            while (true)
            {
                Console.Write("Phone Number (10 digits): ");
                phoneNumber = Console.ReadLine() ?? "";
                if (phoneNumber.Length == 10 && phoneNumber.All(char.IsDigit))
                    break;
                Console.WriteLine("Invalid phone number. Must be exactly 10 digits.");
            }
            // creating new user using constructor
            User user = new User(firstName, lastName, email, countryCode, phoneNumber);
            _userService.AddUser(_users, user);
            Console.WriteLine("User added successfully\n");
        }

        private void GetUserDetailsByEmailUI()
        {
            Console.Write("Enter Email of user to view details: ");
            string email = Console.ReadLine() ?? "";
            User? user = _userService.GetUserByEmail(_users, email);
            if (user != null)
                Console.WriteLine(user.ToString() + "\n");
            else
                Console.WriteLine("User not found\n");
        }

        
        private void GetUserDetailsByPhoneNumberUI()
        {
            Console.Write("Enter Phone Number: ");
            string phoneNumber = Console.ReadLine() ?? "";
            User? user = _userService.GetUserByPhoneNumber(_users, phoneNumber);
            if (user != null)
                Console.WriteLine(user.ToString() + "\n");
            else
                Console.WriteLine("User not found\n");
        }

        private void UpdateUserDetailsUI()
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
                    // updating user details by user
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
                            Console.Write("New Country Code (don't include +): ");
                            string newCountryCode = Console.ReadLine() ?? "";
                            if (!string.IsNullOrEmpty(newCountryCode))
                            {
                                if (!newCountryCode.StartsWith("+")) newCountryCode = "+" + newCountryCode;
                                user.Phone.CountryCode = newCountryCode;
                                Console.WriteLine("Country Code updated.");
                            }
                            break;
                        case 4:
                            Console.Write("New Phone Number (10 digits): ");
                            string newPhoneNumber = Console.ReadLine() ?? "";
                            if (!string.IsNullOrEmpty(newPhoneNumber))
                            {
                                if (newPhoneNumber.Length == 10 && newPhoneNumber.All(char.IsDigit))
                                {
                                    user.Phone.Number = newPhoneNumber;
                                    Console.WriteLine("Phone Number updated.");
                                }
                                else
                                {
                                    Console.WriteLine("Invalid phone number. Must be exactly 10 digits. Update failed.");
                                }
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

        private void DeleteUserUI()
        {
            Console.Write("Enter Email of user to delete: ");
            string email = Console.ReadLine() ?? "";
            User? user = _userService.GetUserByEmail(_users, email);
            if (user != null)
            {
                _userService.DeleteUser(_users, user);
                Console.WriteLine("User deleted successfully\n");
            }
            else
            {
                Console.WriteLine("User not found\n");
            }
        }

        private void NotifyViaSMSUI()
        {
            Console.Write("Enter Email of user to notify via SMS: ");
            string smsEmail = Console.ReadLine() ?? "";
            User? smsUser = _userService.GetUserByEmail(_users, smsEmail);
            if (smsUser != null) _notificationManager.SendNotificationBySMS(smsUser);
            else Console.WriteLine("User not found\n");
        }

        private void NotifyViaEmailUI()
        {
            Console.Write("Enter Email of user to notify via Email: ");
            string emailToNotify = Console.ReadLine() ?? "";
            User? emailUser = _userService.GetUserByEmail(_users, emailToNotify);
            if (emailUser != null) _notificationManager.SendNotificationByEmail(emailUser);
            else Console.WriteLine("User not found\n");
        }

        private void DisplayMenu()
        {
            Console.WriteLine("Please enter one of the options listed below: ");
            Console.WriteLine("1. Add User");
            Console.WriteLine("2. Get User Details by Email");
            Console.WriteLine("3. Get User Details by Phone Number");
            Console.WriteLine("4. Update User Details");
            Console.WriteLine("5. Delete User");
            Console.WriteLine("6. Send Notification by SMS");
            Console.WriteLine("7. Send Notification by Email");
            Console.WriteLine("8. Exit Application");
            Console.Write("Choice: ");
        }
    }
}
