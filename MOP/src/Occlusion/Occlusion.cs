using System;
using UnityEngine;
using System.Xml;

namespace MOP
{
    class Occlusion : MonoBehaviour
    {
        // This class reads through occlusiontable.xml file for files that need to be injected with OcclusionObject script.

        public Occlusion()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Properties.Resources.occlusiontable);
            XmlNodeList root = doc.SelectNodes("World/Object");
            MSCLoader.ModConsole.Print("Initializing occlusion...");
            ReadChildNode(root, "");

            Camera.main.gameObject.AddComponent<OcclusionCamera>();

            MSCLoader.ModConsole.Print("Occlusion Done!");
        }

        void ReadChildNode(XmlNodeList nodeList, string path)
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                XmlNode node = nodeList[i];
                string name = node.Attributes["name"].Value;
                string pathToSelf = path != "" ? path + "/" + name : name;

                if (node.Attributes["exception"] == null)
                {
                    GameObject gm = GameObject.Find(pathToSelf);

                    if (gm == null)
                    {
                        MSCLoader.ModConsole.Error("[MOP] Object not found: " + pathToSelf);
                        continue;
                    }

                    if (gm.GetComponent<OcclusionObject>() == null)
                        gm.AddComponent<OcclusionObject>();
                }

                if (node.ChildNodes.Count > 0)
                {
                    ReadChildNode(node.SelectNodes("Object"), pathToSelf);
                }
            }
        }
    }
}
