namespace Molten.DoublePrecision
{
    ///<summary>A <see cref = "double"/> vector comprised of 3 components.</summary>
    public partial struct Vector3D
	{
    	/// <summary>
        /// Gets a value indicting whether this instance is normalized.
        /// </summary>
        public bool IsNormalized
        {
            get => MathHelper.IsOne((X * X) + (Y * Y) + (Z * Z));
        }

        /// <summary>
        /// Orthonormalizes a list of vectors.
        /// </summary>
        /// <param name="destination">The list of orthonormalized vectors.</param>
        /// <param name="source">The list of vectors to orthonormalize.</param>
        /// <remarks>
        /// <para>Orthonormalization is the process of making all vectors orthogonal to each
        /// other and making all vectors of unit length. This means that any given vector will
        /// be orthogonal to any other given vector in the list.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting vectors
        /// tend to be numerically unstable. The numeric stability decreases according to the vectors
        /// position in the list so that the first vector is the most stable and the last vector is the
        /// least stable.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Orthonormalize(Vector3D[] destination, params Vector3D[] source)
        {
            //Uses the modified Gram-Schmidt process.
            //Because we are making unit vectors, we can optimize the math for orthogonalization
            //and simplify the projection operation to remove the division.
            //q1 = m1 / |m1|
            //q2 = (m2 - (q1 ⋅ m2) * q1) / |m2 - (q1 ⋅ m2) * q1|
            //q3 = (m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2) / |m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2|
            //q4 = (m4 - (q1 ⋅ m4) * q1 - (q2 ⋅ m4) * q2 - (q3 ⋅ m4) * q3) / |m4 - (q1 ⋅ m4) * q1 - (q2 ⋅ m4) * q2 - (q3 ⋅ m4) * q3|
            //q5 = ...

            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            for (int i = 0; i < source.Length; ++i)
            {
                Vector3D newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= Dot(destination[r], newvector) * destination[r];

                newvector.Normalize();
                destination[i] = newvector;
            }
        }

        /// <summary>
        /// Tests whether one 3D vector is near another 3D vector.
        /// </summary>
        /// <param name="left">The left vector.</param>
        /// <param name="right">The right vector.</param>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns><c>true</c> if left and right are near another 3D, <c>false</c> otherwise</returns>
        public static bool NearEqual(Vector3D left, Vector3D right, Vector3D epsilon)
        {
            return NearEqual(ref left, ref right, ref epsilon);
        }

        /// <summary>
        /// Tests whether one 3D vector is near another 3D vector.
        /// </summary>
        /// <param name="left">The left vector.</param>
        /// <param name="right">The right vector.</param>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns><c>true</c> if left and right are near another 3D, <c>false</c> otherwise</returns>
        public static bool NearEqual(ref Vector3D left, ref Vector3D right, ref Vector3D epsilon)
        {
            return MathHelper.WithinEpsilon(left.X, right.X, epsilon.X) && MathHelper.WithinEpsilon(left.Y, right.Y, epsilon.Y) && MathHelper.WithinEpsilon(left.Z, right.Z, epsilon.Z);
        }

        /// <summary>
        /// Converts the <see cref="Vector3D"/> into a unit vector.
        /// </summary>
        /// <param name="value">The <see cref="Vector3D"/> to normalize.</param>
        /// <param name="allowZero">If true, zero values are allowed.</param>
        /// <returns>The normalized <see cref="Vector3D"/>.</returns>
        public static Vector3D Normalize(Vector3D value, bool allowZero = false)
        {
            value.Normalize(allowZero);
            return value;
        }

        /// <summary>
        /// Returns a normalized unit vector of the original vector.
        /// </summary>
        /// <param name="allowZero">If true, zero values are allowed.</param>
        public Vector3D GetNormalized(bool allowZero = false)
        {
            double length = Length();
            if (!MathHelper.IsZero(length))
            {
                double inverse = 1D / length;
                return new Vector3D()
                {
					X = this.X * inverse,
					Y = this.Y * inverse,
					Z = this.Z * inverse
                };
            }
            else
            {
                return new Vector3D()
                {
					X = 0,
					Y = allowZero ? 1 : 0,
					Z = 0
                };
            }
        }

