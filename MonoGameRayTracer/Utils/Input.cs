using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGameRayTracer.Utils
{
    public class Input : GameComponent
    {
        private KeyboardState m_LastState;
        private MouseState m_LastMouseState;
        private KeyboardState m_State;
        private MouseState m_MouseState;
        private Point m_Axis;

        public Input(Game game)
            : base(game)
        {
            game.Components.Add(this);

            m_MouseState = Mouse.GetState();
        }

        public bool GetKeyDown(Keys key) => m_State.IsKeyDown(key) && !m_LastState.IsKeyDown(key);
        public bool GetKeyUp(Keys key) => m_State.IsKeyUp(key) && !m_LastState.IsKeyUp(key);
        public bool GetKey(Keys key) => m_State.IsKeyDown(key);
        public float Horizontal => m_Axis.X;
        public float Vertical => m_Axis.Y;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            m_LastState = m_State;
            m_State = Keyboard.GetState();

            m_LastMouseState = m_MouseState;
            m_MouseState = Mouse.GetState();

            if (m_LastMouseState.X + m_LastMouseState.Y == 0)
                m_LastMouseState = m_MouseState;

            m_Axis.X = m_MouseState.X - m_LastMouseState.X;
            m_Axis.Y = m_MouseState.Y - m_LastMouseState.Y;
        }
    }
}
