using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelCreator : MonoBehaviour
{
    public static LevelCreator Instance;

    [SerializeField] private int rows;
    [SerializeField] private int columns;
    [SerializeField] private int spawnRows;
    [SerializeField] private int spawnColumns;
    [SerializeField] private Transform spawnBackgroundPrefab;
    [SerializeField] private Level level;
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private Transform centerPrefab;
    [SerializeField] private float blockSpawnSize = 0.5f;
    [SerializeField] private List<Sprite> blockSprites;
    [SerializeField] private SpawnedBlock blockPrefab;

    private bool isNewLevel;
    private Cell[,] gridCells;
    private int currentCellFillValue;
    private Dictionary<int, Vector2Int> startCenters;
    private List<Transform> centerObjects;
    private Dictionary<int, SpawnedBlock> spawnedBlocks;
    private Vector3 startPos;

    private void Awake()
    {
        Instance = this;
        SpawnBlock();
        SpawnGrid();
    }

    private void SpawnBlock()
    {
        isNewLevel = !(rows == level.Rows && columns == level.Columns);

        if (isNewLevel)
        {
            level.Rows = rows;
            level.Columns = columns;
            level.BlockRows = spawnRows;
            level.BlockColumns = spawnColumns;
            level.Blocks = new List<BlockPiece>();
            level.Data = new List<int>();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    level.Data.Add(-1);
                }
            }
        }

        gridCells = new Cell[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                gridCells[i,j] = Instantiate(cellPrefab);
                gridCells[i,j].Init(level.Data[i * columns + j]);
                gridCells[i,j].transform.position = new Vector3(j + 0.5f, i + 0.5f, 0f);
            }
        }

        currentCellFillValue = -1;

    }

    private void SpawnGrid()
    {
        startPos = Vector3.zero;
        startPos.x = 0.25f + (level.Columns - level.BlockColumns * blockSpawnSize) * 0.5f;
        startPos.y = -level.BlockRows * +blockSpawnSize - 1f + 0.25f;

        for (int i = 0; i < spawnRows; i++)
        {
            for (int j = 0; j < spawnColumns; j++)
            {
                Vector3 spawnPos = startPos + new Vector3(j, i, 0) * blockSpawnSize;
                Transform spawnCell = Instantiate(spawnBackgroundPrefab);
                spawnCell.position = spawnPos;
            }
        }

        float maxColumns = Mathf.Max(level.Columns, level.BlockColumns * blockSpawnSize);
        float maxRows = level.Rows + 2f + level.BlockRows * blockSpawnSize;
        Camera.main.orthographicSize = Mathf.Max(maxColumns, maxRows) * 0.65f;
        Vector3 camPos = Camera.main.transform.position;
        camPos.x = level.Columns * 0.5f;
        camPos.y = (level.Rows + 0.5f + startPos.y) * 0.5f;
        Camera.main.transform.position = camPos;

        //Set StartCenters
        startCenters = new Dictionary<int, Vector2Int>();
        centerObjects = new List<Transform>();
        spawnedBlocks = new Dictionary<int, SpawnedBlock>();

        List<Sprite> sprites = blockSprites;

        for (int i = 0; i < sprites.Count; i++)
        {
            if (i == 0) continue; // Skip index -1 issues
             
            spawnedBlocks[i - 1] = null;
            startCenters[i - 1] = Vector2Int.zero;
            centerObjects.Add(Instantiate(centerPrefab));
            centerObjects[i - 1].GetChild(0).GetComponent<SpriteRenderer>().sprite = sprites[i];
            centerObjects[i - 1].gameObject.SetActive(false);
        }

        for (int i = 0; i < level.Blocks.Count; i++)
        {
           int tempId = level.Blocks[i].Id;
           Vector2Int pos = level.Blocks[i].CenterPos;
           centerObjects[tempId].gameObject.SetActive(true);
           centerObjects[tempId].transform.position = new Vector3(pos.y + 0.5f, pos.x + 0.5f, 0f);
           spawnedBlocks[tempId] = Instantiate(blockPrefab);
           spawnedBlocks[tempId].Init(level.Blocks[i], startPos);
        }
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            //Set Grid Position
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            Vector2Int mousePosGrid = new Vector2Int(Mathf.FloorToInt(mousePos.y), Mathf.FloorToInt(mousePos.x));

            if(!IsValidPosition(mousePosGrid)) return;

            gridCells[mousePosGrid.x, mousePosGrid.y].Init(currentCellFillValue);
            level.Data[mousePosGrid.x * columns + mousePosGrid.y] = currentCellFillValue;
            EditorUtility.SetDirty(level);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int mousePosGrid = new Vector2Int(Mathf.FloorToInt(mousePos.y), Mathf.FloorToInt(mousePos.x));
            if(!IsValidPosition(mousePosGrid)) return;
            if(currentCellFillValue == -1) return;
            centerObjects[currentCellFillValue].gameObject.SetActive(true);
            centerObjects[currentCellFillValue].transform.position = new Vector3(mousePosGrid.y + 0.5f, mousePosGrid.x + 0.5f, 0);
            startCenters[currentCellFillValue] = mousePosGrid;
            EditorUtility.SetDirty(level);
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(currentCellFillValue == -1) return;
            BlockPiece spawnPiece = GetBlockPiece();
            for (int i = 0; i < level.Blocks.Count; i++)
            {
                if(level.Blocks[i].Id == spawnPiece.Id)
                {
                    level.Blocks.RemoveAt(i);
                    i--;
                }
            }
            level.Blocks.Add(spawnPiece);
            if(spawnedBlocks[currentCellFillValue] != null)
            {
                Destroy(spawnedBlocks[currentCellFillValue].gameObject);
            }
            spawnedBlocks[currentCellFillValue] = Instantiate(blockPrefab);
            spawnedBlocks[currentCellFillValue].Init(spawnPiece,startPos);
            EditorUtility.SetDirty(level);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            MoveBlock(Vector2Int.down);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            MoveBlock(Vector2Int.up);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            MoveBlock(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            MoveBlock(Vector2Int.right);
        }
    }

    private void MoveBlock(Vector2Int offset)
    {
        for (int i = 0; i < level.Blocks.Count; i++)
        {
            if (level.Blocks[i].Id == currentCellFillValue)
            {
                Vector2Int pos = level.Blocks[i].StartPos;
                pos.x += offset.x;
                pos.y += offset.y;
                BlockPiece piece = level.Blocks[i];
                piece.StartPos = pos;
                level.Blocks[i] = piece;
                Vector3 movePos = spawnedBlocks[currentCellFillValue].transform.position;
                movePos.x += offset.y * blockSpawnSize;
                movePos.y += offset.x * blockSpawnSize;
                spawnedBlocks[currentCellFillValue].transform.position = movePos;
            }
        }
        EditorUtility.SetDirty(level);
    }

    private BlockPiece GetBlockPiece()
    {
        int id = currentCellFillValue;
        BlockPiece result = new BlockPiece();
        result.Id = id;
        result.CenterPos = startCenters[id];
        result.StartPos = Vector2Int.zero;
        result.BlockPositions = new List<Vector2Int>();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (gridCells[i,j].CellValue == id)
                {
                    result.BlockPositions.Add(new Vector2Int(i,j) - result.CenterPos);
                }
            }
        }
        return result;
    }

    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < rows && pos.y < columns;
    }

    public void ChangeCellFillValue(int value)
    {
        currentCellFillValue = value;
    }
}
