namespace Entities.Entities.CommonField
{
    public interface ISlugEntity
    {
        string Slug { get; set; }
        string GetSlugSource();
    }
}
