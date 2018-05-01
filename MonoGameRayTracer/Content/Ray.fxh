struct Ray
{
	float3 Origin;
	float3 Direction;
};

float3 PointAtParameter(Ray ray, float t)
{
	return ray.Origin + t * ray.Direction;
}

Ray GetRay(float u, float v)
{
	Ray ray;
	ray.Origin = CameraPosition;
	ray.Direction = LowerLeftCorner + u * Horizontal + v * Vertical - CameraPosition;
	return ray;
}