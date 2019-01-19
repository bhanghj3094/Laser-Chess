﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public abstract class BasePiece : EventTrigger
{
    [HideInInspector]
    public Color mColor = Color.clear;
    public bool mIsFirstMove = true;

    protected Cell mOriginalCell = null;
    protected Cell mCurrentCell = null;

    protected RectTransform mRectTransform = null;
    protected PieceManager mPieceManager;

    protected Cell mTargetCell = null;

    protected Vector3Int mMovement = Vector3Int.one;
    protected List<Cell> mHighlightedCells = new List<Cell>();

    public void Setup(Color newTeamColor, Color32 newSpriteColor, PieceManager newPieceManager)
    {

        mPieceManager = newPieceManager;

        mColor = newTeamColor;
        GetComponent<Image>().color = newSpriteColor;
        mRectTransform = GetComponent<RectTransform>();
    }

    public void Place(Cell newCell)
    {
        //Cell Stuff
        mCurrentCell = newCell;
        mOriginalCell = newCell;
        mCurrentCell.mCurrentPiece = this;

        //Object stuff
        transform.position = newCell.transform.position;
        
        Debug.Log(transform.position.x+", "+ transform.position.y);

        gameObject.SetActive(true);
    }

    public void Reset()
    {
        Kill();
        Place(mOriginalCell);
    }

    public void Kill()
    {
        mCurrentCell.mCurrentPiece = null;

        gameObject.SetActive(false);
    }

    #region Movement
    private void CreateCellPath(int xDirection, int yDirection)
    {
        //TargetPosition
        int currentX = mCurrentCell.mBoardPosition.x;
        int currentY = mCurrentCell.mBoardPosition.y;

        //Check each cell

        currentX += xDirection;
        currentY += yDirection;

        CellState cellState = CellState.None;

        //target cell 의 state
        cellState = mCurrentCell.mBoard.ValidateCell(currentX, currentY, this);

        if (cellState == CellState.Enemy)
        {
            mHighlightedCells.Add(mCurrentCell.mBoard.mAllCells[currentX, currentY]);
            return;
        }

        if (cellState != CellState.Free)
            return;
        //Get the state of the target cell

        //Add to list
        mHighlightedCells.Add(mCurrentCell.mBoard.mAllCells[currentX, currentY]);
        
    }

    public void CheckPathing()
    {

        //Horizontal
        CreateCellPath(1, 0);
        CreateCellPath(-1, 0);

        //Vertical
        CreateCellPath(0, 1);
        CreateCellPath(0, -1);

        //Upper diagonal
        CreateCellPath(1, 1);
        CreateCellPath(-1, 1);

        //Lower diagonal
        CreateCellPath(-1, -1);
        CreateCellPath(1, -1);
        
    }

    protected void ShowCells()
    {
        foreach (Cell cell in mHighlightedCells)
        {
            cell.mOutlineImage.enabled = true;
        }

    }

    protected void ClearCells()
    {

        foreach (Cell cell in mHighlightedCells)
        {
            cell.mOutlineImage.enabled = false;
        }

        mHighlightedCells.Clear();
    }

    protected virtual void Move()
    {
        // if there is an enemy piece, remove it
        mTargetCell.RemovePiece();

        //Clear current
        mCurrentCell.mCurrentPiece = null;

        //Switch cells
        mCurrentCell = mTargetCell;
        mCurrentCell.mCurrentPiece = this;

        //Move on board
        transform.position = mCurrentCell.transform.position;
        mTargetCell = null;

    }
    #endregion

    #region Events
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);


    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        CheckPathing();

        ShowCells();
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        
        //Follow Pointer
        transform.position += (Vector3)eventData.delta;

        foreach ( Cell cell in mHighlightedCells)
        {
            if(RectTransformUtility.RectangleContainsScreenPoint(cell.mRectTransform, Input.mousePosition))
            {
                // if the mouse is within a valid cell
                mTargetCell = cell;
                break;
            }

            //mouse is not within any highlighted cell
            mTargetCell = null;
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

       
        //return to original position
        if (!mTargetCell)
        {
            transform.position = mCurrentCell.gameObject.transform.position;
            ClearCells();
            return;
        }

        Move();
        Debug.Log(mColor);
        mPieceManager.SwitchSides(mColor);
        ClearCells();

    }
    #endregion
}