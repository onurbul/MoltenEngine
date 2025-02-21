using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Molten.HalfPrecision;
using Molten.DoublePrecision;

namespace Molten
{
	///<summary>A <see cref="int"/> vector comprised of two components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=4)]
    [Serializable]
	public partial struct Vector2I : IFormattable, IVector<int>, IEquatable<Vector2I>
	{
		///<summary>The size of <see cref="Vector2I"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2I));

		///<summary>A Vector2I with every component set to 1.</summary>
		public static readonly Vector2I One = new Vector2I(1, 1);

        static readonly string toStringFormat = "X:{0} Y:{1}";

		/// <summary>The X unit <see cref="Vector2I"/>.</summary>
		public static readonly Vector2I UnitX = new Vector2I(1, 0);

		/// <summary>The Y unit <see cref="Vector2I"/>.</summary>
		public static readonly Vector2I UnitY = new Vector2I(0, 1);

		/// <summary>Represents a zero'd Vector2I.</summary>
		public static readonly Vector2I Zero = new Vector2I(0, 0);

		/// <summary>The X component.</summary>
		[DataMember]
		public int X;

		/// <summary>The Y component.</summary>
		[DataMember]
		public int Y;


        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == 0 && Y == 0;
        }

#region Constructors
		/// <summary>Initializes a new instance of <see cref="Vector2I"/>.</summary>
		/// <param name="value">The value that will be assigned to all components.</param>
		public Vector2I(int value)
		{
			X = value;
			Y = value;
		}
		/// <summary>Initializes a new instance of <see cref="Vector2I"/> from an array.</summary>
		/// <param name="values">The values to assign to the X, Y components of the color. This must be an array with at least two elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
		public Vector2I(int[] values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 2)
				throw new ArgumentOutOfRangeException("values", "There must be at least two input values for Vector2I.");

			X = values[0];
			Y = values[1];
		}
		/// <summary>Initializes a new instance of <see cref="Vector2I"/> from a span.</summary>
		/// <param name="values">The values to assign to the X, Y components of the color. This must be an array with at least two elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
		public Vector2I(Span<int> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 2)
				throw new ArgumentOutOfRangeException("values", "There must be at least two input values for Vector2I.");

			X = values[0];
			Y = values[1];
		}
		/// <summary>Initializes a new instance of <see cref="Vector2I"/> from a an unsafe pointer.</summary>
		/// <param name="ptrValues">The values to assign to the X, Y components of the color.
		/// <para>There must be at least two elements available or undefined behaviour will occur.</para></param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ptrValues"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ptrValues"/> contains more or less than four elements.</exception>
		public unsafe Vector2I(int* ptrValues)
		{
			if (ptrValues == null)
				throw new ArgumentNullException("ptrValues");

			X = ptrValues[0];
			Y = ptrValues[1];
		}
		/// <summary>
		/// Initializes a new instance of <see cref="Vector2I"/>.
		/// </summary>
		/// <param name="x">The X component.</param>
		/// <param name="y">The Y component.</param>
		public Vector2I(int x, int y)
		{
			X = x;
			Y = y;
		}

#endregion

#region Instance Methods
        /// <summary>
        /// Determines whether the specified <see cref = "Vector2I"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector2I"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector2I"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Vector2I other)
        {
            return other.X == X && other.Y == Y;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector2I"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector2I"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector2I"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector2I other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector2I"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Vector2I"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector2I"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value is Vector2I v)
               return Equals(ref v);

            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Calculates the squared length of the vector.
        /// </summary>
        /// <returns>The squared length of the vector.</returns>
        /// <remarks>
        /// This method may be preferred to <see cref="Vector2F.Length"/> when only a relative length is needed
        /// and speed is of the essence.
        /// </remarks>
        public int LengthSquared()
        {
            return ((X * X) + (Y * Y));
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector2I"/>.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public int[] ToArray()
        {
            return new int[] { X, Y };
        }
		/// <summary>
        /// Reverses the direction of the current <see cref="Vector2I"/>.
        /// </summary>
        /// <returns>A <see cref="Vector2I"/> facing the opposite direction.</returns>
		public Vector2I Negate()
		{
			return new Vector2I(-X, -Y);
		}
		

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(int min, int max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Vector2I min, Vector2I max)
        {
            Clamp(min, max);
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(ref Vector2I min, ref Vector2I max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
        }
#endregion

#region To-String
		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Vector2I"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, format, X, Y);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Vector2I"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, toStringFormat, X, Y);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Vector2I"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Vector2I"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, toStringFormat, X, Y);
        }

		/// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="Vector2I"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider,
                toStringFormat,
				X.ToString(format, formatProvider),
				Y.ToString(format, formatProvider)
            );
        }
