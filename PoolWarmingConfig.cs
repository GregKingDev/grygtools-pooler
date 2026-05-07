using System;
using UnityEngine;
namespace GrygTools.Pooler
{
	[Serializable]
	public class PoolWarmingConfig
	{
		[SerializeField]
		private GameObject m_Template;
		public GameObject Template => m_Template;
		[SerializeField]
		private int m_Amount;
		public int Amount => m_Amount;
	}
}