        /// <summary>
        /// Converts the vector into a unit vector.
        /// </summary>
        /// <param name="allowZero">If true, zero values are allowed.</param>
        public void Normalize(bool allowZero = false)
        {
            double length = Length();
            if (!MathHelper.IsZero(length))
            {
                double inverse = 1D / length;
				X = (X * inverse);
				Y = (Y * inverse);
				Z = (Z * inverse);
            }
            else
            {
				X = 0;
				Y = allowZero ? 1 : 0;
				Z = 0;
            }
        }

		/// <summary>
        /// Saturates this instance in the range [0,1]
        /// </summary>
        public void Saturate()
        {
			X = X < 0D ? 0D : X > 1D ? 1D : X;
			Y = Y < 0D ? 0D : Y > 1D ? 1D : Y;
			Z = Z < 0D ? 0D : Z > 1D ? 1D : Z;
        }

        /// <summary>Checks to see if any value (x, y, z, w) are within 0.0001 of 0.
        /// If so this method truncates that value to zero.</summary>
        /// <param name="power">The power.</param>
        /// <param name="vec">The vector.</param>
        public static Vector3D Pow(Vector3D vec, double power)
        {
            return new Vector3D()
            {
				X = Math.Pow(vec.X, power),
				Y = Math.Pow(vec.Y, power),
				Z = Math.Pow(vec.Z, power)
            };
        }

		/// <summary>Rounds all components down to the nearest unit.</summary>
        public void Floor()
        {
			X = Math.Floor(X);
			Y = Math.Floor(Y);
			Z = Math.Floor(Z);
        }

        /// <summary>Rounds all components up to the nearest unit.</summary>
        public void Ceiling()
        {
			X = Math.Ceiling(X);
			Y = Math.Ceiling(Y);
			Z = Math.Ceiling(Z);
        }

        /// <summary>Removes the sign from each component of the current <see cref="Vector3D"/>.</summary>
        public void Abs()
        {
			X = Math.Abs(X);
			Y = Math.Abs(Y);
			Z = Math.Abs(Z);
        }


		/// <summary>Truncate each near-zero component of the current vector towards zero.</summary>
        public void Truncate()
        {
			X = (Math.Abs(X) - 0.0001D < 0) ? 0 : X;
			Y = (Math.Abs(Y) - 0.0001D < 0) ? 0 : Y;
			Z = (Math.Abs(Z) - 0.0001D < 0) ? 0 : Z;
        }

		/// <summary>Updates the component values to the power of the specified value.</summary>
        /// <param name="power"></param>
        public void Pow(double power)
        {
			X = Math.Pow(X, power);
			Y = Math.Pow(Y, power);
			Z = Math.Pow(Z, power);
        }

        /// <summary>
        /// Calculates the length of the vector.
        /// </summary>
        /// <returns>The length of the vector.</returns>
        /// <remarks>
        /// <see cref="LengthSquared"/> may be preferred when only the relative length is needed
        /// and speed is of the essence.
        /// </remarks>
        public double Length()
        {
            return Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
        }

        /// <summary>
        /// Calculates the distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The distance between the two vectors.</returns>
        /// <remarks>
        /// <see cref="Vector3D.DistanceSquared(Vector3D, Vector3D)"/> may be preferred when only the relative distance is needed
        /// and speed is of the essence.
        /// </remarks>
        public static double Distance(ref Vector3D value1, ref Vector3D value2)
        {
			double x = value1.X - value2.X;
			double y = value1.Y - value2.Y;
			double z = value1.Z - value2.Z;
           return Math.Sqrt((x * x) + (y * y) + (z * z));
        }

                /// <summary>
        /// Calculates the distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The distance between the two vectors.</returns>
        /// <remarks>
        /// <see cref="Vector3D.DistanceSquared(Vector3D, Vector3D)"/> may be preferred when only the relative distance is needed
        /// and speed is of the essence.
        /// </remarks>
        public static double Distance(Vector3D value1, Vector3D value2)
        {
			double x = value1.X - value2.X;
			double y = value1.Y - value2.Y;
			double z = value1.Z - value2.Z;
            return Math.Sqrt((x * x) + (y * y) + (z * z));
        }

