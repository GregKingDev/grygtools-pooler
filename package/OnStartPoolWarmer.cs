using System;
using UnityEngine;
namespace GrygTools.Pooler
{
	public class OnStartPoolWarmer : MonoBehaviour
	{
		[SerializeReference]
		private PoolWarmingConfig[] m_ObjectsToWarm;

		private void Start()
		{
			foreach (PoolWarmingConfig config in m_ObjectsToWarm)
			{
				PoolManager.Instance.TryGetPool(config.Template, out IPool pool);
				if (pool == null)
				{
					pool = new Pool();
					pool.Init(config.Template);
					PoolManager.Instance.AddPool(config.Template, pool);
				}
				pool.WarmPool(config.Amount);
			}
		}
	}
}
