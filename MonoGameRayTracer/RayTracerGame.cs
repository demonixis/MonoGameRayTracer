using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameRayTracer.Utils;
using System.IO;

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
        private RayTracer m_Raytracer;
        private bool m_ShowUI = true;
        private bool m_Realtime = false;
        private bool m_DisableMouse = true;

        public RayTracerGame()
        {
            m_GraphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.Title = "MonoGame Raytracer";
            m_Input = new Input(this);
        }

        protected override void LoadContent()
        {
            var width = 640;
            var height = 480;
            var scale = 0.5f;
            var rayStep = 1;
            var sceneComplexity = 1;

            var config = new ConfigParser("config.ini");
            config.GetBool("showUI", ref m_ShowUI);
            config.GetBool("Realtime", ref m_Realtime);
            config.GetInteger("Width", ref width);
            config.GetInteger("Height", ref height);
            config.GetFloat("Scale", ref scale);
            config.GetInteger("Step", ref rayStep);
            config.GetInteger("SceneComplexity", ref sceneComplexity);

            m_GraphicsDeviceManager.PreferredBackBufferWidth = width;
            m_GraphicsDeviceManager.PreferredBackBufferHeight = height;
            m_GraphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            m_GraphicsDeviceManager.ApplyChanges();

            m_FrontbufferRect = new Rectangle(0, 0, m_GraphicsDeviceManager.PreferredBackBufferWidth, m_GraphicsDeviceManager.PreferredBackBufferHeight);

            m_SpriteBatch = new SpriteBatch(GraphicsDevice);
            m_SpriteFont = Content.Load<SpriteFont>("Default");

            // Final setup.
            var aspect = (float)m_GraphicsDeviceManager.PreferredBackBufferWidth / (float)m_GraphicsDeviceManager.PreferredBackBufferHeight;

            var scene = SceneFactory.MakeSphereScene(Content, sceneComplexity);

            m_Camera = new Camera(new Vector3(0, 0.5f, 4.5f), new Vector3(-0.22f, 0.15f, 0.0f), Vector3.Up, 75.0f, aspect);
            m_World = new HitableList(scene);

            m_Raytracer = new RayTracer(this, scale);
            m_Raytracer.Step = rayStep;

            if (!m_Realtime)
                m_Raytracer.Render(m_Camera, m_World);
            else
                m_Raytracer.StartMTRenderLoop(m_Camera, m_World);
        }

        protected override void Update(GameTime gameTime)
        {
            if (m_Input.GetKeyDown(Keys.Escape))
            {
                m_Raytracer.Dispose();
                Exit();
            }

            // ---
            // --- RayTracer Parameters
            // ---

            if (m_Input.GetKeyDown(Keys.PageUp))
                m_Raytracer.Step++;

            if (m_Input.GetKeyDown(Keys.PageDown))
                m_Raytracer.Step--;

            if (m_Input.GetKeyDown(Keys.F9))
                m_DisableMouse = !m_DisableMouse;

            if (m_Input.GetKeyDown(Keys.F10))
                m_ShowUI = !m_ShowUI;

            if (m_Input.GetKeyDown(Keys.F11))
            {
                m_Realtime = !m_Realtime;
                if (m_Realtime)
                    m_Raytracer.StartMTRenderLoop(m_Camera, m_World);
                else
                    m_Raytracer.StopRenderLoop();
            }

            if (m_Input.GetKeyDown(Keys.F12) && !m_Realtime)
                m_Raytracer.Render(m_Camera, m_World);

            var upScale = m_Input.GetKeyDown(Keys.Insert);
            var downScale = m_Input.GetKeyDown(Keys.Delete);

            if (upScale || downScale)
            {
                var sign = upScale ? 1.0f : -1.0f;

                if (m_Raytracer.SetupBuffers(m_Raytracer.Scale + 0.05f * sign))
                {
                    if (m_Realtime)
                        m_Raytracer.StartMTRenderLoop(m_Camera, m_World);
                    else
                        m_Raytracer.Render(m_Camera, m_World);
                }
            }

            if (m_Input.GetKey(Keys.LeftControl) && m_Input.GetKeyDown(Keys.S))
            {
                var pp = GraphicsDevice.PresentationParameters;

                using (var stream = File.OpenWrite("screenshot.png"))
                    m_Raytracer.Texture.SaveAsPng(stream, pp.BackBufferWidth, pp.BackBufferHeight);
            }

            // ---
            // --- Camera movements
            // ---

            if (m_Realtime)
            {
                var rotationSpeed = -0.001f * gameTime.ElapsedGameTime.Milliseconds;
                var moveSpeed = 0.001f * gameTime.ElapsedGameTime.Milliseconds;

                if (m_Input.GetKey(Keys.Up) || m_Input.GetKey(Keys.Z))
                    m_Camera.Move(0.0f, 0.0f, -moveSpeed);
                else if (m_Input.GetKey(Keys.Down) || m_Input.GetKey(Keys.S))
                    m_Camera.Move(0.0f, 0.0f, moveSpeed);

                if (m_Input.GetKey(Keys.Left) || m_Input.GetKey(Keys.Q))
                    m_Camera.Move(-moveSpeed, 0.0f, 0.0f);
                else if (m_Input.GetKey(Keys.Right) || m_Input.GetKey(Keys.D))
                    m_Camera.Move(moveSpeed, 0.0f, 0.0f);

                if (m_Input.GetKey(Keys.E))
                    m_Camera.Move(0.0f, moveSpeed, 0.0f);
                else if (m_Input.GetKey(Keys.A))
                    m_Camera.Move(0.0f, -moveSpeed, 0.0f);

                if (!m_DisableMouse)
                    m_Camera.Rotate(rotationSpeed * m_Input.Vertical, rotationSpeed * m_Input.Horizontal, 0.0f);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);

            m_SpriteBatch.Begin();

            if (m_ShowUI)
            {
                if (m_StringPositions == null)
                {
                    var count = 6;
                    m_StringPositions = new Vector2[count];

                    for (var i = 0; i < count; i++)
                    {
                        m_StringPositions[i].X = 5;
                        m_StringPositions[i].Y = 5 + (15 * i);
                    }
                }

                m_SpriteBatch.DrawString(m_SpriteFont, $"Realtime: {m_Realtime}", m_StringPositions[0], Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Elapsed Time: {m_Raytracer.FrameTime}ms", m_StringPositions[1], Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Step: {m_Raytracer.Step}", m_StringPositions[2], Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Screen Width: {m_GraphicsDeviceManager.PreferredBackBufferWidth}", m_StringPositions[3], Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Screen Height: {m_GraphicsDeviceManager.PreferredBackBufferHeight}", m_StringPositions[4], Color.DarkGreen);
                m_SpriteBatch.DrawString(m_SpriteFont, $"Render Scale: {m_Raytracer.Scale * 100.0f}%", m_StringPositions[5], Color.DarkGreen);
            }

            m_SpriteBatch.End();
        }
    }
}
