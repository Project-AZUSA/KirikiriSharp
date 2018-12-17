using System;

namespace Tjs2.Sharpen
{
    public class Authenticator
	{
		protected virtual PasswordAuthentication GetPasswordAuthentication ()
		{
			throw new NotImplementedException ();
		}

		public string GetRequestingHost ()
		{
			throw new NotImplementedException ();
		}

		public int GetRequestingPort ()
		{
			throw new NotImplementedException ();
		}
	}
}
