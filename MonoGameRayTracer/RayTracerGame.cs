using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameRayTracer.Materials;
using MonoGameRayTracer.Utils;
using System.Collections.Generic;

namespace MonoGameRayTracer
{
    public class RayTracerGame : Game
    {
        private GraphicsDeviceManager m_GraphicsDeviceManager;
        private SpriteBatch m_SpriteBatch;
        private Rectangle m_FrontbufferRect;
        private Camera m_Camera;
        private HitableList m_World;
        private SpriteFont m_SpriteFont;
        private Input m_Input;
        private Vector2[] m_StringPositions;
        private RayTracer m_RayTracer;
        private bool m_ShowUI = true;
        private bool m_Realtime = false;

        public RayTracerGame()
        {
            m_GraphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.Title = "MonoGame Raytracer";
            m_Input = new Input(this);
        }

        protected override void LoadContent()
        {
            m_GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
            m_GraphicsDeviceManager.PreferredBackBufferHeight = 720;
            m_GraphicsDeviceManager.ApplyChanges();

            m_FrontbufferRect = new Rectangle(0, 0, m_GraphicsDeviceManager.PreferredBackBufferWidth, m_GraphicsDeviceManager.PreferredBackBufferHeight);

            m_SpriteBatch = new SpriteBatch(GraphicsDevice);
            m_SpriteFont = Content.Load<SpriteFont>("Default");

            m_RayTracer = new RayTracer(GraphicsDevice, 0.75f);

            var spheres = new Hitable[]
            {
                new Sphere(new Vector3(0.0f, 0.0f, -2.0f), 0.5f, new LambertMaterial(0.6f, 0.2f, 0.3f)),
                new Sphere(new Vector3(-1.0f, -0.25f, -0.5f), 0.15f, new MetalMaterial(0.2f, 0.8f, 0.3f, 0.7f)),
                new Sphere(new Vector3(1.5f, 0.0f, -1.5f), 0.5f, new DieletricMaterial(2.5f)),
                new Sphere(new Vector3(-2.0f, 0.0f, -2.0f), 0.5f, new MetalMaterial(0.8f, 0.6f, 0.2f, 0.3f)),
                new Sphere(new Vector3(0.0f, -100.5f, -1.0f), 100, new MetalMaterial(0.8f, 0.8f, 0.8f, 0.1f))
            };

            var list = new List<Hitable>();
            list.Add(new Sphere(new Vector3(0, -1000, 0), 1000, new LambertMaterial(0.5f, 0.5f, 0.5f)));

            var temp = new Vector3(4, 0.2f, 0);
            var step = 3;

            for (var a = -step; a < step; a++)
            {
                for (var b = -step; b < step; b++)
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

            list.Add(new Sphere(new Vector3(0, 1, 0), 1, new DieletricMaterial(1.5f)));
            list.Add(new Sphere(new Vector3(-4, 1, 0), 1, new LambertMaterial(0.4f, 0.2f, 0.1f)));
            list.Add(new Sphere(new Vector3(4, 1, 0), 1, new MetalMaterial(0.7f, 0.6f, 0.5f, 0.0f)));

            var aspect = (float)m_GraphicsDeviceManager.PreferredBackBufferWidth / (float)m_GraphicsDeviceManager.PreferredBackBufferHeight;

            m_Camera = new Camera(new Vector3(0, 0.5f, 4.5f), new Vector3(-0.22f, 0.15f, 0.0f), Vector3.Up, 75.0f, aspect);
            m_World = new HitableList(list);

            if (!m_Realtime)
                m_RayTracer.Render(m_Camera, m_World);
            else
                m_RayTracer.StartThreadedRenderLoop(m_Camera, m_World);
        }

        protected override void Update(GameTime gameTime)
        {
            if (m_Input.GetKeyDown(Keys.Escape))
            {
                m_RayTracer.Dispose();
                Exit();
            }

            // ---
            // --- RayTracer Parameters
            // ---

            if (m_Input.GetKeyDown(Keys.PageUp))
                m_RayTracer.Step++;

            if (m_Input.GetKeyDown(Keys.PageDown))
                m_RayTracer.Step--;

            if (m_Input.GetKeyDown(Keys.F12) && !m_Realtime)
                m_RayTracer.Render(m_Camera, m_World);

            if (m_Input.GetKeyDown(Keys.F11))
            {
                m_Realtime = !m_Realtime;
                if (m_Realtime)
                    m_RayTracer.StartThreadedRenderLoop(m_Camera, m_World);
                else
                    m_RayTracer.StopThreadedRenderingLoop();
            }

            if (m_Input.GetKeyDown(Keys.F10))
                m_ShowUI = !m_ShowUI;

            if (m_Input.GetKeyDown(Keys.Insert))
                m_RayTracer.MaxDepth++;

            if (m_Input.GetKeyDown(Keys.Delete))
                m_RayTracer.MaxDepth--;

            // ---
            // --- Camera movements
            // ---

            if (m_Realtime)
            {
                var rotationSpeed = -0.0001f * gameTime.ElapsedGameTime.Milliseconds;
                var moveSpeed = 0.001f * gameTime.ElapsedGameTime.Milliseconds;

                if (m_Input.GetKey(Keys.Up) || m_Input.GetKey(Keys.Z))
                    m_Camera.Move(0.0f, 0.0f, -moveSpeed);
                else if (m_Input.GetKey(Keys.Down) || m_Input.GetKey(Keys.S))
                    m_Camera.Move(0.0f, 0.0f, moveSpeed);

                if (m_Input.GetKey(Keys.Left) || m_Input.GetKey(Keys.A))
                    m_Camera.Move(-moveSpeed, 0.0f, 0.0f);
                else if (m_Input.GetKey(Keys.Right) || m_Input.GetKey(Keys.E))
                    m_Camera.Move(moveSpeed, 0.0f, 0.0f);

                if (m_Input.GetKey(Keys.R))
                    m_Camera.Move(0.0f, moveSpeed, 0.0f);
                else if (m_Input.GetKey(Keys.F))
                    m_Camera.Move(0.0f, -moveSpeed, 0.0f);

                m_Camera.Rotate(rotationSpeed * m_Input.Vertical, rotationSpeed * m_Input.Horizontal, 0.0f);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            m_SpriteBatch.Begin();

            m_RayTracer.Present(m_SpriteBatch, ref m_FrontbufferRect);

            if (m_ShowUI)
            {
                if (m_StringPositions == null)
                {
                    var count = 7;
                    m_StringPositions = new Vector2[count];

                    for (var i = 0; i < count; i++)
                    {
                        m_StringPositions[i].X = 5;
                        m_StringPositions[i].Y = 5 + (15 * i);
                    }
                }

                m_SpriteBatch.DrawString(m_SpriteFont, $"Realtime: {m_Realtime}", m_StringPositions[0], Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Elapsed Time: {m_RayTracer.FrameTime}ms", m_StringPositions[1], Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Step: {m_RayTracer.Step}", m_StringPositions[2], Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Screen Width: {m_GraphicsDeviceManager.PreferredBackBufferWidth}", m_StringPositions[3], Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Screen Height: {m_GraphicsDeviceManager.PreferredBackBufferHeight}", m_StringPositions[4], Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Render Scale: {m_RayTracer.Scale * 100.0f}%", m_StringPositions[5], Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Depth: {m_RayTracer.MaxDepth}", m_StringPositions[6], Color.DarkGreen);
            }

            m_SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
