using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameRayTracer.Utils;
using System.Collections.Generic;

namespace MonoGameRayTracer
{
    public class GPURaytracer : DrawableGameComponent
    {
        private SpriteBatch m_SpriteBatch;
        private Effect m_Effect;
        private Texture2D m_NoiseTexture;
        private Texture2D m_SceneTexture;
        private RenderTarget2D m_finalTexture;
        private Rectangle m_Rectangle;
        private Camera m_Camera;
        private int m_RenderWidth;
        private int m_RenderHeight;
        private int m_Step;

        public GPURaytracer(Game game, List<Hitable> world, Camera camera, int width, int height)
            : base(game)
        {
            m_Camera = camera;
            m_Step = 5;
            UpdateTextures(world, width, height);
            m_Effect = game.Content.Load<Effect>("Raytracer");
            game.Components.Add(this);
        }

        public void UpdateTextures(List<Hitable> world, int width, int height)
        {
            m_RenderWidth = width;
            m_RenderHeight = height;

            m_NoiseTexture = new Texture2D(Game.GraphicsDevice, width, height, false, SurfaceFormat.Color);
            m_finalTexture = new RenderTarget2D(Game.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 8, RenderTargetUsage.DiscardContents);

            var data = new List<Color>();

            for (var i = 0; i < world.Count; i++)
            {
                if (world[i] is Sphere)
                {
                    var sphere = (Sphere)world[i];
                    data.Add(new Color(sphere.Center.X, sphere.Center.Y, sphere.Center.Z, 1));
                    data.Add(new Color(sphere.Radius, 0, 0, 0));
                    data.Add(new Color(sphere.Material.Value));
                }
            }

            if (data.Count % 2 == 0)
                data.Add(Color.TransparentBlack);

            var size = (int)Mathf.Sqrt(data.Count);

            m_SceneTexture = new Texture2D(Game.GraphicsDevice, size, size, false, SurfaceFormat.Color);
            m_SceneTexture.SetData<Color>(data.ToArray());

            Color[] noise = new Color[m_RenderWidth * m_RenderHeight];

            for (var x = 0; x < m_RenderWidth; x++)
            {
                for (var y = 0; y < m_RenderHeight; y++)
                {
                    var value = Random.Value;
                    noise[x + y * m_RenderWidth] = new Color(value, value, value);
                }
            }

            m_NoiseTexture.SetData(noise);

            m_Rectangle = new Rectangle(0, 0, m_RenderWidth, m_RenderHeight);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            m_Effect.Parameters["SceneTexture"].SetValue(m_SceneTexture);
            m_Effect.Parameters["NoiseTexture"].SetValue(m_NoiseTexture);
            m_Effect.Parameters["LowerLeftCorner"].SetValue(m_Camera.LowerLeftCorner);
            m_Effect.Parameters["Horizontal"].SetValue(m_Camera.Horizontal);
            m_Effect.Parameters["Vertical"].SetValue(m_Camera.Vertical);
            m_Effect.Parameters["CameraPosition"].SetValue(m_Camera.Position);
            m_Effect.Parameters["Step"].SetValue(m_Step);
            m_Effect.Parameters["RenderWidth"].SetValue(m_RenderWidth);
            m_Effect.Parameters["RenderHeight"].SetValue(m_RenderHeight);

            m_SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, m_Effect);
            m_SpriteBatch.Draw(m_finalTexture, m_Rectangle, Color.White);
            m_SpriteBatch.End();
        }
    }
}