        /// <summary>
        /// Performs a Catmull-Rom interpolation using the specified positions.
        /// </summary>
        /// <param name="value1">The first position in the interpolation.</param>
        /// <param name="value2">The second position in the interpolation.</param>
        /// <param name="value3">The third position in the interpolation.</param>
        /// <param name="value4">The fourth position in the interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        public static Vector3D CatmullRom(ref Vector3D value1, ref Vector3D value2, ref Vector3D value3, ref Vector3D value4, double amount)
        {
            double squared = amount * amount;
            double cubed = amount * squared;

            return new Vector3D()
            {
				X = (0.5D * ((((2D * value2.X) + 
                ((-value1.X + value3.X) * amount)) + 
                (((((2D * value1.X) - (5D * value2.X)) + (4D * value3.X)) - value4.X) * squared)) +
                ((((-value1.X + (3D * value2.X)) - (3D * value3.X)) + value4.X) * cubed))),

				Y = (0.5D * ((((2D * value2.Y) + 
                ((-value1.Y + value3.Y) * amount)) + 
                (((((2D * value1.Y) - (5D * value2.Y)) + (4D * value3.Y)) - value4.Y) * squared)) +
                ((((-value1.Y + (3D * value2.Y)) - (3D * value3.Y)) + value4.Y) * cubed))),

				Z = (0.5D * ((((2D * value2.Z) + 
                ((-value1.Z + value3.Z) * amount)) + 
                (((((2D * value1.Z) - (5D * value2.Z)) + (4D * value3.Z)) - value4.Z) * squared)) +
                ((((-value1.Z + (3D * value2.Z)) - (3D * value3.Z)) + value4.Z) * cubed))),

            };
        }

        /// <summary>
        /// Performs a Catmull-Rom interpolation using the specified positions.
        /// </summary>
        /// <param name="value1">The first position in the interpolation.</param>
        /// <param name="value2">The second position in the interpolation.</param>
        /// <param name="value3">The third position in the interpolation.</param>
        /// <param name="value4">The fourth position in the interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>A vector that is the result of the Catmull-Rom interpolation.</returns>
        public static Vector3D CatmullRom(Vector3D value1, Vector3D value2, Vector3D value3, Vector3D value4, double amount)
        {
            return CatmullRom(ref value1, ref value2, ref value3, ref value4, amount);
        }

        		/// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position <see cref="Vector3D"/> vector.</param>
        /// <param name="tangent1">First source tangent <see cref="Vector3D"/> vector.</param>
        /// <param name="value2">Second source position <see cref="Vector3D"/> vector.</param>
        /// <param name="tangent2">Second source tangent <see cref="Vector3D"/> vector.</param>
        /// <param name="amount">Weighting factor.</param>
        public static Vector3D Hermite(ref Vector3D value1, ref Vector3D tangent1, ref Vector3D value2, ref Vector3D tangent2, double amount)
        {
            double squared = amount * amount;
            double cubed = amount * squared;
            double part1 = ((2.0D * cubed) - (3.0D * squared)) + 1D;
            double part2 = (-2.0D * cubed) + (3.0D * squared);
            double part3 = (cubed - (2.0D * squared)) + amount;
            double part4 = cubed - squared;

			return new Vector3D()
			{
				X = (((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4),
				Y = (((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4),
				Z = (((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4),
			};
        }

        /// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position <see cref="Vector3D"/>.</param>
        /// <param name="tangent1">First source tangent <see cref="Vector3D"/>.</param>
        /// <param name="value2">Second source position <see cref="Vector3D"/>.</param>
        /// <param name="tangent2">Second source tangent <see cref="Vector3D"/>.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>The result of the Hermite spline interpolation.</returns>
        public static Vector3D Hermite(Vector3D value1, Vector3D tangent1, Vector3D value2, Vector3D tangent2, double amount)
        {
            return Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount);
        }

#region Static Methods
		/// <summary>Truncate each near-zero component of a vector towards zero.</summary>
        /// <param name="value">The Vector3D to be truncated.</param>
        /// <returns></returns>
        public static Vector3D Truncate(Vector3D value)
        {
            return new Vector3D()
            {
				X = (Math.Abs(value.X) - 0.0001D < 0) ? 0 : value.X,
				Y = (Math.Abs(value.Y) - 0.0001D < 0) ? 0 : value.X,
				Z = (Math.Abs(value.Z) - 0.0001D < 0) ? 0 : value.X
            };
        }
#endregion
	}
}

