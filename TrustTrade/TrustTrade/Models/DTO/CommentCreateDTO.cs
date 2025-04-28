using System.ComponentModel.DataAnnotations;

namespace TrustTrade.Models.DTO
{
    public class CommentCreateDTO
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Post ID must be a positive integer.")]
        public int PostId { get; set; }

        [Required]
        [StringLength(128, MinimumLength = 1, ErrorMessage = "Comment content must be between 1 and 128 characters long.")]
        public string Content { get; set; } = string.Empty;
    }
}