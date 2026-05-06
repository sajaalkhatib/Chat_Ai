using System;
using System.Collections.Generic;

namespace Chat_Ai.Models;

public partial class Message
{
    public int Id { get; set; }

    public int ChatId { get; set; }

    public int SenderType { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Chat Chat { get; set; } = null!;
}
