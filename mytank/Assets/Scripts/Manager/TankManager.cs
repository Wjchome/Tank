using System.Collections.Generic;

public class TankManager:SingletonMono<TankManager>
{
        
     public   List<TankController> controllers = new List<TankController>();
    }
