using HutongGames.PlayMaker;
using UnityEngine;

namespace MOP.FSM.Actions
{
    /// <summary>
    /// This FsmAction role is to instantiate a gameobject.
    /// </summary>
    class CustomCreateObject : FsmStateAction
    {
        GameObject parent;
        GameObject prefab;

        public CustomCreateObject(GameObject parent, GameObject prefab)
        {
            this.parent = parent;
            this.prefab = prefab;
        }

        public override void OnEnter()
        {
            GameObject newObject = GameObject.Instantiate(prefab);
            newObject.transform.position = parent.transform.position;
            newObject.name = newObject.name.Replace("(Clone)(Clone)", "(Clone)");
            newObject.SetActive(true);
        }
    }
}
