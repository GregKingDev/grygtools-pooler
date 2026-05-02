using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

[assembly: InternalsVisibleTo("GrygTools.NetworkPooler")]
namespace GrygTools.Pooler
{
	public interface IPool
	{
		public Task Init(GameObject template);
		internal int Count { get; }
		internal void WarmPool(int amount);
		internal GameObject FindAvailableObject();
		internal void ReturnLeasedObject(GameObject obj);
		internal bool HasComponent(System.Type type);
		internal void ReturnAll();
		internal void RemoveLeasedObject(LeaseHandle leaseHandle, bool destroyOnRemove);
		internal void Clean();
		internal void Clear(bool destroyEvenIfActive = false);
	}
}
