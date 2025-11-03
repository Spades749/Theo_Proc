using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VTools.Grid
{
    public class Cell
    {
        private readonly float _size;
        private Tuple<GridObject, GridObjectController> _object;
        
        public Vector2Int Coordinates { get; }
        public bool ContainObject => _object != null;
        public GridObject GridObject => _object.Item1;
        public GridObjectController View => _object.Item2;
    
        public Cell(int x, int y, float size)
        {
            Coordinates = new Vector2Int(x, y);
            _size = size;
        }
    
        public void AddObject(GridObjectController controller)
        {
            _object = new Tuple<GridObject, GridObjectController>(controller.GridObject, controller);
        }
        
        public void ClearGridObject()
        {
            if (_object != null)
            {
                Object.Destroy(_object.Item2.gameObject);
                _object = null;
            }
        }
    
        public Vector3 GetCenterPosition(Vector3 originPosition)
        {
            return new Vector3(Coordinates.x + _size / 2, 0, Coordinates.y + _size / 2) * _size + originPosition;
        }
    }
}