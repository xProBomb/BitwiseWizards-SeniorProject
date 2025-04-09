using System.ComponentModel.DataAnnotations;

namespace TrustTrade.ViewModels;

public class PostEditVM
{
    public int Id { get; set; }

    [Display(Name = "Title")]
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(128, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 128 characters long.")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Content")]
    [Required(ErrorMessage = "Content is required.")]
    [StringLength(1024, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 1024 characters long.")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Privacy Setting")]
    [Required(ErrorMessage = "Privacy setting is required.")]
    public bool IsPublic { get; set; }

    // Tag properties
    [Display(Name = "Tags")]
    public List<string> AvailableTags { get; set; } = new List<string>();

    [MaxLength(5, ErrorMessage = "No more than 5 tags are allowed.")]
    public List<string> SelectedTags { get; set; } = new List<string>();
}