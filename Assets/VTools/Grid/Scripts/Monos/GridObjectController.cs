using UnityEngine;
using VTools.Utility;

namespace VTools.Grid
{
    public class GridObjectController : MonoBehaviour
    {
        public GridObject GridObject { get; private set; }
        
        public void Initialize(GridObject gridObject)
        {
            GridObject = gridObject;
        }
        
        public void ApplyTransform(float localRotation, Vector3 scale)
        {
            transform.localScale = scale;
            transform.localRotation = Quaternion.Euler(0, localRotation, 0);
        }
        
        public void MoveTo(Vector3 position)
        {
            transform.position = position;
        }
        
        public void Rotate(int angle)
        {
            angle = angle.NormalizeAngle();
            transform.localRotation = Quaternion.Euler(0, angle, 0);
            GridObject.Rotate(angle);
        }

        public void AddToGrid(Cell cell, Grid grid, Transform parent)
        {
            GridObject.SetGridData(cell, grid);
            MoveTo(cell.GetCenterPosition(grid.OriginPosition));
        }
    }
}