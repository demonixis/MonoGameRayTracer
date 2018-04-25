using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MonoGameRayTracer
{
    public class RayTracerGame : Game
    {
        private static Random m_Random = new Random(DateTime.Now.Millisecond);
        private GraphicsDeviceManager m_GraphicsDeviceManager;
        private SpriteBatch m_SpriteBatch;
        private Texture2D m_FrontBuffer;
        private int m_RenderWidth;
        private int m_RenderHeight;
        private int m_ScreenWidth;
        private int m_ScreenHeight;
        private Color[] m_BackBuffer;
        private Rectangle m_FrontbufferRect;
        private Camera m_Camera;
        private HitableList m_World;
        private int m_NS = 2;
        private Thread m_Thread;
        private SpriteFont m_SpriteFont;
        private Stopwatch m_Stopwatch;
        private bool m_ShowUI = true;
        private int m_ThreadSleepTime = 10;
        private bool m_UseThread = false;
        private bool m_ParallelFor = false;
        private bool m_Realtime = false;

        public RayTracerGame()
        {
            m_GraphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.Title = "MonoGame Raytracer";
        }

        private void SetRenderSize(int screenWidth, int screenHeight, float multiplier)
        {
            m_ScreenWidth = screenWidth;
            m_ScreenHeight = screenHeight;
            m_GraphicsDeviceManager.PreferredBackBufferWidth = screenWidth;
            m_GraphicsDeviceManager.PreferredBackBufferHeight = screenHeight;
            m_GraphicsDeviceManager.ApplyChanges();

            m_RenderWidth = (int)(screenWidth * multiplier);
            m_RenderHeight = (int)(screenHeight * multiplier);

            m_SpriteBatch = new SpriteBatch(GraphicsDevice);
            m_FrontBuffer = new Texture2D(GraphicsDevice, m_RenderWidth, m_RenderHeight, false, SurfaceFormat.Color);
            m_BackBuffer = new Color[m_RenderWidth * m_RenderHeight];

            m_FrontbufferRect = new Rectangle(0, 0, screenWidth, screenHeight);
        }

        protected override void LoadContent()
        {
            m_SpriteFont = Content.Load<SpriteFont>("Default");
            m_Stopwatch = new Stopwatch();

            SetRenderSize(640, 480, 0.25f);

            var spheres = new Hitable[]
            {
                new Sphere(new Vector3(0.0f, 0.0f, -1.0f), 0.5f, new LambertMaterial(0.8f, 0.3f, 0.3f)),
                new Sphere(new Vector3(-1.0f, -0.25f, -0.5f), 0.15f, new MetalMaterial(0.2f, 0.8f, 0.3f, 0.7f)),
                new Sphere(new Vector3(1.5f, 0.0f, -1.5f), 0.5f, new LambertMaterial(0.8f, 0.8f, 0.0f)),
                new Sphere(new Vector3(-2.0f, 0.0f, -2.0f), 0.5f, new MetalMaterial(0.8f, 0.6f, 0.2f, 0.3f)),
                new Sphere(new Vector3(0.0f, -100.5f, -1.0f), 100, new MetalMaterial(0.8f, 0.8f, 0.8f, 0.1f))
            };

            m_Camera = new Camera();
            m_World = new HitableList(spheres);

            if (!m_Realtime)
                UpdatePixels();
            else if (m_UseThread)
                StartThreadedRenderLoop();
        }

        protected override void Update(GameTime gameTime)
        {
            var state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Escape))
                Exit();

            if (state.IsKeyDown(Keys.Up))
                m_Camera.Move(0, 0, 1);
            else if (state.IsKeyDown(Keys.Down))
                m_Camera.Move(0, 0, -1);

            if (state.IsKeyDown(Keys.Left))
                m_Camera.Move(-1, 0, 0);
            else if (state.IsKeyDown(Keys.Right))
                m_Camera.Move(1, 0, -1);

            if (state.IsKeyDown(Keys.PageUp))
                m_NS++;

            if (state.IsKeyDown(Keys.PageDown))
            {
                m_NS--;

                if (m_NS < 1)
                    m_NS = 1;
            }

            if (state.IsKeyDown(Keys.P))
                m_ParallelFor = !m_ParallelFor;

            if (state.IsKeyDown(Keys.R))
            {
                if (m_UseThread && m_Thread != null)
                {
                    m_Thread.Abort();
                    m_UseThread = false;
                }

                m_Realtime = true;
            }

            if (state.IsKeyDown(Keys.T))
            {
                m_UseThread = true;
                StartThreadedRenderLoop();
            }

            if (state.IsKeyDown(Keys.Space))
                UpdatePixels();

            if (state.IsKeyDown(Keys.I))
                m_ShowUI = !m_ShowUI;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (m_Realtime && !m_UseThread)
                UpdatePixels();

            m_SpriteBatch.Begin();
            m_SpriteBatch.Draw(m_FrontBuffer, m_FrontbufferRect, null, Color.White, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);

            if (m_ShowUI)
            {
                m_SpriteBatch.DrawString(m_SpriteFont, $"Realtime: {m_Realtime}", new Vector2(5, 5), Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Threading: {m_UseThread}", new Vector2(5, 20), Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Elapsed Time: {m_Stopwatch.ElapsedMilliseconds}ms", new Vector2(5, 35), Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"ParallelFor: {m_ParallelFor}", new Vector2(5, 50), Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Step: {m_NS}", new Vector2(5, 65), Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Screen Width: {m_ScreenWidth}", new Vector2(5, 80), Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Screen Height: {m_ScreenHeight}", new Vector2(5, 95), Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Render Scale: {(float)m_RenderWidth / (float)m_ScreenWidth}%", new Vector2(5, 110), Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Working: {(m_Realtime ? "Yes" : m_Stopwatch.IsRunning ? "Yes" : "No")}", new Vector2(5, 125), Color.DarkGreen);
            }

            m_SpriteBatch.End();

            base.Draw(gameTime);
        }

        #region Ray Tracing

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

        private void StartThreadedRenderLoop()
        {
            m_Thread = new Thread(() =>
            {
                while (true)
                {
                    UpdatePixels();
                    Thread.Sleep(m_ThreadSleepTime);
                }
            });

            m_Thread.Start();
        }

        private void UpdatePixels()
        {
            m_Stopwatch.Restart();

            for (var j = m_RenderHeight - 1; j >= 0; j--)
                for (var i = 0; i < m_RenderWidth; i++)
                    UpdatePixel(i, j);

            m_FrontBuffer.SetData<Color>(m_BackBuffer);

            m_Stopwatch.Stop();
        }

        private void UpdatePixel(int i, int j)
        {
            var color = Vector3.Zero;

            if (m_ParallelFor)
            {
                Parallel.For(0, m_NS, index =>
                {
                    var u = (float)(i + (float)m_Random.NextDouble()) / m_RenderWidth;
                    var v = (float)(j + (float)m_Random.NextDouble()) / m_RenderHeight;
                    var ray = m_Camera.GetRay(ref u, ref v);
                    color += GetColor(ref ray, m_World, 0);
                });
            }
            else
            {
                for (var s = 0; s < m_NS; s++)
                {
                    var u = (float)(i + (float)m_Random.NextDouble()) / m_RenderWidth;
                    var v = (float)(j + (float)m_Random.NextDouble()) / m_RenderHeight;
                    var ray = m_Camera.GetRay(ref u, ref v);
                    color += GetColor(ref ray, m_World, 0);
                }
            }

            color /= (float)m_NS;
            color.X = (float)Math.Sqrt(color.X);
            color.Y = (float)Math.Sqrt(color.Y);
            color.Z = (float)Math.Sqrt(color.Z);

            m_BackBuffer[i + j * m_RenderWidth] = new Color(color.X, color.Y, color.Z);
        }

        #endregion

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
