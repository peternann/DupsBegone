using System;

namespace DupsBegone
{
	/// <summary>
	/// AB hash. A simple xor hash based on, and incorporating summation of, 2 integer values.
	/// Use of xor means that order of presentation of data is not important.
	/// </summary>
	public class ABHash
	{
		public ulong a {get;set;}
		public ulong b {get;set;}
		private ulong hash;

		private ABHash()
		{
		}

		public ABHash(int a, int b)
		{
			this.a = (ulong)a;
			this.b = (ulong)b;
			hash = 0;
			hash ^= (ulong)a << ( a & 7 );
			hash ^= (ulong)b << ( b & 7 ) << 15;
		}

		public void Add(ABHash abHash)
		{
			a += abHash.a;
			b += abHash.b;
			hash ^= abHash.hash;
		}

		public override string ToString()
		{
			return ToString("{0:D}-{1:D}-{2:X}");
		}

		public string ToString(string format)
		{
			return String.Format(format, a, b, hash);
		}

	}
}

