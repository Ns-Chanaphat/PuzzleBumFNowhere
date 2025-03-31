using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundCell : MonoBehaviour
{
    [HideInInspector] public bool IsBlocked;
    [HideInInspector] public bool IsFilled;

    [SerializeField] private SpriteRenderer backgroundSpite;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite blockedSprite;
    [SerializeField] private Color startColor; 
    [SerializeField] private Color correctColor;
    [SerializeField] private Color incorrectColor;

    public void Init(int blockValue)
    {
        IsBlocked = blockValue == -1;
        if(IsBlocked)
        {
            IsFilled = true;
        }
        backgroundSpite.sprite = IsBlocked ? blockedSprite : emptySprite;
        //ตัวแปร = เงื่อนไข ? ค่าถ้าจริง : ค่าถ้าเท็จ;
        // ถ้า IsBlocked เป็น true  → backgroundSpite.sprite = blockedSprite 
        // ถ้า IsBlocked เป็น false → backgroundSpite.sprite = emptySprite
    }

    public void ResetHighLight()
    {
        backgroundSpite.color = startColor;
    }

    public void UpdateHighLight(bool isCorrect)
    {
        backgroundSpite.color = isCorrect ? correctColor : incorrectColor;
        //ตัวแปร = เงื่อนไข ? ค่าถ้าจริง : ค่าถ้าเท็จ;
        // ถ้า isCorrect เป็น true  → backgroundSpite.color = correctColor 
        // ถ้า isCorrect เป็น false → backgroundSpite.color = incorrectColor
    }
}

