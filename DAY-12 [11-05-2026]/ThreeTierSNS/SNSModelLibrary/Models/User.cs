namespace SNSModelLibrary
{
    // in real world scenarios country code is important for sms notifications
    // so i am adding it here
    public class Phone
    {
        public string CountryCode { get; set; } = "+91";
        public string Number { get; set; } = "";
    }

    public class Name
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
    }
    public partial class User
    {
        public int Id { get; set; }
        public Name Name { get; set; } = new Name { FirstName = "Guest", LastName = "User" };
        public string Email { get; set; } = "";
        public Phone Phone { get; set; } = new Phone();

        public User() { }

        // parameterised constructor overriding the default constructor.
        public User(string firstName, string lastName, string email, string countryCode, string phoneNumber)
        {
            Name = new Name { FirstName = firstName, LastName = lastName };
            Email = email;
            Phone = new Phone { CountryCode = countryCode, Number = phoneNumber };
        }

        // overriding the ToString() method - Dynamic Polymorphism
        public override string ToString()
        {
            return $"Name: {Name.FirstName} {Name.LastName} \nEmail: {Email} \nPhone: {Phone.CountryCode} {Phone.Number}";
        }
    }
}