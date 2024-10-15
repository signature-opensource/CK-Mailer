using System.Collections.Generic;
using System.Linq;

namespace CK.Mailer.Models;

public class SendResponse
{
    public string? MessageId { get; set; } = null;

    public IList<string> ErrorMessages { get; set; } = [];

    public bool Successful => !ErrorMessages.Any();
}
