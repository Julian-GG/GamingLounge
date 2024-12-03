using UnityEngine;

/// <summary>
/// Handles the state for removing objects from the grid in the placement system.
/// </summary>
public class RemovingState : IBuildingState
{
    private int gameObjectIndex = -1;
    private Grid grid;
    private PlacementSystem placementSystem;
    private PreviewSystem previewSystem;
    private GridData floorData, wallData, wallDecorData, furnitureData;
    private ObjectPlacer objectPlacer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemovingState"/> class.
    /// </summary>
    /// <param name="grid">The grid system for placement.</param>
    /// <param name="previewSystem">The system for showing removal previews.</param>
    /// <param name="placementSystem">The main placement system.</param>
    /// <param name="floorData">Grid data for floor objects.</param>
    /// <param name="wallData">Grid data for wall objects.</param>
    /// <param name="wallDecorData">Grid data for wall decorations.</param>
    /// <param name="furnitureData">Grid data for furniture objects.</param>
    /// <param name="objectPlacer">Manages the placement and removal of objects.</param>
    public RemovingState(Grid grid,
                         PreviewSystem previewSystem,
                         PlacementSystem placementSystem,
                         GridData floorData,
                         GridData wallData,
                         GridData wallDecorData,
                         GridData furnitureData,
                         ObjectPlacer objectPlacer)
    {
        this.grid = grid;
        this.placementSystem = placementSystem;
        this.previewSystem = previewSystem;
        this.floorData = floorData;
        this.wallData = wallData;
        this.wallDecorData = wallDecorData;
        this.furnitureData = furnitureData;
        this.objectPlacer = objectPlacer;

        previewSystem.StartShowingRemovePreview();
    }

    /// <summary>
    /// Ends the current state, stopping the preview system.
    /// </summary>
    public void EndState()
    {
        previewSystem.StopShowingPreview();
    }

    /// <summary>
    /// Handles the action of removing an object at the specified grid position.
    /// </summary>
    /// <param name="gridPosition">The grid position where the removal action is performed.</param>
    public void OnAction(Vector3Int gridPosition)
    {
        Vector3 newPosition = Vector3.zero;
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = placementSystem.cam.nearClipPlane;
        Ray ray = placementSystem.cam.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100, placementSystem.placementMask))
        {
            newPosition = hit.point;
        }

        Debug.DrawLine(placementSystem.player.transform.position, newPosition, Color.green, 5f);

        GridData selectedData = GetGridDataForRemoval(gridPosition, newPosition);
        
        if (selectedData == null)
        {
            // Optionally: play a sound indicating nothing to remove
        }
        else
        {
            gameObjectIndex = selectedData.GetRepresentationIndex(gridPosition);
            if (gameObjectIndex == -1) return;

            selectedData.RemoveObjectAt(gridPosition);
            objectPlacer.RemoveObjectAt(gameObjectIndex);
        }

        Vector3 cellPosition = grid.CellToWorld(gridPosition);
        previewSystem.UpdatePosition(cellPosition, CheckIfSelectionIsValid(gridPosition));
    }

    /// <summary>
    /// Checks if a grid position has a valid object for removal.
    /// </summary>
    /// <param name="gridPosition">The grid position to check.</param>
    /// <returns>True if there is an object to remove; otherwise, false.</returns>
    private bool CheckIfSelectionIsValid(Vector3Int gridPosition)
    {
        return !(floorData.CanPlaceObjectAt(gridPosition, Vector2Int.one)
            && wallData.CanPlaceObjectAt(gridPosition, Vector2Int.one)
            && wallDecorData.CanPlaceObjectAt(gridPosition, Vector2Int.one)
            && furnitureData.CanPlaceObjectAt(gridPosition, Vector2Int.one));
    }

    /// <summary>
    /// Updates the state by checking the validity of the current grid position for removal.
    /// </summary>
    /// <param name="gridPosition">The current grid position.</param>
    public void UpdateState(Vector3Int gridPosition)
    {
        bool validity = CheckIfSelectionIsValid(gridPosition);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), validity);
    }

    /// <summary>
    /// Determines which grid data to use for removal based on the grid position and distance.
    /// </summary>
    /// <param name="gridPosition">The grid position to check.</param>
    /// <param name="newPosition">The new position from the raycast hit.</param>
    /// <returns>The appropriate GridData for removal, or null if none is valid.</returns>
    private GridData GetGridDataForRemoval(Vector3Int gridPosition, Vector3 newPosition)
    {
        if (!floorData.CanPlaceObjectAt(gridPosition, Vector2Int.one) && IsWithinRemovalRange(newPosition))
            return floorData;
        if (!wallData.CanPlaceObjectAt(gridPosition, Vector2Int.one) && IsWithinRemovalRange(newPosition))
            return wallData;
        if (!furnitureData.CanPlaceObjectAt(gridPosition, Vector2Int.one) && IsWithinRemovalRange(newPosition))
            return furnitureData;

        return null;
    }

    /// <summary>
    /// Checks if the new position is within the valid range for removal.
    /// </summary>
    /// <param name="newPosition">The position to check.</param>
    /// <returns>True if within range; otherwise, false.</returns>
    private bool IsWithinRemovalRange(Vector3 newPosition)
    {
        return Vector3.Distance(placementSystem.player.transform.position, newPosition) < 3;
    }
}
