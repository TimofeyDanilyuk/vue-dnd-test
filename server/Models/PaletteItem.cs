namespace server.Models;

public class PaletteItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public PaletteItem()
    {
        
    }
}