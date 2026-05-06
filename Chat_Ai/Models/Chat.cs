using System;
using System.Collections.Generic;

namespace Chat_Ai.Models;

public partial class Chat
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string? Title { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual User User { get; set; } = null!;
}
