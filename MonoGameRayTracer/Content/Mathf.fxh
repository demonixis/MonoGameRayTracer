float LengthSquared(float3 vec)
{
	return sqrt(float3(vec.x * vec.x, vec.y * vec.y, vec.z * vec.z));
}

float3 RandomInUnitySphere(float2 uv)
{
	float3 vec;
	float3 noise = tex2D(noiseSampler, uv);
	float3 one = float3(1.0f, 1.0f, 1.0f);

	do
	{
		vec = 2.0f * noise - one;
	} while (LengthSquared(vec) > 1.0f);

	return vec;
}

float3 UnitVector(float3 vec)
{
	return vec / length(vec);
}

float3 MakeUnitVector(float vec)
{
	float3 result = vec;
	float k = 1.0f / sqrt(result.x * result.x + result.y * result.y + result.z * result.z);
	result *= k;
	return result;
}