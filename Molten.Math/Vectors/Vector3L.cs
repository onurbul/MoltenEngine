using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "long"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Vector3L
	{
		///<summary>The X component.</summary>
		public long X;

		///<summary>The Y component.</summary>
		public long Y;

		///<summary>The Z component.</summary>
		public long Z;

		///<summary>Creates a new instance of <see cref = "Vector3L"/></summary>
		public Vector3L(long x, long y, long z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region operators
		public static Vector3L operator +(Vector3L left, Vector3L right)
		{
			return new Vector3L(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3L operator -(Vector3L left, Vector3L right)
		{
			return new Vector3L(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3L operator /(Vector3L left, Vector3L right)
		{
			return new Vector3L(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3L operator *(Vector3L left, Vector3L right)
		{
			return new Vector3L(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}
#endregion
	}
}