#endregion

#region Add operators
		///<summary>Performs a add operation on two <see cref="Vector2I"/>.</summary>
		///<param name="a">The first <see cref="Vector2I"/> to add.</param>
		///<param name="b">The second <see cref="Vector2I"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Vector2I a, ref Vector2I b, out Vector2I result)
		{
			result.X = a.X + b.X;
			result.Y = a.Y + b.Y;
		}

		///<summary>Performs a add operation on two <see cref="Vector2I"/>.</summary>
		///<param name="a">The first <see cref="Vector2I"/> to add.</param>
		///<param name="b">The second <see cref="Vector2I"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2I operator +(Vector2I a, Vector2I b)
		{
			Add(ref a, ref b, out Vector2I result);
			return result;
		}

		///<summary>Performs a add operation on a $<see cref="Vector2I"/> and a $<see cref="int"/>.</summary>
		///<param name="a">The <see cref="Vector2I"/> to add.</param>
		///<param name="b">The <see cref="int"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Vector2I a, int b, out Vector2I result)
		{
			result.X = a.X + b;
			result.Y = a.Y + b;
		}

		///<summary>Performs a add operation on a $<see cref="Vector2I"/> and a $<see cref="int"/>.</summary>
		///<param name="a">The <see cref="Vector2I"/> to add.</param>
		///<param name="b">The <see cref="int"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2I operator +(Vector2I a, int b)
		{
			Add(ref a, b, out Vector2I result);
			return result;
		}

		///<summary>Performs a add operation on a $<see cref="int"/> and a $<see cref="Vector2I"/>.</summary>
		///<param name="a">The <see cref="int"/> to add.</param>
		///<param name="b">The <see cref="Vector2I"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2I operator +(int a, Vector2I b)
		{
			Add(ref b, a, out Vector2I result);
			return result;
		}


		/// <summary>
        /// Assert a <see cref="Vector2I"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector2I"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector2I"/>.</returns>
        public static Vector2I operator +(Vector2I value)
        {
            return value;
        }
#endregion

#region Subtract operators
		///<summary>Performs a subtract operation on two <see cref="Vector2I"/>.</summary>
		///<param name="a">The first <see cref="Vector2I"/> to subtract.</param>
		///<param name="b">The second <see cref="Vector2I"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Vector2I a, ref Vector2I b, out Vector2I result)
		{
			result.X = a.X - b.X;
			result.Y = a.Y - b.Y;
		}

		///<summary>Performs a subtract operation on two <see cref="Vector2I"/>.</summary>
		///<param name="a">The first <see cref="Vector2I"/> to subtract.</param>
		///<param name="b">The second <see cref="Vector2I"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2I operator -(Vector2I a, Vector2I b)
		{
			Subtract(ref a, ref b, out Vector2I result);
			return result;
		}

		///<summary>Performs a subtract operation on a $<see cref="Vector2I"/> and a $<see cref="int"/>.</summary>
		///<param name="a">The <see cref="Vector2I"/> to subtract.</param>
		///<param name="b">The <see cref="int"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Vector2I a, int b, out Vector2I result)
		{
			result.X = a.X - b;
			result.Y = a.Y - b;
		}

		///<summary>Performs a subtract operation on a $<see cref="Vector2I"/> and a $<see cref="int"/>.</summary>
		///<param name="a">The <see cref="Vector2I"/> to subtract.</param>
		///<param name="b">The <see cref="int"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2I operator -(Vector2I a, int b)
		{
			Subtract(ref a, b, out Vector2I result);
			return result;
		}

		///<summary>Performs a subtract operation on a $<see cref="int"/> and a $<see cref="Vector2I"/>.</summary>
		///<param name="a">The <see cref="int"/> to subtract.</param>
		///<param name="b">The <see cref="Vector2I"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2I operator -(int a, Vector2I b)
		{
			Subtract(ref b, a, out Vector2I result);
			return result;
		}


        /// <summary>
        /// Negate/reverse the direction of a <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector2I"/> to reverse.</param>
        /// <param name="result">The output for the reversed <see cref="Vector2I"/>.</param>
        public static void Negate(ref Vector2I value, out Vector2I result)
        {
			result.X = -value.X;
			result.Y = -value.Y;
            
        }

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector2I"/> to reverse.</param>
        /// <returns>The reversed <see cref="Vector2I"/>.</returns>
        public static Vector2I operator -(Vector2I value)
        {
            Negate(ref value, out value);
            return value;
        }
