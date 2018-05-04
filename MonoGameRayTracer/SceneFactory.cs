using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameRayTracer.Materials;
using MonoGameRayTracer.Primitives;
using MonoGameRayTracer.Textures;
using System.Collections.Generic;

namespace MonoGameRayTracer
{
    public static class SceneFactory
    {
        public static List<Hitable> MakeSphereScene(ContentManager content, int sceneComplexity)
        {
            var list = new List<Hitable>();
            list.Add(new Sphere(new Vector3(0, -1000, 0), 1000, new LambertMaterial(new CheckerTexture())));

            var temp = new Vector3(4, 0.2f, 0);

            for (var a = -sceneComplexity; a < sceneComplexity; a++)
            {
                for (var b = -sceneComplexity; b < sceneComplexity; b++)
                {
                    var chooseMat = Random.Value;
                    var center = new Vector3(a + 0.9f + Random.Value, 0.2f, b + 0.9f + Random.Value);

                    if ((center - temp).Length() > 0.9f)
                    {
                        if (chooseMat < 0.8f)
                            list.Add(new Sphere(center, 0.2f, new LambertMaterial(Random.Vector3Twice)));
                        else if (chooseMat < 0.95f)
                            list.Add(new Sphere(center, 0.2f, new MetalMaterial(0.5f * (1 + Random.Value), 0.5f * (1 + Random.Value), 0.5f * (1 + Random.Value), 0.5f * Random.Value)));
                        else
                            list.Add(new Sphere(center, 0.2f, new DieletricMaterial(1.5f)));
                    }
                }
            }

            var earthTexture = content.Load<Texture2D>("earth");

            list.Add(new Sphere(new Vector3(0, 1, 0), 1, new LambertMaterial(new ImageTexture(earthTexture))));
            list.Add(new Sphere(new Vector3(-4, 1, 0), 1, new LambertMaterial(new NoiseTexture())));
            list.Add(new Sphere(new Vector3(4, 1, 0), 1, new MetalMaterial(0.7f, 0.6f, 0.5f, 0.0f)));

            return list;
        }
    }
}
