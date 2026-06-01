namespace CMS.Server.Interfaces
{
    public interface IUserService
    {
        void hashPassword(string Password);
        void getUser();
        void createUser(IUser User);
        void changeUser(string Id, IUser User);
        void deleteUser(string Id);
    }
}
