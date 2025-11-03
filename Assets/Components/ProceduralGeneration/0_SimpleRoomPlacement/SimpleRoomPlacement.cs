using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
    public class SimpleRoomPlacement : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 10;
        
        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            // Declare variables here
            // ........

            for (int i = 0; i < _maxSteps; i++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();
                
                // Your algorithm here
                // .......

                // Waiting between steps to see the result.
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken : cancellationToken);
            }
            
            // Final ground building.
            BuildGround();
        }
        
        private void BuildGround()
        {
            var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");
            
            // Instantiate ground blocks
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                    {
                        Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                        continue;
                    }
                    
                    GridGenerator.AddGridObjectToCell(chosenCell, groundTemplate, false);
                }
            }
        }
    }
}