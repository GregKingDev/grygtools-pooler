namespace GrygTools.Pooler
{
	public interface IPoolableObject
	{
		void InitPoolable();
		void ReturnPoolable();
	}
}