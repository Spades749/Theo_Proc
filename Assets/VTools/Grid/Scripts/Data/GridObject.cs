namespace VTools.Grid
{
    public class GridObject
    {
        public GridObjectTemplate Template { get; }
        public Cell Cell { get; private set; }
        public int Rotation { get; private set; }
        
        public GridObject(GridObjectTemplate template)
        {
            Template = template;
        }
        
        public void Rotate(int angle)
        {
            Rotation = angle;
        }
        
        public virtual void SetGridData(Cell originCell, Grid _)
        {
            Cell = originCell;
        }
    }
}