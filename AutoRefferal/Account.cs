namespace AutoRefferal
{
    public class Account
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public Account()
        {

        }

        public Account(string name, string email)
        {
            Name = name;
            Email = email;
        }
    }
}
