using EnhancedSNS.Models;
using EnhancedSNS.Repositories;
using EnhancedSNS.Services;

namespace EnhancedSNS.UI
{
    internal class ConsoleUI
    {
        private readonly UserService _userService;
        private readonly NotificationRepository _notificationRepository;
        private readonly EmailNotificationService _emailService;
        private readonly SMSNotificationService _smsService;

        public ConsoleUI()
        {
            _userService = new UserService(new UserRepository());
            _notificationRepository = new NotificationRepository();
            _emailService = new EmailNotificationService();
            _smsService = new SMSNotificationService();
        }

        public void Start()
        {
            Console.WriteLine("=== Enhanced Simple Notification System ===");
            Console.WriteLine("Welcome to the SNS Management System!");

            while (true)
            {
                Console.WriteLine("\n--- Main Menu ---");
                Console.WriteLine("1. User Management");
                Console.WriteLine("2. Send Notification");
                Console.WriteLine("3. Exit");
                Console.Write("Enter your choice (1-3): ");

                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        UserManagementMenu();
                        break;
                    case "2":
                        SendNotificationMenu();
                        break;
                    case "3":
                        Console.WriteLine("Thank you for using SNS. Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        private void UserManagementMenu()
        {
            while (true)
            {
                Console.WriteLine("\n---- User Management ----");
                Console.WriteLine("1. Add New User");
                Console.WriteLine("2. View All Users");
                Console.WriteLine("3. Find User by Email");
                Console.WriteLine("4. Update User");
                Console.WriteLine("5. Delete User");
                Console.WriteLine("6. Back to Main Menu");
                Console.Write("Enter your choice (1-6): ");

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
                        FindUserByEmail();
                        break;
                    case "4":
                        UpdateUser();
                        break;
                    case "5":
                        DeleteUser();
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        private void AddUser()
        {
            Console.WriteLine("\n--- Add New User ---");
            Console.Write("Enter First Name: ");
            string firstName = Console.ReadLine() ?? "";
            
            Console.Write("Enter Last Name: ");
            string lastName = Console.ReadLine() ?? "";
            
            Console.Write("Enter Email: ");
            string email = Console.ReadLine() ?? "";
            
            Console.Write("Enter Country Code (e.g., +91): ");
            string countryCode = Console.ReadLine() ?? "+91";
            
            Console.Write("Enter Phone Number: ");
            string phoneNumber = Console.ReadLine() ?? "";

            var user = _userService.AddUser(firstName, lastName, email, countryCode, phoneNumber);
            
            Console.WriteLine("User added successfully!");
            Console.WriteLine(user.ToString());
        }

        private void ViewAllUsers()
        {
            Console.WriteLine("\n--- All Users ---");
            var users = _userService.GetAllUsers();
            
            if (users == null || users.Count == 0)
            {
                Console.WriteLine("No users found.");
                return;
            }

            for (int i = 0; i < users.Count; i++)
            {
                Console.WriteLine($"\n--- User {i + 1} ---");
                Console.WriteLine(users[i].ToString());
            }
        }

        private void FindUserByEmail()
        {
            Console.Write("\nEnter email to search: ");
            string email = Console.ReadLine() ?? "";
            
            var user = _userService.GetUserByEmail(email);
            if (user != null)
            {
                Console.WriteLine("User found:");
                Console.WriteLine(user.ToString());
            }
            else
            {
                Console.WriteLine("User not found.");
            }
        }

        private void UpdateUser()
        {
            Console.Write("\nEnter email of user to update: ");
            string email = Console.ReadLine() ?? "";
            
            var existingUser = _userService.GetUserByEmail(email);
            if (existingUser == null)
            {
                Console.WriteLine("User not found.");
                return;
            }

            Console.WriteLine("Current user details:");
            Console.WriteLine(existingUser.ToString());

            Console.Write("Enter new First Name (leave empty to keep current): ");
            string firstName = Console.ReadLine();

            Console.Write("Enter new Last Name (leave empty to keep current): ");
            string lastName = Console.ReadLine();

            Console.Write("Enter new Email (leave empty to keep current): ");
            string newEmail = Console.ReadLine();

            Console.Write("Enter new Phone Number (leave empty to keep current): ");
            string phoneNumber = Console.ReadLine();

            bool success = _userService.UpdateUser(email, firstName, lastName, newEmail, phoneNumber);
            if (success)
            {
                var updatedUser = _userService.GetUserByEmail(newEmail ?? email);
                Console.WriteLine("User updated successfully!");
                Console.WriteLine(updatedUser?.ToString());
            }
            else
            {
                Console.WriteLine("Failed to update user.");
            }
        }

        private void DeleteUser()
        {
            Console.Write("\nEnter email of user to delete: ");
            string email = Console.ReadLine() ?? "";
            
            var user = _userService.GetUserByEmail(email);
            if (user == null)
            {
                Console.WriteLine("User not found.");
                return;
            }

            Console.WriteLine("User to delete:");
            Console.WriteLine(user.ToString());

            Console.Write("Are you sure you want to delete this user? (y/N): ");
            string confirm = Console.ReadLine() ?? "";
            
            if (confirm.ToLower() == "y")
            {
                bool success = _userService.DeleteUser(email);
                if (success)
                {
                    Console.WriteLine("User deleted successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to delete user.");
                }
            }
            else
            {
                Console.WriteLine("Deletion cancelled.");
            }
        }

        private void SendNotificationMenu()
        {
            Console.WriteLine("\n--- Send Notification ---");
            Console.Write("Enter recipient email: ");
            string email = Console.ReadLine() ?? "";

            var user = _userService.GetUserByEmail(email);
            if (user == null)
            {
                Console.WriteLine("User not found.");
                return;
            }

            Console.Write("Enter message: ");
            string message = Console.ReadLine() ?? "";

            Console.WriteLine("\nChoose notification method:");
            Console.WriteLine("1. Email");
            Console.WriteLine("2. SMS");
            Console.Write("Enter choice (1-2): ");
            string choice = Console.ReadLine() ?? "";

            bool success = false;
            string notificationType = "";

            switch (choice)
            {
                case "1":
                    success = _emailService.SendNotification(user, message);
                    notificationType = "Email";
                    break;
                case "2":
                    success = _smsService.SendNotification(user, message);
                    notificationType = "SMS";
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    return;
            }

            if (success)
            {
                var notification = new Notification(message ?? "No message", notificationType);
                _notificationRepository.Create(notification);
                Console.WriteLine("Notification saved to system.");
            }
        }
    }
}
