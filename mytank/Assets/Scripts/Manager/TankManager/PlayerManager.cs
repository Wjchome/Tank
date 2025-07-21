
    using System.Collections.Generic;

    public class PlayerManager:SingletonMono<PlayerManager>
    {
        public List<TankController> activePlayers = new List<TankController>();
        
    }
