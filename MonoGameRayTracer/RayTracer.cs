using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameRayTracer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MonoGameRayTracer
{
    public class SubRect
    {
        public bool Done { get; set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public SubRect(int x, int y, int width, int height, int slice)
        {
            X = width / slice * x;
            Y = height / slice * y;
            Width = width / slice;
            Height = height / slice;
        }
    }

    public class RayTracer : DrawableGameComponent
    {
        private const int MaxDepth = 50;
        private const int ThreadSleepDuration = 1;
        private Color[] m_BackBuffer;
        private List<Thread> m_Threads;
        private Stopwatch m_Stopwatch;
        private SubRect[] m_subRects;
        private Rectangle m_DrawRectangle;
        private SpriteBatch m_SpriteBatch;
        private int m_RenderWidth;
        private int m_RenderHeight;
        private int m_Step = 1;
        private bool m_ThreadRunning = false;
        private int m_nbSlicePixels = 4;

        public int Step
        {
            get => m_Step;
            set
            {
                m_Step = value;
                if (m_Step < 1)
                    m_Step = 1;
            }
        }

        public float FrameTime { get; private set; }

        public float Scale { get; set; } = 1.0f;

        public Texture2D BackBufferTexture { get; private set; }

        public RayTracer(Game game, float scale)
            : base(game)
        {
            m_Stopwatch = new Stopwatch();
            m_Threads = new List<Thread>();
            m_nbSlicePixels = (int)Math.Sqrt(Environment.ProcessorCount);
            m_subRects = new SubRect[m_nbSlicePixels * m_nbSlicePixels];
            m_SpriteBatch = new SpriteBatch(Game.GraphicsDevice);

            SetupBuffers(scale);

            game.Components.Add(this);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                Stop();
                BackBufferTexture.Dispose();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            var count = 0;

            foreach (var rect in m_subRects)
                if (rect.Done)
                    count++;

            var flushBackbuffer = count == m_subRects.Length;
            if (flushBackbuffer)
                BackBufferTexture.SetData<Color>(m_BackBuffer);

            m_SpriteBatch.Begin();
            m_SpriteBatch.Draw(BackBufferTexture, m_DrawRectangle, null, Color.White);
            m_SpriteBatch.End();

            if (flushBackbuffer)
                foreach (var rect in m_subRects)
                    rect.Done = false;
        }

        public bool SetupBuffers(float scale)
        {
            if (scale < 0.1f)
                return false;

            Scale = scale;

            if (m_ThreadRunning)
                Stop();

            var device = Game.GraphicsDevice;
            var screenWidth = device.PresentationParameters.BackBufferWidth;
            var screenHeight = device.PresentationParameters.BackBufferHeight;

            m_RenderWidth = (int)(screenWidth * scale);
            m_RenderHeight = (int)(screenHeight * scale);

            if (BackBufferTexture != null)
                BackBufferTexture.Dispose();

            BackBufferTexture = new Texture2D(device, m_RenderWidth, m_RenderHeight, false, SurfaceFormat.Color);
            m_BackBuffer = new Color[m_RenderWidth * m_RenderHeight];

            for (var y = 0; y < m_nbSlicePixels; y++)
                for (var x = 0; x < m_nbSlicePixels; x++)
                    m_subRects[x + y * m_nbSlicePixels] = new SubRect(x, y, m_RenderWidth, m_RenderHeight, m_nbSlicePixels);

            m_DrawRectangle = new Rectangle(0, 0, screenWidth, screenHeight);

            return true;
        }

        #region Threads Management

        public void StartSingleThread(Camera camera, Hitable world)
        {
            if (m_ThreadRunning)
                throw new Exception("The Thread is still running");

            m_ThreadRunning = true;

            var thread = new Thread(() =>
            {
                while (m_ThreadRunning)
                {
                    Render(camera, world);
                    Thread.Sleep(ThreadSleepDuration);
                }
            });

            m_Threads.Add(thread);

            thread.Start();
        }

        public void Stop()
        {
            m_ThreadRunning = false;

            foreach (var thread in m_Threads)
                if (thread.IsAlive)
                    thread.Abort();
        }

        public void Start(Camera camera, Hitable world)
        {
            if (m_ThreadRunning)
                throw new Exception("The Thread is still running");

            m_ThreadRunning = true;

            for (var i = 0; i < m_subRects.Length; i++)
                StartRenderLoop(camera, world, i);
        }

        private void StartRenderLoop(Camera camera, Hitable world, int subRect)
        {
            var thread = new Thread(() =>
            {
                while (m_ThreadRunning)
                {
                    RenderMT(camera, world, subRect);
                    Thread.Sleep(ThreadSleepDuration);
                }
            });

            m_Threads.Add(thread);

            thread.Start();
        }

        #endregion

        private Vector3 GetColorRecursive(ref Ray ray, Hitable world, int depth)
        {
            HitRecord record = new HitRecord();
            if (world.Hit(ref ray, 0.001f, float.MaxValue, ref record))
            {
                var scattered = new Ray();
                var attenuation = Vector3.Zero;

                if (record.Material != null)
                {
                    if (depth < MaxDepth && record.Material.Scatter(ref ray, ref record, ref attenuation, ref scattered))
                        return attenuation * GetColor(ref scattered, world, depth + 1);
                    else
                        return Vector3.Zero;
                }

                var target = record.P + record.Normal + Mathf.RandomInUnitySphere();
                var nRay = new Ray(record.P, target - record.P);
                return 0.5f * GetColor(ref nRay, world, depth);
            }

            // Background color.
            var unitDirection = Mathf.UnitVector(ray.Direction);
            var t = 0.5f * (unitDirection.Y + 1.0f);
            return (1.0f - t) * Vector3.One + t * new Vector3(0.5f, 0.7f, 1.0f);
        }


        private Vector3 GetColor(ref Ray ray, Hitable world, int depth)
        {
            HitRecord record = new HitRecord();

            var hit = world.Hit(ref ray, 0.001f, float.MaxValue, ref record);
            var wasInLoop = hit;
            var color = Vector3.Zero;
            var scattered = new Ray();
            var attenuation = Vector3.Zero;

            while (hit && depth < MaxDepth)
            {
                color += record.Material.Emitted(ref record.U, ref record.V, ref record.P);

                hit = record.Material.Scatter(ref ray, ref record, ref attenuation, ref scattered);

                if (hit)
                {
                    if (depth == 0)
                        color += attenuation;
                    else
                        color *= attenuation;

                    hit = world.Hit(ref scattered, 0.001f, float.MaxValue, ref record);

                    depth++;
                }
            }

            if (wasInLoop)
                return color;

            // Background color.
            var unitDirection = Mathf.UnitVector(ray.Direction);
            var t = 0.5f * (unitDirection.Y + 1.0f);
            return (1.0f - t) * Vector3.One + t * new Vector3(0.5f, 0.7f, 1.0f);
        }

        public void Render(Camera camera, Hitable world)
        {
            m_Stopwatch.Restart();

            for (var j = 0; j < m_RenderHeight; j++)
                for (var i = 0; i < m_RenderWidth; i++)
                    UpdatePixel(ref i, ref j, camera, world);

            BackBufferTexture.SetData<Color>(m_BackBuffer);

            m_Stopwatch.Stop();
            FrameTime = m_Stopwatch.ElapsedMilliseconds;
        }

        private void UpdatePixel(ref int i, ref int j, Camera camera, Hitable world)
        {
            var color = Vector3.Zero;
            var ray = new Ray();

            for (var s = 0; s < m_Step; s++)
            {
                var u = (float)(i + Random.Value) / m_RenderWidth;
                var v = (float)(j + Random.Value) / m_RenderHeight;
                camera.GetRay(ref ray, u, v);
                color += GetColor(ref ray, world, 0);
            }

            color /= (float)m_Step;
            Mathf.Sqrt(ref color);

            m_BackBuffer[i + j * m_RenderWidth] = new Color(color.X, color.Y, color.Z);
        }

        private void RenderMT(Camera camera, Hitable world, int subRectIndex)
        {
            var subRect = m_subRects[subRectIndex];

            while (subRect.Done)
                Thread.Sleep(1);

            if (subRect.X == 0 && subRect.Y == 0)
                m_Stopwatch.Restart();

            for (var j = 0; j < subRect.Height; j++)
                for (var i = 0; i < subRect.Width; i++)
                    UpdatePixelMT(i, j, camera, world, subRectIndex);

            subRect.Done = true;

            if (subRect.X == 0 && subRect.Y == 0)
            {
                m_Stopwatch.Stop();
                FrameTime = m_Stopwatch.ElapsedMilliseconds;
            }
        }

        private void UpdatePixelMT(int i, int j, Camera camera, Hitable world, int subRectIndex)
        {
            var subRect = m_subRects[subRectIndex];
            var color = Vector3.Zero;
            int x = i + subRect.X;
            int y = j + subRect.Y;
            var ray = new Ray();

            for (var s = 0; s < m_Step; s++)
            {
                var u = (float)(x + Random.Value) / m_RenderWidth;
                var v = (float)(y + Random.Value) / m_RenderHeight;
                camera.GetRay(ref ray, u, v);
                color += GetColor(ref ray, world, 0);
            }

            color /= (float)m_Step;
            color.X = Mathf.Sqrt(color.X);
            color.Y = Mathf.Sqrt(color.Y);
            color.Z = Mathf.Sqrt(color.Z);

            m_BackBuffer[x + y * m_RenderWidth].R = (byte)(color.X * 255);
            m_BackBuffer[x + y * m_RenderWidth].G = (byte)(color.Y * 255);
            m_BackBuffer[x + y * m_RenderWidth].B = (byte)(color.Z * 255);
            m_BackBuffer[x + y * m_RenderWidth].A = 255;
        }
    }
}
