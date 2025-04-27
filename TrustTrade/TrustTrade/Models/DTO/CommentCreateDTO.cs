using System.ComponentModel.DataAnnotations;

namespace TrustTrade.Models.DTO
{
    public class CommentCreateDTO
    {
        [Required]
        [StringLength(128, MinimumLength = 1, ErrorMessage = "Comment content must be between 1 and 128 characters long.")]
        public string Content { get; set; } = string.Empty;
    }
}