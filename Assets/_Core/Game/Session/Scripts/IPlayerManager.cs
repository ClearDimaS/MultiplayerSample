using Fusion;
using System;

namespace MS.GameSession
{
    public interface IPlayerManager
    {
        public IPlayer Get(PlayerRef playerref);
        public void HandleNewPlayers();
        public void AddPlayer(IPlayer player);

        public void RemovePlayer(IPlayer player);
    }
}
