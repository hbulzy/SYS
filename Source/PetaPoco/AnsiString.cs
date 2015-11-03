using System;
namespace PetaPoco
{
	public class AnsiString
	{
		public string Value
		{
			get;
			private set;
		}
		public AnsiString(string str)
		{
			this.Value = str;
		}
	}
}
