using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    private static readonly KeyCode[] SUPPORTED_KEYS = new KeyCode[]{
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F,
        KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R,
        KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X,
        KeyCode.Y, KeyCode.Z
    };

    private Row[] rows;

    private string[] solutions;
    private string[] validWords;

    private string word;

    private int rowIndex;
    private int columnIndex;

    [Header("States")]
    public Tile.State emptyState;
    public Tile.State occupiedState;
    public Tile.State correctState;
    public Tile.State wrongSpotState;
    public Tile.State incorrectState;

    [Header("UI")]
    public TextMeshProUGUI invalidWordText;
    public Button newWordButton;
    public Button tryAgainButton;

    private void Awake()
    {
        rows = GetComponentsInChildren<Row>();
    }

    private void Start()
    {
        LoadData();
        NewGame();
    }

    public void NewGame()
    {
        ClearBoard();
        SetRandomWord();
        enabled = true;
    }

    public void TryAgain()
    {
        ClearBoard();
        enabled = true;
    }

    private void LoadData()
    {
        TextAsset textFile = Resources.Load("official_wordle_all") as TextAsset;
        validWords = textFile.text.Split('\n');

        textFile = Resources.Load("official_wordle_common") as TextAsset;
        solutions = textFile.text.Split('\n');
    }

    private void SetRandomWord()
    {
        //Random a word
        // ถ้าจะไม่สุ่ม ให้ word = คำๆ นั้นไปเลย แต่ตัวอักษรได้แค่ 5 ตัว
        word = solutions[Random.Range(0, solutions.Length)];
        word = word.ToLower().Trim();
    }

    private void Update()
    {
        Row currentRow = rows[rowIndex];

        if (Input.GetKeyDown(KeyCode.Backspace)) {
            columnIndex = Mathf.Max(columnIndex - 1, 0);

            currentRow.tiles[columnIndex].SetLetter('\0');
            currentRow.tiles[columnIndex].SetState(emptyState);

            invalidWordText.gameObject.SetActive(false);
        }
        else if (columnIndex >= rows[rowIndex].tiles.Length) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                SubmitRow(currentRow);
            }
        }
        else {
            for (int i = 0; i < SUPPORTED_KEYS.Length; i++) {
                if (Input.GetKeyDown(SUPPORTED_KEYS[i])) {
                    currentRow.tiles[columnIndex].SetLetter((char)SUPPORTED_KEYS[i]);
                    currentRow.tiles[columnIndex].SetState(occupiedState);
                    columnIndex++;
                    break;
                }
            }
        }
        
    }

    private void SubmitRow(Row row)
    {
        if (!IsValidWord(row.word))
        {
            invalidWordText.gameObject.SetActive(true);
            return;
        }

        string remaining = word;

        for (int i = 0; i < row.tiles.Length; i++)
        {
            Tile tile = row.tiles[i];

            if (tile.letter == word[i])
            {
                //correct state
                tile.SetState(correctState);

                remaining = remaining.Remove(i, 1);
                remaining = remaining.Insert(i, " ");
            }
            else if (!word.Contains(tile.letter))
            {
                //incorrect
                tile.SetState(incorrectState);
            }
        }

        //wrong spot
        for (int i = 0; i < row.tiles.Length; i++)
        {
            Tile tile = row.tiles[i];
            
            if (tile.state != correctState && tile.state != incorrectState)
            {
                //If input same letters twice or more, and the word have same letters twice or more.
                if (remaining.Contains(tile.letter))
                {
                    tile.SetState(wrongSpotState);

                    int index = remaining.IndexOf(tile.letter);
                    remaining = remaining.Remove(index, 1);
                    remaining = remaining.Insert(index, " ");
                }
                //If input same letters twice or more but the word have only 1 letter.
                else
                {
                    tile.SetState(incorrectState);
                }
            }
        }

        if(HasWon(row))
        {
            enabled = false;
        }

        /*for(int i = 0;i < row.tiles.Length;i++)
        {
            Tile tile = row.tiles[i];

            if(tile.letter == word[i]) {
                //correct state
                tile.SetState(correctState);
            }
            else if (word.Contains(tile.letter)) {
                //wrong spot
                tile.SetState(wrongSpotState);
            }
            else {
                //incorrect
                tile.SetState(incorrectState);
            }
        }*/

        rowIndex++;
        columnIndex = 0;

        if (rowIndex >= rows.Length) {
            enabled = false;
        }
    }

    private void ClearBoard()
    {
        for (int row = 0; row < rows.Length; row++)
        {
            for (int col = 0; col < rows[row].tiles.Length; col++)
            {
                rows[row].tiles[col].SetLetter('\0');
                rows[row].tiles[col].SetState(emptyState);
            }
        }

        rowIndex = 0;
        columnIndex = 0;
    }

    private bool IsValidWord(string word)
    {
        for (int i = 0; i < validWords.Length; i++)
        {
            if (validWords[i].Equals(word)) {
                return true;
            }
        }

        return false;
    }

    private bool HasWon(Row row)
    {
        for (int i = 0; i < row.tiles.Length; i++)
        {
            if (row.tiles[i].state != correctState)
            {
                return false;
            }
        }
        
        return true;
    }

    private void OnEnable()
    {
        tryAgainButton.gameObject.SetActive(false);
        newWordButton.gameObject.SetActive(false);
    }

    private void OnDisable() 
    {
        tryAgainButton.gameObject.SetActive(true);
        newWordButton.gameObject.SetActive(true);
    }

}
