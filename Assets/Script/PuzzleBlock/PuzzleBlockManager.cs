using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleBlockManager : MonoBehaviour
{
    public static PuzzleBlockManager Instance;

    [SerializeField] private Level level;
    [SerializeField] private BackgroundCell backgroundCellPrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private float blockSpawnSize;
    [SerializeField] private float blockHighLightSize;
    [SerializeField] private float blockPutSize;

    private BackgroundCell[,] backgroundCellGrid;
    private bool hasGameFinished;
    private Block currentBlock;
    private Vector2 currentPos, previousPos;
    private List<Block> gridBlocks;

    private void Awake()
    {
        Instance = this;
        hasGameFinished = false;
        gridBlocks = new List<Block>();
        SpawnGrid();
        SpawnBlocks();
    }

    private void SpawnGrid()
    {
        backgroundCellGrid = new BackgroundCell[level.Rows, level.Columns];
        for (int i = 0; i < level.Rows; i++)
        {
            for (int j = 0; j < level.Columns; j++)
            {
                BackgroundCell backgroundCell = Instantiate(backgroundCellPrefab);
                backgroundCell.transform.position = new Vector3(j + 0.5f, i + 0.5f, 0f);
                backgroundCell.Init(level.Data[i * level.Columns + j]);
                backgroundCellGrid[i,j] = backgroundCell;
            }
        }
    }

    private void SpawnBlocks()
    {
        Vector3 startPos = Vector3.zero;
        startPos.x = 0.25f + (level.Columns - level.BlockColumns * blockSpawnSize) * 0.5f;
        startPos.y = -level.BlockRows * blockSpawnSize + 0.25f - 1f;

        for (int i = 0; i < level.Blocks.Count; i++)
        {
            Block block = Instantiate(blockPrefab);
            Vector2Int blockPos = level.Blocks[i].StartPos;
            Vector3 blockSpawnPos = startPos + new Vector3(blockPos.y, blockPos.x, 0) * blockSpawnSize;
            block.transform.position = blockSpawnPos;
            block.Init(level.Blocks[i].BlockPositions, blockSpawnPos, level.Blocks[i].Id);
        }

        float maxColumns = Mathf.Max(level.Columns, level.BlockColumns * blockSpawnSize);
        float maxRows = level.Rows + 2f + level.BlockRows * blockSpawnSize;
        Camera.main.orthographicSize = Mathf.Max(maxColumns, maxRows) *0.65f;
        Vector3 camPos = Camera.main.transform.position;
        camPos.x = level.Columns * 0.5f;
        camPos.y = (level.Rows + 0.5f + startPos.y) * 0.5f;
        Camera.main.transform.position = camPos;
    }

    private void Update()
    {
        if (hasGameFinished) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (!hit) return;
            currentBlock = hit.collider.transform.parent.GetComponent<Block>();
            if (currentBlock == null) return;
            currentPos = mousePos2D;
            previousPos = mousePos2D;
            currentBlock.ElevateSprites();
            currentBlock.transform.localScale = Vector3.one * blockHighLightSize;
            if(gridBlocks.Contains(currentBlock))
            {
                gridBlocks.Remove(currentBlock);
            }
            UpdateFilled();
            ResetHighLight();
            UpdateHighLight();
        }
        else if (Input.GetMouseButton(0) && currentBlock != null)
        {
            currentPos = mousePos;
            currentBlock.UpdatePos(currentPos - previousPos);
            previousPos = currentPos;
            ResetHighLight();
            UpdateHighLight();
        }
        else if (Input.GetMouseButtonUp(0) && currentBlock != null)
        {
            currentBlock.ElevateSprites(true);

            if(IsCorrectMove())
            {
                currentBlock.UpdateCorrectMove();
                currentBlock.transform.localScale = Vector3.one * blockPutSize;
                gridBlocks.Add(currentBlock);
            }
            else if (mousePos2D.y < 0)
            {
                currentBlock.UpdateStartMove();
                currentBlock.transform.localScale = Vector3.one * blockSpawnSize;
            }
            else
            {
                currentBlock.UpdateIncorrectMove();
                if (currentBlock.CurrentPos.y > 0)
                {
                    gridBlocks.Add(currentBlock);
                    currentBlock.transform.localScale = Vector3.one * blockPutSize;
                }
                else
                {
                    currentBlock.transform.localScale = Vector3.one * blockSpawnSize;
                }
            }

            currentBlock = null;
            ResetHighLight();
            UpdateFilled();
            CheckWin();
        }
    }

    private void ResetHighLight()
    {
        for (int i = 0; i < level.Rows; i++)
        {
            for (int j = 0; j < level.Columns; j++)
            {
                if (!backgroundCellGrid[i,j].IsBlocked)
                {
                    backgroundCellGrid[i,j].ResetHighLight();
                }
            }
        }
    }

    private void UpdateFilled()
    {
        for (int i = 0; i < level.Rows; i++)
        {
            for (int j = 0; j < level.Columns; j++)
            {
                if (!backgroundCellGrid[i,j].IsBlocked)
                {
                    backgroundCellGrid[i,j].IsFilled = false;
                }
            }
        }

        foreach (var block in gridBlocks)
        {
            foreach (var pos in block.BlockPositions())
            {
                if(IsValidPos(pos))
                {
                    backgroundCellGrid[pos.x, pos.y].IsFilled = true;
                }
            }
        }
    }

    private void UpdateHighLight()
    {
        bool isCorrect = IsCorrectMove();

        foreach (var pos in currentBlock.BlockPositions())
        {
            if(IsValidPos(pos))
            {
                backgroundCellGrid[pos.x, pos.y].UpdateHighLight(isCorrect);
            }
        }
    }

    private bool IsCorrectMove()
    {
        foreach (var pos in currentBlock.BlockPositions())
        {
            if(!IsValidPos(pos) || backgroundCellGrid[pos.x, pos.y].IsFilled)          
            {
                return false;
            }
        }
        return true;
    }

    private bool IsValidPos(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < level.Rows && pos.y < level.Columns;
    }

    private void CheckWin()
    {
        for (int i = 0; i < level.Rows; i++)
        {
            for (int j = 0; j < level.Columns; j++)
            {
                if (!backgroundCellGrid[i,j].IsFilled) return;
            }
        }

        hasGameFinished = true;
        StartCoroutine(GameWin());
    }

    private IEnumerator GameWin()
    {
        yield return new WaitForSeconds(2f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
