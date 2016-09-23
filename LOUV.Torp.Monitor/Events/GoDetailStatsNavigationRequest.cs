namespace LOUV.Torp.Monitor.Events
{
    public class GoDetailStatsNavigationRequest 
    {
        private string _titile;
        public GoDetailStatsNavigationRequest(string titile)
		{
		    _titile = titile;
		}

        public string Titile
        {
            get { return _titile; }
        }
    }
}