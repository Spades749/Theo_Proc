using System;
using UnityEngine;
using VTools.Grid;
using VTools.RandomService;

namespace Components.ProceduralGeneration
{
    public class ProceduralGridGenerator : BaseGridGenerator
    {
        [Header("Generation Parameters")]
        [SerializeField] private ProceduralGenerationMethod _generationMethod;
        [SerializeField] private bool _drawDebug;
        [SerializeField] private int _seed = 1234;
        [SerializeField, Range(1,2000), Tooltip("Delay between each steps in milliseconds")] private int _stepDelay = 500;

        public int StepDelay => _stepDelay;
        
        public override void GenerateGrid()
        {
            // Generate grid base data.
            base.GenerateGrid();

            if (_drawDebug)
                Grid.DrawGridDebug();
            
            // Apply generation.
            ApplyGeneration();
        }

        private async void ApplyGeneration()
        {
            Debug.Log($"Starting generation {_generationMethod.name} ...");
            var time = DateTime.Now;
            
            _generationMethod.Initialize(this, new RandomService(_seed));
            await _generationMethod.Generate();
            
            Debug.Log($"Generation {_generationMethod.name} completed in {(DateTime.Now - time).TotalSeconds : 0.00} seconds.");
        }
    }
}