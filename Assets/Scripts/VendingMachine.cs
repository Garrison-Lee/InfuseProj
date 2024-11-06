using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

public class VendingMachine : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform buttonSpawn;

    public NetHandler netHandler;

    private IEnumerator SetCarDisplayTexture(string url, CarDisplay car)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture texture = DownloadHandlerTexture.GetContent(www);
                car.displayTexture = texture;
            }
            else
            {
                Debug.Log(www.error);
                yield break;
            }
        }
    }

    private IEnumerator PopulateVendingMachine()
    {
        while (!netHandler.IsReady)
        {
            yield return new WaitForSeconds(0.1f);
        }

        foreach (CarDisplay car in netHandler.Cars)
        {
            GameObject newButton = Instantiate(buttonPrefab, buttonSpawn);
            ButtonComponentReferences refs = newButton.GetComponent<ButtonComponentReferences>();

            refs.Label.text = $"<size=16><color=\"green\">{car.Name}\n<size=11><color=\"white\">{car.Price}";
            refs.Spawner.CarSpawnLocation = netHandler.SpawnLocation;
            refs.Spawner.Car = car;
            refs.Spawner.netHandler = netHandler;

            yield return SetCarDisplayTexture(car.ThumbURL, car);

            refs.ButtonImage.texture = car.displayTexture;
        }
    }

    private void Start()
    {
        StartCoroutine(PopulateVendingMachine());
    }
}
