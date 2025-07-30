
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.UI;

    public class LandmineMachine : SpawnerEntity
    {
 
        
        protected override void SpawnMapEntity()
        {
            EntityManager.Instance.InitLandmine(tank);
           
        }
      
    }
