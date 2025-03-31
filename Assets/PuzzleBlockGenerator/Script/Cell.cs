using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int CellValue;
    [SerializeField] private List<Sprite> cellSprites;
    [SerializeField] private SpriteRenderer cellRenderer;

    private int spriteIndex => CellValue + 1;

    public void Init(int cellValue)
    {
        CellValue = cellValue;
        cellRenderer.sprite = cellSprites[spriteIndex];
    }
}
