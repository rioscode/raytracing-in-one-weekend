using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace RaytracerInOneWeekend
{
	struct Camera
	{
		public readonly float3 Origin;
		public readonly float3 LowerLeftCorner;
		public readonly float3 Horizontal, Vertical;

		public readonly float3 Forward, Up, Right;

		public readonly float LensRadius;
		public readonly float2 TimeRange;

		public Camera(float3 origin, float3 lookAt, float3 up, float verticalFov, float aspect, float aperture, float focusDistance, float t0, float t1)
		{
			LensRadius = aperture / 2;

			float theta = verticalFov * PI / 180;
			float halfHeight = tan(theta / 2);
			float halfWidth = aspect * halfHeight;

			Forward = normalize(origin - lookAt);
			Right = normalize(cross(Forward, up));
			Up = cross(Right, Forward);

			LowerLeftCorner = halfWidth * focusDistance * -Right +
							  halfHeight * focusDistance * -Up +
							  focusDistance * -Forward;

			Horizontal = 2 * halfWidth * focusDistance * Right;
			Vertical = 2 * halfHeight * focusDistance * Up;

			Origin = origin;

			TimeRange = float2(t0, t1);
		}

		public Ray GetRay(float2 normalizedCoordinates, Random rng)
		{
			float2 rd = LensRadius * rng.InUnitDisk();
			float3 offset = Right * rd.x + Up * rd.y;

			return new Ray(Origin + offset,
				LowerLeftCorner - offset +
				normalizedCoordinates.x * Horizontal +
				normalizedCoordinates.y * Vertical,
				rng.NextFloat(TimeRange.x, TimeRange.y));
		}
	}
}