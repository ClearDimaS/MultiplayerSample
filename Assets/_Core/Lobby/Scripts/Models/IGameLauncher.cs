namespace MS.Lobby
{
    public interface IGameLauncher 
	{
        public void OnHostOptions(string room);

        public void OnJoinOptions(string room);
    }
}