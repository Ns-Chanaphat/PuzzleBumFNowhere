using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnedBlock : MonoBehaviour
{
    [SerializeField] private Transform blockPrefab;
    [SerializeField] private List<Sprite> blockSprites;
    [SerializeField] private float blockSize;

    public void Init(BlockPiece piece, Vector3 gridStart)
    {
        transform.localScale = Vector3.one * blockSize;
        transform.position = gridStart + new Vector3(piece.StartPos.y * blockSize, piece.StartPos.x * blockSize, 0);

        Sprite currentSprite = blockSprites[piece.Id + 1];   
        
        for (int i = 0; i < piece.BlockPositions.Count; i++)
        {
            Transform block = Instantiate(blockPrefab, transform);
            block.transform.localPosition = new Vector3(piece.BlockPositions[i].y, piece.BlockPositions[i].x, 0);
            block.GetComponent<SpriteRenderer>().sprite = currentSprite;
        }
    }
}
