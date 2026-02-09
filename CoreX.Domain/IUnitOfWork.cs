namespace CoreX.Domain
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
    }
}
