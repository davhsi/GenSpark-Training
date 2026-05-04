using System;
using System.Collections.Generic;
using SimpleNotificationSystem.Models;
using SimpleNotificationSystem.Services;

namespace SimpleNotificationSystem.UI.Commands
{
    internal class GetUserByPhoneNumberCommand : ICommand
    {
        private List<User> _users;
        private UserService _userService;

        public GetUserByPhoneNumberCommand(List<User> users, UserService userService)
        {
            _users = users;
            _userService = userService;
        }

        public string Description => "Get User Details by Phone Number";

        public void Execute()
        {
            Console.Write("Enter Phone Number: ");
            string phoneNumber = Console.ReadLine() ?? "";
            User? user = _userService.GetUserByPhoneNumber(_users, phoneNumber);
            if (user != null)
                Console.WriteLine(user.ToString() + "\n");
            else
                Console.WriteLine("User not found\n");
        }
    }
}
