
namespace FlitBit.Core.Net
{
    /// <summary>
    /// Http authentication kinds.
    /// </summary>
    public enum HttpAuthenticationKind
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,
        /// <summary>
        /// Indicates Basic authentication.
        /// </summary>
        Basic = 1,
        /// <summary>
        /// Indicates digest authentication.
        /// </summary>
        Digest = 2,
        /// <summary>
        /// Indicates NTLM authentication (Windows specific).
        /// </summary>
        NTLM = 3,
    }
}
