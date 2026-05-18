using LibraryManagement.Model.Models;

namespace LibraryManagement.BLL.Session
{
    /// <summary>
    /// Holds the currently logged-in member for the duration of the application session.
    /// All UI menus read from here instead of prompting for a member ID.
    /// </summary>
    public static class UserSession
    {
        /// <summary>The currently authenticated member. Null when no one is logged in.</summary>
        public static Member? CurrentMember { get; private set; }

        /// <summary>Sets the current member after successful login.</summary>
        public static void Login(Member member)
        {
            CurrentMember = member;
        }

        /// <summary>Clears the session, effectively logging the member out.</summary>
        public static void Logout()
        {
            CurrentMember = null;
        }

        /// <summary>Returns true if a member is currently logged in.</summary>
        public static bool IsLoggedIn => CurrentMember != null;
    }
}
