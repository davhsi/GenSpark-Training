namespace LibraryManagement.Model.Exceptions
{
    /// <summary>
    /// Thrown when a membership type ID does not correspond to any valid type.
    /// </summary>
    public class InvalidMembershipTypeException : LibraryException
    {
        public InvalidMembershipTypeException(int typeId)
            : base($"Membership type with ID {typeId} does not exist.") { }
    }
}