#endregion

#region division operators
		///<summary>Performs a divide operation on two <see cref="Vector2I"/>.</summary>
		///<param name="a">The first <see cref="Vector2I"/> to divide.</param>
		///<param name="b">The second <see cref="Vector2I"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref Vector2I a, ref Vector2I b, out Vector2I result)
		{
			result.X = a.X / b.X;
			result.Y = a.Y / b.Y;
		}

		///<summary>Performs a divide operation on two <see cref="Vector2I"/>.</summary>
		///<param name="a">The first <see cref="Vector2I"/> to divide.</param>
		///<param name="b">The second <see cref="Vector2I"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2I operator /(Vector2I a, Vector2I b)
		{
			Divide(ref a, ref b, out Vector2I result);
			return result;
		}

		///<summary>Performs a divide operation on a $<see cref="Vector2I"/> and a $<see cref="int"/>.</summary>
		///<param name="a">The <see cref="Vector2I"/> to divide.</param>
		///<param name="b">The <see cref="int"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref Vector2I a, int b, out Vector2I result)
		{
			result.X = a.X / b;
			result.Y = a.Y / b;
		}

		///<summary>Performs a divide operation on a $<see cref="Vector2I"/> and a $<see cref="int"/>.</summary>
		///<param name="a">The <see cref="Vector2I"/> to divide.</param>
		///<param name="b">The <see cref="int"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2I operator /(Vector2I a, int b)
		{
			Divide(ref a, b, out Vector2I result);
			return result;
		}

		///<summary>Performs a divide operation on a $<see cref="int"/> and a $<see cref="Vector2I"/>.</summary>
		///<param name="a">The <see cref="int"/> to divide.</param>
		///<param name="b">The <see cref="Vector2I"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2I operator /(int a, Vector2I b)
		{
			Divide(ref b, a, out Vector2I result);
			return result;
		}

#endregion

