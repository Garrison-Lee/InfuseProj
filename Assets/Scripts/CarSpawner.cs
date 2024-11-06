using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class CarSpawner : MonoBehaviour
{
    public NetHandler netHandler;
    public Transform CarSpawnLocation;
    public CarDisplay Car;

    private bool loading = false;

    private IEnumerator DownloadAndLoadAsset(string url)
    {
        loading = true;
        Debug.Log($"Downloading bundle from {url}");

        using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
                // Seems to only be one asset per bundle this is hacky af but short on time
                GameObject asset = bundle.LoadAsset(bundle.GetAllAssetNames()[0]) as GameObject;

                Debug.Log(asset);
                GameObject curObj = Instantiate(asset, CarSpawnLocation);
                curObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                var collider = curObj.AddComponent<BoxCollider>();

                //GROSS
                collider.material = netHandler.shaderExample.GetComponent<SphereCollider>().material;
                collider.center = new Vector3(0f, 1.5f, 0f);
                collider.size = new Vector3(3.75f, 2.5f, 10f);
                var rb = curObj.AddComponent<Rigidbody>();
                rb.mass = 1f;
                curObj.AddComponent<XRGrabInteractable>();
                curObj.AddComponent<XRGeneralGrabTransformer>();

                // Gross af but y'know
                MeshRenderer[] renderers = curObj.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer r in renderers)
                {
                    r.material.shader = netHandler.carShader;
                }

                bundle.Unload(false);
            }
            else
            {
                Debug.Log(www.error);
                yield break;
            }
        }

        loading = false;
    }

    public void SpawnCar()
    {
        if (loading) return;
        StartCoroutine(DownloadAndLoadAsset(Car.ModelURL));
    }
}
