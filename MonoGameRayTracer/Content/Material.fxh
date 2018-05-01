struct Material
{
	float3 Value;
};

struct ScatterResult
{
	HitRecord Record;
	Ray Scattered;
	float3 Attenuation;
	bool Result;
};

struct RefractResult
{
	bool Result;
	float3 Refracted;
};

RefractResult Refract(float3 v, float3 n, float niOverNt, float3 refracted)
{
	RefractResult result;

	float3 uv = UnitVector(v);
	float dt = dot(uv, n);
	float discriminant = 1.0f - niOverNt * niOverNt * (1.0f - dt * dt);

	if (discriminant > 0)
	{
		result.Refracted = niOverNt * (uv - n * dt) - n * sqrt(discriminant);
		result.Result = true;
		return result;
	}

	result.Refracted = refracted;
	return result;
}

float Schlick(float cosine, float refIdx)
{
	float r0 = (1.0f - refIdx) / (1 + refIdx);
	r0 = r0 * r0;
	return r0 + (1.0f - r0) * pow((1.0f - cosine), 5.0f);
}