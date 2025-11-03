using UnityEngine;

namespace VTools.Grid
{
    public static class GridObjectFactory
    {
        /// <summary>
        /// Spawn a Grid Object with all required data setup. 
        /// </summary>
        /// <returns>The GridObjectController mono behaviour that represents the view of the object.</returns>
        public static GridObjectController SpawnFrom(GridObjectTemplate template, Transform parent = null, int rotation = 0, 
            Vector3? scale = null)
        {
            var finalScale = scale ?? Vector3.one;

            // 1. Instantiate controller from prefab
            GridObjectController view = UnityEngine.Object.Instantiate(template.View, parent);

            // 2. Create the data model
            GridObject gridObject = template.CreateInstance();

            // 3. Inject into a controller and finalize the view
            view.Initialize(gridObject);
            view.ApplyTransform(rotation, finalScale);
            view.Rotate(rotation);
            
            return view;
        }
        
        /// <summary>
        /// Spawn a Grid Object with all required data setup. Add the object to the grid at the correct position.
        /// </summary>
        /// <returns>The GridObjectController mono behaviour that represents the view of the object.</returns>
        public static GridObjectController SpawnOnGridFrom(GridObjectTemplate template, Cell cell, Grid grid,
            Transform parent = null, int rotation = 0, Vector3? scale = null)
        {
            var view = SpawnFrom(template, parent, rotation, scale);

            view.AddToGrid(cell, grid, parent);
            cell.AddObject(view);

            return view;
        }
    }
}