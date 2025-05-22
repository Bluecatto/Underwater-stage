using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics.Contracts;

public class CollectableObject : MonoBehaviour
{
    public TextMeshProUGUI collectedObjectsText;
    public static int collectedCount = 0;
    public static int totalObjects = 0;

    private void Start()
    {
        collectedCount = 0;

        UpdateText();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Object collected!");

            collectedCount++;
            UpdateText();
            Destroy(gameObject);
        }
    }

    private void UpdateText()
    {
        collectedObjectsText.text = $"Collected objects: {collectedCount}/{totalObjects}";
    }
}