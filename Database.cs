using Sneaky.Classes;

namespace Sneaky
{
    public class Database
    {
        public static void AddData(Context context)
        {
            // Check if admin exists in database
            if (context.Users.Any())
            {
                return;
            }

            // Define all necessary users, in our case it is admin
            var users = new User[]
            {
                new()
                {
                    Role = User.Roles.Admin,
                    Login = "Kirils",
                    Password = "Password",
                }
            };
            // Add users to database
            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
}