#region Multiply operators
		///<summary>Performs a multiply operation on two <see cref="Vector2I"/>.</summary>
		///<param name="a">The first <see cref="Vector2I"/> to multiply.</param>
		///<param name="b">The second <see cref="Vector2I"/> to multiply.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Multiply(ref Vector2I a, ref Vector2I b, out Vector2I result)
		{
			result.X = a.X * b.X;
			result.Y = a.Y * b.Y;
		}

		///<summary>Performs a multiply operation on two <see cref="Vector2I"/>.</summary>
		///<param name="a">The first <see cref="Vector2I"/> to multiply.</param>
		///<param name="b">The second <see cref="Vector2I"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2I operator *(Vector2I a, Vector2I b)
		{
			Multiply(ref a, ref b, out Vector2I result);
			return result;
		}

		///<summary>Performs a multiply operation on a $<see cref="Vector2I"/> and a $<see cref="int"/>.</summary>
		///<param name="a">The <see cref="Vector2I"/> to multiply.</param>
		///<param name="b">The <see cref="int"/> to multiply.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Multiply(ref Vector2I a, int b, out Vector2I result)
		{
			result.X = a.X * b;
			result.Y = a.Y * b;
		}

		///<summary>Performs a multiply operation on a $<see cref="Vector2I"/> and a $<see cref="int"/>.</summary>
		///<param name="a">The <see cref="Vector2I"/> to multiply.</param>
		///<param name="b">The <see cref="int"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2I operator *(Vector2I a, int b)
		{
			Multiply(ref a, b, out Vector2I result);
			return result;
		}

		///<summary>Performs a multiply operation on a $<see cref="int"/> and a $<see cref="Vector2I"/>.</summary>
		///<param name="a">The <see cref="int"/> to multiply.</param>
		///<param name="b">The <see cref="Vector2I"/> to multiply.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2I operator *(int a, Vector2I b)
		{
			Multiply(ref b, a, out Vector2I result);
			return result;
		}

#endregion

