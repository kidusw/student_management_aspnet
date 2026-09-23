namespace StudentManagement.Models
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Staff = "Staff";
        public const string Viewer = "Viewer";

        public static readonly string[] All = { Admin, Staff, Viewer };
    }
}
