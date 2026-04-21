using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform heartsPanel;

    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite threeQuartersHeart;
    [SerializeField] private Sprite halfHeart;
    [SerializeField] private Sprite quarterHeart;
    [SerializeField] private Sprite emptyHeart;

    private List<Image> hearts = new List<Image>();

    void OnEnable()
    {
        PlayerHealth.OnHealthChanged += UpdateHearts;
    }

    void OnDisable()
    {
        PlayerHealth.OnHealthChanged -= UpdateHearts;
    }

    void Start()
    {
        UpdateHearts();
    }

    private void AddHeartPrefab()
    {
        GameObject heartObj = Instantiate(heartPrefab, heartsPanel);
        Image heartImage = heartObj.GetComponent<Image>();
        hearts.Add(heartImage);
    }

    private void UpdateHearts()
    {
        while (hearts.Count < playerHealth.maxHearts)
        {
            AddHeartPrefab();
        }

        int segmentsPerHeart = playerHealth.segmentsPerHeart;

        for (int i = 0; i < hearts.Count; i++)
        {
            int heartSegments = Mathf.Clamp(playerHealth.currentSegment - (i * segmentsPerHeart), 0, segmentsPerHeart);

            switch (heartSegments)
            {
                case 4:
                    hearts[i].sprite = fullHeart; break;
                case 3:
                    hearts[i].sprite = threeQuartersHeart; break;
                case 2: hearts[i].sprite = halfHeart; break;
                case 1: hearts[i].sprite = quarterHeart; break;
                default: hearts[i].sprite = emptyHeart; break;
            }
        }
    }
}
