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
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Color[] Buffer { get; private set; }

        public SubRect(int x, int y, int width, int height, int slice)
        {
            X = x;
            Y = y;
            Width = width / slice;
            Height = height / slice;
            Buffer = new Color[width / slice * height / slice];
        }
    }

    public class RayTracer : IDisposable
    {
        private Texture2D m_backBufferTexture;
        private Color[] m_BackBuffer;
        private List<Thread> m_Threads;
        private Stopwatch m_Stopwatch;
        private int m_RenderWidth;
        private int m_RenderHeight;
        private int m_ThreadSleepTime = 10;
        private int m_MaxDepth = 50;
        private int m_Step = 1;
        private float m_LastFrameTime = 0.0f;
        private float m_Scale = 1.0f;
        private bool m_ThreadRunning = false;
        private int m_nbSlicePixels = 4;
        private SubRect[] m_subRects;

        public int MaxDepth
        {
            get => m_MaxDepth;
            set
            {
                m_MaxDepth = value;
                if (m_MaxDepth < 1)
                    m_MaxDepth = 1;
            }
        }

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

        public float FrameTime => m_LastFrameTime;

        public bool ThreadRunning => m_ThreadRunning;

        public float Scale => m_Scale;

        public RayTracer(GraphicsDevice device, float multiplier)
        {
            var screenWidth = device.PresentationParameters.BackBufferWidth;
            var screenHeight = device.PresentationParameters.BackBufferHeight;

            m_RenderWidth = (int)(screenWidth * multiplier);
            m_RenderHeight = (int)(screenHeight * multiplier);

            m_backBufferTexture = new Texture2D(device, m_RenderWidth, m_RenderHeight, false, SurfaceFormat.Color);
            m_BackBuffer = new Color[m_RenderWidth * m_RenderHeight];

            m_Scale = multiplier;
            m_Stopwatch = new Stopwatch();
            m_Threads = new List<Thread>();

            m_subRects = new SubRect[m_nbSlicePixels * m_nbSlicePixels];

            for (int y = 0; y < m_nbSlicePixels; y++)
            {
                for (int x = 0; x < m_nbSlicePixels; x++)
                {
                    var yMin = m_RenderHeight / m_nbSlicePixels * y;
                    var xMin = m_RenderWidth / m_nbSlicePixels * x;
                    m_subRects[x + m_nbSlicePixels * y] = new SubRect(xMin, yMin, m_RenderWidth, m_RenderHeight, m_nbSlicePixels);
                }
            }
        }

        private Vector3 GetColor(Ray ray, Hitable world, int depth) => GetColor(ref ray, world, depth);

        private Vector3 GetColor(ref Ray ray, Hitable world, int depth)
        {
            HitRecord record = new HitRecord();
            if (world.Hit(ref ray, 0.001f, float.MaxValue, ref record))
            {
                var scattered = new Ray();
                var attenuation = Vector3.Zero;

                if (record.Material != null)
                {
                    if (depth < m_MaxDepth && record.Material.Scatter(ref ray, ref record, ref attenuation, ref scattered))
                        return attenuation * GetColor(ref scattered, world, depth + 1);
                    else
                        return Vector3.Zero;
                }

                var target = record.P + record.Normal + Mathf.RandomInUnitySphere();
                return 0.5f * GetColor(new Ray(record.P, target - record.P), world, depth);
            }

            var unitDirection = Mathf.UnitVector(ray.Direction);
            var t = 0.5f * (unitDirection.Y + 1.0f);
            return (1.0f - t) * Vector3.One + t * new Vector3(0.5f, 0.7f, 1.0f);
        }

        public void StartRenderLoop(Camera camera, Hitable world)
        {
            if (m_ThreadRunning)
                throw new Exception("The Thread is still running");

            m_ThreadRunning = true;

            var thread = new Thread(() =>
            {
                while (m_ThreadRunning)
                {
                    Render(camera, world);
                    Thread.Sleep(m_ThreadSleepTime);
                }
            });

            m_Threads.Add(thread);

            thread.Start();
        }

        public void StopRenderLoop()
        {
            foreach (var thread in m_Threads)
                if (thread.IsAlive)
                    thread.Abort();
        }

        public void StartMTRenderLoop(Camera camera, Hitable world)
        {
            if (m_ThreadRunning)
                throw new Exception("The Thread is still running");

            m_ThreadRunning = true;

            for (var i = 0; i < m_subRects.Length; i++)
                StartThreadedRenderLoop(camera, world, m_subRects[i]);
        }

        private void StartThreadedRenderLoop(Camera camera, Hitable world, SubRect subRect)
        {
            var thread = new Thread(() =>
            {
                while (m_ThreadRunning)
                {
                    RenderMT(camera, world, subRect);
                    Thread.Sleep(m_ThreadSleepTime);
                }
            });

            m_Threads.Add(thread);

            thread.Start();
        }

        public void Render(Camera camera, Hitable world)
        {
            m_Stopwatch.Restart();

            for (var j = m_RenderHeight - 1; j >= 0; j--)
                for (var i = 0; i < m_RenderWidth; i++)
                    UpdatePixel(ref i, ref j, camera, world);

            m_backBufferTexture.SetData<Color>(m_BackBuffer);

            m_Stopwatch.Stop();
            m_LastFrameTime = m_Stopwatch.ElapsedMilliseconds;
        }

        private void UpdatePixel(ref int i, ref int j, Camera camera, Hitable world)
        {
            var color = Vector3.Zero;

            for (var s = 0; s < m_Step; s++)
            {
                var u = (float)(i + Random.Value) / m_RenderWidth;
                var v = (float)(j + Random.Value) / m_RenderHeight;
                var ray = camera.GetRay(ref u, ref v);
                color += GetColor(ref ray, world, 0);
            }

            color /= (float)m_Step;
            color.X = Mathf.Sqrt(color.X);
            color.Y = Mathf.Sqrt(color.Y);
            color.Z = Mathf.Sqrt(color.Z);

            m_BackBuffer[i + j * m_RenderWidth] = new Color(color.X, color.Y, color.Z);
        }

        private void RenderMT(Camera camera, Hitable world, SubRect subRect)
        {
            if (subRect.X == 0 && subRect.Y == 0)
                m_Stopwatch.Restart();

            for (var j = 0; j < subRect.Height - 1; j++)
                for (var i = 0; i < subRect.Width - 1; i++)
                    UpdatePixelMT(ref i, ref j, camera, world, subRect);

            m_backBufferTexture.SetData<Color>(0, new Rectangle(subRect.X, subRect.Y, subRect.Width, subRect.Height), subRect.Buffer, 0, subRect.Buffer.Length);

            if (subRect.X == 0 && subRect.Y == 0)
            {
                m_Stopwatch.Stop();
                m_LastFrameTime = m_Stopwatch.ElapsedMilliseconds;
            }
        }

        private void UpdatePixelMT(ref int i, ref int j, Camera camera, Hitable world, SubRect subRect)
        {
            var color = Vector3.Zero;
            int x = i + subRect.X;
            int y = j + subRect.Y;

            for (var s = 0; s < m_Step; s++)
            {
                var u = (float)(x + Random.Value) / subRect.Width;
                var v = (float)(y + Random.Value) / subRect.Height;
                var ray = camera.GetRay(ref u, ref v);
                color += GetColor(ref ray, world, 0);
            }

            color /= (float)m_Step;
            color.X = Mathf.Sqrt(color.X);
            color.Y = Mathf.Sqrt(color.Y);
            color.Z = Mathf.Sqrt(color.Z);

            subRect.Buffer[i + j * subRect.Width] = new Color(color.X, color.Y, color.Z);
        }

        public void Dispose()
        {
            StopRenderLoop();
            m_backBufferTexture.Dispose();
        }

        public void Present(SpriteBatch spriteBatch, ref Rectangle rectangle)
        {
            spriteBatch.Draw(m_backBufferTexture, rectangle, null, Color.White, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);
        }
    }
}
