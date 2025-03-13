using UnityEngine;

public class GameConfig : MonoBehaviour
{
    public float timeScale = 1.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = timeScale;
    }

    
}