#region Operators - Equality
        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector2I left, Vector2I right)
        {
            return left.Equals(ref right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector2I left, Vector2I right)
        {
            return !left.Equals(ref right);
        }
#endregion

#region Static Methods
        /// <summary>
        /// Performs a cubic interpolation between two vectors.
        /// </summary>
        /// <param name="start">Start vector.</param>
        /// <param name="end">End vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2I SmoothStep(ref Vector2I start, ref Vector2I end, float amount)
        {
            amount = MathHelper.SmoothStep(amount);
            return Lerp(ref start, ref end, amount);
        }

        /// <summary>
        /// Performs a cubic interpolation between two vectors.
        /// </summary>
        /// <param name="start">Start vector.</param>
        /// <param name="end">End vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The cubic interpolation of the two vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2I SmoothStep(Vector2I start, Vector2I end, int amount)
        {
            return SmoothStep(ref start, ref end, amount);
        }    

        /// <summary>
        /// Orthogonalizes a list of <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="destination">The list of orthogonalized <see cref="Vector2I"/>.</param>
        /// <param name="source">The list of vectors to orthogonalize.</param>
        /// <remarks>
        /// <para>Orthogonalization is the process of making all vectors orthogonal to each other. This
        /// means that any given vector in the list will be orthogonal to any other given vector in the
        /// list.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting vectors
        /// tend to be numerically unstable. The numeric stability decreases according to the vectors
        /// position in the list so that the first vector is the most stable and the last vector is the
        /// least stable.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Orthogonalize(Vector2I[] destination, params Vector2I[] source)
        {
            //Uses the modified Gram-Schmidt process.
            //q1 = m1
            //q2 = m2 - ((q1 ⋅ m2) / (q1 ⋅ q1)) * q1
            //q3 = m3 - ((q1 ⋅ m3) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m3) / (q2 ⋅ q2)) * q2
            //q4 = m4 - ((q1 ⋅ m4) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m4) / (q2 ⋅ q2)) * q2 - ((q3 ⋅ m4) / (q3 ⋅ q3)) * q3
            //q5 = ...

            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            for (int i = 0; i < source.Length; ++i)
            {
                Vector2I newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= (Dot(destination[r], newvector) / Dot(destination[r], destination[r])) * destination[r];

                destination[i] = newvector;
            }
        }

        

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Vector2I"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="Vector2I"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
		/// <param name="xIndex">The axis index to use for the new X value.</param>
		/// <param name="yIndex">The axis index to use for the new Y value.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector2I Swizzle(Vector2I val, int xIndex, int yIndex)
        {
            return new Vector2I()
            {
			   X = (&val.X)[xIndex],
			   Y = (&val.X)[yIndex],
            };
        }

        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector2I Swizzle(Vector2I val, uint xIndex, uint yIndex)
        {
            return new Vector2I()
            {
			    X = (&val.X)[xIndex],
			    Y = (&val.X)[yIndex],
            };
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="Vector2I"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector2I"/> source vector</param>
        /// <param name="right">Second <see cref="Vector2I"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Dot(ref Vector2I left, ref Vector2I right)
        {
			return ((left.X * right.X) + (left.Y * right.Y));
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Vector2I"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector2I"/> source vector</param>
        /// <param name="right">Second <see cref="Vector2I"/> source vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Dot(Vector2I left, Vector2I right)
        {
			return ((left.X * right.X) + (left.Y * right.Y));
        }

		/// <summary>
        /// Returns a <see cref="Vector2I"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Vector2I"/> containing the 2D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Vector2I"/> containing the 2D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Vector2I"/> containing the 2D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Vector2I Barycentric(ref Vector2I value1, ref Vector2I value2, ref Vector2I value3, int amount1, int amount2)
        {
			return new Vector2I(
				((value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X))), 
				((value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y)))
			);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector2I"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Lerp(ref Vector2I start, ref Vector2I end, float amount, out Vector2I result)
        {
			result.X = (int)((1F - amount) * start.X + amount * end.X);
			result.Y = (int)((1F - amount) * start.Y + amount * end.Y);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2I Lerp(Vector2I start, Vector2I end, float amount)
        {
			return new Vector2I()
			{
				X = (int)((1F - amount) * start.X + amount * end.X),
				Y = (int)((1F - amount) * start.Y + amount * end.Y),
			};
        }

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2I Lerp(ref Vector2I start, ref Vector2I end, float amount)
        {
			return new Vector2I()
			{
				X = (int)((1F - amount) * start.X + amount * end.X),
				Y = (int)((1F - amount) * start.Y + amount * end.Y),
			};
        }

        /// <summary>
        /// Returns a <see cref="Vector2I"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2I"/>.</param>
        /// <param name="right">The second source <see cref="Vector2I"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector2I"/>.</param>
        /// <returns>A <see cref="Vector2I"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Min(ref Vector2I left, ref Vector2I right, out Vector2I result)
		{
				result.X = (left.X < right.X) ? left.X : right.X;
				result.Y = (left.Y < right.Y) ? left.Y : right.Y;
		}

        /// <summary>
        /// Returns a <see cref="Vector2I"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2I"/>.</param>
        /// <param name="right">The second source <see cref="Vector2I"/>.</param>
        /// <returns>A <see cref="Vector2I"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2I Min(ref Vector2I left, ref Vector2I right)
		{
			Min(ref left, ref right, out Vector2I result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector2I"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2I"/>.</param>
        /// <param name="right">The second source <see cref="Vector2I"/>.</param>
        /// <returns>A <see cref="Vector2I"/> containing the smallest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2I Min(Vector2I left, Vector2I right)
		{
			return new Vector2I()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
			};
		}

        /// <summary>
        /// Returns a <see cref="Vector2I"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2I"/>.</param>
        /// <param name="right">The second source <see cref="Vector2I"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector2I"/>.</param>
        /// <returns>A <see cref="Vector2I"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Max(ref Vector2I left, ref Vector2I right, out Vector2I result)
		{
				result.X = (left.X > right.X) ? left.X : right.X;
				result.Y = (left.Y > right.Y) ? left.Y : right.Y;
		}

        /// <summary>
        /// Returns a <see cref="Vector2I"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2I"/>.</param>
        /// <param name="right">The second source <see cref="Vector2I"/>.</param>
        /// <returns>A <see cref="Vector2I"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2I Max(ref Vector2I left, ref Vector2I right)
		{
			Max(ref left, ref right, out Vector2I result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector2I"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector2I"/>.</param>
        /// <param name="right">The second source <see cref="Vector2I"/>.</param>
        /// <returns>A <see cref="Vector2I"/> containing the largest components of the source vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2I Max(Vector2I left, Vector2I right)
		{
			return new Vector2I()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
			};
		}

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2I"/> vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The squared distance between the two vectors.</returns>
        /// <remarks>Distance squared is the value before taking the square root. 
        /// Distance squared can often be used in place of distance if relative comparisons are being made. 
        /// For example, consider three points A, B, and C. To determine whether B or C is further from A, 
        /// compare the distance between A and B to the distance between A and C. Calculating the two distances 
        /// involves two square roots, which are computationally expensive. However, using distance squared 
        /// provides the same information and avoids calculating two square roots.
        /// </remarks>
		public static int DistanceSquared(ref Vector2I value1, ref Vector2I value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;

            return ((x * x) + (y * y));
        }

        /// <summary>
        /// Calculates the squared distance between two <see cref="Vector2I"/> vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The squared distance between the two vectors.</returns>
        /// <remarks>Distance squared is the value before taking the square root. 
        /// Distance squared can often be used in place of distance if relative comparisons are being made. 
        /// For example, consider three points A, B, and C. To determine whether B or C is further from A, 
        /// compare the distance between A and B to the distance between A and C. Calculating the two distances 
        /// involves two square roots, which are computationally expensive. However, using distance squared 
        /// provides the same information and avoids calculating two square roots.
        /// </remarks>
		public static int DistanceSquared(Vector2I value1, Vector2I value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;

            return ((x * x) + (y * y));
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector2I"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2I Clamp(Vector2I value, int min, int max)
        {
			return new Vector2I()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
			};
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector2I"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        /// <param name="result">The output for the resultant <see cref="Vector2I"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref Vector2I value, ref Vector2I min, ref Vector2I max, out Vector2I result)
        {
				result.X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X;
				result.Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector2I"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2I Clamp(Vector2I value, Vector2I min, Vector2I max)
        {
			return new Vector2I()
			{
				X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X,
				Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y,
			};
        }

        /// <summary>
        /// Returns the reflection of a vector off a surface that has the specified normal. 
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">Normal of the surface.</param>
        /// <remarks>Reflect only gives the direction of a reflection off a surface, it does not determine 
        /// whether the original vector was close enough to the surface to hit it.</remarks>
        public static Vector2I Reflect(ref Vector2I vector, ref Vector2I normal)
        {
            int dot = (vector.X * normal.X) + (vector.Y * normal.Y);

            return new Vector2I()
            {
				X = (int)(vector.X - ((2 * dot) * normal.X)),
				Y = (int)(vector.Y - ((2 * dot) * normal.Y)),
            };
        }
#endregion

#region Tuples
        public static implicit operator (int x, int y)(Vector2I val)
        {
            return (val.X, val.Y);
        }

        public static implicit operator Vector2I((int x, int y) val)
        {
            return new Vector2I(val.x, val.y);
        }
#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of a component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on. This must be between 0 and 1</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 1].</exception>  
		public int this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2I run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2I run from 0 to 1, inclusive.");
			}
		}
#endregion

#region Casts - vectors
		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="SByte2"/>.</summary>
		public static explicit operator SByte2(Vector2I value)
		{
			return new SByte2((sbyte)value.X, (sbyte)value.Y);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="SByte3"/>.</summary>
		public static explicit operator SByte3(Vector2I value)
		{
			return new SByte3((sbyte)value.X, (sbyte)value.Y, (sbyte)1);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="SByte4"/>.</summary>
		public static explicit operator SByte4(Vector2I value)
		{
			return new SByte4((sbyte)value.X, (sbyte)value.Y, (sbyte)1, (sbyte)1);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Byte2"/>.</summary>
		public static explicit operator Byte2(Vector2I value)
		{
			return new Byte2((byte)value.X, (byte)value.Y);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Byte3"/>.</summary>
		public static explicit operator Byte3(Vector2I value)
		{
			return new Byte3((byte)value.X, (byte)value.Y, (byte)1);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Byte4"/>.</summary>
		public static explicit operator Byte4(Vector2I value)
		{
			return new Byte4((byte)value.X, (byte)value.Y, (byte)1, (byte)1);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector3I"/>.</summary>
		public static explicit operator Vector3I(Vector2I value)
		{
			return new Vector3I(value.X, value.Y, 1);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector4I"/>.</summary>
		public static explicit operator Vector4I(Vector2I value)
		{
			return new Vector4I(value.X, value.Y, 1, 1);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector2UI"/>.</summary>
		public static explicit operator Vector2UI(Vector2I value)
		{
			return new Vector2UI((uint)value.X, (uint)value.Y);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector3UI"/>.</summary>
		public static explicit operator Vector3UI(Vector2I value)
		{
			return new Vector3UI((uint)value.X, (uint)value.Y, 1U);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector4UI"/>.</summary>
		public static explicit operator Vector4UI(Vector2I value)
		{
			return new Vector4UI((uint)value.X, (uint)value.Y, 1U, 1U);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector2S"/>.</summary>
		public static explicit operator Vector2S(Vector2I value)
		{
			return new Vector2S((short)value.X, (short)value.Y);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector3S"/>.</summary>
		public static explicit operator Vector3S(Vector2I value)
		{
			return new Vector3S((short)value.X, (short)value.Y, (short)1);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector4S"/>.</summary>
		public static explicit operator Vector4S(Vector2I value)
		{
			return new Vector4S((short)value.X, (short)value.Y, (short)1, (short)1);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector2US"/>.</summary>
		public static explicit operator Vector2US(Vector2I value)
		{
			return new Vector2US((ushort)value.X, (ushort)value.Y);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector3US"/>.</summary>
		public static explicit operator Vector3US(Vector2I value)
		{
			return new Vector3US((ushort)value.X, (ushort)value.Y, (ushort)1);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector4US"/>.</summary>
		public static explicit operator Vector4US(Vector2I value)
		{
			return new Vector4US((ushort)value.X, (ushort)value.Y, (ushort)1, (ushort)1);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector2L"/>.</summary>
		public static explicit operator Vector2L(Vector2I value)
		{
			return new Vector2L((long)value.X, (long)value.Y);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector3L"/>.</summary>
		public static explicit operator Vector3L(Vector2I value)
		{
			return new Vector3L((long)value.X, (long)value.Y, 1L);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector4L"/>.</summary>
		public static explicit operator Vector4L(Vector2I value)
		{
			return new Vector4L((long)value.X, (long)value.Y, 1L, 1L);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector2UL"/>.</summary>
		public static explicit operator Vector2UL(Vector2I value)
		{
			return new Vector2UL((ulong)value.X, (ulong)value.Y);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector3UL"/>.</summary>
		public static explicit operator Vector3UL(Vector2I value)
		{
			return new Vector3UL((ulong)value.X, (ulong)value.Y, 1UL);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector4UL"/>.</summary>
		public static explicit operator Vector4UL(Vector2I value)
		{
			return new Vector4UL((ulong)value.X, (ulong)value.Y, 1UL, 1UL);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector2F"/>.</summary>
		public static explicit operator Vector2F(Vector2I value)
		{
			return new Vector2F((float)value.X, (float)value.Y);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector3F"/>.</summary>
		public static explicit operator Vector3F(Vector2I value)
		{
			return new Vector3F((float)value.X, (float)value.Y, 1F);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector4F"/>.</summary>
		public static explicit operator Vector4F(Vector2I value)
		{
			return new Vector4F((float)value.X, (float)value.Y, 1F, 1F);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector2D"/>.</summary>
		public static explicit operator Vector2D(Vector2I value)
		{
			return new Vector2D((double)value.X, (double)value.Y);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector3D"/>.</summary>
		public static explicit operator Vector3D(Vector2I value)
		{
			return new Vector3D((double)value.X, (double)value.Y, 1D);
		}

		///<summary>Casts a <see cref="Vector2I"/> to a <see cref="Vector4D"/>.</summary>
		public static explicit operator Vector4D(Vector2I value)
		{
			return new Vector4D((double)value.X, (double)value.Y, 1D, 1D);
		}

#endregion
	}
}

