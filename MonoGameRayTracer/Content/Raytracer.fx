float3 LowerLeftCorner;
float3 Horizontal;
float3 Vertical;
float3 CameraPosition;
int Step;
int RenderWidth;
int RenderHeight;

texture SceneTexture;
sampler sceneSampler = sampler_state
{
	Texture = <sceneTexture>;
	MinFilter = Anisotropic;
	MagFilter = Linear;
	MipFilter = Linear;
};

texture NoiseTexture;
sampler noiseSampler = sampler_state
{
	Texture = <sceneTexture>;
	MinFilter = Anisotropic;
	MagFilter = Linear;
	MipFilter = Linear;
};

struct PixelShaderInput
{
	float4 Position : SV_Position;
	float4 Color : COLOR0;
	float2 UV : TEXCOORD0;
};

struct Ray
{
	float3 Origin;
	float3 Direction;
};

struct Material
{
	float3 Value;
};

struct HitRecord
{
	float T;
	float3 P;
	float3 Normal;
	Material Material;
	bool Result;
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

float3 PointAtParameter(Ray ray, float t)
{
	return ray.Origin + t * ray.Direction;
}

HitRecord SphereHit(Ray ray, float min, float max, float3 center, float radius, Material material)
{
	HitRecord record;

	float3 oc = ray.Origin - center;
	float a = dot(ray.Direction, ray.Direction);
	float b = dot(oc, ray.Direction);
	float c = dot(oc, oc) - radius * radius;
	float disciminent = b * b - a * c;

	if (disciminent > 0)
	{
		float temp = (-b - sqrt(b * b - a * c)) / a;

		if (temp < max && temp > min)
		{
			record.T = temp;
			record.P = PointAtParameter(ray, record.T);
			record.Normal = (record.P - center) / radius;
			record.Material = material;
			record.Result = true;
		}

		temp = (-b + sqrt(b * b - a * c)) / a;

		if (temp < max && temp > min)
		{
			record.T = temp;
			record.P = PointAtParameter(ray, record.T);
			record.Normal = (record.P - center) / radius;
			record.Material = material;
			record.Result = true;
		}
	}

	return record;
}

Ray GetRay(float u, float v)
{
	Ray ray;
	ray.Origin = CameraPosition;
	ray.Direction = LowerLeftCorner + u * Horizontal + v * Vertical - CameraPosition;
	return ray;
}

ScatterResult Scatter(Ray ray, HitRecord record, float2 uv, float3 albedo)
{
	ScatterResult result;
	result.Result = true;

	float3 target = record.P + record.Normal + RandomInUnitySphere(uv);

	Ray tmp;
	tmp.Origin = record.P;
	tmp.Direction = target - record.P;

	result.Scattered = tmp;
	result.Attenuation = albedo;
	return result;
}

HitRecord WorldHit(Ray ray, float min, float max)
{
	HitRecord record;
	bool hitAnything = false;
	float closestSoFar = max;
	uint size = RenderWidth * RenderHeight;

	[loop]
	for (uint i = 0; i < size; i++)
	{
		float2 uv = float2(i % RenderWidth, i / RenderWidth);
		float4 pixel = tex2D(sceneSampler, uv);

		if (pixel.x + pixel.y + pixel.z == 0)
			continue;

		// Pixel[i].XYZ => Position
		// Pixel[i].A => Type
		if (pixel.a == 1) // Sphere
		{
			float3 center = float3(pixel.x, pixel.y, pixel.z);

			i++; // Increase the cursor
			uv = float2(i % RenderWidth, i / RenderWidth);
			pixel = tex2D(sceneSampler, uv);

			// Pixel[i].x => Radius
			float radius = pixel.x;

			i++; // Increase the cursor
			uv = float2(i % RenderWidth, i / RenderWidth);
			pixel = tex2D(sceneSampler, uv);

			// Pixel[i].XYZ => Albedo/Value
			// Pixel[i].A => Type
			Material material;
			material.Value = pixel.xyz;

			HitRecord hit = SphereHit(ray, min, closestSoFar, center, radius, material);

			if (hit.Result == true)
			{
				hitAnything = true;
				closestSoFar = hit.T;
				record = hit;

				return hit;
			}
		}
	}

	return record;
}

float3 GetColor(Ray ray, float2 uv)
{
	uint depth = 0;
	uint maxDepth = 50;
	float3 color = float3(0.0f, 0.0f, 0.0f);

	HitRecord record = WorldHit(ray, 0.001f, 10000000.0f);

	bool wasInLoop = record.Result == true;

	[loop]
	while (depth < maxDepth && record.Result == true)
	{
		if (record.Result == true)
		{
			ScatterResult scatterResult = Scatter(ray, record, uv, record.Material.Value);

			if (scatterResult.Result == true)
			{
				color *= scatterResult.Attenuation;
				record = WorldHit(scatterResult.Scattered, 0.001f, 10000000.0f);
			}
		}

		depth++;
	}

	if (wasInLoop)
		return color;

	float3 unitDirection = UnitVector(ray.Direction);
	float t = 0.5f * (unitDirection.y + 1.0f);
	return (1.0f - t) * float3(1.0f, 1.0f, 1.0f) + t * float3(0.5f, 0.7f, 1.0f);
}

float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
	float3 color = float3(0.0f, 0.0f, 0.0f);
	float2 noise = tex2D(noiseSampler, input.UV);

	for (int s = 0; s < Step; s++)
	{
		float u = (float)(input.UV.x + noise.x) / RenderWidth;
		float v = (float)(input.UV.y + noise.y) / RenderHeight;
		Ray ray = GetRay(u, v);
		color += GetColor(ray, input.UV);
	}

	color /= (float)Step;
	color = sqrt(color);

	return float4(color.x, color.y, color.z, 1.0f);
}

technique Technique0
{
	pass Raytracer
	{
#if SM4
		PixelShader = compile ps_4_0 PixelShaderFunction();
#else
		PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
	}
}