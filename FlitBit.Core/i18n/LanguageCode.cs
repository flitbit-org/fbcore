
namespace FlitBit.Core.i18n
{
	/// <summary>
	/// IETF Langage Code.
	/// </summary>
	public sealed class LanguageCode
	{
		/// <summary>
		/// The two digit language code.
		/// </summary>
		public string Code { get; private set; }

		/// <summary>
		/// The language's name.
		/// </summary>
		public string Language { get; private set; }

		internal LanguageCode(string code, string name)
		{
			this.Code = code;
			this.Language = name;
		}
	}
}
