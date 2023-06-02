namespace SmartHome.Core
{
    public interface IDeleted
    {
        bool IsDeleted { get; set; }
    }

    public interface IId
    {
        int Id { get; set; }
    }
}