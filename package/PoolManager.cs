using GrygToolsUtils;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("GrygTools.NetworkPooling.Runtime")]
namespace GrygTools.Pooler
{
    ///Designed as a pooling system for projects using pre 2021 unity versions
    public class PoolManager : MbSingleton<PoolManager>
    {
        private readonly Dictionary<GameObject, IPool> m_TemplateToPools = new();
        private readonly Dictionary<int, Transform> m_Lanes = new Dictionary<int, Transform>();
        private readonly Dictionary<GameObject, LeaseHandle> m_ObjectToHandleDictionary = new Dictionary<GameObject, LeaseHandle>();
        private readonly Dictionary<GameObject, IPool> m_ObjectToPool = new Dictionary<GameObject, IPool>();
        internal Dictionary<GameObject, IPool> ObjectToPool => m_ObjectToPool;
        private Transform m_PoolRoot;
        private bool m_IsQuitting = false;
        
        protected override void Init()
        {
            base.Init();
            m_PoolRoot = new GameObject("Pool").transform;
            m_PoolRoot.parent = transform;
        }
        
        public bool IsLeasedObj(GameObject obj)
        {
            return obj != null && m_ObjectToPool.ContainsKey(obj);
        }
        
        public bool IsLeasedObj(MonoBehaviour obj)
        {
            return obj != null && m_ObjectToPool.ContainsKey(obj.gameObject);
        }

        public T LeaseObject<T>(T template, Transform parent) where T : MonoBehaviour
        {
            T newObject = LeaseObject(template);
            newObject.transform.SetParent(parent);
            newObject.transform.position = parent.position;
            
            return newObject;
        }
        
        public GameObject LeaseObject(GameObject template, Transform parent)
        {
            GameObject obj = LeaseObject(template);
            obj.transform.SetParent(parent);
            obj.transform.position = parent.position;
			
            return obj;
        }

        public T LeaseObject<T>(T template) where T : MonoBehaviour
        {
            GameObject newObject = LeaseObject(template.gameObject);
            if (newObject.TryGetComponent(out T component))
            {
                return component;
            }
            Destroy(newObject);
            
            return null;
        }
        
        public GameObject LeaseObject(GameObject template)
        {
            if (template == null)
            {
                return null;
            }
			
            if(!m_TemplateToPools.TryGetValue(template, out IPool pool))
            {
                pool = new Pool();
                pool.Init(template);
            }

            return pool.FindAvailableObject();
        }
        
        internal void AddPool(GameObject template, IPool pool)
        {
            if (pool is MonoBehaviour mb)
            {
                mb.transform.SetParent(m_PoolRoot);
            }
            m_TemplateToPools[template] = pool;
        }
        
        internal bool TryGetPool(GameObject template, out IPool pool)
        {
            return m_TemplateToPools.TryGetValue(template, out pool);
        }

        public void WarmPool(GameObject template, int targetCount)
        {
            if (template == null)
            {
                return;
            }
			
            if(!m_ObjectToPool.TryGetValue(template, out IPool pool))
            {
                pool = new Pool();
                pool.Init(template);
            }
            pool.WarmPool(targetCount);
        }
        
        public void WarmPool<T>(T template, int targetCount) where T: MonoBehaviour
        {
            WarmPool(template.gameObject, targetCount);
        }

        public void ReturnLeasedObj(MonoBehaviour behaviour)
        {
            ReturnLeasedObj(behaviour.gameObject);
        }
        
        public void ReturnLeasedObj(GameObject obj)
        {
            if (m_IsQuitting || obj == null)
            {
                return;
            }
			
            if(m_ObjectToPool.TryGetValue(obj, out IPool pool))
            {
                pool.ReturnLeasedObject(obj);
            }
            else
            {
                obj.SetActive(false);
            }
			
            obj.transform.localPosition = Vector3.zero;
        }

        public void ReturnAll(GameObject template)
        {
            if (m_TemplateToPools.TryGetValue(template, out IPool pool))
            {
                pool.ReturnAll();
            }
        }
        
        internal void RemoveObj(LeaseHandle handle, bool destroyOnRemove = true)
        {
            if (m_IsQuitting)
            {
                return;
            }
            if(m_ObjectToPool.TryGetValue(handle.Obj, out IPool pool))
            {
                pool.RemoveLeasedObject(handle, destroyOnRemove);
            }
        }

        public void ReturnObjectsOfType(params System.Type[] targetTypes)
        {
            foreach (System.Type targetType in targetTypes)
            {
                ReturnObjectsOfType(targetType);
            }
        }
		
        public void ReturnObjectsOfType(System.Type targetType)
        {
            foreach (KeyValuePair<GameObject,IPool> pair in m_ObjectToPool)
            {
                if (pair.Value.HasComponent(targetType))
                {
                    pair.Value.ReturnAll();
                }
            }
        }
        
        public void CleanPool(GameObject template)
        {
            if (m_IsQuitting)
            {
                return;
            }
            if (m_TemplateToPools.TryGetValue(template, out IPool pool))
            {
                pool.Clean();
            }
        }

        public void CleanAllPools()
        {
            if (m_IsQuitting)
            {
                return;
            }

            foreach (KeyValuePair<GameObject,IPool> pair in m_TemplateToPools)
            {
                pair.Value.Clean();
            }
        }
        
        public void ClearPool(GameObject template, bool destroyEvenIfActive = false)
        {
            if (m_IsQuitting)
            {
                return;
            }
            
            if(m_TemplateToPools.TryGetValue(template, out IPool pool))
            {
                pool.Clear(destroyEvenIfActive);
            }
        }
        
        public void ClearAllPools()
        {
            if (m_IsQuitting)
            {
                return;
            }

            foreach (KeyValuePair<GameObject,IPool> pair in m_TemplateToPools)
            {
                pair.Value.Clear();
            }
            m_Lanes.Clear();
        }

        public int GetPoolCount(GameObject template)
        {
            if(m_TemplateToPools.TryGetValue(template, out IPool pool))
            {
                return pool.Count;
            }

            return 0;
        }

        private void OnApplicationQuit()
        {
            m_IsQuitting = true;
        }
        
        internal Transform GetLane(GameObject template)
        {
            if (m_Lanes.TryGetValue(template.GetHashCode(), out Transform holder))
            {
                return holder;
            }

            holder = new GameObject("Lane(" + template.name + ")").transform;
            m_Lanes[template.GetHashCode()] = holder;
            holder.SetParent(m_PoolRoot);
            return holder;
        }
    }
}