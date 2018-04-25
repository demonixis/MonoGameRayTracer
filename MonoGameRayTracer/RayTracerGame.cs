using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MonoGameRayTracer
{
    public class RayTracerGame : Game
    {
        private static Random m_Random = new Random(DateTime.Now.Millisecond);
        private GraphicsDeviceManager m_GraphicsDeviceManager;
        private SpriteBatch m_SpriteBatch;
        private Texture2D m_FrontBuffer;
        private int m_Width;
        private int m_Height;
        private Color[] m_BackBuffer;
        private Rectangle m_FrontbufferRect;
        private Camera m_Camera;
        private HitableList m_World;
        private int m_NS = 5;
        

        public RayTracerGame()
        {
            m_GraphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        Vector3 GetColor(Ray ray, Hitable world, int depth) => GetColor(ref ray, world, depth);

        Vector3 GetColor(ref Ray ray, Hitable world, int depth)
        {
            HitRecord record = new HitRecord();
            if (world.Hit(ref ray, 0.001f, float.MaxValue, ref record))
            {
                var scattered = new Ray();
                var attenuation = Vector3.Zero;

                if (record.Material != null)
                {
                    if (depth < 50 && record.Material.Scatter(ref ray, ref record, ref attenuation, ref scattered))
                        return attenuation * GetColor(ref scattered, world, depth + 1);
                    else
                        return Vector3.Zero;
                }

                var target = record.P + record.Normal + RandomInUnitySphere();
                return 0.5f * GetColor(new Ray(record.P, target - record.P), world, depth);
            }

            var unitDirection = UnitVector(ray.Direction);
            var t = 0.5f * (unitDirection.Y + 1.0f);
            return (1.0f - t) * Vector3.One + t * new Vector3(0.5f, 0.7f, 1.0f);
        }

        private void SetRenderSize(int screenWidth, int screenHeight, float multiplier)
        {
            m_GraphicsDeviceManager.PreferredBackBufferWidth = screenWidth;
            m_GraphicsDeviceManager.PreferredBackBufferHeight = screenHeight;
            m_GraphicsDeviceManager.ApplyChanges();

            m_Width = (int)(screenWidth * multiplier);
            m_Height = (int)(screenHeight * multiplier);

            m_SpriteBatch = new SpriteBatch(GraphicsDevice);
            m_FrontBuffer = new Texture2D(GraphicsDevice, m_Width, m_Height, false, SurfaceFormat.Color);
            m_BackBuffer = new Color[m_Width * m_Height];

            m_FrontbufferRect = new Rectangle(0, 0, screenWidth, screenHeight);
        }

        protected override void LoadContent()
        {
            SetRenderSize(800, 480, 0.25f);

            var spheres = new Hitable[]
            {
                new Sphere(new Vector3(0.0f, 0.0f, -1.0f), 0.5f, new LambertMaterial(0.8f, 0.3f, 0.3f)),
                new Sphere(new Vector3(1.5f, 0.0f, -1.5f), 0.5f, new LambertMaterial(0.8f, 0.8f, 0.0f)),
                new Sphere(new Vector3(-2.0f, 0.0f, -2.0f), 0.5f, new MetalMaterial(0.8f, 0.6f, 0.2f, 0.3f)),
                new Sphere(new Vector3(0.0f, -100.5f, -1.0f), 100, new MetalMaterial(0.8f, 0.8f, 0.8f, 1.0f))
            };


            m_Camera = new Camera();
            m_NS = 2;
            m_World = new HitableList(spheres);

            var thread = new Thread(() =>
            {
                while (true)
                {
                    UpdateRender();
                    Thread.Sleep(100);
                }
            });

            thread.Start();
        }

        private void UpdateRender()
        {
            for (var j = m_Height - 1; j >= 0; j--)
            {
                for (var i = 0; i < m_Width; i++)
                {
                    var color = Vector3.Zero;

                    for (var s = 0; s < m_NS; s++)
                    {
                        var u = (float)(i + (float)m_Random.NextDouble()) / m_Width;
                        var v = (float)(j + (float)m_Random.NextDouble()) / m_Height;
                        var ray = m_Camera.GetRay(u, v);
                        var p = ray.PointAtParameter(2.0f);
                        color += GetColor(ref ray, m_World, 0);
                    }

                    color /= (float)m_NS;
                    color.X = (float)Math.Sqrt(color.X);
                    color.Y = (float)Math.Sqrt(color.Y);
                    color.Z = (float)Math.Sqrt(color.Z);

                    m_BackBuffer[i + j * m_Width] = new Color(color.X, color.Y, color.Z);
                }
            }

            m_FrontBuffer.SetData<Color>(m_BackBuffer);
        }

        protected override void Update(GameTime gameTime)
        {
            var state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Escape))
                Exit();

            if (state.IsKeyDown(Keys.Up))
            {
                m_Camera.Move(0, 0, 1);
            }
            else if (state.IsKeyDown(Keys.Down))
            {
                m_Camera.Move(0, 0, -1);
            }

            if (state.IsKeyDown(Keys.Left))
            {
                m_Camera.Move(-1, 0, 0);
            }
            else if (state.IsKeyDown(Keys.Right))
            {
                m_Camera.Move(1, 0, -1);
            }

            if (state.IsKeyDown(Keys.PageUp))
            {
                m_NS++;
            }
            else if (state.IsKeyDown(Keys.PageDown))
            {
                m_NS--;

                if (m_NS < 0)
                    m_NS = 0;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

           // UpdateRender();

            m_SpriteBatch.Begin();
            m_SpriteBatch.Draw(m_FrontBuffer, m_FrontbufferRect, null, Color.White, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);
            m_SpriteBatch.End();

            base.Draw(gameTime);
        }

        #region Utils

        Vector3 RandomInUnitySphere()
        {
            var vector = Vector3.Zero;

            do
            {
                vector = 2.0f * new Vector3((float)m_Random.NextDouble(), (float)m_Random.NextDouble(), (float)m_Random.NextDouble()) - Vector3.One;
            }
            while (vector.LengthSquared() > 1.0f);

            return vector;
        }

        Vector3 UnitVector(Vector3 vector) => vector / vector.Length();

        public void MakeUnitVector(ref Vector3 vector)
        {
            float k = 1.0f / (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
            vector *= k;
        }

        #endregion
    }
}
