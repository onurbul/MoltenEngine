namespace Molten
{
    ///<summary>A <see cref = "float"/> vector comprised of 3 components.</summary>
    public partial struct Vector3F
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
        public static void Orthonormalize(Vector3F[] destination, params Vector3F[] source)
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
                Vector3F newvector = source[i];

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
        public static bool NearEqual(Vector3F left, Vector3F right, Vector3F epsilon)
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
        public static bool NearEqual(ref Vector3F left, ref Vector3F right, ref Vector3F epsilon)
        {
            return MathHelper.WithinEpsilon(left.X, right.X, epsilon.X) && MathHelper.WithinEpsilon(left.Y, right.Y, epsilon.Y) && MathHelper.WithinEpsilon(left.Z, right.Z, epsilon.Z);
        }

        /// <summary>
        /// Converts the <see cref="Vector3F"/> into a unit vector.
        /// </summary>
        /// <param name="value">The <see cref="Vector3F"/> to normalize.</param>
        /// <param name="allowZero">If true, zero values are allowed.</param>
        /// <returns>The normalized <see cref="Vector3F"/>.</returns>
        public static Vector3F Normalize(Vector3F value, bool allowZero = false)
        {
            value.Normalize(allowZero);
            return value;
        }

        /// <summary>
        /// Returns a normalized unit vector of the original vector.
        /// </summary>
        /// <param name="allowZero">If true, zero values are allowed.</param>
        public Vector3F GetNormalized(bool allowZero = false)
        {
            float length = Length();
            if (!MathHelper.IsZero(length))
            {
                float inverse = 1F / length;
                return new Vector3F()
                {
					X = this.X * inverse,
					Y = this.Y * inverse,
					Z = this.Z * inverse
                };
            }
            else
            {
                return new Vector3F()
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
            float length = Length();
            if (!MathHelper.IsZero(length))
            {
                float inverse = 1F / length;
				X = (float)(X * inverse);
				Y = (float)(Y * inverse);
				Z = (float)(Z * inverse);
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
			X = X < 0F ? 0F : X > 1F ? 1F : X;
			Y = Y < 0F ? 0F : Y > 1F ? 1F : Y;
			Z = Z < 0F ? 0F : Z > 1F ? 1F : Z;
        }

        /// <summary>Checks to see if any value (x, y, z, w) are within 0.0001 of 0.
        /// If so this method truncates that value to zero.</summary>
        /// <param name="power">The power.</param>
        /// <param name="vec">The vector.</param>
        public static Vector3F Pow(Vector3F vec, float power)
        {
            return new Vector3F()
            {
				X = MathF.Pow(vec.X, power),
				Y = MathF.Pow(vec.Y, power),
				Z = MathF.Pow(vec.Z, power)
            };
        }

		/// <summary>Rounds all components down to the nearest unit.</summary>
        public void Floor()
        {
			X = MathF.Floor(X);
			Y = MathF.Floor(Y);
			Z = MathF.Floor(Z);
        }

        /// <summary>Rounds all components up to the nearest unit.</summary>
        public void Ceiling()
        {
			X = MathF.Ceiling(X);
			Y = MathF.Ceiling(Y);
			Z = MathF.Ceiling(Z);
        }

        /// <summary>Removes the sign from each component of the current <see cref="Vector3F"/>.</summary>
        public void Abs()
        {
			X = MathF.Abs(X);
			Y = MathF.Abs(Y);
			Z = MathF.Abs(Z);
        }


		/// <summary>Truncate each near-zero component of the current vector towards zero.</summary>
        public void Truncate()
        {
			X = (MathF.Abs(X) - 0.0001F < 0) ? 0 : X;
			Y = (MathF.Abs(Y) - 0.0001F < 0) ? 0 : Y;
			Z = (MathF.Abs(Z) - 0.0001F < 0) ? 0 : Z;
        }

		/// <summary>Updates the component values to the power of the specified value.</summary>
        /// <param name="power"></param>
        public void Pow(float power)
        {
			X = MathF.Pow(X, power);
			Y = MathF.Pow(Y, power);
			Z = MathF.Pow(Z, power);
        }

        /// <summary>
        /// Calculates the length of the vector.
        /// </summary>
        /// <returns>The length of the vector.</returns>
        /// <remarks>
        /// <see cref="LengthSquared"/> may be preferred when only the relative length is needed
        /// and speed is of the essence.
        /// </remarks>
        public float Length()
        {
            return MathF.Sqrt((X * X) + (Y * Y) + (Z * Z));
        }

        /// <summary>
        /// Calculates the distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The distance between the two vectors.</returns>
        /// <remarks>
        /// <see cref="Vector3F.DistanceSquared(Vector3F, Vector3F)"/> may be preferred when only the relative distance is needed
        /// and speed is of the essence.
        /// </remarks>
        public static float Distance(ref Vector3F value1, ref Vector3F value2)
        {
			float x = value1.X - value2.X;
			float y = value1.Y - value2.Y;
			float z = value1.Z - value2.Z;
           return MathF.Sqrt((x * x) + (y * y) + (z * z));
        }

                /// <summary>
        /// Calculates the distance between two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The distance between the two vectors.</returns>
        /// <remarks>
        /// <see cref="Vector3F.DistanceSquared(Vector3F, Vector3F)"/> may be preferred when only the relative distance is needed
        /// and speed is of the essence.
        /// </remarks>
        public static float Distance(Vector3F value1, Vector3F value2)
        {
			float x = value1.X - value2.X;
			float y = value1.Y - value2.Y;
			float z = value1.Z - value2.Z;
            return MathF.Sqrt((x * x) + (y * y) + (z * z));
        }

        /// <summary>
        /// Performs a Catmull-Rom interpolation using the specified positions.
        /// </summary>
        /// <param name="value1">The first position in the interpolation.</param>
        /// <param name="value2">The second position in the interpolation.</param>
        /// <param name="value3">The third position in the interpolation.</param>
        /// <param name="value4">The fourth position in the interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        public static Vector3F CatmullRom(ref Vector3F value1, ref Vector3F value2, ref Vector3F value3, ref Vector3F value4, float amount)
        {
            float squared = amount * amount;
            float cubed = amount * squared;

            return new Vector3F()
            {
				X = (float)(0.5F * ((((2F * value2.X) + 
                ((-value1.X + value3.X) * amount)) + 
                (((((2F * value1.X) - (5F * value2.X)) + (4F * value3.X)) - value4.X) * squared)) +
                ((((-value1.X + (3F * value2.X)) - (3F * value3.X)) + value4.X) * cubed))),

				Y = (float)(0.5F * ((((2F * value2.Y) + 
                ((-value1.Y + value3.Y) * amount)) + 
                (((((2F * value1.Y) - (5F * value2.Y)) + (4F * value3.Y)) - value4.Y) * squared)) +
                ((((-value1.Y + (3F * value2.Y)) - (3F * value3.Y)) + value4.Y) * cubed))),

				Z = (float)(0.5F * ((((2F * value2.Z) + 
                ((-value1.Z + value3.Z) * amount)) + 
                (((((2F * value1.Z) - (5F * value2.Z)) + (4F * value3.Z)) - value4.Z) * squared)) +
                ((((-value1.Z + (3F * value2.Z)) - (3F * value3.Z)) + value4.Z) * cubed))),

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
        public static Vector3F CatmullRom(Vector3F value1, Vector3F value2, Vector3F value3, Vector3F value4, float amount)
        {
            return CatmullRom(ref value1, ref value2, ref value3, ref value4, amount);
        }

        		/// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position <see cref="Vector3F"/> vector.</param>
        /// <param name="tangent1">First source tangent <see cref="Vector3F"/> vector.</param>
        /// <param name="value2">Second source position <see cref="Vector3F"/> vector.</param>
        /// <param name="tangent2">Second source tangent <see cref="Vector3F"/> vector.</param>
        /// <param name="amount">Weighting factor.</param>
        public static Vector3F Hermite(ref Vector3F value1, ref Vector3F tangent1, ref Vector3F value2, ref Vector3F tangent2, float amount)
        {
            float squared = amount * amount;
            float cubed = amount * squared;
            float part1 = ((2.0F * cubed) - (3.0F * squared)) + 1F;
            float part2 = (-2.0F * cubed) + (3.0F * squared);
            float part3 = (cubed - (2.0F * squared)) + amount;
            float part4 = cubed - squared;

			return new Vector3F()
			{
				X = (((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4),
				Y = (((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4),
				Z = (((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4),
			};
        }

        /// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position <see cref="Vector3F"/>.</param>
        /// <param name="tangent1">First source tangent <see cref="Vector3F"/>.</param>
        /// <param name="value2">Second source position <see cref="Vector3F"/>.</param>
        /// <param name="tangent2">Second source tangent <see cref="Vector3F"/>.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>The result of the Hermite spline interpolation.</returns>
        public static Vector3F Hermite(Vector3F value1, Vector3F tangent1, Vector3F value2, Vector3F tangent2, float amount)
        {
            return Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount);
        }

#region Static Methods
		/// <summary>Truncate each near-zero component of a vector towards zero.</summary>
        /// <param name="value">The Vector3F to be truncated.</param>
        /// <returns></returns>
        public static Vector3F Truncate(Vector3F value)
        {
            return new Vector3F()
            {
				X = (MathF.Abs(value.X) - 0.0001F < 0) ? 0 : value.X,
				Y = (MathF.Abs(value.Y) - 0.0001F < 0) ? 0 : value.X,
				Z = (MathF.Abs(value.Z) - 0.0001F < 0) ? 0 : value.X
            };
        }
#endregion
	}
}

