using System;
namespace PetaPoco
{
	public class Transaction : ITransaction, System.IDisposable
	{
		private Database _db;
		public Transaction(Database db)
		{
			this._db = db;
			this._db.BeginTransaction();
		}
		public void Complete()
		{
			this._db.CompleteTransaction();
			this._db = null;
		}
		public void Dispose()
		{
			if (this._db != null)
			{
				this._db.AbortTransaction();
			}
		}
	}
}
