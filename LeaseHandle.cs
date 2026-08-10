using UnityEngine;

namespace GrygTools.Pooler
{
	internal class LeaseHandle
    {
        private bool m_InUse;
        public bool InUse => m_InUse;
        private readonly GameObject m_Obj;
        public GameObject Obj => m_Obj;
        private readonly GameObject m_Template;
        private readonly IPoolableObject m_PoolableObject;

        internal LeaseHandle(GameObject mTemplate, Transform lane)
        {
            this.m_Template = mTemplate;
            m_Obj = Object.Instantiate(mTemplate, lane);
            m_Obj.SetActive(true);
            m_Obj.TryGetComponent(out m_PoolableObject);
        }
        
        internal bool TryLease(out GameObject leaseObject)
        {
            leaseObject = null;
            if (m_InUse)
            {
                return false;
            }
            leaseObject = m_Obj;
            m_InUse = true;
            if (m_PoolableObject != null)
            {
                m_PoolableObject.InitPoolable();
            }
			
            return true;
        }
        
        internal void Return(Transform lane)
        {
            if (m_Obj == null)
            {
                PoolManager.Instance.RemoveObj(this, true);
            }
            else
            {
                if (m_PoolableObject != null)
                {
                    m_PoolableObject.ReturnPoolable();
                }
                
                m_InUse = false;
                m_Obj.SetActive(false);
                m_Obj.transform.SetParent(lane);
            }
        }
    }
}
