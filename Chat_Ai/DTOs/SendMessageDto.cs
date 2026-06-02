namespace Chat_Ai.DTOs
{
    public class SendMessageDto
    {
        public string Message { get; set; } = null!;
        public int? ChatId { get; set; }
    }
}
