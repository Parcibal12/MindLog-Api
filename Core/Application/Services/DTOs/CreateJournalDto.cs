using System.ComponentModel.DataAnnotations;

namespace MindLog.Api.Core.Application.DTOs
{
    public class CreateJournalDto
    {
        [Required(ErrorMessage = "El ID del usuario es obligatorio.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "El contenido del diario no puede estar vacío.")]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int EmotionId { get; set; }

        [Required]
        public string EmotionName { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "La intensidad debe estar estrictamente entre 1 y 10.")]
        public int Intensity { get; set; }

        public List<int> ContextTagIds { get; set; } = new List<int>();

        public string? AiFeedback { get; set; }
        public string? AiPattern { get; set; }
    }
}