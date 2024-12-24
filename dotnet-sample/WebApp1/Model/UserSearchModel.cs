namespace WebApp1.Model
{
    public class UserSearchModel
    {
        public bool UserFound { get; set; }
        public string Created { get; set; }
        public string LastLogIn { get; set; }
        public string AccountEnabled { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ObjectId { get; set; }
    }


    public class ExportedUserModel : UserSearchModel
    {
        public string UserName { get; set; }
    }
}
