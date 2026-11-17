using CK.AppIdentity;
using CK.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Mailer;

/// <summary>
/// The senders of a local party: a feature of each local party on which the "MailSender" feature is allowed (see
/// <see cref="MailSenderFeatureDriver"/>). A sender has a name: the one of its configuration ("NoReply", in the section of
/// its kind: <c>"MailKit": { "NoReply": {...} }</c>) or the name of a relay remote (<c>"$CorpRelay"</c>).
/// <para>
/// A sender is resolved when a mail is sent (<see cref="GetSender(string?)"/>): a relay may be added after the setup,
/// and a missing sender is an error of the sending, not of the configuration.
/// </para>
/// </summary>
public sealed class MailSenders
{
    readonly ILocalParty _party;
    readonly string? _defaultName;
    readonly IEmailSender _default;
    ImmutableDictionary<string, IEmailSender> _senders;

    internal MailSenders( ILocalParty party, IDictionary<string, IEmailSender> senders, string? defaultName )
    {
        _party = party;
        _senders = ImmutableDictionary.CreateRange( StringComparer.OrdinalIgnoreCase, senders );
        _defaultName = defaultName;
        _default = new NamedSender( this, null );
    }

    /// <summary>
    /// Gets the local party of these senders.
    /// </summary>
    public ILocalParty Party => _party;

    /// <summary>
    /// Gets the configured name of the default sender (<c>"MailSender": { "Default": "..." }</c>). Null when not
    /// configured: the single sender is the default.
    /// </summary>
    public string? DefaultName => _defaultName;

    /// <summary>
    /// Gets the names of the current senders.
    /// </summary>
    public IReadOnlyCollection<string> Names => _senders.Keys.ToArray();

    /// <summary>
    /// Finds a current sender by its name.
    /// </summary>
    /// <param name="name">The sender name.</param>
    /// <returns>The sender or null.</returns>
    public IEmailSender? Find( string name ) => _senders.TryGetValue( name, out var s ) ? s : null;

    /// <summary>
    /// Gets the default sender: <see cref="DefaultName"/>, or the single sender. It is resolved when a mail is sent.
    /// </summary>
    public IEmailSender Default => _default;

    /// <summary>
    /// Gets a sender by its name, the <see cref="Default"/> one when <paramref name="name"/> is null. It is resolved when a
    /// mail is sent: a missing sender fails the sending (the <see cref="SendResponse.ErrorMessages"/> say why).
    /// </summary>
    /// <param name="name">The sender name, null for the default one.</param>
    /// <returns>The sender.</returns>
    public IEmailSender GetSender( string? name ) => name == null ? _default : new NamedSender( this, name );

    /// <summary>
    /// Adds a sender: for a package that brings senders at run time (a relay remote). The name must not be used.
    /// </summary>
    /// <param name="monitor">The monitor to use.</param>
    /// <param name="name">The sender name.</param>
    /// <param name="sender">The sender.</param>
    /// <returns>True on success, false if the name is already used (an error is logged).</returns>
    public bool TryAdd( IActivityMonitor monitor, string name, IEmailSender sender )
    {
        Throw.CheckNotNullOrEmptyArgument( name );
        Throw.CheckNotNullArgument( sender );
        if( !ImmutableInterlocked.TryAdd( ref _senders, name, sender ) )
        {
            monitor.Error( $"'{_party}': the sender '{name}' already exists." );
            return false;
        }
        return true;
    }

    /// <summary>
    /// Removes a sender added by <see cref="TryAdd(IActivityMonitor, string, IEmailSender)"/>.
    /// </summary>
    /// <param name="name">The sender name.</param>
    /// <param name="sender">The sender to remove (another sender with the same name is not removed).</param>
    /// <returns>True if it has been removed.</returns>
    public bool Remove( string name, IEmailSender sender )
    {
        var d = _senders;
        while( d.TryGetValue( name, out var s ) && ReferenceEquals( s, sender ) )
        {
            var r = Interlocked.CompareExchange( ref _senders, d.Remove( name ), d );
            if( r == d ) return true;
            d = r;
        }
        return false;
    }

    /// <summary>
    /// Checks that the default sender is defined when there are several senders: an error is logged otherwise.
    /// </summary>
    /// <param name="monitor">The monitor to use.</param>
    /// <returns>True if the default sender is well defined.</returns>
    public bool CheckDefault( IActivityMonitor monitor )
    {
        if( _defaultName == null && _senders.Count > 1 )
        {
            monitor.Error( $"'{_party}' has several senders ('{_senders.Keys.Concatenate( "', '" )}'): \"{MailSenderFeatureDriver.MailSenderFeatureName}:Default\" must name one." );
            return false;
        }
        return true;
    }

    /// <summary>
    /// Gets whether a sender name is the name of a relay remote (a party name: it starts with '$').
    /// </summary>
    /// <param name="name">The sender name.</param>
    /// <returns>True for a relay name.</returns>
    public static bool IsRelayName( string name ) => name.StartsWith( '$' );

    /// <summary>
    /// Gets whether a name is a valid configured sender name: an identifier (letters, digits, '_', '-' and '.'), that
    /// cannot start with '$' (a relay is named after its remote).
    /// </summary>
    /// <param name="name">The name to test.</param>
    /// <returns>True if the name is valid.</returns>
    public static bool IsValidSenderName( string? name )
    {
        if( string.IsNullOrEmpty( name ) || !char.IsLetter( name[0] ) ) return false;
        foreach( var c in name )
        {
            if( !char.IsLetterOrDigit( c ) && c != '_' && c != '-' && c != '.' ) return false;
        }
        return true;
    }

    IEmailSender? Resolve( string? name, out string? error )
    {
        error = null;
        name ??= _defaultName;
        if( name != null )
        {
            var s = Find( name );
            if( s == null ) error = $"'{_party}' has no sender '{name}' (its senders are '{_senders.Keys.Concatenate( "', '" )}').";
            return s;
        }
        var all = _senders;
        if( all.Count == 1 ) return all.Values.First();
        error = all.Count == 0
                    ? $"'{_party}' has no mail sender: its \"{MailSenderFeatureDriver.MailSenderFeatureName}\" defines none."
                    : $"'{_party}' has several senders ('{all.Keys.Concatenate( "', '" )}'): \"{MailSenderFeatureDriver.MailSenderFeatureName}:Default\" must name one.";
        return null;
    }

    sealed class NamedSender : IEmailSender
    {
        readonly MailSenders _owner;
        readonly string? _name;

        public NamedSender( MailSenders owner, string? name )
        {
            _owner = owner;
            _name = name;
        }

        public Task<SendResponse> SendAsync( IActivityMonitor monitor, SimpleEmail email, CancellationToken token = default )
        {
            var sender = _owner.Resolve( _name, out var error );
            if( sender == null )
            {
                Throw.DebugAssert( error != null );
                monitor.Error( error );
                var r = new SendResponse();
                r.ErrorMessages.Add( error );
                return Task.FromResult( r );
            }
            return sender.SendAsync( monitor, email, token );
        }
    }
}
