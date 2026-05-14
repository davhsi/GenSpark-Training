using SNSDALLibrary;
using SNSModelLibrary;
using SNSBALLibrary.Services;
using SNSModelLibrary.Exceptions;

namespace SNSFEApplication.UI
{
    internal class ConsoleUI
    {
        private readonly UserService _userService;
        private readonly NotificationRepository _notificationRepository;
        private readonly NotificationService _notificationService;

        public ConsoleUI()
        {
            
            _userService = new UserService(new UserRepository());
            _notificationRepository = new NotificationRepository();
            _notificationService = new NotificationService(_notificationRepository);
        }

        public void Start()
        {
            Console.WriteLine("=== 3Tier Notification System ===");

            while (true)
            {
                Console.WriteLine("\n1. Add User");
                Console.WriteLine("2. View All Users");
                Console.WriteLine("3. Update User");
                Console.WriteLine("4. Delete User");
                Console.WriteLine("5. Send Notification");
                Console.WriteLine("6. Send Notification to Everyone");
                Console.WriteLine("7. View Sent Notifications");
                Console.WriteLine("8. Exit");
                Console.Write("Select option: ");

                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        AddUser();
                        break;
                    case "2":
                        ViewAllUsers();
                        break;
                    case "3":
                        UpdateUser();
                        break;
                    case "4":
                        DeleteUser();
                        break;
                    case "5":
                        SendNotification();
                        break;
                    case "6":
                        SendToEveryone();
                        break;
                    case "7":
                        ViewNotifications();
                        break;
                    case "8":
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        private void AddUser()
        {
            Console.Write("First Name: ");
            string fName = Console.ReadLine() ?? "";
            Console.Write("Last Name: ");
            string lName = Console.ReadLine() ?? "";
            Console.Write("Email: ");
            string email = Console.ReadLine() ?? "";
            Console.Write("Country Code (e.g. +91): ");
            string countryCode = Console.ReadLine() ?? "";
            Console.Write("Phone: ");
            string phone = Console.ReadLine() ?? "";

            try
            {
                _userService.AddUser(fName, lName, email, countryCode, phone);
                Console.WriteLine($"User {fName} {lName} has been added successfully.");
            }
            catch (InvalidPhoneNumberException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InvalidEmailException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (UserAlreadyExistsException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Validation Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }

        private void ViewAllUsers()
        {
            var users = _userService.GetAllUsers();
            if (users.Count == 0) Console.WriteLine("No users found. Try adding a user first.");
            else users.ForEach(Console.WriteLine);
        }

        private void UpdateUser()
        {
            Console.Write("Please enter Email to update: ");
            string email = Console.ReadLine() ?? "";
            Console.Write("New First Name (empty to skip): ");
            string? fName = Console.ReadLine();
            Console.Write("New Last Name (empty to skip): ");
            string? lName = Console.ReadLine();
            Console.Write("New Phone (empty to skip): ");
            string? phone = Console.ReadLine();

            try
            {
                if (_userService.UpdateUser(email, fName, lName, phone))
                {
                    Console.WriteLine($"User updated successfully.");
                }
                else
                {
                    Console.WriteLine("Sorry, User not found.\nKindly ensure the email is correct and try again.");
                }
            }
            catch (InvalidPhoneNumberException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InvalidEmailException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Validation Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }

        private void DeleteUser()
        {
            Console.Write("Email to delete: ");
            string email = Console.ReadLine() ?? "";
            if (_userService.DeleteUser(email))
            {
                Console.WriteLine("Deleted.");
            }
            else
            {
                Console.WriteLine("Sorry, User not found.\nKindly ensure the email is correct and try again.");
            }
        }

        private void SendNotification()
        {
            Console.Write("Please enter Recipient Email: ");
            string email = Console.ReadLine() ?? "";

            User user;
            try
            {
                user = _userService.GetUserByEmail(email);
            }
            catch (UserNotFoundException)
            {
                Console.WriteLine("Sorry, User not found.\nKindly ensure the email is correct and try again.");
                return;
            }

            Console.Write("Message: ");
            string msg = Console.ReadLine() ?? "";

            Console.WriteLine("Type:\n 1. Email\n2. SMS");
            string typeChoice = Console.ReadLine() ?? "";
            string type = typeChoice == "2" ? "SMS" : "Email";

            try
            {
                _notificationService.ProcessAndSendNotification(user, type, msg);
                Console.WriteLine($"Notification sent successfully to {user.Email}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sorry, an error occurred: {ex.Message}");
            }
        }

        private void ViewNotifications()
        {
            var notes = _notificationRepository.GetAllWithUserDetails();
            if (notes == null || notes.Count == 0)
            {
                Console.WriteLine("No notifications found.");
                return;
            }

            foreach (var n in notes)
            {
                Console.WriteLine(n.ToString());
            }
        }

        private void SendToEveryone()
        {
            var users = _userService.GetAllUsers();
            if (users.Count == 0)
            {
                Console.WriteLine("No users found. Add some users first.");
                return;
            }

            Console.Write("Message to all: ");
            string msg = Console.ReadLine() ?? "";

            Console.WriteLine("Type:\n 1. Email\n2. SMS");
            string typeChoice = Console.ReadLine() ?? "";
            string type = typeChoice == "2" ? "SMS" : "Email";

            try
            {
                _notificationService.SendToAllNotifications(users, type, msg);
                Console.WriteLine($"Notification sent successfully to {users.Count} users.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sorry, an error occurred: {ex.Message}");
            }
        }
    }
}
