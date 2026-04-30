using Assets.Scripts.Core;
using UnityEngine;

public class sample : MonoBehaviour
{
    [SerializeField] private Transform root;
    [SerializeField] private PartsManager prefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instantiate(prefab, root);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
