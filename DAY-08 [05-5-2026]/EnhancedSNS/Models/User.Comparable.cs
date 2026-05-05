using System;

namespace EnhancedSNS.Models
{
    public partial class User : IComparable<User>
    {
        public int CompareTo(User? other)
        {
            if (other == null) return 1;
            
            // Compare by email first, then by name if emails are the same
            int emailComparison = string.Compare(this.Email, other.Email, StringComparison.OrdinalIgnoreCase);
            if (emailComparison != 0) return emailComparison;
            
            return string.Compare($"{this.Name.FirstName} {this.Name.LastName}", 
                                $"{other.Name.FirstName} {other.Name.LastName}", 
                                StringComparison.OrdinalIgnoreCase);
        }
    }
}
