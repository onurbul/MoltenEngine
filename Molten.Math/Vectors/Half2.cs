using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "short"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public partial struct Half2
	{
		///<summary>The X component.</summary>
		public short X;

		///<summary>The Y component.</summary>
		public short Y;


		public static Half2 One = new Half2((short)1, (short)1);

		public static Half2 Zero = new Half2(0, 0);

		///<summary>Creates a new instance of <see cref = "Half2"/></summary>
		public Half2(short x, short y)
		{
			X = x;
			Y = y;
		}

#region operators
		public static Half2 operator +(Half2 left, Half2 right)
		{
			return new Half2(left.X + right.X, left.Y + right.Y);
		}

		public static Half2 operator -(Half2 left, Half2 right)
		{
			return new Half2(left.X - right.X, left.Y - right.Y);
		}

		public static Half2 operator /(Half2 left, Half2 right)
		{
			return new Half2(left.X / right.X, left.Y / right.Y);
		}

		public static Half2 operator *(Half2 left, Half2 right)
		{
			return new Half2(left.X * right.X, left.Y * right.Y);
		}
#endregion
	}
}

