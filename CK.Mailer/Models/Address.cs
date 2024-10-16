namespace CK.Mailer;

public readonly record struct Address( string Email, string? Name = null )
{
    public override string ToString() => Name is null ? Email : $"{Name} <{Email}>";
}
