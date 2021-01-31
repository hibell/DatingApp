namespace API.Entities
{
    public class AppUser
    {
        // Entity framework will automatically recognize that Id will be our primary key of our database, and will
        // automatically increment the Id field when a new record is added since it is an integer.
        public int Id { get; set; }

        // ASP.NET Core Identity uses `UserName` so we should too to save ourselves from having to refactor in the
        // future.
        public string UserName { get; set; }
    }
}