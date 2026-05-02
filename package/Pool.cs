        public Task Init(GameObject template)
        {
            m_Template = template;
            PoolManager.Instance.AddPool(template, this);
            m_Lane = PoolManager.Instance.GetLane(template);
            return Task.CompletedTask;
        }
        
        void IPool.WarmPool(int amount)
        {
            for (int i = 0; i < amount - m_Handles.Count; i++)
            {
                LeaseHandle newLease = new LeaseHandle(m_Template, m_Lane);
                m_Handles.Add(newLease.Obj, newLease);
                PoolManager.Instance.ObjectToPool.Add(newLease.Obj, this);
            }
        }
        
        GameObject IPool.FindAvailableObject()
        {
            GameObject leaseObject;
            foreach (KeyValuePair<GameObject,LeaseHandle> pair in m_Handles)
            {
                LeaseHandle lease = pair.Value;

                if (lease == null)
                {
                    m_Handles.Remove(pair.Key);
                }
                else if (lease.TryLease(out leaseObject))
                {
                    leaseObject.transform.rotation = m_Template.transform.rotation;
                    leaseObject.transform.localScale = m_Template.transform.localScale;
                    leaseObject.SetActive(true);
				
                    return leaseObject;
                }
            }

            LeaseHandle newLease = new LeaseHandle(m_Template, m_Lane);
            m_Handles.Add(newLease.Obj, newLease);
            newLease.TryLease(out leaseObject);
            PoolManager.Instance.ObjectToPool.Add(leaseObject, this);
            return leaseObject;
        }
        
        void IPool.ReturnLeasedObject(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }
            if (m_Handles.TryGetValue(obj, out LeaseHandle handle))
            {
                handle.Return(m_Lane);
            }
        }
        
        bool IPool.HasComponent(System.Type type)
        {
            return m_Template != null && m_Template.TryGetComponent(type, out Component test);

        }

        void IPool.ReturnAll()
        {
            foreach (KeyValuePair<GameObject, LeaseHandle> pair in m_Handles)
            {
                LeaseHandle lease = pair.Value;
                if (lease != null)
                {
                    lease.Return(m_Lane);
                }
            }
        }

        void IPool.RemoveLeasedObject(LeaseHandle leaseHandle, bool destroyOnRemove)
        {
            foreach (KeyValuePair<GameObject,LeaseHandle> pair in m_Handles)
            {
                m_Handles.Remove(pair.Key);
                PoolManager.Instance.ObjectToPool.Remove(pair.Key);
                if (destroyOnRemove && pair.Value.Obj != null)
                {
                    Object.Destroy(pair.Value.Obj);
                }
            }
        }

        void IPool.Clean()
        {
            List<GameObject> toBeRemoved = new();
            foreach (KeyValuePair<GameObject,LeaseHandle> pairs in m_Handles)
            {
                if(pairs.Value == null || pairs.Value.Obj == null)
                {
                    toBeRemoved.Add(pairs.Key);
                }
            }
            foreach (GameObject gameObject in toBeRemoved)
            {
                PoolManager.Instance.ObjectToPool.Remove(gameObject);
                m_Handles.Remove(gameObject);
            }
        }

        void IPool.Clear(bool destroyEvenIfActive)
        {
            List<GameObject> toBeRemoved = new();
            foreach (KeyValuePair<GameObject, LeaseHandle> pair in m_Handles)
            {
                if (destroyEvenIfActive || !pair.Value.InUse)
                {
                    Object.Destroy(pair.Value.Obj);
                    toBeRemoved.Add(pair.Key);
                }
            }
            foreach (GameObject gameObject in toBeRemoved)
            {
                PoolManager.Instance.ObjectToPool.Remove(gameObject);
                m_Handles.Remove(gameObject);
            }
        }
    }
}
