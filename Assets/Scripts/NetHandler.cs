using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

/// <summary>
/// Single monolithic script for handling all the webby stuff for the infuse proj. Normally I'd divy this up
///  into smaller responsibilities but... 3 hours
/// </summary>
public class NetHandler : MonoBehaviour
{
    public string API_URL = "https://infusemedota.com/sites/progtest/vehicles.php";

    public Transform SpawnLocation;

    public List<CarDisplay> Cars = new List<CarDisplay>();
    public List<Texture> DownloadedTextures = new List<Texture>();
    public List<GameObject> DownloadedAssets = new List<GameObject>();
    public MeshRenderer shaderExample;
    public Shader carShader;
    public bool IsReady = false;


    private bool abort = false;
    private string jsonResponse;

    private IEnumerator HitAPI(string urlSuffix = "")
    {
        using (UnityWebRequest www = UnityWebRequest.Get(API_URL + urlSuffix))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                jsonResponse = www.downloadHandler.text;
                Debug.Log(jsonResponse);
                InfuseResponse response = JsonConvert.DeserializeObject<InfuseResponse>(jsonResponse);

                foreach (CarEntry entry in response.response)
                {
                    Cars.Add(new CarDisplay(entry));
                }
            }
            else
            {
                abort = true;
                Debug.Log("Error: " + www.error);
                yield break;
            }
        }
    }

    private IEnumerator DownloadAndLoadAsset(string assetName, string url)
    {
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
                GameObject curObj = Instantiate(asset, SpawnLocation);
                curObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                var collider = curObj.AddComponent<BoxCollider>();

                //GROSS
                collider.material = shaderExample.GetComponent<SphereCollider>().material;
                collider.center = new Vector3(0f, 1.5f, 0f);
                collider.size = new Vector3(3.75f, 2.5f, 10f);
                var rb = curObj.AddComponent<Rigidbody>();
                rb.mass = 1f;
                curObj.AddComponent<XRGrabInteractable>();
                curObj.AddComponent<XRGeneralGrabTransformer>();
                DownloadedAssets.Add(curObj);

                // Gross af but y'know
                MeshRenderer[] renderers = curObj.GetComponentsInChildren<MeshRenderer>();
                foreach(MeshRenderer r in renderers)
                {
                    r.material.shader = carShader;
                }


                bundle.Unload(false);
            }
            else
            {
                abort = true;
                Debug.Log(www.error);
                yield break;
            }
        }


    }

    private IEnumerator DownloadTexture(string url)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture texture = DownloadHandlerTexture.GetContent(www);
                DownloadedTextures.Add(texture);
            }
            else
            {
                abort = true;
                Debug.Log(www.error);
                yield break;
            }
        }
    }

    private IEnumerator WholeEnchilada()
    {
        yield return HitAPI();
        if (abort) yield break;

        IsReady = true;
        //foreach (CarDisplay car in Cars)
        //{
        //    yield return DownloadAndLoadAsset(car.Name, car.ModelURL);
        //    if (abort) yield break;
        //}
    }

    private void Start()
    {
        carShader = shaderExample.material.shader;
        StartCoroutine(WholeEnchilada());
    }
}

#region JSON_CLASSES
[System.Serializable]
public class InfuseResponse
{
    public CarEntry[] response;
}

[System.Serializable]
public class CarEntry
{
    public int id;
    public string name;
    public int year;
    public string imageURL;
    public string thumbURL;
    public string price;
    public string location;
    public float latitude;
    public float longitude;
    public string modelURL;
}
#endregion
