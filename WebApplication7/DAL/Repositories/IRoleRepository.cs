using WebApplication7.DAL.Enteties;

namespace WebApplication7.DAL.Repositories
{
    public interface IRoleRepository
    {
        public Task<Role[]> GetAll();
        public Task<Role> GetRoleById(int id);
        public Task<Role[]> GetAuthorRolesAsync(Author author);
    }
}
