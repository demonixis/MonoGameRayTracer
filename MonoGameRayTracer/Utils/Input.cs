using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGameRayTracer.Utils
{
    public class Input : GameComponent
    {
        private KeyboardState m_LastState;
        private KeyboardState m_State;

        public Input(Game game)
            : base(game)
        {
            game.Components.Add(this);
        }

        public bool GetKeyDown(Keys key) => m_State.IsKeyDown(key) && !m_LastState.IsKeyDown(key);
        public bool GetKeyUp(Keys key) => m_State.IsKeyUp(key) && !m_LastState.IsKeyUp(key);
        public bool GetKey(Keys key) => m_State.IsKeyDown(key);

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            m_LastState = m_State;
            m_State = Keyboard.GetState();
        }
    }
}
