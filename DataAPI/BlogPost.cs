using Dynamic.Core.Data.Interfaces;

public class BlogPost : IStorable
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public BlogPost()
    {
        // Default constructor for serialization
    }

    public BlogPost(string id, string title, string content)
    {
        Id = id;
        Title = title;
        Content = content;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContent(string newContent)
    {
        Content = newContent;
        UpdatedAt = DateTime.UtcNow;
    }
}